using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBirdBehavior : MonoBehaviour
{
    public Sprite normalBirdSprite;
    public Sprite flapBirdSprite;
    public SpriteRenderer spriteRenderer;

    public Rigidbody2D rigidBody;
    public PolygonCollider2D collider;
    public GameObstacleBehavior gameObstacleScript;

    public ObstaclePrefab op;

    public float minHorizontalImpulse = 3.0f;
    public float maxHorizontalImpulse = 3.5f;

    public float minVerticalImpulse = 2.0f;
    public float maxVerticalImpulse = 3.0f;

    private float startY;

    private float horizontalImpulseValue;
    private float verticalImpulseValue;

    private bool changeSprite;
    private float changeSpriteTimer;
    static private float changeSpriteWaitFor = 0.20f;

    private Direction direction;

    private bool init;

    private float MAX_GRAVITY_SCALE = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        op = gameObstacleScript.op;

        System.Random sysRandom = new System.Random();
        horizontalImpulseValue = (float)(sysRandom.NextDouble() * (maxHorizontalImpulse - minHorizontalImpulse) + minHorizontalImpulse);
        verticalImpulseValue = (float)(sysRandom.NextDouble() * (maxVerticalImpulse - minVerticalImpulse) + minVerticalImpulse);

        direction = op.directions[sysRandom.Next(0, op.directions.Length)];

        Vector3 pos = transform.localPosition;
        pos.x = op.startX;
        pos.y = Random.Range(op.minGenY, op.maxGenY);
        transform.localPosition = pos;

        startY = pos.y;

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
            if (Mathf.Approximately(rigidBody.gravityScale, 0f))
            {
                rigidBody.gravityScale = MAX_GRAVITY_SCALE;
            }

            // Do whatever is needed to move the object
            if (gameObstacleScript.disableColliders && collider.enabled)
            {
                collider.enabled = false;
            }
            else if (!gameObstacleScript.disableColliders && !collider.enabled)
            {
                collider.enabled = true;
            }

            // Wait to change back to the normal bird sprite after flapping
            if (changeSprite)
            {
                changeSpriteTimer += Time.deltaTime;
                if (changeSpriteTimer >= changeSpriteWaitFor)
                {
                    changeSprite = false;
                    spriteRenderer.sprite = normalBirdSprite;
                    changeSpriteTimer = 0f;
                }
            }

            Vector3 pos = transform.localPosition;
            switch (direction)
            {
                case Direction.up:
                    if (rigidBody.velocity.y <= -1.0f)
                    {
                        ApplyImpulse();
                        startY = pos.y;
                    }
                    break;

                case Direction.down:
                    if (pos.y <= startY)
                    {
                        ApplyImpulse();
                        startY -= 0.2f;
                    }
                    break;

                case Direction.none:
                    if (pos.y <= startY)
                    {
                        ApplyImpulse();
                    }
                    break;
            }

            if (pos.x <= op.endX)
            {
                gameObstacleScript.cleanup = true;
            }

            // Rotate the bird based on its velocity
            rigidBody.rotation = rigidBody.velocity.y * -2f;
        }
        else
        {
            if (Mathf.Approximately(rigidBody.gravityScale, MAX_GRAVITY_SCALE))
            {
                rigidBody.gravityScale = 0f;
            }
        }
    }

    private void ApplyImpulse()
    {
        rigidBody.velocity = new Vector2(0, 0);
        rigidBody.AddForce(new Vector2(-1 * horizontalImpulseValue, verticalImpulseValue), ForceMode2D.Impulse);

        spriteRenderer.sprite = flapBirdSprite;
        changeSprite = true;
        changeSpriteTimer = 0f;
    }
}
