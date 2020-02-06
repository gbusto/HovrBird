using UnityEngine;
using UnityEngine.UI;

public class ProgressLineBehavior : MonoBehaviour
{
    public Text progressText;

    public enum LevelProgress
    {
        twentyFive = 25,
        fifty = 50,
        seventyFive = 75
    };

    private float speed;
    private bool destroyOnInvisible;
    private CommonBehavior script;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (script.GetGameState() == CommonBehavior.GameState.Active)
        {
            Vector3 pos = transform.localPosition;
            pos.x -= speed;
            transform.localPosition = pos;
        }
    }

    private void OnBecameVisible()
    {
        // Since this item will be generated off screen, I'm not sure if OnBecameInvisible
        // will fire when it's instantiated. Just to be safe, only destroy the game object
        // after it's become visible which sets this boolean to true.
        destroyOnInvisible = true;
    }

    private void OnBecameInvisible()
    {
        if (destroyOnInvisible)
        {
            Destroy(gameObject);
        }
    }

    public void InitProgressLine(CommonBehavior script, LevelProgress progress, Vector3 position, float speed)
    {
        this.speed = speed;
        this.script = script;
        transform.localPosition = position;

        switch (progress)
        {
            case LevelProgress.twentyFive:
                progressText.text = "25%";
                break;

            case LevelProgress.fifty:
                progressText.text = "50%";
                break;

            case LevelProgress.seventyFive:
                progressText.text = "75%";
                break;
        }
    }
}
