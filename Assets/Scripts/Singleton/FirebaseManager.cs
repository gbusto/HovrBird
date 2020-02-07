using Firebase.Analytics;
using UnityEngine;

public class FirebaseManager
{
    public static bool firebaseReady;
    public static bool shouldLogEvents;

    public const string CURRENCY_SPEND_HATCH_EGG = "hatch_egg";
    public const string CURRENCY_SPEND_RESCUE_USER = "rescue_user";

    public const string CURRENCY_NAME_COINS = "coins";
    public const string CURRENCY_NAME_BANANAS = "bananas";
    public const string CURRENCY_NAME_BLUEBERRIES = "blueberries";
    public const string CURRENCY_NAME_STRAWBERRIES = "strawberries";

    public const string CHARACTER_NAME_KOKO = "koko";
    public const string CHARACTER_NAME_SAM = "sam";
    public const string CHARACTER_NAME_NIGEL = "nigel";
    public const string CHARACTER_NAME_STEVEN = "steven";

    public const string ACHIEVEMENT_ID_HATCH_SAM = "hatched_sam";
    public const string ACHIEVEMENT_ID_HATCH_NIGEL = "hatched_nigel";
    public const string ACHIEVEMENT_ID_HATCH_STEVEN = "hatched_steven";
    /* Examples of other achievements:
     * - Score milestones in classic (10, 50, 100, 200, 300, 400, 500, 1000, etc)
     * - Hatching other birds
    public const string ACHIEVEMENT_ID_CLASSIC_100 = "classic_score_100";
    public const string ACHIEVEMENT_ID_CLASSIC_1000 = "classic_score_1000";
    */

    public static void InitializeFirebase()
    {
        shouldLogEvents = true;

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your Firebase App,
                // where app is a Firebase.FirebaseApp property of your application class.
                //      app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                firebaseReady = true;
                Debug.Log("Firebase initialized successfully!");
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}",
                    dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    public static void LogBeginCheckoutEvent()
    {
        if (false == shouldLogEvents)
        {
            return;
        }

    }

    public static void LogShareEvent()
    {
        if (false == shouldLogEvents)
        {
            return;
        }

    }

    public static void LogTutorialBeginEvent()
    {
        if (false == shouldLogEvents)
        {
            return;
        }

        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventTutorialBegin);
    }

    public static void LogTutorialCompleteEvent()
    {
        if (false == shouldLogEvents)
        {
            return;
        }

        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventTutorialComplete);
    }

    public static void LogSpendVirtualCurrencyEvent(string itemName, string currencyName, long value)
    {
        if (false == shouldLogEvents)
        {
            return;
        }

        Parameter[] parameters =
        {
            new Parameter(FirebaseAnalytics.ParameterItemName, itemName),
            new Parameter(FirebaseAnalytics.ParameterVirtualCurrencyName, currencyName),
            new Parameter(FirebaseAnalytics.ParameterValue, value)
        };

        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventSpendVirtualCurrency, parameters);
    }

    public static void LogEarnVirtualCurrencyEvent(string currencyName, long value)
    {
        if (false == shouldLogEvents)
        {
            return;
        }

        Parameter[] parameters =
        {
            new Parameter(FirebaseAnalytics.ParameterVirtualCurrencyName, currencyName),
            new Parameter(FirebaseAnalytics.ParameterValue, value)
        };

        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventEarnVirtualCurrency, parameters);
    }

    public static void LogPostScoreEvent(long score, long levelNumber, uint birdId)
    {
        if (false == shouldLogEvents)
        {
            return;
        }

        string character;
        switch (birdId)
        {
            case InventoryData.KOKO_ID:
                character = CHARACTER_NAME_KOKO;
                break;

            case InventoryData.SAM_ID:
                character = CHARACTER_NAME_SAM;
                break;

            default:
                character = CHARACTER_NAME_KOKO;
                break;
        }

        Parameter[] parameters =
        {
            new Parameter(FirebaseAnalytics.ParameterScore, score),
            new Parameter(FirebaseAnalytics.ParameterLevel, levelNumber),
            new Parameter(FirebaseAnalytics.ParameterCharacter, character)
        };

        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPostScore, parameters);
    }

    public static void LogUnlockAchievementEvent(string achievement)
    {
        if (false == shouldLogEvents)
        {
            return;
        }

        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventUnlockAchievement,
            FirebaseAnalytics.ParameterAchievementId, achievement);
    }

    public static void LogLevelStartEvent(long levelNumber)
    {
        if (false == shouldLogEvents)
        {
            return;
        }

        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelStart,
            FirebaseAnalytics.ParameterLevel, levelNumber);
    }

    public static void LogLevelEndEvent(long levelNumber, long success)
    {
        if (false == shouldLogEvents)
        {
            return;
        }

        Parameter[] parameters =
        {
            new Parameter(FirebaseAnalytics.ParameterLevel, levelNumber),
            new Parameter(FirebaseAnalytics.ParameterSuccess, success)
        };

        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelEnd, parameters);
    }
}
