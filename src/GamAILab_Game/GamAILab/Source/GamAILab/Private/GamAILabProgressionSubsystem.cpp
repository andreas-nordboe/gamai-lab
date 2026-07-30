// Fill out your copyright notice in the Description page of Project Settings.


#include "GamAILabProgressionSubsystem.h"

#include "HttpModule.h"
#include "JsonObjectConverter.h"
#include "GenericPlatform/GenericPlatformHttp.h"
#include "Interfaces/IHttpRequest.h"
#include "Interfaces/IHttpResponse.h"


void UGamAILabProgressionSubsystem::Initialize(FSubsystemCollectionBase& CollectionBase)
{
    UGameInstanceSubsystem::Initialize(CollectionBase);
    
    const bool bLoaded = GConfig->GetString(TEXT("GamAILab.Api"),TEXT("BaseUrl"),BaseUrl,GGameIni);

    if (!bLoaded || BaseUrl.IsEmpty())
    {
        UE_LOG(LogTemp, Warning, TEXT("Base URL is missing: %s"), *BaseUrl);

        BaseUrl = TEXT("http://localhost:5270");
    }
    
    BaseUrl.RemoveFromEnd(TEXT("/"));

    UE_LOG(LogTemp,Log,TEXT("Game progress subsystem uses API URL: %s"),*BaseUrl);

}

void UGamAILabProgressionSubsystem::SetAccessToken(const FString& InAccessToken)
{
    AccessToken = InAccessToken;
}

void UGamAILabProgressionSubsystem::ClearAccessToken()
{
    AccessToken.Empty();
}

void UGamAILabProgressionSubsystem::SaveLearnerGameProgress(const FGameProgress& Progress)
{
    FString RequestJson;

    
    if (!FJsonObjectConverter::UStructToJsonObjectString(Progress, RequestJson, 0, 0))
    {
        OnGameProgressSaved.Broadcast(false,TEXT("Could not serialise learner game progress"));
        return;
    }

    TSharedRef<IHttpRequest, ESPMode::ThreadSafe> HttpRequest =CreateRequest(TEXT("/api/game/progress"),TEXT("POST"));
    HttpRequest->SetContentAsString(RequestJson);

    HttpRequest->OnProcessRequestComplete().BindUObject(
        this,
        &UGamAILabProgressionSubsystem::HandleGameProgressSaved
    );

    if (!HttpRequest->ProcessRequest())
    {
        OnGameProgressSaved.Broadcast(false,TEXT("Failed to save game progress"));
    }
}

void UGamAILabProgressionSubsystem::LoadLearnerGameProgress()
{
    TSharedRef<IHttpRequest, ESPMode::ThreadSafe> HttpRequest =CreateRequest(TEXT("/api/game/game-progress"), TEXT("GET"));

    HttpRequest->OnProcessRequestComplete().BindUObject(this,&UGamAILabProgressionSubsystem::HandleGameProgressLoaded);

    if (!HttpRequest->ProcessRequest())
    {
        OnGameProgressLoaded.Broadcast(false,TArray<FGameProgress>(), TEXT("Failed to load game progress"));
    }
}

void UGamAILabProgressionSubsystem::ListCustomData()
{
    TSharedRef<IHttpRequest, ESPMode::ThreadSafe> HttpRequest = CreateRequest(TEXT("/api/game/custom-data"), TEXT("GET"));

    HttpRequest->OnProcessRequestComplete().BindUObject(this, &UGamAILabProgressionSubsystem::HandleCustomDataListLoaded);

    if (!HttpRequest->ProcessRequest())
    {
        OnCustomDataListLoaded.Broadcast(false,TArray<FCustomData>(), TEXT("Failed to list custom data"));
    }
}

void UGamAILabProgressionSubsystem::GetCustomData(const FString& Key)
{
    if (Key.IsEmpty())
    {
        OnCustomDataLoaded.Broadcast(false, FCustomData(), TEXT("Key for custom data is empty"));
        return;
    }

    const FString EncodedKey = FGenericPlatformHttp::UrlEncode(Key);

    TSharedRef<IHttpRequest, ESPMode::ThreadSafe> HttpRequest =
        CreateRequest(TEXT("/api/game/custom-data/") + EncodedKey, TEXT("GET"));

    HttpRequest->OnProcessRequestComplete().BindUObject(this, &UGamAILabProgressionSubsystem::HandleCustomDataLoaded);

    if (!HttpRequest->ProcessRequest())
    {
        OnCustomDataLoaded.Broadcast(false, FCustomData(), TEXT("Failed to get custom data for key"));
    }

}

void UGamAILabProgressionSubsystem::SaveCustomData(const FCustomData& CustomData)
{
    FString RequestJson;

    if (!FJsonObjectConverter::UStructToJsonObjectString(CustomData, RequestJson, 0, 0))
    {
        OnCustomDataSaved.Broadcast(false,TEXT("Failed to serialise custom data"));
        return;
    }

    TSharedRef<IHttpRequest, ESPMode::ThreadSafe> HttpRequest =
        CreateRequest(TEXT("/api/game/custom-data"),TEXT("POST"));

    HttpRequest->SetContentAsString(RequestJson);
    HttpRequest->OnProcessRequestComplete().BindUObject(this, &UGamAILabProgressionSubsystem::HandleCustomDataSaved);

    if (!HttpRequest->ProcessRequest())
    {
        OnCustomDataSaved.Broadcast(false,TEXT("Failed to save custom data"));
    }

}

