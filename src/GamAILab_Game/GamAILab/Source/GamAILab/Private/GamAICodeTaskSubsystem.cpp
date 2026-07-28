// Fill out your copyright notice in the Description page of Project Settings.


#include "GamAICodeTaskSubsystem.h"

#include "GamAILab.h"
#include "GamAILabTypes.h"
#include "HttpModule.h"
#include "Interfaces/IHttpResponse.h"
#include "Misc/ConfigCacheIni.h"
#include "Serialization/JsonReader.h"
#include "Serialization/JsonSerializer.h"

class FJsonObject;
class IHttpRequest;

void UGamAICodeTaskSubsystem::Initialize(FSubsystemCollectionBase& CollectionBase)
{
	Super::Initialize(CollectionBase);

	// TODO move this to a separate service to avoid duplication
	
	const bool bLoaded = GConfig->GetString(TEXT("GamAILab.Api"),TEXT("BaseUrl"),BaseUrl, GGameIni);
	BaseUrl = "http://localhost:5270";
	
	if (!bLoaded || BaseUrl.IsEmpty())
	{
		UE_LOG(LogTemp, Warning, TEXT("Base URL is missing: %s"), *BaseUrl);
	}
}

void UGamAICodeTaskSubsystem::ListCodeTasks()
{
	TSharedRef<IHttpRequest, ESPMode::ThreadSafe> HttpRequest = FHttpModule::Get().CreateRequest();

	HttpRequest->SetURL(BaseUrl + "/api/code-tasks/tasks");
	
	HttpRequest->SetVerb("GET");
	
	HttpRequest->SetHeader("Content-Type", "application/json");
	HttpRequest->SetHeader("Accept", "application/json");
	
	// TODO Add JWT as part of request payload
	
	HttpRequest->OnProcessRequestComplete().BindUObject(this, &UGamAICodeTaskSubsystem::HandleTasksReceived);
	
	if (!HttpRequest->ProcessRequest())
	{
		FOnListCodetasks.Broadcast(false, TArray<FCodeTask>());
	}

	
}

void UGamAICodeTaskSubsystem::ExecuteCode(const FString& Code)
{
	if (Code.IsEmpty())
	{
		FOnCodeExecution.Broadcast(false, FCodeExecutionResponse());
		return;
	}
	
	
	const TSharedRef<FJsonObject> Json = MakeShared<FJsonObject>();
	Json->SetStringField(TEXT("code"), Code);
	
	FString RequestJson;
	
	const TSharedRef< TJsonWriter<> > Writer = TJsonWriterFactory<>::Create(&RequestJson);
	
	if (!FJsonSerializer::Serialize(Json, Writer))
	{
		FOnCodeExecution.Broadcast(false, FCodeExecutionResponse());
		return;
	}
	
	TSharedRef<IHttpRequest, ESPMode::ThreadSafe> HttpRequest = FHttpModule::Get().CreateRequest();

	HttpRequest->SetURL(BaseUrl + "/api/code-execution/execute");
	HttpRequest->SetVerb("POST");
	HttpRequest->SetHeader("Content-Type", "application/json");
	HttpRequest->SetHeader("Accept", "application/json");
	
	// TODO Add JWT as part of request payload

	HttpRequest->SetContentAsString(RequestJson);
	
	HttpRequest->OnProcessRequestComplete().BindUObject(this, &UGamAICodeTaskSubsystem::HandleCodeExecution);
	
	if (!HttpRequest->ProcessRequest())
	{
		FOnCodeExecution.Broadcast(false, FCodeExecutionResponse());
	}
}

void UGamAICodeTaskSubsystem::HandleTasksReceived(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bSuccess)
{
	if (!bSuccess || !Response.IsValid())
	{
		FOnListCodetasks.Broadcast(false, TArray<FCodeTask>());
		return;
	}
	
	const int32 StatusCode = Response->GetResponseCode();
	const FString ResponseBody = Response->GetContentAsString();
	
	const bool bIsSuccess = StatusCode >= 200 && StatusCode < 300;
	
	if (!bIsSuccess)
	{
		FOnListCodetasks.Broadcast(false, TArray<FCodeTask>());
		
		return;
	}
	
	FString ResponseString = Response->GetContentAsString();
	TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(ResponseString);
	TArray<TSharedPtr<FJsonValue>> JsonArray;

	if (FJsonSerializer::Deserialize(Reader, JsonArray))
	{
		TArray<FCodeTask> ParsedTasks;

		for (const TSharedPtr<FJsonValue>& Value : JsonArray)
		{
			TSharedPtr<FJsonObject> JsonObject = Value->AsObject();
			if (!JsonObject.IsValid()) continue;

			FCodeTask Task;
			Task.Id = JsonObject->GetIntegerField(TEXT("id"));
			Task.Title = JsonObject->GetStringField(TEXT("title"));
			Task.Description = JsonObject->GetStringField(TEXT("description"));
			Task.DefaultCode = JsonObject->GetStringField(TEXT("defaultCode"));
			Task.Version = JsonObject->GetIntegerField(TEXT("version"));
			Task.Difficulty = JsonObject->GetIntegerField(TEXT("difficulty"));
			JsonObject->TryGetStringArrayField(TEXT("examples"), Task.Examples);
			JsonObject->TryGetStringArrayField(TEXT("constraints"), Task.Constraints);

			ParsedTasks.Add(Task);
		}
		
		FOnListCodetasks.Broadcast(true, ParsedTasks);
	}
}

void UGamAICodeTaskSubsystem::HandleCodeExecution(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bSuccess)
{
	if (!bSuccess || !Response.IsValid())
	{
		FOnCodeExecution.Broadcast(false, FCodeExecutionResponse());
		return;
	}
	
	const int32 StatusCode = Response->GetResponseCode();
	const FString ResponseBody = Response->GetContentAsString();
	
	const bool bIsSuccess = StatusCode >= 200 && StatusCode < 300;
	
	if (!bIsSuccess)
	{
		FOnCodeExecution.Broadcast(false, FCodeExecutionResponse());
		
		return;
	}
	
	FString ResponseString = Response->GetContentAsString();
	TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(ResponseString);
	TSharedPtr<FJsonObject> Json = MakeShared<FJsonObject>();

	UE_LOG(LogTemp, Warning, TEXT("Code Execution Response %s"), *ResponseString);
	
	if (FJsonSerializer::Deserialize(Reader, Json))
	{
		FCodeExecutionResponse CodeExecution;
		
		Json->TryGetStringField(TEXT("codeOutput"), CodeExecution.CodeOutput);
		Json->TryGetStringField(TEXT("codeError"), CodeExecution.CodeError);
		Json->TryGetBoolField(TEXT("didComplete"), CodeExecution.DidComplete);
		Json->TryGetBoolField(TEXT("timedOut"), CodeExecution.TimedOut);
		//Json->TryGetStringField(TEXT("executionDuration"), CodeExecution.ExecutionDuration);
		
		FOnCodeExecution.Broadcast(true, CodeExecution);
	}
}

void UGamAICodeTaskSubsystem::HandleCodeSubmission(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bSuccess)
{
	
}
