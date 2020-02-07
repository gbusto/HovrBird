using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishBehavior : MonoBehaviour
{
    public Rigidbody2D rigidBody;
    private float startingY;
    private float impulse;

    private bool active;

    private const float ZERO_GRAVITY = 0.0f;
    private const float GRAVITY = 2.0f;

    private void Start()
    {
        startingY = transform.localPosition.y;
        rigidBody.gravityScale = GRAVITY;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 velocity = rigidBody.velocity;
        rigidBody.MoveRotation(velocity.y * -2.5f);

        if (transform.localPosition.y <= startingY)
        {
            rigidBody.velocity = new Vector2(0, 0);
            rigidBody.AddForce(new Vector2(0, impulse), ForceMode2D.Impulse);
        }
    }

    public void SetImpulse(float value)
    {
        impulse = value;
    }

    public void SetActive(bool value)
    {
        active = value;

        if (false == active)
        {
            rigidBody.gravityScale = ZERO_GRAVITY;
        }
        else
        {
            rigidBody.gravityScale = GRAVITY;
        }
    }
}
