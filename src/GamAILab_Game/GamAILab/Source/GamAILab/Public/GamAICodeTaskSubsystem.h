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

DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(
	FOnCodeExecution,
	bool, bSuccess,
	const FCodeExecutionResponse&, Response
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
	
	
	UPROPERTY(BlueprintAssignable, Category = "GAMAILab|CodeTasks")
	FOnListCodetasks FOnListCodetasks;
	
	UFUNCTION(BlueprintCallable, Category = "GAMAILab|CodeTasks")
	void ListCodeTasks();

	UPROPERTY(BlueprintAssignable, Category = "GAMAILab|CodeTasks")
	FOnCodeExecution FOnCodeExecution;
	
	UFUNCTION(BlueprintCallable, Category = "GAMAILab|CodeExecution")
	void ExecuteCode(const FString& Code);

	//UFUNCTION(BlueprintCallable, Category = "GAMAILab|CodeExecution")
	//void SubmitCode(const FString& Code);
	
private:
	
	void HandleTasksReceived(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bSuccess);
	void HandleCodeExecution(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bSuccess);
	void HandleCodeSubmission(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bSuccess);
	
	FString BaseUrl;
	
};
