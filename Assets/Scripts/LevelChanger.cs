using UnityEngine;

public class LevelChanger : MonoBehaviour
{
    public Animator animator;

    private enum Direction
    {
        back = -1,
        same = 0,
        forward = 1,
        nextLevel,
    };

    private string sceneToLoad;
    private Direction direction;

    public void FadeToScene(string sceneName)
    {
        sceneToLoad = sceneName;
        direction = Direction.forward;

        animator.SetTrigger("FadeOut");
    }

    public void FadeToPreviousScene()
    {
        direction = Direction.back;

        animator.SetTrigger("FadeOut");
    }

    public void FadeToSameScene()
    {
        direction = Direction.same;

        animator.SetTrigger("FadeOut");
    }

    public void FadeToNextLevel()
    {
        direction = Direction.nextLevel;

        animator.SetTrigger("FadeOut");
    }

    public void OnFadeComplete()
    {
        switch (direction)
        {
            case Direction.back:
                Navigator.LoadPreviousScene();
                break;

            case Direction.same:
                Navigator.ReloadCurrentScene();
                break;

            case Direction.forward:
                Navigator.LoadScene(sceneToLoad);
                break;

            case Direction.nextLevel:
                Navigator.LoadNextLevelScene();
                break;
        }
    }
}
