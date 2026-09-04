// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GamAILabTypes.h"
#include "Interfaces/IHttpRequest.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "GamAICodeTaskSubsystem.generated.h"

class UGamAILabAuthenticationSystem;

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

DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(
	FOnCodeSubmission,
	bool, bSuccess,
	const FCodeSubmission&, Response
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

	// List code tasks
	
	UPROPERTY(BlueprintAssignable, Category = "GAMAILab|CodeTasks")
	FOnListCodetasks FOnListCodetasks;
	
	UFUNCTION(BlueprintCallable, Category = "GAMAILab|CodeTasks")
	void ListCodeTasks();

	// Execute code
	
	UPROPERTY(BlueprintAssignable, Category = "GAMAILab|CodeTasks")
	FOnCodeExecution FOnCodeExecution;
	
	UFUNCTION(BlueprintCallable, Category = "GAMAILab|CodeExecution")
	void ExecuteCode(const FString& Code);

	// Submit Code
	
	UPROPERTY(BlueprintAssignable, Category = "GAMAILab|CodeSubmission")
	FOnCodeSubmission FOnCodeSubmission;
	
	UFUNCTION(BlueprintCallable, Category = "GAMAILab|CodeExecution")
	void SubmitCode(const int32 codeTaskId, const FString& Code);
	
private:
	
	void HandleTasksReceived(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bSuccess);
	void HandleCodeExecution(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bSuccess);
	void HandleCodeSubmission(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bSuccess);
	
	FString BaseUrl;
	
	FString GetAccessToken();
	
};
