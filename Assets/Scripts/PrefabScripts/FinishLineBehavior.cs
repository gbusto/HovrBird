using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLineBehavior : MonoBehaviour
{
    public EdgeCollider2D collider;
    public GameObstacleBehavior gameObstacleScript;

    public ObstaclePrefab op;

    private float xDelta;

    private bool init;

    // Start is called before the first frame update
    void Start()
    {
        op = gameObstacleScript.op;

        xDelta = Random.Range(op.minSpeed, op.maxSpeed);

        Vector3 pos = transform.localPosition;
        pos.x = op.startX;
        transform.localPosition = pos;

        collider.enabled = true;

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
