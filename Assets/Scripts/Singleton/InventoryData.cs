using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public struct InventoryDataStruct
{
    // { "coin": coinCount, "banana": bananaCount, ... }
    public Dictionary<string, int> collectibleDict;
    // { id: true (hatched), id: false (unhatched) }
    public Dictionary<uint, bool> birdDict; // BirdStruct is defined in BirdManager.cs
    // Counter for the number of lives the user has
    public int livesCount;
    public DateTime dateToClaimLives;
}

public class InventoryData
{
    // { "coin": coinCount, "banana": bananaCount, ... }
    public Dictionary<string, int> collectibleDict;
    // { id: true (hatched), id: false (unhatched) }
    public Dictionary<uint, bool> birdDict; // BirdStruct is defined in BirdManager.cs
    public int livesCount;
    private DateTime dateToClaimLives;

    private static readonly string dataFilename = "po3th6hafn95.hb";

    private static InventoryData _instance;

    public static readonly string coinKey = "coins";
    public static readonly string bananaKey = "bananas";
    public static readonly string blueberryKey = "blueberries";
    public static readonly string strawberryKey = "strawberries";

    public const uint KOKO_ID = 0x1;
    public const uint SAM_ID = 0x8c771755; // Toucan Sam
    public const uint NIGEL_ID = 0x1eab5960; // Nigel the Pelican
    public const uint STEVEN_ID = 0xbd894384; // Steven Seagel (seagull)
    public const uint ANGRY_KOKO_ID = 0x43b4bea1;
    public const uint ANGRY_SAM_ID = 0xc63de70e;
    public const uint ANGRY_NIGEL_ID = 0x7e379d99;
    public const uint ANGRY_STEVEN_ID = 0x5de79f9c;

    public static Dictionary<string, int> angryKokoRequirements = new Dictionary<string, int>
    {
        // Coins used to be a requirement for hatching a bird, but that was ruled out
        // to save coins for buying lives
        [bananaKey] = 45,
        [blueberryKey] = 18,
        [strawberryKey] = 1
    };

    public static Dictionary<string, int> samRequirements = new Dictionary<string, int>
    {
        // Coins used to be a requirement for hatching a bird, but that was ruled out
        // to save coins for buying lives
        [bananaKey] = 50,
        [blueberryKey] = 20,
        [strawberryKey] = 1
    };

    public static Dictionary<string, int> angrySamRequirements = new Dictionary<string, int>
    {
        // Coins used to be a requirement for hatching a bird, but that was ruled out
        // to save coins for buying lives
        [bananaKey] = 50,
        [blueberryKey] = 20,
        [strawberryKey] = 1
    };

    public static Dictionary<string, int> nigelRequirements = new Dictionary<string, int>
    {
        [bananaKey] = 60,
        [blueberryKey] = 24,
        [strawberryKey] = 2
    };

    public static Dictionary<string, int> angryNigelRequirements = new Dictionary<string, int>
    {
        [bananaKey] = 60,
        [blueberryKey] = 24,
        [strawberryKey] = 2
    };

    public static Dictionary<string, int> stevenRequirements = new Dictionary<string, int>
    {
        [bananaKey] = 70,
        [blueberryKey] = 28,
        [strawberryKey] = 2
    };

    public static Dictionary<string, int> angryStevenRequirements = new Dictionary<string, int>
    {
        [bananaKey] = 70,
        [blueberryKey] = 28,
        [strawberryKey] = 2
    };

    public static uint[] BIRDS =
    {
        KOKO_ID,
        ANGRY_KOKO_ID,
        SAM_ID,
        ANGRY_SAM_ID,
        NIGEL_ID,
        ANGRY_NIGEL_ID,
        STEVEN_ID,
        ANGRY_STEVEN_ID,
    };

    public const bool UNHATCHED = false;
    public const bool HATCHED = true;

    private InventoryDataStruct inventoryData;

    private NotificationManager notificationMgr;


    private static byte[] key = {
        0x9c, 0xdb, 0xa3, 0xcb, 0xa9, 0x0b, 0xf1, 0x04, 0xc3, 0xb8, 0x79, 0x85, 0xb8, 0x22, 0xf8, 0x57
    };

    private static byte[] iv =
    {
        0x99, 0x6d, 0xb0, 0x25, 0xb4, 0xd3, 0xd0, 0x49, 0xe1, 0x8b, 0x73, 0x0a, 0x72, 0x05, 0x7b, 0xb4
    };


    private static int CLAIM_LIVES_AMOUNT = 3;
    private static int STARTING_LIFE_COUNT = 5;



    // Protected initializer; call Instance() method to initialize and load class
    protected InventoryData()
    {
        collectibleDict = new Dictionary<string, int>
        {
            [coinKey] = 0,
            [bananaKey] = 0,
            [blueberryKey] = 0,
            [strawberryKey] = 0
        };

        birdDict = new Dictionary<uint, bool>
        {
            [KOKO_ID] = HATCHED
        };

        livesCount = STARTING_LIFE_COUNT;
        dateToClaimLives = DateTime.Now;
    }

