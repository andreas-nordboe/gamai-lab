// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GamAILabTypes.h"
#include "Interfaces/IHttpRequest.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "GamAICodeTaskSubsystem.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(
	FOnListCodetasks,
	bool, bSuccess,
	const TArray<FCodeTask>&, Response
);

/**
 * 
 */
UCLASS()
class GAMAILAB_API UGamAICodeTaskSubsystem : public UGameInstanceSubsystem
{
	GENERATED_BODY()
	
public:
	
	
	virtual void Initialize(
		FSubsystemCollectionBase& CollectionBase
	) override;
	
	
	UPROPERTY(BlueprintAssignable, Category = "GAMAILab|Authentication")
	FOnListCodetasks FOnListCodetasks;
	
	UFUNCTION(BlueprintCallable, Category = "GAMAILab|Authentication")
	void ListCodeTasks();

	
private:
	
	void HandleTasksReceived(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bSuccess);
	
	FString BaseUrl;
	
};
