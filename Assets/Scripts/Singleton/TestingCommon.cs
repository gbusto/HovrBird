//#define TESTING

using System.Collections.Generic;

#if TESTING
public static class TestingCommon
{
    public static void DeleteAdventureData()
    {
        // Only delete user data associated with adventure data
        LevelData.DeleteUserData();
        InventoryData.DeleteUserData();
        NotificationManager.DeleteUserData();
    }

    public static void EnableAllLevels()
    {
        LevelData data = LevelData.Instance();
        data.levelData.level1Complete = true;
        data.levelData.level2Complete = true;
        data.levelData.level3Complete = true;
        data.levelData.level4Complete = true;
        data.levelData.level5Complete = true;
        data.SaveUserData();
    }

    public static void GetAllCollectibles()
    {
        InventoryData data = InventoryData.Instance();

        Dictionary<string, int> currencyDict = new Dictionary<string, int>
        {
            [InventoryData.coinKey] = 1000,
            [InventoryData.bananaKey] = 1000,
            [InventoryData.blueberryKey] = 1000,
            [InventoryData.strawberryKey] = 1000
        };
        data.AddCurrency(currencyDict);

        data.SaveUserData();
    }
}
#endif
