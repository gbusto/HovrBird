using UnityEngine;
using UnityEngine.UI;

public class PropPlaneBehavior : MonoBehaviour
{
    public PolygonCollider2D collider;
    public GameObstacleBehavior gameObstacleScript;
    public Text bannerText;

    public ObstaclePrefab op;

    private float xDelta;

    private bool init;

    private static string[] textOptions =
    {
        "You're doing great, kid!",
        "Visit https://ashgames.xyz",
        "Are you having fun yet?",
    };

    // Start is called before the first frame update
    void Start()
    {
        op = gameObstacleScript.op;

        xDelta = Random.Range(op.minSpeed, op.maxSpeed);

        Vector3 pos = transform.localPosition;
        pos.x = op.startX;
        pos.y = Random.Range(op.minGenY, op.maxGenY);
        transform.localPosition = pos;

        System.Random sysRandom = new System.Random();
        bannerText.text = textOptions[sysRandom.Next(0, textOptions.Length)];

        init = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (false == init)
        {
            Start();
        }

        if (gameObstacleScript.active)
        {
            // Do whatever is needed to move the object
            if (gameObstacleScript.disableColliders && collider.enabled)
            {
                collider.enabled = false;
            }
            else if (!gameObstacleScript.disableColliders && !collider.enabled)
            {
                collider.enabled = true;
            }

            Vector3 pos = transform.localPosition;
            pos.x -= xDelta;
            transform.localPosition = pos;

            if (pos.x <= op.endX)
            {
                gameObstacleScript.cleanup = true;
            }
        }
    }
}