void UGamAILabProgressionSubsystem::LoadGameObjectives()
{
    TSharedRef<IHttpRequest, ESPMode::ThreadSafe> HttpRequest = CreateRequest(TEXT("/api/game/objectives"), TEXT("GET"));

    HttpRequest->OnProcessRequestComplete().BindUObject(this, &UGamAILabProgressionSubsystem::HandleGameObjectivesLoaded);

    if (!HttpRequest->ProcessRequest())
    {
        OnGameObjectivesLoaded.Broadcast(false, TArray<FGameObjective>(),TEXT("Could not start the load objectives request."));
    }
}

void UGamAILabProgressionSubsystem::GetGameObjective(const FString& ObjectiveId)
{
    if (ObjectiveId.IsEmpty())
    {
        OnGameObjectiveLoaded.Broadcast(false,FGameObjective(), TEXT("Objective ID is empty."));
        return;
    }

    const FString EncodedObjectiveId = FGenericPlatformHttp::UrlEncode(ObjectiveId);

    TSharedRef<IHttpRequest, ESPMode::ThreadSafe> HttpRequest = CreateRequest(TEXT("/api/game/objectives/") + EncodedObjectiveId, TEXT("GET"));

    HttpRequest->OnProcessRequestComplete().BindUObject(this, &UGamAILabProgressionSubsystem::HandleGameObjectiveLoaded);

    if (!HttpRequest->ProcessRequest())
    {
        OnGameObjectiveLoaded.Broadcast(false, FGameObjective(), TEXT("Failed to get game objective"));
    }

}

void UGamAILabProgressionSubsystem::SaveGameObjective(const FGameObjective& Objective)
{
    FString RequestJson;

    if (!FJsonObjectConverter::UStructToJsonObjectString(Objective, RequestJson, 0, 0))
    {
        OnGameObjectiveSaved.Broadcast(false, TEXT("Could not serialize the game objective."));
        return;
    }

    TSharedRef<IHttpRequest, ESPMode::ThreadSafe> HttpRequest =
        CreateRequest(TEXT("/api/game/objectives"),TEXT("POST"));

    HttpRequest->SetContentAsString(RequestJson);
    HttpRequest->OnProcessRequestComplete().BindUObject(
        this,
        &UGamAILabProgressionSubsystem::HandleGameObjectiveSaved
    );

    if (!HttpRequest->ProcessRequest())
    {
        OnGameObjectiveSaved.Broadcast(false, TEXT("Failed to save game objective")
        );
    }

}

void UGamAILabProgressionSubsystem::HandleGameProgressSaved(FHttpRequestPtr Request, FHttpResponsePtr Response,bool bSuccess)
{
    if (!ResponseWasSuccessful(Response, bSuccess))
    {
        OnGameProgressSaved.Broadcast(false, UGamAILabProgressionSubsystem::GetResponseError(Response, bSuccess));
        
        return;
    }

    OnGameProgressSaved.Broadcast(true, FString());

}

void UGamAILabProgressionSubsystem::HandleGameProgressLoaded(FHttpRequestPtr Request, FHttpResponsePtr Response,bool bSuccess)
{
    if (!ResponseWasSuccessful(Response, bSuccess))
    {
        OnGameProgressLoaded.Broadcast(false,TArray<FGameProgress>(), GetResponseError(Response, bSuccess));
        return;
    }

    TArray<FGameProgress> Progress;
    FText ParseError;

    
    
    if (!FJsonObjectConverter::JsonArrayStringToUStruct<FGameProgress>(Response->GetContentAsString(), &Progress, 0, 0, false, &ParseError, nullptr))
    {
        OnGameProgressLoaded.Broadcast(
            false,
            TArray<FGameProgress>(),
            ParseError.ToString()
        );
        return;
    }

    OnGameProgressLoaded.Broadcast(
        true,
        Progress,
        FString()
    );
}

void UGamAILabProgressionSubsystem::HandleCustomDataListLoaded(FHttpRequestPtr Request, FHttpResponsePtr Response,bool bSuccess)
{
    if (!ResponseWasSuccessful(Response, bSuccess))
    {
        OnCustomDataListLoaded.Broadcast(false,TArray<FCustomData>(), UGamAILabProgressionSubsystem::GetResponseError(Response, bSuccess));
        return;
    }

    TArray<FCustomData> CustomData;
    FText ParseError;

    if (!FJsonObjectConverter::JsonArrayStringToUStruct<FCustomData>(Response->GetContentAsString(), &CustomData, 0, 0, false, &ParseError, nullptr))
    {
        OnCustomDataListLoaded.Broadcast(false,TArray<FCustomData>(),ParseError.ToString());
        return;
    }

    OnCustomDataListLoaded.Broadcast(true,CustomData,FString());

}

