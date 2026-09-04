// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Interfaces/IHttpRequest.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "GamAILabAuthenticationSubSystem.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE_ThreeParams(
	FAPIRequestFinished,
	bool, bSuccess,
	int32, StatusCode,
	const FString&, Response
);

/**
 * 
 */
UCLASS()
class GAMAILAB_API UGamAILabAuthenticationSystem : public UGameInstanceSubsystem
{
	GENERATED_BODY()
	
public:
	
	virtual void Initialize(
		FSubsystemCollectionBase& CollectionBase
	) override;
	
	UPROPERTY(BlueprintAssignable, Category = "GAMAILab|Authentication")
	FAPIRequestFinished OnLoginFinished;
	
	UPROPERTY(BlueprintAssignable, Category = "GAMAILab|Authentication")
	FAPIRequestFinished OnRegisterFinished;
	
	UFUNCTION(BlueprintCallable, Category = "GAMAILab|Authentication")
	void Login(const FString& Email, const FString& Password);
	
	UFUNCTION(BlueprintCallable, Category = "GAMAILab|Authentication")
	void Register(const FString& Email, const FString& Password, const FString& ConfirmPassword);
	
	UFUNCTION(BlueprintPure, Category = "GAMAILab|Authentication")
	FString GetAccessToken()
	{
		return AccessToken;	
	};
	
	UFUNCTION(BlueprintCallable, Category = "GAMAILab|Authentication")
	bool IsAuthenticated() const
	{
		return !AccessToken.IsEmpty();
	}
	
	UFUNCTION(BlueprintCallable, Category = "GAMAILab|Authentication")
	void Logout()
	{
		AccessToken.Empty();
	}
	
	
	
private:
	
	FString BaseUrl;
	
	UPROPERTY()
	FString AccessToken;
	
	void HandleLogin(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bSuccess);
	
	void HandleRegister(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bSuccess);
	
	
};
