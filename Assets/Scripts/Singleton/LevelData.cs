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

    public bool level6Complete;
    public int level6HighScore;
    public int level6TimesPlayed;

    public bool level7Complete;
    public int level7HighScore;
    public int level7TimesPlayed;

    public bool level8Complete;
    public int level8HighScore;
    public int level8TimesPlayed;

    public bool level9Complete;
    public int level9HighScore;
    public int level9TimesPlayed;

    public bool level10Complete;
    public int level10HighScore;
    public int level10TimesPlayed;

    public bool level11Complete;
    public int level11HighScore;
    public int level11TimesPlayed;

    public bool level12Complete;
    public int level12HighScore;
    public int level12TimesPlayed;

    public bool level13Complete;
    public int level13HighScore;
    public int level13TimesPlayed;

    public bool level14Complete;
    public int level14HighScore;
    public int level14TimesPlayed;

    public bool level15Complete;
    public int level15HighScore;
    public int level15TimesPlayed;
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

            level6Complete = false,
            level6HighScore = 0,
            level6TimesPlayed = 0,

            level7Complete = false,
            level7HighScore = 0,
            level7TimesPlayed = 0,

            level8Complete = false,
            level8HighScore = 0,
            level8TimesPlayed = 0,

            level9Complete = false,
            level9HighScore = 0,
            level9TimesPlayed = 0,

            level10Complete = false,
            level10HighScore = 0,
            level10TimesPlayed = 0,

            level11Complete = false,
            level11HighScore = 0,
            level11TimesPlayed = 0,

            level12Complete = false,
            level12HighScore = 0,
            level12TimesPlayed = 0,

            level13Complete = false,
            level13HighScore = 0,
            level13TimesPlayed = 0,

            level14Complete = false,
            level14HighScore = 0,
            level14TimesPlayed = 0,

            level15Complete = false,
            level15HighScore = 0,
            level15TimesPlayed = 0,
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
        if (false == Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string fullPath = Path.Combine(directory, dataFilename);

        return fullPath;
    }
}