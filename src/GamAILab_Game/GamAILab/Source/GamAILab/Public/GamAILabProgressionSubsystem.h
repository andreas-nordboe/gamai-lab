// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "GamAILabTypes.h"
#include "Interfaces/IHttpRequest.h"
#include "GamAILabProgressionSubsystem.generated.h"


// Delegates

DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(
	FOnSaveComplete,
	bool, bSuccess,
	const FString&, ErrorMessage
);

DECLARE_DYNAMIC_MULTICAST_DELEGATE_ThreeParams(
	FOnGameProgressLoaded,
	bool, bSuccess,
	const TArray<FGameProgress>&, Progress,
	const FString&, ErrorMessage
);

DECLARE_DYNAMIC_MULTICAST_DELEGATE_ThreeParams(
	FOnCustomDataListLoaded,
	bool, bSuccess,
	const TArray<FCustomData>&, CustomData,
	const FString&, ErrorMessage
);

DECLARE_DYNAMIC_MULTICAST_DELEGATE_ThreeParams(
	FOnCustomDataLoaded,
	bool, bSuccess,
	const FCustomData&, CustomData,
	const FString&, ErrorMessage
);

DECLARE_DYNAMIC_MULTICAST_DELEGATE_ThreeParams(
	FOnGameObjectivesLoaded,
	bool, bSuccess,
	const TArray<FGameObjective>&, Objectives,
	const FString&, ErrorMessage
);

DECLARE_DYNAMIC_MULTICAST_DELEGATE_ThreeParams(
	FOnGameObjectiveLoaded,
	bool, bSuccess,
	const FGameObjective&, Objective,
	const FString&, ErrorMessage
);




/**
 * 
 */
UCLASS()
class GAMAILAB_API UGamAILabProgressionSubsystem : public UGameInstanceSubsystem
{
	GENERATED_BODY()
	
public:
	
	virtual void Initialize(FSubsystemCollectionBase& CollectionBase) override;
	
	// Auth
	// TODO these are duplicated across services for now, this will be cleaner and less code duplication when moved into a separate class
	
	UFUNCTION(BlueprintCallable, Category = "GamAILab|Authentication")
	void SetAccessToken(const FString& InAccessToken);

	UFUNCTION(BlueprintCallable, Category = "GamAILab|Authentication")
	void ClearAccessToken();

	
	// Game Progress
	
	UPROPERTY(BlueprintAssignable, Category = "GamAILab|Game Progress")
	FOnSaveComplete OnGameProgressSaved;

	UPROPERTY(BlueprintAssignable, Category = "GamAILab|Game Progress")
	FOnGameProgressLoaded OnGameProgressLoaded;

	UFUNCTION(BlueprintCallable, Category = "GamAILab|Game Progress")
	void SaveLearnerGameProgress(
		const FGameProgress& Progress
	);

	UFUNCTION(BlueprintCallable, Category = "GamAILab|Game Progress")
	void LoadLearnerGameProgress();
	
	// Custom data
	
	UPROPERTY(BlueprintAssignable, Category = "GamAILab|Custom Data")
	FOnCustomDataListLoaded OnCustomDataListLoaded;

	UPROPERTY(BlueprintAssignable, Category = "GamAILab|Custom Data")
	FOnCustomDataLoaded OnCustomDataLoaded;

	UPROPERTY(BlueprintAssignable, Category = "GamAILab|Custom Data")
	FOnSaveComplete OnCustomDataSaved;

	UFUNCTION(BlueprintCallable, Category = "GamAILab|Custom Data")
	void ListCustomData();

	UFUNCTION(BlueprintCallable, Category = "GamAILab|Custom Data")
	void GetCustomData(const FString& Key);

	UFUNCTION(BlueprintCallable, Category = "GamAILab|Custom Data")
	void SaveCustomData(const FCustomData& CustomData);
	
	// Objectives
	
	UPROPERTY(BlueprintAssignable, Category = "GamAILab|Objectives")
	FOnGameObjectivesLoaded OnGameObjectivesLoaded;

	UPROPERTY(BlueprintAssignable, Category = "GamAILab|Objectives")
	FOnGameObjectiveLoaded OnGameObjectiveLoaded;

	UPROPERTY(BlueprintAssignable, Category = "GamAILab|Objectives")
	FOnSaveComplete OnGameObjectiveSaved;

	UFUNCTION(BlueprintCallable, Category = "GamAILab|Objectives")
	void LoadGameObjectives();

	UFUNCTION(BlueprintCallable, Category = "GamAILab|Objectives")
	void GetGameObjective(const FString& ObjectiveId);

	UFUNCTION(BlueprintCallable, Category = "GamAILab|Objectives")
	void SaveGameObjective(const FGameObjective& Objective);

private:
	
	FString BaseUrl;
	FString AccessToken;

	void HandleGameProgressSaved(FHttpRequestPtr Request,FHttpResponsePtr Response,bool bSuccess);

	void HandleGameProgressLoaded(FHttpRequestPtr Request,FHttpResponsePtr Response,bool bSuccess);
	
	void HandleCustomDataListLoaded(FHttpRequestPtr Request,FHttpResponsePtr Response,bool bSuccess);

	void HandleCustomDataLoaded(FHttpRequestPtr Request,FHttpResponsePtr Response,bool bSuccess);

	void HandleCustomDataSaved(FHttpRequestPtr Request,FHttpResponsePtr Response,bool bSuccess);
	
	void HandleGameObjectivesLoaded(FHttpRequestPtr Request,FHttpResponsePtr Response,bool bSuccess);

	void HandleGameObjectiveLoaded(FHttpRequestPtr Request,FHttpResponsePtr Response,bool bSuccess);

	void HandleGameObjectiveSaved(FHttpRequestPtr Request,FHttpResponsePtr Response,bool bSuccess);

	// Create request for all endpoint calls
	TSharedRef<IHttpRequest, ESPMode::ThreadSafe> CreateRequest(const FString& Route,const FString& Verb) const;
	
	bool ResponseWasSuccessful(const FHttpResponsePtr& Response,const bool bWasSuccessful);
	FString GetResponseError(const FHttpResponsePtr& Response,const bool bWasSuccessful);
};
