using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] GameObject ballEventsGameObject;

    private BallEvents eventsScript;
    private Rigidbody2D rigidbody2d;

    // Delete this after testing
    public bool cameraShouldMoveUp;

    // Start is called before the first frame update
    void Start()
    {
        eventsScript = ballEventsGameObject.GetComponent<BallEvents>();
        rigidbody2d = GetComponent<Rigidbody2D>();
        rigidbody2d.velocity = Vector2.zero;
    }

    // Update is called once per frame
    private void Update()
    {
        // Delete all of this after testing
        if (cameraShouldMoveUp)
        {
            MoveCamera(5);
            cameraShouldMoveUp = false;
        }
    }

    /// <summary>
    /// Start  an event after detecting a ball event or event collider
    /// </summary>
    /// <param name="collision">Collider from ball event of event collider</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals("BallEvent"))
        {
            if (eventsScript.numberedEvent != 13)
            {
                eventsScript.StartEvent();
            }
        }
        else if (collision.gameObject.name.Equals("FifthCollider"))
        {
            // Will not add numbered event since the same attack would perform at the center of screen,
            // therefore there is no need for it
            Destroy(collision.gameObject);
            eventsScript.MoveBallEventUp(5f);
        }
        else if (collision.gameObject.name.Equals("SixthCollider"))
        {
            Destroy(collision.gameObject);
            eventsScript.AddNumberedEvent();
            eventsScript.StartEvent();
        }
        else if (collision.gameObject.name.Equals("PurpleClouds"))
        {
            MoveCamera(0f);
        }
    }

    /// <summary>
    /// Move camera up
    /// </summary>
    /// <param name="velocityY">Velocity on Y axis</param>
    public void MoveCamera(float velocityY)
    {
        rigidbody2d.velocity = new Vector2(0f, velocityY);
    }
}
