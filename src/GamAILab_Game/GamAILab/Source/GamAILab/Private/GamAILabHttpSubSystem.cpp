// Fill out your copyright notice in the Description page of Project Settings.


//#include "GamAILabHttpSubSystem.h"
#include "Public/GamAILabHttpSubSystem.h"
#include "Dom/JsonObject.h"
#include "HttpModule.h"
#include "Components/SlateWrapperTypes.h"
#include "Interfaces/IHttpResponse.h"
#include "Misc/ConfigCacheIni.h"
#include "Serialization/JsonSerializer.h"
#include "Serialization/JsonWriter.h"


void UGamAILabHttpSubSystem::Initialize(FSubsystemCollectionBase& CollectionBase)
{
	Super::Initialize(CollectionBase);
	
	const bool bLoaded = GConfig->GetString(TEXT("GamAILab.Api"),TEXT("BaseUrl"),BaseUrl, GGameIni);
	
	BaseUrl = "http://localhost:5270";
	
	if (!bLoaded || BaseUrl.IsEmpty())
	{
		UE_LOG(LogTemp, Warning, TEXT("Base URL is missing: %s"), *BaseUrl);
		return;
	}
	
	// if (BaseUrl.EndsWith(TEXT("/")))
	// {
	// 	BaseUrl.LeftChopInline(1);
	// }
}

void UGamAILabHttpSubSystem::Login(const FString& Email, const FString& Password)
{
	if (Email.IsEmpty() || Password.IsEmpty())
	{
		OnLoginFinished.Broadcast(false, 0, TEXT("Email and password are missing"));
		return;
	}
	
	const TSharedRef<FJsonObject> Json = MakeShared<FJsonObject>();
	Json->SetStringField(TEXT("email"), Email);
	Json->SetStringField(TEXT("password"), Password);
	
	FString RequestJson;
	
	const TSharedRef< TJsonWriter<> > Writer = TJsonWriterFactory<>::Create(&RequestJson);
	
	if (!FJsonSerializer::Serialize(Json, Writer))
	{
		OnLoginFinished.Broadcast(false, 0, TEXT("Failed to create login request"));
		
		return;
	}
	
	const TSharedRef< IHttpRequest, ESPMode::ThreadSafe > HttpRequest = FHttpModule::Get().CreateRequest();
	HttpRequest->SetURL(BaseUrl + "/api/auth/login");
	
	HttpRequest->SetVerb("POST");
	
	HttpRequest->SetHeader("Content-Type", "application/json");
	HttpRequest->SetHeader("Accept", "application/json");
	
	HttpRequest->SetContentAsString(RequestJson);
	
	HttpRequest->OnProcessRequestComplete().BindUObject(this, &UGamAILabHttpSubSystem::HandleLogin);
	
	if (!HttpRequest->ProcessRequest())
	{
		OnLoginFinished.Broadcast(false, 0, TEXT("Login request failed"));
	}
}

void UGamAILabHttpSubSystem::Register(const FString& Email, const FString& Password, const FString& ConfirmPassword)
{
	if (Email.IsEmpty() || Password.IsEmpty() || ConfirmPassword.IsEmpty())
	{
		OnRegisterFinished.Broadcast(false, 0, TEXT("Registration fields are missing"));
		return;
	}
	
	if (Password != ConfirmPassword)
	{
		OnRegisterFinished.Broadcast(false, 0, TEXT("Passwords do not match"));
		return;
	}
	
	
	const TSharedRef<FJsonObject> Json = MakeShared<FJsonObject>();
	Json->SetStringField(TEXT("email"), Email);
	Json->SetStringField(TEXT("password"), Password);
	Json->SetStringField(TEXT("conFrimPassword"), ConfirmPassword); // I made a mistake with API spelling so I'll just use that for now
	
	FString RequestJson;
	
	const TSharedRef< TJsonWriter<> > Writer = TJsonWriterFactory<>::Create(&RequestJson);
	
	if (!FJsonSerializer::Serialize(Json, Writer))
	{
		OnRegisterFinished.Broadcast(false, 0, TEXT("Failed to create registration request"));
		return;
	}
	
	const TSharedRef< IHttpRequest, ESPMode::ThreadSafe > HttpRequest = FHttpModule::Get().CreateRequest();
	HttpRequest->SetURL(BaseUrl + "/api/auth/register");
	
	HttpRequest->SetVerb("POST");
	
	HttpRequest->SetHeader("Content-Type", "application/json");
	HttpRequest->SetHeader("Accept", "application/json");
	
	HttpRequest->SetContentAsString(RequestJson);
	
	HttpRequest->OnProcessRequestComplete().BindUObject(this, &UGamAILabHttpSubSystem::HandleRegister);
	
	if (!HttpRequest->ProcessRequest())
	{
		OnRegisterFinished.Broadcast(false, 0, TEXT("Register request failed"));
	}
}

void UGamAILabHttpSubSystem::HandleLogin(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bSuccess)
{
	if (!bSuccess || !Response.IsValid())
	{
		AccessToken.Empty();
		OnLoginFinished.Broadcast(false, 0, TEXT("Login failed"));
		return;
	}
	
	const int32 StatusCode = Response->GetResponseCode();
	const FString ResponseBody = Response->GetContentAsString();
	
	const bool bIsSuccess = StatusCode >= 200 && StatusCode < 300;
	
	if (!bIsSuccess)
	{
		AccessToken.Empty();
		
		OnLoginFinished.Broadcast(false, StatusCode, ResponseBody);
		
		return;
	}
	
	TSharedPtr<FJsonObject> Json = MakeShared<FJsonObject>();
	const TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(ResponseBody);
	if (!FJsonSerializer::Deserialize(Reader, Json) || !Json.IsValid())
	{
		AccessToken.Empty();
		
		OnLoginFinished.Broadcast(false, StatusCode, ResponseBody);
		return;
	}
	
	if (!Json->TryGetStringField(TEXT("accessToken"), AccessToken) || AccessToken.IsEmpty())
	{
		AccessToken.Empty();
		OnLoginFinished.Broadcast(false, StatusCode, ResponseBody);
		return;
	}
	
	OnLoginFinished.Broadcast(true, StatusCode, ResponseBody);
}

void UGamAILabHttpSubSystem::HandleRegister(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bSuccess)
{
	if (!bSuccess || !Response.IsValid())
	{
		OnRegisterFinished.Broadcast(false, 0, TEXT("Registration failed"));
		
		return;
	}
	
	const int32 StatusCode = Response->GetResponseCode();
	const FString ResponseBody = Response->GetContentAsString();
	
	const bool bIsSuccess = StatusCode >= 200 && StatusCode < 300;
	
	if (!bIsSuccess)
	{
		AccessToken.Empty();
		
		OnRegisterFinished.Broadcast(false, StatusCode, ResponseBody);
		
		return;
	}
	
	TSharedPtr<FJsonObject> Json = MakeShared<FJsonObject>();
	const TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(ResponseBody);
	if (!FJsonSerializer::Deserialize(Reader, Json) || !Json.IsValid())
	{
		AccessToken.Empty();
		
		OnRegisterFinished.Broadcast(false, StatusCode, ResponseBody);
		return;
	}
	
	if (!Json->TryGetStringField(TEXT("accessToken"), AccessToken) || AccessToken.IsEmpty())
	{
		AccessToken.Empty();
		OnRegisterFinished.Broadcast(false, StatusCode, ResponseBody);
		return;
	}
	
	OnRegisterFinished.Broadcast(true, StatusCode, ResponseBody);
}

