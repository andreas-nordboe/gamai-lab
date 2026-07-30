// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include <Windows.ApplicationModel.Activation.h>

#include "CoreMinimal.h"
#include "GamAILabTypes.generated.h"

UENUM(BlueprintType)
enum class EGamAILabPanel : uint8
{
    LearningProgress,
    CodeTasks,
    Achievements,
    Menu,
    CodePanel
};

USTRUCT(BlueprintType)
struct FCodeTask
{
    GENERATED_BODY()
    
    UPROPERTY(BlueprintReadWrite)
    int32 Id = 0;
    
    UPROPERTY(BlueprintReadWrite)
    FString Title;
    
    UPROPERTY(BlueprintReadWrite)
    FString Description;
    
    UPROPERTY(BlueprintReadWrite)
    FString DefaultCode;
    
    UPROPERTY(BlueprintReadWrite)
    int32 Version = 0;
    
    UPROPERTY(BlueprintReadWrite)
    int32 Difficulty = 0;
    
    UPROPERTY(BlueprintReadWrite)
    TArray<FString> Examples;
    
    UPROPERTY(BlueprintReadWrite)
    TArray<FString> Constraints;
    
};


USTRUCT(BlueprintType)
struct FCodeTestResult
{
    GENERATED_BODY()
    
    UPROPERTY(BlueprintReadWrite)
    FString Name;
    
    UPROPERTY(BlueprintReadWrite)
    bool Passed = false;
    
    UPROPERTY(BlueprintReadWrite)
    FString ExpectedResult;

    UPROPERTY(BlueprintReadWrite)
    FString ActualResult;
    
    UPROPERTY(BlueprintReadWrite)
    FString Error;
};


USTRUCT(BlueprintType)
struct FCodeExecutionResult
{
    GENERATED_BODY()
    
    UPROPERTY(BlueprintReadWrite)
    bool DidComplete = false;
    
    UPROPERTY(BlueprintReadWrite)
    bool TimedOut = false;
    
    UPROPERTY(BlueprintReadWrite)
    bool EveryTestPassed = false;

    UPROPERTY(BlueprintReadWrite)
    int32 ExitCode = 0;
    
    UPROPERTY(BlueprintReadWrite)
    FString StandardOutput;
    
    UPROPERTY(BlueprintReadWrite)
    FString StandardError;

    UPROPERTY(BlueprintReadWrite)
    FString FatalError;

    UPROPERTY(BlueprintReadWrite)
    FString ExecutionDuration; // = FTimespan::Zero();

    UPROPERTY(BlueprintReadWrite)
    TArray<FCodeTestResult> CodeTests;
};

USTRUCT(BlueprintType)
struct FCodeAIFeedback
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadWrite)
    int32 Id = 0;

    UPROPERTY(BlueprintReadWrite)
    FString TaskOutcome;

    UPROPERTY(BlueprintReadWrite)
    FString HintMessage;

    // Holds JSON inside a JSON string
    UPROPERTY(BlueprintReadWrite)
    FString CodeTaskExecutionEvidence;

    UPROPERTY(BlueprintReadWrite)
    FString LlmModelUsed;

    UPROPERTY(BlueprintReadWrite)
    FString Explanation;

    UPROPERTY(BlueprintReadWrite)
    FString CreatedAt;

    UPROPERTY(BlueprintReadWrite)
    int32 GeneationTimeInMs = 0;
};


USTRUCT(BlueprintType)
struct FCodeSubmission
{
    GENERATED_BODY()
    
    UPROPERTY(BlueprintReadWrite)
    int32 SubmissionId = 0;
    
    UPROPERTY(BlueprintReadWrite)
    int32 AttemptNumber = 0;
    
    UPROPERTY(BlueprintReadWrite)
    FCodeTask CodeTask;

    UPROPERTY(BlueprintReadWrite)
    FCodeExecutionResult CodeExecution;

    UPROPERTY(BlueprintReadWrite)
    FString ExecutionDuration; // = FTimespan::Zero();

    UPROPERTY(BlueprintReadWrite)
    FString SubmittedCode;

    UPROPERTY(BlueprintReadWrite)
    FCodeAIFeedback AIFeedback;
};


USTRUCT(BlueprintType)
struct FCodeExecutionResponse
{
    GENERATED_BODY()
    
    UPROPERTY(BlueprintReadWrite)
    FString CodeOutput;
    
    UPROPERTY(BlueprintReadWrite)
    FString CodeError;
    
    UPROPERTY(BlueprintReadWrite)
    bool DidComplete;

    UPROPERTY(BlueprintReadWrite)
    bool TimedOut;

    UPROPERTY(BlueprintReadWrite)
    FString ExecutionDuration; // = FTimespan::Zero();
};

USTRUCT(BlueprintType)
struct FAchievement
{
    GENERATED_BODY()
    
    UPROPERTY(BlueprintReadWrite)
    FString AchievementId;
    
    UPROPERTY(BlueprintReadWrite)
    FString Title;
    
    UPROPERTY(BlueprintReadWrite)
    FString Description;
};

USTRUCT(BlueprintType)
struct FCustomData
{
    GENERATED_BODY()
    
    UPROPERTY(BlueprintReadWrite)
    FString Key;
    
    UPROPERTY(BlueprintReadWrite)
    FString Value;
};

USTRUCT(BlueprintType)
struct FGameObjective
{
    GENERATED_BODY()
    
    UPROPERTY(BlueprintReadWrite)
    FString ObjectiveId;
    
    UPROPERTY(BlueprintReadWrite)
    FString Title;
    
    UPROPERTY(BlueprintReadWrite)
    FString Description;
    
    UPROPERTY(BlueprintReadWrite)
    bool bIsCompleted;
    
    UPROPERTY(BlueprintReadWrite)
    int32 TargetValue;
    
    UPROPERTY(BlueprintReadWrite)
    int32 CurrentValue;
};

USTRUCT(BlueprintType)
struct FGameProgress
{
    GENERATED_BODY()
    
    UPROPERTY(BlueprintReadWrite)
    int32 Level;
    
    UPROPERTY(BlueprintReadWrite)
    int32 Currency;
    
    UPROPERTY(BlueprintReadWrite)
    TArray<FAchievement> Achievements;
};

USTRUCT(BlueprintType)
struct FNPCData
{
    GENERATED_BODY()
    
    UPROPERTY(BlueprintReadWrite)
    FString Name;
    
    UPROPERTY(BlueprintReadWrite)
    TArray<FGameObjective> StartsObjectives;
    
    UPROPERTY(BlueprintReadWrite)
    TArray<FGameObjective> FinishesObjectives;
};