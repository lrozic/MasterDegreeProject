using UnityEngine;

public class KnightColliderHitbox : MonoBehaviour
{
    /// <summary>
    /// If enemy touches player, player loses one heart.
    /// </summary>
    /// <param name="collision">Collider from enemy gameobject</param>
    private void OnTriggerStay2D(Collider2D collision){
        if (collision.transform.CompareTag("Enemy") 
            && !transform.parent.gameObject.GetComponent<Paladin>().immortal 
            && !transform.parent.gameObject.GetComponent<Paladin>().m_rolling)          
        {
            transform.parent.gameObject.GetComponent<Paladin>().DecreaseHealth();
        }
    }
}
