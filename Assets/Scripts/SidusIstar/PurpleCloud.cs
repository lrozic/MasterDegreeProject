using UnityEngine;

public class PurpleCloud : MonoBehaviour
{
    [SerializeField] GameObject cameraUp;
    [SerializeField] float velocityY;
    [SerializeField] float lifeTime;

    public bool secondPlayerStanding;
    private bool firstPlayerStanding;
    private bool movingUp;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        movingUp = false;
        firstPlayerStanding = false;
        secondPlayerStanding = false;

        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// When player one (and two if playing multiplayer) stands on cloud, it moves up.
    /// If collision is from space, then lower the gravity scale to players and make cloud disappear.
    /// Note: cloud will udpdate the speed of the camera, which will be the same as the cloud's
    /// </summary>
    /// <param name="collision">Collision with another gameobject</param>
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

        if (!movingUp
            && (firstPlayerStanding
                || secondPlayerStanding))
        {
            movingUp = true;
            cameraUp.GetComponent<CameraControl>().MoveCamera(velocityY);
            GetComponent<Rigidbody2D>().velocity = Vector2.up * velocityY;
            Invoke(nameof(InSpace), lifeTime);
        }
    }

    /// <summary>
    /// Both players must be standing on the cloud if playing multiplayer
    /// </summary>
    /// <param name="collision">Collision with another gameobject</param>
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
    /// Purple clouds arrival to space.
    /// Method changes the player's gravity
    /// </summary>
    private void InSpace()
    {
        GetComponent<EdgeCollider2D>().enabled = false;
        animator.SetTrigger("Disappear");

        if (GameObject.Find("PlayerKnight1") != null)
        {
            GameObject.Find("PlayerKnight1").GetComponent<Paladin>().m_body2d.gravityScale = 1;
        }

        if (GameObject.Find("PlayerKnight2") != null)
        {
            GameObject.Find("PlayerKnight2").GetComponent<Paladin>().m_body2d.gravityScale = 1;
        }

        Destroy(gameObject, 1f);
    }
}
