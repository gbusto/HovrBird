using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Navigator
{
    public static Stack<string> sceneStack;

    public static void LoadScene(string sceneName)
    {
        Initialize();

        sceneStack.Push(sceneName);
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

    private static void Initialize()
    {
        if (null == sceneStack)
        {
            sceneStack = new Stack<string>();
            sceneStack.Push("GameMenuScene");
        }
    }
}
