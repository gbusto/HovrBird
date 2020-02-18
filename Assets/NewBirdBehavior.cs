using UnityEngine;
using UnityEngine.UI;

public class NewBirdBehavior : MonoBehaviour
{
    public Rigidbody2D rb;
    public Sprite birdLeft;
    public Sprite birdRight;
    public SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Flap(float xDelta)
    {
        Vector3 currentPos = transform.localPosition;
        float diff = xDelta - currentPos.x;

        if (diff > 6f)
        {
            diff = 6f;
        }
        if (diff < -6f)
        {
            diff = -6f;
        }

        if (diff > 0)
        {
            spriteRenderer.sprite = birdRight;
        }
        else
        {
            spriteRenderer.sprite = birdLeft;
        }

        rb.velocity = new Vector2(0, 0);
        rb.AddForce(new Vector2(diff, 10f), ForceMode2D.Impulse);
        /*
        if (xDelta > 0)
        {
            rb.AddForce(new Vector2(4f, 10f), ForceMode2D.Impulse);
        }
        else if (xDelta < 0)
        {
            rb.AddForce(new Vector2(-4f, 10f), ForceMode2D.Impulse);
        }
        else
        {
            rb.AddForce(new Vector2(0f, 10f), ForceMode2D.Impulse);
        }
        */
    }
}
