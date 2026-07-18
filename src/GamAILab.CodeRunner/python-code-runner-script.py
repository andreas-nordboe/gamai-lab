import contextlib
import json
import importlib.util
import io
import traceback

def load_learner_code():
    loc = importlib.util.spec_from_file_location("learner_code", "/workspace/code_submission.py")

    if loc is None or loc.loader is None:
        raise ImportError("Could not find learner code")
    
    module = importlib.util.module_from_spec(loc)
    loc.loader.exec_module(module)

    return module

def run_learner_code(learner_code, test):
    output = {
        "name": test["name"],
        "passed": False,
        "expectedResult": test["expectedResult"],
        "actualResult": None,
        "error": None
    }

    try:

        function = getattr(learner_code, test["functionName"])

        if not callable(function):
            raise TypeError(f"{test['functionName']} cannot be called")

        actual_result = function(*test["arguments"])

        output["actualResult"] = actual_result
        output["passed"] = actual_result == test["expectedResult"]

    except Exception as exception:
        output["error"] = str(exception)

    return output

def main():
    with open("/workspace/tests.json", encoding="utf-8") as test_file:
        tests = json.load(test_file)

        stdout = io.StringIO()
        stderr = io.StringIO()

        output = []
        fatal_error = None

        with contextlib.redirect_stdout(stdout), contextlib.redirect_stderr(stderr):
            try:
                learner_code = load_learner_code()

                for test in tests:
                    output.append(run_learner_code(learner_code, test))
            
            except BaseException as exception:
                # fatal_error = traceback.format_exc()
                fatal_error = f"{type(exception).__name__}: {exception}"

    print (json.dumps({
        "didComplete": fatal_error is None,
        "standardOutput": stdout.getvalue(),
        "standardError": stderr.getvalue(),
        "fatalError": fatal_error,
        "testOutputs": output
    }))

if __name__ == "__main__":
    main()