void UGamAILabProgressionSubsystem::HandleCustomDataLoaded(FHttpRequestPtr Request, FHttpResponsePtr Response,bool bSuccess)
{
    if (!ResponseWasSuccessful(Response, bSuccess))
    {
        OnCustomDataLoaded.Broadcast(false,FCustomData(), GetResponseError(Response, bSuccess));
        
        return;
    }

    FCustomData CustomData;
    FText ParseError;
    
    if (FJsonObjectConverter::JsonObjectStringToUStruct<FCustomData>(Response->GetContentAsString(), &CustomData,0, 0, false, &ParseError,nullptr))
    {
        OnCustomDataLoaded.Broadcast(false,FCustomData(),ParseError.ToString());
        return;
    }

    OnCustomDataLoaded.Broadcast(true,CustomData,FString());

}

void UGamAILabProgressionSubsystem::HandleCustomDataSaved(FHttpRequestPtr Request, FHttpResponsePtr Response,bool bSuccess)
{
    if (!ResponseWasSuccessful(Response, bSuccess))
    {
        OnCustomDataSaved.Broadcast(false,GetResponseError(Response, bSuccess));
        return;
    }

    OnCustomDataSaved.Broadcast(true, FString());
}

void UGamAILabProgressionSubsystem::HandleGameObjectivesLoaded(FHttpRequestPtr Request, FHttpResponsePtr Response,bool bSuccess)
{
    if (!ResponseWasSuccessful(Response, bSuccess))
    {
        OnGameObjectivesLoaded.Broadcast(false, TArray<FGameObjective>(), GetResponseError(Response, bSuccess));
        
        return;
    }

    TArray<FGameObjective> Objectives;
    FText ParseError;

    if (!FJsonObjectConverter::JsonArrayStringToUStruct<FGameObjective>(Response->GetContentAsString(), &Objectives, 0, 0, false, &ParseError, nullptr))
    {
        OnGameObjectivesLoaded.Broadcast(false,TArray<FGameObjective>(),ParseError.ToString());
        
        return;
    }

    OnGameObjectivesLoaded.Broadcast(true, Objectives, FString());

}

void UGamAILabProgressionSubsystem::HandleGameObjectiveLoaded(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bSuccess)
{
    if (!ResponseWasSuccessful(Response, bSuccess))
    {
        OnGameObjectiveLoaded.Broadcast(false,FGameObjective(), GetResponseError(Response, bSuccess));
        return;
    }

    FGameObjective Objective;
    FText ParseError;

    if (FJsonObjectConverter::JsonObjectStringToUStruct<FGameObjective>(Response->GetContentAsString(), &Objective,0, 0, false, &ParseError,nullptr))
    {
        OnGameObjectiveLoaded.Broadcast(false,FGameObjective(),ParseError.ToString());
        return;
    }

    OnGameObjectiveLoaded.Broadcast(true,Objective,FString());
    
}

void UGamAILabProgressionSubsystem::HandleGameObjectiveSaved(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bSuccess)
{
    if (!ResponseWasSuccessful(Response, bSuccess))
    {
        OnGameObjectiveSaved.Broadcast(false,GetResponseError(Response, bSuccess));
        
        return;
    }

    OnGameObjectiveSaved.Broadcast(true, FString());

}

TSharedRef<IHttpRequest, ESPMode::ThreadSafe> UGamAILabProgressionSubsystem::CreateRequest(const FString& Route, const FString& Verb) const
{
    TSharedRef<IHttpRequest, ESPMode::ThreadSafe> HttpRequest = FHttpModule::Get().CreateRequest();

    HttpRequest->SetURL(BaseUrl + Route);
    HttpRequest->SetVerb(Verb);
    HttpRequest->SetHeader(TEXT("Accept"),TEXT("application/json"));
    HttpRequest->SetHeader(TEXT("Content-Type"),TEXT("application/json"));

    if (!AccessToken.IsEmpty())
    {
        HttpRequest->SetHeader(
            TEXT("Authorization"),
            TEXT("Bearer ") + AccessToken
        );
    }

    return HttpRequest;

}

bool UGamAILabProgressionSubsystem::ResponseWasSuccessful(const FHttpResponsePtr& Response, const bool bWasSuccessful)
{
    if (!bWasSuccessful || !Response.IsValid())
    {
        return false;
    }

    const int32 StatusCode = Response->GetResponseCode();
    return StatusCode >= 200 && StatusCode < 300;
}

FString UGamAILabProgressionSubsystem::GetResponseError(const FHttpResponsePtr& Response, const bool bWasSuccessful)
{
    if (!bWasSuccessful)
    {
        return TEXT("HTTP response did not complete");
    }

    if (!Response.IsValid())
    {
        return TEXT("There was an invalid response from the server");
    }

    const int32 StatusCode = Response->GetResponseCode();
    const FString ResponseBody = Response->GetContentAsString();

    if (!ResponseBody.IsEmpty())
    {
        return FString::Printf(TEXT("HTTP %d: %s"),StatusCode,*ResponseBody);
    }

    return FString::Printf(TEXT("Server responded with status code: %d"),StatusCode);

}
