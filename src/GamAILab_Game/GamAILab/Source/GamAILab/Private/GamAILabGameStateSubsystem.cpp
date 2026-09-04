    // Fill out your copyright notice in the Description page of Project Settings.


    #include "GamAILabGameStateSubsystem.h"

    #include "GamAILab.h"
    #include "GamAILabProgressionSubsystem.h"

    void UGamAILabGameStateSubsystem::LoadLearnerGameProgress()
    {
        UWorld* World = GetWorld();
        if (!World)
        {
            return;
        }
	    
        UGameInstance* GameInstance = GetGameInstance();
        if (!GameInstance)
        {
            return;
        }
	    
        UGamAILabProgressionSubsystem* ProgressionSubsystem = GameInstance->GetSubsystem<UGamAILabProgressionSubsystem>();
        if (!ProgressionSubsystem)
        {
            UE_LOG(LogGamAILab, Warning, TEXT("Failed to load game progress: ProgressionSubsystem is unavailable"));
            return;
        }
        
        ItemsToLoad = 3;
        bLoadFailed = false;

        ProgressionSubsystem->OnGameProgressLoaded.AddUniqueDynamic(this, &UGamAILabGameStateSubsystem::OnProgressLoaded);
        ProgressionSubsystem->OnGameObjectivesLoaded.AddUniqueDynamic(this, &UGamAILabGameStateSubsystem::OnObjectivesLoaded);
        ProgressionSubsystem->OnCustomDataListLoaded.AddUniqueDynamic(this, &UGamAILabGameStateSubsystem::OnCustomDataLoaded);
        
        // Call Http endpoints
        ProgressionSubsystem->LoadLearnerGameProgress();
        ProgressionSubsystem->LoadGameObjectives();
        ProgressionSubsystem->ListCustomData();
        
        // TODO Potentially get code task statuses from separate subsystem as well
    }

    void UGamAILabGameStateSubsystem::ClearLearnerGameProgress()
    {
        UWorld* World = GetWorld();
        if (!World)
        {
            return;
        }
        
        GameProgress = FGameProgress();
        CustomData.Empty();
        Objectives.Empty();
    }

    void UGamAILabGameStateSubsystem::VerifyLoadingProgress()
    {
        ItemsToLoad--;
        
        if (ItemsToLoad <= 0)
        {
            const bool bDidLoadSuccessfully = !bLoadFailed;
            
            UE_LOG(LogGamAILab, Log, TEXT("Learner progress loaded successfully"));
            
            OnLoadComplete.Broadcast(bDidLoadSuccessfully);
        }
    }

    void UGamAILabGameStateSubsystem::OnProgressLoaded(bool bSuccess, FGameProgress Progress, const FString& ErrorMessage)
    {
        if (UGameInstance* GameInstance = GetGameInstance())
        {
            if (auto* progressionSubsystem = GameInstance->GetSubsystem<UGamAILabProgressionSubsystem>())
            {
                progressionSubsystem->OnGameProgressLoaded.RemoveDynamic(this, &UGamAILabGameStateSubsystem::OnProgressLoaded);
            }
        }

        if (bSuccess)
        {
            GameProgress = Progress;
            UE_LOG(LogGamAILab, Log, TEXT("Progress loaded successfully"));
        }
        else
        {
            bLoadFailed = true;
            UE_LOG(LogGamAILab, Error, TEXT("Progress load failed: %s"), *ErrorMessage);
        }

        VerifyLoadingProgress();
    }

    void UGamAILabGameStateSubsystem::OnObjectivesLoaded(bool bSuccess, const TArray<FGameObjective>& InObjectives, const FString& ErrorMessage)
    {
        if (UGameInstance* GameInstance = GetGameInstance())
        {
            if (auto* progressionSubsystem = GameInstance->GetSubsystem<UGamAILabProgressionSubsystem>())
            {
                progressionSubsystem->OnGameObjectivesLoaded.RemoveDynamic(this, &UGamAILabGameStateSubsystem::OnObjectivesLoaded);
            }
        }

        if (bSuccess)
        {
            Objectives = InObjectives;
            UE_LOG(LogGamAILab, Log, TEXT("Successfully loaded objectives."));
        }
        else
        {
            bLoadFailed = true;
            UE_LOG(LogGamAILab, Error, TEXT("Failed loading objectives: %s"), *ErrorMessage);
        }

        VerifyLoadingProgress();
    }

    void UGamAILabGameStateSubsystem::OnCustomDataLoaded(bool bSuccess, const TArray<FCustomData>& InCustomData,
        const FString& ErrorMessage)
    {
        if (UGameInstance* GameInstance = GetGameInstance())
        {
            if (auto* progressionSubsystem = GameInstance->GetSubsystem<UGamAILabProgressionSubsystem>())
            {
                progressionSubsystem->OnCustomDataListLoaded.RemoveDynamic(this, &UGamAILabGameStateSubsystem::OnCustomDataLoaded);
            }
        }

        if (bSuccess)
        {
            CustomData = InCustomData;
            UE_LOG(LogGamAILab, Log, TEXT("Successfully loaded custom data list."));
        }
        else
        {
            bLoadFailed = true;
            UE_LOG(LogGamAILab, Error, TEXT("Failed loading custom data list: %s"), *ErrorMessage);
        }

        VerifyLoadingProgress();

    }
