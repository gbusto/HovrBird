using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public struct LevelDataStruct
{
    // Number of consecutive days the user has played the game
    // If it has been more than 24 hours, this variable gets reset to 0
    public int consecutiveDaysPlayed;

    // Last game in which a user played
    public DateTime lastGame;

    // Boolean that specifies whether or not the device is jail broken
    // I gave it an inconvenient and ambiguous name to avoid the symbol name giving it away
    public bool a;

    public int level0HighScore;
    public int level0TimesPlayed;

    public bool level1Complete;
    public int level1HighScore;
    public int level1TimesPlayed;

    public bool level2Complete;
    public int level2HighScore;
    public int level2TimesPlayed;

    public bool level3Complete;
    public int level3HighScore;
    public int level3TimesPlayed;

    public bool level4Complete;
    public int level4HighScore;
    public int level4TimesPlayed;

    public bool level5Complete;
    public int level5HighScore;
    public int level5TimesPlayed;
}

public class LevelData
{
    public LevelDataStruct levelData;

    private static readonly string dataFilename = "1rslvecl9wop.hb";

    private static LevelData _instance;

    private static byte[] key =
    {
        0xa2, 0x79, 0xce, 0x42, 0xf6, 0xc2, 0xbc, 0x66, 0x2d, 0x9f, 0xd0, 0x3c, 0x4d, 0x43, 0xe7, 0x53
    };

    private static byte[] iv =
    {
        0x76, 0xaa, 0xdf, 0x88, 0xde, 0x47, 0xc7, 0xb8, 0xe7, 0xa3, 0x02, 0x4c, 0xde, 0xb0, 0x40, 0x42
    };

    // Protected initializer; call Instance() method to initialize and load class
    protected LevelData()
    {
        levelData = new LevelDataStruct()
        {
            consecutiveDaysPlayed = 0,
            lastGame = DateTime.Now,
            a = false,

            level0HighScore = 0,
            level0TimesPlayed = 0,

            level1Complete = false,
            level1HighScore = 0,
            level1TimesPlayed = 0,

            level2Complete = false,
            level2HighScore = 0,
            level2TimesPlayed = 0,
            
            level3Complete = false,
            level3HighScore = 0,
            level3TimesPlayed = 0,

            level4Complete = false,
            level4HighScore = 0,
            level4TimesPlayed = 0,

            level5Complete = false,
            level5HighScore = 0,
            level5TimesPlayed = 0,
        };
    }

    protected LevelData(LevelDataStruct data)
    {
        levelData = data;
    }

    public static LevelData Instance()
    {
        if (null == _instance)
        {
            _instance = LoadUserData();
        }

        return _instance;
    }

    public void SaveUserData()
    {
        UpdateConsecutiveDays(DateTime.Now);

        string fullPath = GetFullPath();
        IFormatter formatter = new BinaryFormatter();

        using (MemoryStream memStream = new MemoryStream())
        {
            formatter.Serialize(memStream, levelData);
            memStream.Seek(0, SeekOrigin.Begin);
            byte[] serializedData = memStream.ToArray();
            EncryptedDataManager.WriteData(fullPath, key, iv, serializedData);
        }
    }

    private void UpdateConsecutiveDays(DateTime now)
    {
        DateTime normalizedNow = NormalizeDateTime(now);
        DateTime normalizedLastGame = NormalizeDateTime(levelData.lastGame);
        // Only keep the year, month, and day of the last attempt and add a day to it
        // We want to compare what would be the "next day" from the user's last game
        // with their current their current game
        DateTime normalizedNextDay = normalizedLastGame.AddDays(1);
        int result = normalizedNow.CompareTo(normalizedNextDay);
        if (result < 0)
        {
            // Today is still before tomorrow
            // Don't need to update consecutive days played because it's still today...
        }
        else if (0 == result)
        {
            // Today is the same day
            // Update consecutive days played
            levelData.consecutiveDaysPlayed += 1;
        }
        else
        {
            // User missed a day... reset consecutive days counter
            levelData.consecutiveDaysPlayed = 0;
        }

        levelData.lastGame = now;
    }

    private DateTime NormalizeDateTime(DateTime date)
    {
        // Year, month, day, hour, minute, second
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
    }

    // Marked private because this should only be called via the Instance() method
    private static LevelData LoadUserData()
    {
        string fullPath = GetFullPath();

        if (File.Exists(fullPath))
        {
            byte[] serializedData = EncryptedDataManager.ReadData(fullPath, key, iv);
            using (MemoryStream memStream = new MemoryStream(serializedData))
            {
                IFormatter formatter = new BinaryFormatter();
                LevelDataStruct data = (LevelDataStruct)formatter.Deserialize(memStream);
                return new LevelData(data);
            }
        }

        return new LevelData();
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