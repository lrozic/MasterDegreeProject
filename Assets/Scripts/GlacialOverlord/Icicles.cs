using System.Collections;
using UnityEngine;

public class Icicles : MonoBehaviour
{
    [SerializeField] GameObject iceGenerator;
    [SerializeField] float velocityX;
    [SerializeField] float velocityY;

    private Rigidbody2D rigidbody2d;

    private bool fall;
    private bool wiggled;
    private bool firstPlayerStanding;
    private bool secondPlayerStanding;

    float time;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        rigidbody2d.velocity = new Vector2(0, velocityY);

        wiggled = false;

        firstPlayerStanding = false;
        secondPlayerStanding = false;

    time = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (fall)
        {
            time += Time.deltaTime;
            rigidbody2d.velocity = new Vector2(0f, -time * 20);
        }
    }

    /// <summary>
    /// Start an event after colliding with event collider, overlord's sword or ground
    /// </summary>
    /// <param name="collision">Event collider or ground</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals("PlayerKnight1"))
        {
            firstPlayerStanding = true;
        }

        if (collision.gameObject.name.Equals("PlayerKnight2"))
        {
            secondPlayerStanding = true;
        }

        if (collision.gameObject.CompareTag("Event")
            && !wiggled)
        {
            wiggled = true;
            StartCoroutine(Wiggle());
        }

        if (transform.CompareTag("Event")
            && collision.gameObject.layer == 6)
        {
            fall = false;
            rigidbody2d.velocity = new Vector2(0f, 0f);
        }

        if (collision.CompareTag("OverlordSword"))
        {
            SingletonSFX.Instance.PlaySFX("SFX55_icicle_destroyed");

            Instantiate(
                iceGenerator,
                new Vector2(
                    transform.position.x,
                    transform.position.y),
                Quaternion.identity);

            Instantiate(
                iceGenerator,
                new Vector2(
                    transform.position.x,
                    transform.position.y),
                Quaternion.identity);

            Destroy(gameObject, 0.05f);
        }

        CollidingWithGround(collision);
    }

    /// <summary>
    /// Do actions for icicles when colliding with ground.
    /// Staying icicles stay for short amount of time on the ground and then get destroyed
    /// or Overlord destroys them.
    /// Other icicles get destroyed with particle effects.
    /// </summary>
    /// <param name="collision"></param>
    private void CollidingWithGround(Collider2D collision)
    {
        if (collision.CompareTag("Ground")
            && transform.name.StartsWith("IcicleStaying"))
        {
            SingletonSFX.Instance.PlaySFX("SFX56_ice_crystal_grounded");
            fall = false;
            transform.tag = "Ground";

            int layerIgnoreRaycast = LayerMask.NameToLayer("Ground");
            gameObject.layer = layerIgnoreRaycast;
        }
        else if (collision.CompareTag("Ground"))
        {
            SingletonSFX.Instance.PlaySFX("SFX55_icicle_destroyed");
            Instantiate(
                iceGenerator,
                new Vector2(
                    transform.position.x,
                    transform.position.y),
                Quaternion.identity);
            Destroy(gameObject, 0.05f);
        }
    }

    /// <summary>
    /// After exiting collision with players, set false to player's standing on staying icicle
    /// </summary>
    /// <param name="collision">Collision with player</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals("PlayerKnight1"))
        {
            firstPlayerStanding = false;
        }

        if (collision.gameObject.name.Equals("PlayerKnight2"))
        {
            secondPlayerStanding = false;
        }
    }

    /// <summary>
    /// Makes icicle wiggle before falling
    /// </summary>
    /// <returns></returns>
    private IEnumerator Wiggle()
    {
        rigidbody2d.gravityScale = 0;
        rigidbody2d.velocity = new Vector2(0f, 0f);
        yield return new WaitForSeconds(0.3f);
        rigidbody2d.velocity = new Vector2(velocityX, 0f);
        yield return new WaitForSeconds(0.2f);
        rigidbody2d.velocity = new Vector2(-velocityX, 0f);
        yield return new WaitForSeconds(0.2f);
        rigidbody2d.velocity = new Vector2(velocityX, 0f);
        yield return new WaitForSeconds(0.2f);
        rigidbody2d.velocity = new Vector2(-velocityX, 0f);
        yield return new WaitForSeconds(0.2f);

        SingletonSFX.Instance.PlaySFX("SFX57_icicle_falling");
        fall = true;
    }

    /// <summary>
    /// Check if players are standing on icicle.
    /// If true, then decrease m_ColCount in SensorHeroKnight.cs.
    /// This is important because of bug that lets players jump indefinitely.
    /// </summary>
    private void OnDestroy()
    {
        if (transform.name.StartsWith("IcicleStaying"))
        {
            if (firstPlayerStanding)
            {
                GameObject.Find("PlayerKnight1").transform.GetChild(5).GetComponent<SensorHeroKnight>().NotStayingOnIcicle(false);
            }

            if (secondPlayerStanding)
            {
                GameObject.Find("PlayerKnight2").transform.GetChild(5).GetComponent<SensorHeroKnight>().NotStayingOnIcicle(false);
            }
        }
    }
}
