// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

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
