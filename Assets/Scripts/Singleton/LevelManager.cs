public class LevelManager
{
    private static int levelNumber = 0;

    public static int GetCurrentLevel()
    {
        return levelNumber;
    }

    public static void SetLevelNumber(int level)
    {
        levelNumber = level;
    }
}
