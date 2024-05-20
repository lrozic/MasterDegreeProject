using UnityEngine;

public class GlacialOverlordSword : MonoBehaviour
{
    /// <summary>
    /// Set the name of arena if player is in front of the door of the arena
    /// </summary>
    /// <param name="collision">Colider from another gameobject</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Sword"))
        {
            SingletonSFX.Instance.PlaySFX("SFX61_sword_clash");

            if (transform.parent.transform.position.x > collision.transform.position.x)
            {
                GetComponent<PolygonCollider2D>().enabled = false;
                GetComponentInParent<GlacialOverlord>().forcedMovement = 35f;
            }
            else 
            {
                GetComponent<PolygonCollider2D>().enabled = false;
                GetComponentInParent<GlacialOverlord>().forcedMovement = -35f;
            }

            Invoke(nameof(RemoveForcedMovement), 0.075f);
        }
    }

    /// <summary>
    /// Stop Overlord from moving backwards after paladin successfully defended 
    /// </summary>
    private void RemoveForcedMovement()
    {
        GetComponentInParent<GlacialOverlord>().forcedMovement = 0f;
        GetComponentInParent<Rigidbody2D>().velocity = Vector2.zero;
    }
}
