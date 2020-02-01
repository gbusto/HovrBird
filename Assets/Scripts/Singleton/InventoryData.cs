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
}

public class InventoryData
{
    // { "coin": coinCount, "banana": bananaCount, ... }
    public Dictionary<string, int> collectibleDict;
    // { id: true (hatched), id: false (unhatched) }
    public Dictionary<uint, bool> birdDict; // BirdStruct is defined in BirdManager.cs

    private static readonly string dataFilename = "po3th6hafn95.hb";

    private static InventoryData _instance;

    public static readonly string coinKey = "coins";
    public static readonly string bananaKey = "bananas";
    public static readonly string blueberryKey = "blueberries";
    public static readonly string strawberryKey = "strawberries";

    public const uint KOKO_ID = 0x1;
    public const uint SAM_ID = 0x8c771755;

    public static Dictionary<string, int> samRequirements = new Dictionary<string, int>
    {
        [coinKey] = 150,
        [bananaKey] = 50,
        [blueberryKey] = 20,
        [strawberryKey] = 1
    };

    public static uint[] BIRDS =
    {
        KOKO_ID,
        SAM_ID
    };

    public const bool UNHATCHED = false;
    public const bool HATCHED = true;

    private InventoryDataStruct inventoryData;

    private bool wasEggAddedToInventory;

    private static byte[] key = {
        0x9c, 0xdb, 0xa3, 0xcb, 0xa9, 0x0b, 0xf1, 0x04, 0xc3, 0xb8, 0x79, 0x85, 0xb8, 0x22, 0xf8, 0x57
    };

    private static byte[] iv =
    {
        0x99, 0x6d, 0xb0, 0x25, 0xb4, 0xd3, 0xd0, 0x49, 0xe1, 0x8b, 0x73, 0x0a, 0x72, 0x05, 0x7b, 0xb4
    };



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
    }

    protected InventoryData(InventoryDataStruct data)
    {
        collectibleDict = data.collectibleDict;
        birdDict = data.birdDict;
    }

    public static InventoryData Instance()
    {
        if (null == _instance)
        {
            _instance = LoadUserData();
        }

        return _instance;
    }

    public void SaveUserData()
    {
        inventoryData.collectibleDict = collectibleDict;
        inventoryData.birdDict = birdDict;

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

        SaveUserData();
    }

    public void UpdateItemCount(string itemName, int itemValue)
    {
        if (collectibleDict.ContainsKey(itemName))
        {
            collectibleDict[itemName] += itemValue;
        }
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
            birdDict[id] = UNHATCHED;
            wasEggAddedToInventory = true;
        }
    }

    public bool NotifyForNewEgg()
    {
        if (wasEggAddedToInventory)
        {
            wasEggAddedToInventory = false;
            return true;
        }

        return false;
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
                        FirebaseManager.LogUnlockAchievementEvent(FirebaseManager.ACHIEVEMENT_ID_HATCH_SAM);
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
                if (inventoryData.collectibleDict[req.Key] < reqsDict[req.Key])
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
            birdDict = instance.birdDict
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
        MonoBehaviour.print("Unity: Checking if adventure mode directory exists at '" + directory + "'...");
        if (false == Directory.Exists(directory))
        {
            MonoBehaviour.print("Unity: Directory didn't exist! Creating it at: '" + directory + "'...");
            Directory.CreateDirectory(directory);
        }

        string fullPath = Path.Combine(directory, dataFilename);
        MonoBehaviour.print("Unity: Full path for adventure mode sky series: '" + fullPath + "'");

        return fullPath;
    }
}
