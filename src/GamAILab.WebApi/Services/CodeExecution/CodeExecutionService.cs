using System.Diagnostics;
using System.Text.Json;
using GamAILab.Shared.Models.AICodeEvaluation;
using GamAILab.Shared.Models.CodeExecution;

namespace GamAILab.WebApi.Services.CodeExecution;

public class CodeExecutionService : ICodeExecutionService
{
    private const string DockerImage = "gamai-lab-code-runner:0.1";
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);
    
    // TODO I might change these later, its to prevent many containers from starting together (which will be important for AI persona testing later)
    private static readonly SemaphoreSlim CodeExecutions = new SemaphoreSlim(4, 4);
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly ILogger<CodeExecutionService> _logger;

public CodeExecutionService(ILogger<CodeExecutionService> logger)
{
    _logger = logger;
}

public async Task<CodeExecutionResult> ExecuteCodeAsync(string learnerCode, AICodeEvaluationPlan codeEvaluationPlan,
    CancellationToken cancellationToken = default)
{
    await CodeExecutions.WaitAsync(cancellationToken);

    try
    {
        return await ExecuteCode(learnerCode, codeEvaluationPlan, true, cancellationToken);
    }
    finally
    {
        CodeExecutions.Release();
    }
}

// Executes code without running AI evaluation plan (so learners can see outputs from the frontend)
public async Task<CodeExecutionResponse> ExecuteCodeNoTests(string learnerCode, CancellationToken cancellationToken = default)
{
    await CodeExecutions.WaitAsync(cancellationToken);

    try
    {
        var result = await ExecuteCode(learnerCode, null, false, cancellationToken);
        
        // It's probably enough to provide the client with minimal required since this only runs the code without the tests 
        return new CodeExecutionResponse
        {
            CodeOutput = result.StandardOutput,
            CodeError = !string.IsNullOrEmpty(result.FatalError) ? result.FatalError : result.StandardError,
            DidComplete = result.DidComplete,
            TimedOut = result.TimedOut,
            ExecutionDurion = result.ExecutionDuration
        };
    }
    finally
    {
        CodeExecutions.Release();
    }
}

private async Task<CodeExecutionResult> ExecuteCode(string learnerCode, AICodeEvaluationPlan codeEvaluationPlan, bool runTests, CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(learnerCode))
    {
        throw new ArgumentException("Learner code cannot be empty", nameof(learnerCode));
    }

    if (runTests)
    {
        // I'll just hard-code Python for now and make the system more flexible later 
        if(!string.Equals(codeEvaluationPlan.Language, "Python", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Language is not supported", nameof(codeEvaluationPlan.Language));
        }

        if (codeEvaluationPlan.Tests.Count == 0)
        {
            throw new ArgumentException("There are no tests ot evaluate");
        }
    }
    
    var timerStart = Stopwatch.GetTimestamp();
    var codeExecutionId = Guid.NewGuid().ToString("N"); // Digits should be fine as I'll use it consistently
    var containerName = $"gamai-lab-code-runner-{codeExecutionId}"; // TODO append code languages here later
    
    var tempDirectory = Path.Combine(Path.GetTempPath(), $"gamai-code-executon-{codeExecutionId}");
    
    Directory.CreateDirectory(tempDirectory);

    try
    {
        var submissionPath = Path.Combine(tempDirectory, "code_submission.py");
        
        var testsPath = Path.Combine(tempDirectory, "tests.json");
        
        await File.WriteAllTextAsync(submissionPath, learnerCode, cancellationToken);
        
        var testsJsonFormat = runTests ? JsonSerializer.Serialize(codeEvaluationPlan.Tests, JsonSerializerOptions) : "[]";

        await File.WriteAllTextAsync(testsPath, testsJsonFormat, cancellationToken);

        using var dockerInstance = new Process
        {
            StartInfo = CreateDockerInstanceStartInfo(containerName, tempDirectory )
        };
        
        _logger.LogInformation($"Executing code in docker container {containerName} with code {learnerCode} and tests {testsJsonFormat}");
        _logger.LogInformation("Starting up docker container ");

        if (!dockerInstance.Start())
        {
            throw new ApplicationException("Docker container could not be started");
        }
        
        // Read/"cache" early to avoid blocked buffers
        var standardOutput = dockerInstance.StandardOutput.ReadToEndAsync();
        var standardError = dockerInstance.StandardError.ReadToEndAsync();
        
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(Timeout);

        try
        {
            await dockerInstance.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException e) when(!cancellationToken.IsCancellationRequested)
        {
            // Cleanup and attempt to kill the process
            await RemoveContainer(containerName);
            AttemptKillProcess(dockerInstance);
            
            var timeeoutOutput = await standardOutput;
            var timeoutError = await standardError;

            return new CodeExecutionResult
            {
                DidComplete = false,
                TimedOut = true,
                EveryTestPassed = false,
                ExitCode = -1,
                StandardOutput = TruncateString(timeeoutOutput),
                StandardError = TruncateString(timeoutError),
                FatalError = $"Code execution timed out after {Timeout.TotalSeconds} seconds",
                ExecutionDuration = Stopwatch.GetElapsedTime(timerStart)
            };
        }
        
        var dockerContainerOutput = await standardOutput;
        var dockerContainerError = await standardError;

        if (dockerInstance.ExitCode != 0)
        {
            return new CodeExecutionResult
            {
                DidComplete = false,
                TimedOut = false,
                EveryTestPassed = false,
                ExitCode = dockerInstance.ExitCode,
                StandardOutput = TruncateString(dockerContainerOutput),
                StandardError = TruncateString(dockerContainerError),
                FatalError = $"The docker container exited unsuccessfully",
                ExecutionDuration = Stopwatch.GetElapsedTime(timerStart)
            };
        }
        
        CodeExecutionRunnerResponse runnerResponse;

        try
        {
            runnerResponse = JsonSerializer.Deserialize<CodeExecutionRunnerResponse>(dockerContainerOutput, JsonSerializerOptions) 
                ?? throw new JsonException("Docker container could not be deserialised or was null");
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Code runner provided invalid response: {runnerResponse}");
            
            return new CodeExecutionResult
            {
                DidComplete = false,
                TimedOut = false,
                EveryTestPassed = false,
                ExitCode = dockerInstance.ExitCode,
                StandardOutput = TruncateString(dockerContainerOutput),
                StandardError = TruncateString(dockerContainerError),
                FatalError = $"The returned code runner JSON is invalid",
                ExecutionDuration = Stopwatch.GetElapsedTime(timerStart)
            };
        }
        
        var testResults = runTests ? (runnerResponse.TestsOutputs ?? []).Select(test => new CodeTestResult
        {
            Name = test.Name,
            Passed = test.Passed,
            ExpectedResult = test.ExpectedResult is { } expectedOutput ? expectedOutput.Clone() : null,
            ActualOutput = 
                test.ActualOutput is { } actualResult
                    ? actualResult.Clone()
                    : null,
            Error = test.Error
        }).ToList() : new List<CodeTestResult>();

        return new CodeExecutionResult
        {
            DidComplete = runnerResponse.DidComplete,
            TimedOut = false,
            EveryTestPassed = runTests && runnerResponse.DidComplete && testResults.Count > 0 && testResults.All(test => test.Passed),
            ExitCode = dockerInstance.ExitCode,
            StandardOutput = TruncateString(runnerResponse.StandardOutput),
            StandardError = TruncateString(runnerResponse.StandardError),
            FatalError = runnerResponse.FatalError,
            CodeTests = testResults,
            ExecutionDuration = Stopwatch.GetElapsedTime(timerStart)
        };

    }
    finally
    {
        await RemoveContainer(containerName);

        try
        {
            Directory.Delete(tempDirectory, true);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, $"Could not delete temp directory {tempDirectory}");
        }
    }
    

}

