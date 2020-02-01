#define TESTING

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
        data.UpdateItemCount(InventoryData.coinKey, 1000);
        data.UpdateItemCount(InventoryData.bananaKey, 1000);
        data.UpdateItemCount(InventoryData.blueberryKey, 1000);
        data.UpdateItemCount(InventoryData.strawberryKey, 1000);

        data.SaveUserData();
    }
}
#endif