    protected InventoryData(InventoryDataStruct data)
    {
        collectibleDict = data.collectibleDict;
        birdDict = data.birdDict;
        livesCount = data.livesCount;
        dateToClaimLives = data.dateToClaimLives;
    }

    public static InventoryData Instance()
    {
        if (null == _instance)
        {
            _instance = LoadUserData();
            _instance.notificationMgr = NotificationManager.Instance();
        }

        return _instance;
    }

    public void SaveUserData()
    {
        inventoryData.collectibleDict = collectibleDict;
        inventoryData.birdDict = birdDict;
        inventoryData.livesCount = livesCount;
        inventoryData.dateToClaimLives = dateToClaimLives;

        string fullPath = GetFullPath();
        IFormatter formatter = new BinaryFormatter();

        using (MemoryStream memStream = new MemoryStream())
        {
            formatter.Serialize(memStream, inventoryData);
            memStream.Seek(0, SeekOrigin.Begin);
            byte[] serializedData = memStream.ToArray();
            EncryptedDataManager.WriteData(fullPath, key, iv, serializedData);
        }
    }

    public static Dictionary<string, int> GetRequirementForId(uint id)
    {
        switch (id)
        {
            case InventoryData.SAM_ID:
                return InventoryData.samRequirements;

            case InventoryData.NIGEL_ID:
                return InventoryData.nigelRequirements;

            case InventoryData.STEVEN_ID:
                return InventoryData.stevenRequirements;

            case InventoryData.ANGRY_KOKO_ID:
                return InventoryData.angryKokoRequirements;

            case InventoryData.ANGRY_SAM_ID:
                return InventoryData.angrySamRequirements;

            case InventoryData.ANGRY_NIGEL_ID:
                return InventoryData.angryNigelRequirements;

            case InventoryData.ANGRY_STEVEN_ID:
                return InventoryData.angryStevenRequirements;
        }

        return new Dictionary<string, int>();
    }

    private string ConvertCurrencyNameForFirebase(string inventoryCurrencyName)
    {
        if (inventoryCurrencyName == coinKey)
        {
            return FirebaseManager.CURRENCY_NAME_COINS;
        }

        if (inventoryCurrencyName == bananaKey)
        {
            return FirebaseManager.CURRENCY_NAME_BANANAS;
        }

        if (inventoryCurrencyName == blueberryKey)
        {
            return FirebaseManager.CURRENCY_NAME_BLUEBERRIES;
        }

        if (inventoryCurrencyName == strawberryKey)
        {
            return FirebaseManager.CURRENCY_NAME_STRAWBERRIES;
        }

        return "";
    }

    public void SpendCurrency(Dictionary<string, int> currencyDict, string itemName)
    {
        foreach (KeyValuePair<string, int> currency in currencyDict)
        {
            if (collectibleDict.ContainsKey(currency.Key))
            {
                collectibleDict[currency.Key] -= currency.Value;
                string firebaseCurrencyName = ConvertCurrencyNameForFirebase(currency.Key);
                if (firebaseCurrencyName.Length > 0)
                {
                    FirebaseManager.LogSpendVirtualCurrencyEvent(itemName, firebaseCurrencyName, currency.Value);
                }
            }
        }

        SaveUserData();
    }

    public void AddCurrency(Dictionary<string, int> currencyDict)
    {
        if (false == PlayerPrefsCommon.GetFirstItemsCollected())
        {
            notificationMgr.NotificationChange(NotificationManager.firstTimeItemsNotificationId,
                                               NotificationManager.SHOW_NOTIFICATION);
        }

        foreach (KeyValuePair<string, int> currency in currencyDict)
        {
            if (collectibleDict.ContainsKey(currency.Key))
            {
                collectibleDict[currency.Key] += currency.Value;
                string firebaseCurrencyName = ConvertCurrencyNameForFirebase(currency.Key);
                if (firebaseCurrencyName.Length > 0)
                {
                    FirebaseManager.LogEarnVirtualCurrencyEvent(firebaseCurrencyName, currency.Value);
                }
            }
        }

        foreach (KeyValuePair<uint, bool> bird in birdDict)
        {
            if (UNHATCHED == bird.Value)
            {
                if (CanHatchEggWithId(bird.Key))
                {

                    notificationMgr.NotificationChange(NotificationManager.canHatchNotificationId,
                                                       NotificationManager.SHOW_NOTIFICATION);
                }
            }
        }

        SaveUserData();
    }

    public int GetItemCount(string itemName)
    {
        if (collectibleDict.ContainsKey(itemName))
        {
            return collectibleDict[itemName];
        }

        return -1;
    }

    public void AddEggToInventory(uint id)
    {
        if (false == birdDict.ContainsKey(id))
        {
            MonoBehaviour.print("ADDING BIRD TO INVENTORY WITH ID " + id);
            notificationMgr.NotificationChange(NotificationManager.newEggNotificationId,
                                               NotificationManager.SHOW_NOTIFICATION);
            birdDict[id] = UNHATCHED;
            SaveUserData();
        }
    }

    public bool IsBirdInInventory(uint id)
    {
        return birdDict.ContainsKey(id);
    }

