using UnityEngine;

public class Fish2Behavior : MonoBehaviour
{
    public Rigidbody2D rigidBody;
    private float startingY;
    private const float IMPULSE = 12f;

    private void Start()
    {
        startingY = transform.localPosition.y;
        rigidBody.gravityScale = 2.0f;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 velocity = rigidBody.velocity;
        rigidBody.MoveRotation(velocity.y * -3f);

        if (transform.localPosition.y <= startingY)
        {
            rigidBody.velocity = new Vector2(0, 0);
            rigidBody.AddForce(new Vector2(0, IMPULSE), ForceMode2D.Impulse);
        }
    }
}
