using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Navigator
{
    public static Stack<string> sceneStack;

    public const string GAME_MENU_SCENE = "GameMenuScene";
    
    public const string LEVEL_SERIES1_SCENE = "Level1Scene";
    public const string LEVEL_SERIES2_SCENE = "WaterSeriesScene";
    public const string LEVEL_SERIES3_SCENE = "BeachSeriesScene";

    private static readonly string[] level_scenes =
    {
        LEVEL_SERIES1_SCENE,
        LEVEL_SERIES2_SCENE
    };

    public static void LoadScene(string sceneName)
    {
        Initialize();

        if (false == sceneStack.Contains(sceneName))
        {
            sceneStack.Push(sceneName);
        }
        SceneManager.LoadScene(sceneName);
    }

    public static void LoadPreviousScene()
    {
        Initialize();

        if (sceneStack.Count > 1)
        {
            sceneStack.Pop();
        }

        string sceneName = sceneStack.ToArray()[0];
        SceneManager.LoadScene(sceneName);
    }

    public static void ReloadCurrentScene()
    {
        Initialize();

        string sceneName = sceneStack.ToArray()[0];
        SceneManager.LoadScene(sceneName);
    }

    public static void LoadNextLevelScene()
    {
        Initialize();

        // Get next level scene name to push onto stack
        string levelSceneName = GetSceneForLevelNumber(LevelManager.GetCurrentLevel());
        // Pop off the current scene at the top if it's a level scene
        if (LevelSceneAtTop())
        {
            sceneStack.Pop();
        }
        LoadScene(levelSceneName);
    }

    public static string GetSceneForLevelNumber(int levelNumber)
    {
        if (levelNumber >= 1 && levelNumber <= 5)
        {
            return LEVEL_SERIES1_SCENE;
        }

        if (levelNumber >= 6 && levelNumber <= 10)
        {
            return LEVEL_SERIES2_SCENE;
        }

        if (levelNumber >= 11 && levelNumber <= 15)
        {
            return LEVEL_SERIES3_SCENE;
        }

        return GAME_MENU_SCENE;
    }

    private static bool LevelSceneAtTop()
    {
        string sceneName = sceneStack.ToArray()[0];
        for (int i = 0; i < level_scenes.Length; ++i)
        {
            if (level_scenes[i] == sceneName)
            {
                return true;
            }
        }

        return false;
    }

    private static void Initialize()
    {
        if (null == sceneStack)
        {
            sceneStack = new Stack<string>();
            sceneStack.Push(GAME_MENU_SCENE);
        }
    }
}
