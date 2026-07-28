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
    int32 Id;
    
    UPROPERTY(BlueprintReadWrite)
    FString Title;
    
    UPROPERTY(BlueprintReadWrite)
    FString Description;
    
    UPROPERTY(BlueprintReadWrite)
    FString DefaultCode;
    
    UPROPERTY(BlueprintReadWrite)
    int32 Version;
    
    UPROPERTY(BlueprintReadWrite)
    int32 Difficulty;
    
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
    bool Passed;
    
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
    bool DidComplete;
    
    UPROPERTY(BlueprintReadWrite)
    bool TimedOut;
    
    UPROPERTY(BlueprintReadWrite)
    bool EveryTestPassed;

    UPROPERTY(BlueprintReadWrite)
    int32 ExitCode;
    
    UPROPERTY(BlueprintReadWrite)
    FString StandardOutput;
    
    UPROPERTY(BlueprintReadWrite)
    FString StandardError;

    UPROPERTY(BlueprintReadWrite)
    FString FatalError;

    UPROPERTY(BlueprintReadWrite)
    FTimespan ExecutionDuration;

    UPROPERTY(BlueprintReadWrite)
    TArray<FCodeTestResult> CodeTests;
};


USTRUCT(BlueprintType)
struct FCodeSubmission
{
    GENERATED_BODY()
    
    UPROPERTY(BlueprintReadWrite)
    int32 SubmissionId;
    
    UPROPERTY(BlueprintReadWrite)
    int32 AttemptNumber;
    
    UPROPERTY(BlueprintReadWrite)
    FCodeTask CodeTask;

    UPROPERTY(BlueprintReadWrite)
    FCodeExecutionResult CodeExecution;

    UPROPERTY(BlueprintReadWrite)
    FTimespan ExecutionDuration;

    UPROPERTY(BlueprintReadWrite)
    FString SubmittedCode;
    
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
    FTimespan ExecutionDuration;
};