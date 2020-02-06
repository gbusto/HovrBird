using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsCommon
{
    private const string eggHighlightKey = "EggHighlight";
    private const string howtoPopupKey = "HowtoPopup";
    private const string activeBirdKey = "ActiveBirdId";
    private const string firstTimeItemsKey = "FirstTimeItems";
    private const string howtoPlayPopupKey = "HowtoPlay";
    private const string completedLevel1Key = "Level1Complete";
    private const string uncommonFruitNewKey = "UncommonFruitNew";
    private const string rareFruitNewKey = "RareFruitNew";
    private const string collectedFirstEggKey = "CollectedFirstEgg";
    private const string howtoPlayClassicKey = "HowtoPlayClassic";
    private const string moreToComeKey = "MoreToCome";
    private const string userIsOfflineKey = "UserIsOffline";
    private const string reviewPrompt1Key = "ReviewPrompt1";

    /*
     * Functions to handle the egg highlight setting;
     * it should be set to some value other than 0 to disable it.
     */
    public static int GetEggHighlight()
    {
        return PlayerPrefs.GetInt(eggHighlightKey);
    }

    public static void SetEggHighlight(int value)
    {
        PlayerPrefs.SetInt(eggHighlightKey, value);
    }

    /*
     * Functions to handle the popup to show the user how adventure mode works;
     * it should be set to some value other than 0 to disable it.
     */
    public static int GetHowtoPopup()
    {
        if (false == PlayerPrefs.HasKey(howtoPopupKey))
        {
            // XXX UNCOMMENT THIS TO SHOW THESE OLD POPUPS
            //SetHowtoPopup(0);
            SetHowtoPopup(1);
        }

        return PlayerPrefs.GetInt(howtoPopupKey);
    }

    public static void SetHowtoPopup(int value)
    {
        PlayerPrefs.SetInt(howtoPopupKey, value);
    }

    /*
     * Functions to handle the active bird id in the game to know which bird
     * prefab to load.
     */
    public static void SetActiveBirdId(uint id)
    {
        PlayerPrefs.SetInt(activeBirdKey, (int)id);
    }

    public static uint GetActiveBirdId()
    {
        if (false == PlayerPrefs.HasKey(activeBirdKey))
        {
            SetActiveBirdId(InventoryData.KOKO_ID);
        }
        return (uint)PlayerPrefs.GetInt(activeBirdKey);
    }

    /*
     * Functions to handle showing a notification when user collects items
     * for the first time
     */
    public static void SetFirstItemsCollected(bool b)
    {
        if (b)
        {
            PlayerPrefs.SetInt(firstTimeItemsKey, 1);
        }
        else
        {
            PlayerPrefs.SetInt(firstTimeItemsKey, 0);
        }
    }

    // Return true if notification has been shown; false otherwise
    public static bool GetFirstItemsCollected()
    {
        if (false == PlayerPrefs.HasKey(firstTimeItemsKey))
        {
            SetFirstItemsCollected(false);
        }

        return 1 == PlayerPrefs.GetInt(firstTimeItemsKey);
    }

    /*
     * Functions to handle whether or not the "how to play adventure mode" popup
     * is shown.
     */
    public static void SetHowtoPlayPopup(bool b)
    {
        if (b)
        {
            PlayerPrefs.SetInt(howtoPlayPopupKey, 1);
        }
        else
        {
            PlayerPrefs.SetInt(howtoPlayPopupKey, 0);
        }
    }

    public static bool GetHowtoPlayPopup()
    {
        if (false == PlayerPrefs.HasKey(howtoPlayPopupKey))
        {
            SetHowtoPlayPopup(false);
        }

        return 1 == PlayerPrefs.GetInt(howtoPlayPopupKey);
    }

    /*
     * Functions to handle whether or not to congratulate the user when they
     * complete level 1
     */
    public static void SetCompletedLevel1(bool b)
    {
        if (b)
        {
            PlayerPrefs.SetInt(completedLevel1Key, 1);
        }
        else
        {
            PlayerPrefs.SetInt(completedLevel1Key, 0);
        }
    }

    public static bool GetCompletedLevel1()
    {
        if (false == PlayerPrefs.HasKey(completedLevel1Key))
        {
            SetCompletedLevel1(false);
        }

        return 1 == PlayerPrefs.GetInt(completedLevel1Key);
    }

    /*
     * Functions to handle whether or not the inform the user of blueberries
     * (uncommon fruits) being added to the game.
     */
    public static void SetUncommonFruitNew(bool b)
    {
        if (b)
        {
            PlayerPrefs.SetInt(uncommonFruitNewKey, 1);
        }
        else
        {
            PlayerPrefs.SetInt(uncommonFruitNewKey, 0);
        }
    }

    public static bool GetUncommonFruitNew()
    {
        if (false == PlayerPrefs.HasKey(uncommonFruitNewKey))
        {
            SetUncommonFruitNew(false);
        }

        return 1 == PlayerPrefs.GetInt(uncommonFruitNewKey);
    }

    /*
     * Functions to handle whether or not to inform the user of strawberries
     * (rare fruits) and eggs being added to the game.
     */
    public static void SetRareFruitNew(bool b)
    {
        if (b)
        {
            PlayerPrefs.SetInt(rareFruitNewKey, 1);
        }
        else
        {
            PlayerPrefs.SetInt(rareFruitNewKey, 0);
        }
    }

    public static bool GetRareFruitNew()
    {
        if (false == PlayerPrefs.HasKey(rareFruitNewKey))
        {
            SetUncommonFruitNew(false);
        }

        return 1 == PlayerPrefs.GetInt(rareFruitNewKey);
    }

    /*
     * Functions to handle whether or not to inform the user that they collected
     * their first egg and how the requirements work.
     */
    public static void SetCollectedFirstEgg(bool b)
    {

        if (b)
        {
            PlayerPrefs.SetInt(collectedFirstEggKey, 1);
        }
        else
        {
            PlayerPrefs.SetInt(collectedFirstEggKey, 0);
        }
    }

    public static bool GetCollectedFirstEgg()
    {
        if (false == PlayerPrefs.HasKey(collectedFirstEggKey))
        {
            SetCollectedFirstEgg(false);
        }

        return 1 == PlayerPrefs.GetInt(collectedFirstEggKey);
    }


    public static void SetHowtoPlayClassic(bool b)
    {
        if (b)
        {
            PlayerPrefs.SetInt(howtoPlayClassicKey, 1);
        }
        else
        {
            PlayerPrefs.SetInt(howtoPlayClassicKey, 0);
        }
    }

    public static bool GetHowtoPlayClassic()
    {
        if (false == PlayerPrefs.HasKey(howtoPlayClassicKey))
        {
            SetHowtoPlayClassic(false);
        }

        return 1 == PlayerPrefs.GetInt(howtoPlayClassicKey);
    }


    public static void SetMoreToCome(bool b)
    {
        if (b)
        {
            PlayerPrefs.SetInt(moreToComeKey, 1);
        }
        else
        {
            PlayerPrefs.SetInt(moreToComeKey, 0);
        }
    }

    public static bool GetMoreToCome()
    {
        if (false == PlayerPrefs.HasKey(moreToComeKey))
        {
            SetMoreToCome(false);
        }

        return 1 == PlayerPrefs.GetInt(moreToComeKey);
    }


    public static void SetUserIsOffline(bool b)
    {
        if (b)
        {
            PlayerPrefs.SetInt(userIsOfflineKey, 1);
        }
        else
        {
            PlayerPrefs.SetInt(userIsOfflineKey, 0);
        }
    }

    public static bool GetUserIsOffline()
    {
        if (false == PlayerPrefs.HasKey(userIsOfflineKey))
        {
            SetUserIsOffline(false);
        }

        return 1 == PlayerPrefs.GetInt(userIsOfflineKey);
    }


    public static void SetReviewPrompt1(bool b)
    {
        if (b)
        {
            PlayerPrefs.SetInt(reviewPrompt1Key, 1);
        }
        else
        {
            PlayerPrefs.SetInt(reviewPrompt1Key, 0);
        }
    }

    public static bool GetReviewPrompt1()
    {
        if (false == PlayerPrefs.HasKey(reviewPrompt1Key))
        {
            SetReviewPrompt1(false);
        }

        return 1 == PlayerPrefs.GetInt(reviewPrompt1Key);
    }

    /*
     * Delete all the preferences settings
     */
    public static void DeleteAllPreferences()
    {
        PlayerPrefs.DeleteAll();
    }
}
