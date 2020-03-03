#define TESTING

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
        data.levelData.level1Complete  = true;
        data.levelData.level2Complete  = true;
        data.levelData.level3Complete  = true;
        data.levelData.level4Complete  = true;
        data.levelData.level5Complete  = true;
        data.levelData.level6Complete  = true;
        data.levelData.level7Complete  = true;
        data.levelData.level8Complete  = true;
        data.levelData.level9Complete  = true;
        data.levelData.level10Complete = true;
        data.levelData.level11Complete = true;
        data.levelData.level12Complete = true;
        data.levelData.level13Complete = true;
        data.levelData.level14Complete = true;
        data.levelData.level15Complete = true;
        data.SaveUserData();
    }

    public static void GetAllCollectibles()
    {
        InventoryData data = InventoryData.Instance();

        Dictionary<string, int> currencyDict = new Dictionary<string, int>
        {
            [InventoryData.coinKey] = 150,
            [InventoryData.bananaKey] = 60,
            [InventoryData.blueberryKey] = 20,
            [InventoryData.strawberryKey] = 1
        };
        data.AddCurrency(currencyDict);

        data.SaveUserData();
    }
}
#endif
