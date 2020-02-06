using System.Collections.Generic;
using UnityEngine;

public class BirdBehavior : MonoBehaviour
{
    public Sprite birdNormal;
    public Sprite birdFlap;

    public GameObject commonObject;
    public CommonBehavior commonScript;

    public bool startGame;
    public bool fly;
    public bool triggerCollision;
    public string triggerCollisionTag;
    public string collisionTag;
    // True by default
    public bool enableCollisions = true;

    public float flyForce = 6f;
    public float gravity = 2f;

    private Rigidbody2D rb;
    private bool gravityEnabled;

    private Color normalColor = new Color();
    private Color fadedColor = new Color();

    private bool blink;
    private float timer;
    private float transitionTimer;
    static private float transitionWaitFor = 0.5f;
    static private float waitFor = 3.0f;

    private bool changeSprite;
    private float changeSpriteTimer;
    static private float changeSpriteWaitFor = 0.25f;

    private Renderer thisRenderer;
    private SpriteRenderer thisSpriteRenderer;

    private List<string> collisionTags;
    private List<string> triggerCollisionTags;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // In case this isn't already set to 0, set it to 0
        rb.gravityScale = 0f;

        tag = "Bird";

        collisionTags = new List<string>();
        triggerCollisionTags = new List<string>();

        normalColor = this.GetComponent<Renderer>().material.color;
        fadedColor = new Color(normalColor.r, normalColor.g, normalColor.b, 0.5f);

        thisRenderer = GetComponent<Renderer>();
        thisSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (startGame)
        {
            // Game is active! Go go go
            if (false == gravityEnabled)
            {
                EnableGravity();
            }

            if (blink)
            {
                timer += Time.deltaTime;
                transitionTimer += Time.deltaTime;

                if (timer >= waitFor)
                {
                    blink = false;
                    timer = 0f;
                    transitionTimer = 0f;
                    // Ensure we end up with the correct color after we stop blinking
                    thisRenderer.material.color = normalColor;
                    // Re-enable the pipe colliders
                    commonScript.EnableCollisions();
                }
                else if (transitionTimer >= transitionWaitFor)
                {
                    transitionTimer = 0f;
                    ChangeColor();
                }
            }
        }

        if (changeSprite)
        {
            changeSpriteTimer += Time.deltaTime;
            if (changeSpriteTimer >= changeSpriteWaitFor)
            {
                changeSprite = false;
                thisSpriteRenderer.sprite = birdNormal;
                changeSpriteTimer = 0f;
            }
        }
    }

    private void FixedUpdate()
    {
        // Apply an impulse if user touch was detected from LevelBehavior
        if (fly)
        {
            rb.velocity = new Vector2(0, 0);
            rb.AddForce(new Vector2(0, flyForce), ForceMode2D.Impulse);
            thisSpriteRenderer.sprite = birdFlap;
            changeSprite = true;
            changeSpriteTimer = 0f;
            fly = false;
        }
    }

    public void Rescued()
    {
        // The bird was rescued by the user

        timer = 0f;
        transitionTimer = 0f;
        blink = true;

        // Disable pipe colliders so we can go through them for 3 seconds to recover
        commonScript.DisableCollisions();
    }

    public void AddCollisionTags(List<string> tags)
    {
        foreach (string t in tags)
        {
            collisionTags.Add(t);
        }
    }

    public void AddTriggerCollisionTags(List<string> tags)
    {
        foreach (string t in tags)
        {
            triggerCollisionTags.Add(t);
        }
    }

    public void RemoveAllCollisionTags()
    {
        collisionTags.Clear();
        triggerCollisionTags.Clear();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (startGame && false == blink)
        {
            if (collisionTags.Contains(collision.gameObject.tag))
            {
                commonScript.CollisionOccurred(collision.gameObject);
                collisionTag = collision.gameObject.tag;
                DisableGravity();
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (startGame && false == blink)
        {
            if (collisionTags.Contains(collision.gameObject.tag))
            {
                commonScript.CollisionOccurred(collision.gameObject);
                collisionTag = collision.gameObject.tag;
                DisableGravity();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerCollisionTags.Contains(other.gameObject.tag))
        {
            commonScript.TriggerCollisionOccurred(other.gameObject);
            PolygonCollider2D pc = other.gameObject.GetComponent<PolygonCollider2D>();
            if (null != pc)
            {
                pc.enabled = false;
            }
        }
    }

    private void ChangeColor()
    {
        if (thisRenderer.material.color.a >= 0.9999f)
        {
            thisRenderer.material.color = fadedColor;
        }
        else
        {
            thisRenderer.material.color = normalColor;
        }
    }

    public void DisableGravity()
    {
        rb.velocity = new Vector2(0, 0);
        rb.gravityScale = 0f;
        gravityEnabled = false;
    }

    public void EnableGravity()
    {
        rb.gravityScale = gravity;
        gravityEnabled = true;
    }
}
