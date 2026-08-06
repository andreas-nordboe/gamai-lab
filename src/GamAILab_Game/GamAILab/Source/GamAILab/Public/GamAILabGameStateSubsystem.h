// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"

#include "GamAILabTypes.h"
#include "GamAILabGameStateSubsystem.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnGameStateLoadCompleted, bool, WasSuccessful);

/**
 * 
 */
UCLASS()
class GAMAILAB_API UGamAILabGameStateSubsystem : public UGameInstanceSubsystem
{
	GENERATED_BODY()
	
public:
	
	UPROPERTY(BlueprintAssignable, Category = "GamAILab|GameState")
	FOnGameStateLoadCompleted OnLoadComplete;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category = "GamAILab|GameState")
	FGameProgress GameProgress;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category = "GamAILab|GameState")
	TArray<FCustomData> CustomData;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category = "GamAILab|GameState")
	TArray<FGameObjective> Objectives;
	
	// Loads progress from API after login
	UFUNCTION(BlueprintCallable)
	void LoadLearnerGameProgress();
	
	UFUNCTION(BlueprintCallable)
	void ClearLearnerGameProgress();
	
private:
	int32 ItemsToLoad = 0;
	bool bLoadFailed = false; // in case one fails
	
	void VerifyLoadingProgress();
	
	UFUNCTION()
	void OnProgressLoaded(bool bSuccess, FGameProgress Progress, const FString& ErrorMessage);
	
	UFUNCTION()
	void OnObjectivesLoaded(bool bSuccess, const TArray<FGameObjective>& InObjectives, const FString& ErrorMessage);
	
	UFUNCTION()
	void OnCustomDataLoaded(bool bSuccess, const TArray<FCustomData>& InCustomData, const FString& ErrorMessage);
	
	
	
};