    public bool HasEggHatched(uint id)
    {
        if (birdDict.ContainsKey(id))
        {
            return birdDict[id];
        }
        return UNHATCHED;
    }

    public void HatchEggWithId(uint id)
    {
        if (birdDict.ContainsKey(id))
        {
            if (CanHatchEggWithId(id))
            {
                Dictionary<string, int> reqsDict = GetRequirementForId(id);

                // Spend the currency
                SpendCurrency(reqsDict, FirebaseManager.CURRENCY_SPEND_HATCH_EGG);

                // Mark the egg as hatched in the dictionary
                birdDict[id] = true;

                // Tell Firebase about the user achievement
                switch (id)
                {
                    case SAM_ID:
                        notificationMgr.NotificationChange(NotificationManager.canHatchNotificationId,
                                   NotificationManager.CLEAR_NOTIFICATION);

                        FirebaseManager.LogUnlockAchievementEvent(FirebaseManager.ACHIEVEMENT_ID_HATCH_SAM);
                        break;

                    case NIGEL_ID:
                        notificationMgr.NotificationChange(NotificationManager.canHatchNotificationId,
                                   NotificationManager.CLEAR_NOTIFICATION);

                        FirebaseManager.LogUnlockAchievementEvent(FirebaseManager.ACHIEVEMENT_ID_HATCH_NIGEL);
                        break;


                    case STEVEN_ID:
                        notificationMgr.NotificationChange(NotificationManager.canHatchNotificationId,
                                   NotificationManager.CLEAR_NOTIFICATION);

                        FirebaseManager.LogUnlockAchievementEvent(FirebaseManager.ACHIEVEMENT_ID_HATCH_STEVEN);
                        break;

                    case ANGRY_KOKO_ID:
                        FirebaseManager.LogUnlockAchievementEvent(FirebaseManager.ACHIEVEMENT_ID_HATCH_ANGRY_KOKO);
                        break;

                    case ANGRY_SAM_ID:
                        FirebaseManager.LogUnlockAchievementEvent(FirebaseManager.ACHIEVEMENT_ID_HATCH_ANGRY_SAM);
                        break;

                    case ANGRY_NIGEL_ID:
                        FirebaseManager.LogUnlockAchievementEvent(FirebaseManager.ACHIEVEMENT_ID_HATCH_ANGRY_NIGEL);
                        break;

                    case ANGRY_STEVEN_ID:
                        FirebaseManager.LogUnlockAchievementEvent(FirebaseManager.ACHIEVEMENT_ID_HATCH_ANGRY_STEVEN);
                        break;

                        // Add new birds here
                }

                // Save the data after this milestone
                SaveUserData();
            }
        }
    }

    public bool CanHatchEggWithId(uint id)
    {
        if (birdDict.ContainsKey(id))
        {
            Dictionary<string, int> reqsDict = GetRequirementForId(id);
            foreach (KeyValuePair<string, int> req in reqsDict)
            {
                if (collectibleDict[req.Key] < reqsDict[req.Key])
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    public bool ValidateBird(uint id)
    {
        if (birdDict.ContainsKey(id))
        {
            return birdDict[id];
        }

        return false;
    }

    public void ClaimLives()
    {
        if (dateToClaimLives <= DateTime.Now)
        {
            livesCount += CLAIM_LIVES_AMOUNT;
            dateToClaimLives = DateTime.Now.AddDays(1);
            SaveUserData();
        }
    }

    public DateTime GetDateToClaimLives()
    {
        return dateToClaimLives;
    }

    public void AddLives(int count)
    {
        livesCount += count;
        SaveUserData();
    }

    public bool UserCanBeRescued()
    {
        return livesCount > 0;
    }

    public void RescueUser()
    {
        // XXX Should there be a Firebase metric being tracked here?
        livesCount -= 1;
    }

    public int GetLifeCount()
    {
        return livesCount;
    }

    // Marked private because this should only be called via the Instance() method
    private static InventoryData LoadUserData()
    {
        string fullPath = GetFullPath();

        if (File.Exists(fullPath))
        {
            byte[] serializedData = EncryptedDataManager.ReadData(fullPath, key, iv);
            using (MemoryStream memStream = new MemoryStream(serializedData))
            {
                IFormatter formatter = new BinaryFormatter();
                InventoryDataStruct data = (InventoryDataStruct)formatter.Deserialize(memStream);
                return new InventoryData(data);
            }
        }

        InventoryData instance = new InventoryData();
        instance.inventoryData = new InventoryDataStruct
        {
            collectibleDict = instance.collectibleDict,
            birdDict = instance.birdDict,
            livesCount = instance.livesCount,
            dateToClaimLives = instance.dateToClaimLives,
        };

        return instance;
    }

    public static void DeleteUserData()
    {
        string fullPath = GetFullPath();

        if (File.Exists(fullPath))
        {
            _instance = null;
            File.Delete(fullPath);
        }
    }

    private static string GetFullPath()
    {
        string directory = NativeMobileStorage.GetApplicationDirectory();
        if (false == Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string fullPath = Path.Combine(directory, dataFilename);

        return fullPath;
    }
}