private static ProcessStartInfo CreateDockerInstanceStartInfo(string containerName, string tempDirectory)
{
    var startInfo = new ProcessStartInfo()
    {
        FileName = "docker",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };
    
    AddArguments(
        startInfo,
        "run", "--rm", "--name",
        containerName,
        "--network", "none", "--memory",
        "128m", "--memory-swap", "128m",
        "--cpus", "0.5",
        "--pids-limit",
        "32", "--read-only",
        "--cap-drop",
        "ALL",
        "--security-opt",
        "no-new-privileges",
        "--init",
        "--tmpfs",
        "/tmp:rw,noexec,nosuid,size=16m",
        "--mount",
        $"type=bind," +
        $"source={Path.GetFullPath(tempDirectory)}," +
        "target=/workspace," +
        "readonly",
        DockerImage);

    return startInfo;
}

private static void AddArguments(ProcessStartInfo startInfo, params string[] arguments)
{
    foreach (var argument in arguments)
    {
        startInfo.ArgumentList.Add(argument);
    }
}

private static async Task RemoveContainer(string container)
{
    var startInfo = new ProcessStartInfo()
    {
        FileName = "docker",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };
    
    startInfo.ArgumentList.Add("rm");
    startInfo.ArgumentList.Add("-f");
    startInfo.ArgumentList.Add(container);

    try
    {
        using var cleanup =  Process.Start(startInfo);
        if (cleanup is null)
            return;
        
        var outputTask = cleanup.StandardOutput.ReadToEndAsync();
        var errorTask = cleanup.StandardError.ReadToEndAsync();
        
        await cleanup.WaitForExitAsync();
    }
    catch (Exception e)
    {
        // cleanup as the container could have been removed by --rm already
    }
    
}

private static string TruncateString(string? value)
{
    const int maxLength = 32_000; // TODO power of two should suffice for now.. 

    if (string.IsNullOrEmpty(value))
    {
        return string.Empty;
    }
    
    return value.Length <= maxLength ? value : value[..maxLength];
}


private static void AttemptKillProcess(Process process)
{
    try
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
        }
    }
    catch
    {
        // leave as empy!! as I use it for cleanup
    }
}
    

}