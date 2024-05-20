using UnityEngine;

public class BlueDVDStar : MonoBehaviour
{
    private Rigidbody2D rigidbody2d;

    // Called before Start
    private void Awake()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        rigidbody2d.velocity = new Vector2(1f, 1f);
    }

    /// <summary>
    /// Collision of the DVD star with ground, walls and ceiling
    /// </summary>
    /// <param name="collision">Collision from ground, wall or ceiling</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name.StartsWith("ChangeX"))
        {
            ChangeVelocity(directionX: -1f);
        }
        else if (collision.name.StartsWith("ChangeY"))
        {
            ChangeVelocity(directionY: -1f);
        }
    }

    /// <summary>
    /// Change the velocity of the DVD star
    /// </summary>
    /// <param name="directionX">Velocity X</param>
    /// <param name="directionY">Velocity Y</param>
    public void ChangeVelocity(float directionX = 1f, float directionY = 1f)
    {
        SingletonSFX.Instance.PlaySFX("SFX48_parried_star");

        rigidbody2d.velocity = new Vector2(
            rigidbody2d.velocity.x * directionX, 
            rigidbody2d.velocity.y * directionY);
    }
}
