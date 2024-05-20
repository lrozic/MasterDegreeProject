using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class BodyPart : MonoBehaviour
{
    [SerializeField] GameObject fireDestroyedBodyPart;
    [SerializeField] GameObject positionBodyPart;
    [SerializeField] GameObject maggots;
    [SerializeField] GameObject heartBlood;
    [SerializeField] protected int bodyPartHealth;
    [SerializeField] bool invincible;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody2d;
    private ChainedUndead chainedUndead;

    bool destroyed;

    // Start is called before the first frame update
    private void Start()
    {
        rigidBody2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody2d.gravityScale = 0;
        destroyed = false;

        if (GameObject.Find("PlayerKnight2") != null)
        {
            bodyPartHealth += (int)Math.Round(bodyPartHealth * (1d / 3d), 0);
        }

        chainedUndead = GameObject.Find("ThirdBossChainedUndead").GetComponent<ChainedUndead>();
        chainedUndead.AddBodyPartToDictionary(name);
    }

    // Update is called once per frame
    private void Update()
    {
        if (destroyed)
        {
            rigidBody2d.gravityScale = 4;
        }
    }

    /// <summary>
    /// Change the invincibility status to false.
    /// Used since it doesn't make sense to destroy upper part of leg while lower is still attached to it
    /// </summary>
    public void ChangeInvincibilityStatus() 
    { 
        invincible = false; 
    }

    /// <summary>
    /// Call method for decreasing bosses' health after getting in contact with player's sword or projectile
    /// </summary>
    /// <param name="collision">Collider from another gameobject</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!invincible) 
        { 
            if (collision.gameObject.layer == 16)
            {
                bodyPartHealth -= 2;
                SingletonSFX.Instance.PlaySFX("SFX15_blood_splash");

                if (name.Equals("Heart"))
                {
                    heartBlood.GetComponent<ParticleSystem>().Play();
                }

                spriteRenderer.color = new Color(1f, 0.3915094f, 0.3915094f);

                Invoke(nameof(ReturnNormalColor), 0.2f);
            }

            if (bodyPartHealth <= 0 
                && !destroyed)
            {
                if (transform.childCount == 2) 
                {
                    GameObject chain = transform.GetChild(1).gameObject;
                    SingletonSFX.Instance.PlaySFX("SFX44_steel_chain_rattle");        
                    
                    float chainVelocityX = chain.transform.rotation.eulerAngles.z / 10;
                    if (chain.transform.rotation.eulerAngles.z > 90)
                    {
                        chainVelocityX *= -1;
                    }

                    chain.GetComponent<Rigidbody2D>().velocity = new Vector2(chainVelocityX, 35f);
                }

                chainedUndead.ChangeBodyPartStatusInDictionary(name);
                ButcheredDestroyed();
            }
        }
    }

    /// <summary>
    /// Return to original color after certain time when taking damage
    /// </summary>
    private void ReturnNormalColor() 
    {
        spriteRenderer.color = new Color(1f, 1f, 1f);
    }

    /// <summary>
    /// Method which sets gravity scale in order for body part to fall.
    /// Also calls method for spawning fire around destroyed body
    /// </summary>
    private void ButcheredDestroyed()
    {
        destroyed = false;
        transform.GetComponent<PolygonCollider2D>().enabled = false;

        spriteRenderer.sortingOrder = 2;
        rigidBody2d.gravityScale = 4;

        if (maggots != null)
        {
            InstantiateMaggots();
        }

        GameObject.Find("RespawnController").GetComponent<ConcreteSubject>().AddBodyPartsDestroyed();

        StartCoroutine(PlayDeathFireAnimation());
        Destroy(this, 3f);
    }
    /// <summary>
    /// Method for spawning maggots
    /// </summary>
    private void InstantiateMaggots()
    {
        int randomMaggetNumber = Random.Range(3, 6);
        for (int i = 0; i < randomMaggetNumber; i++)
        {
            int randomPlacementX = Random.Range(-2, 3);
            int randomPlacementY = Random.Range(-3, 4);

            GameObject maggot = Instantiate(
               maggots,
               new Vector2(
                   positionBodyPart.transform.position.x + randomPlacementX,
                   positionBodyPart.transform.position.y + randomPlacementY),
               Quaternion.identity);

            maggot.transform.rotation= Quaternion.Euler(
                0f,
                0f,
                -90f);
        }
    }

    /// <summary>
    /// Instantiates fire around destroyed body part and plays certain SFX
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayDeathFireAnimation()
    {
        if (!name.Equals("Heart"))
        {
            int numberOfFireDestroyed = Random.Range(8, 13);

            for (int i = 0; i < numberOfFireDestroyed; i++)
            {
                int randomPlacementX = Random.Range(-2, 3);
                int randomPlacementY = Random.Range(-3, 4);

                var clonedFireDestroyedBodyPart = Instantiate(
                    fireDestroyedBodyPart, new Vector3(
                        positionBodyPart.transform.position.x + randomPlacementX,
                        positionBodyPart.transform.position.y + randomPlacementY,
                        positionBodyPart.transform.position.z),
                    Quaternion.identity);

                Destroy(clonedFireDestroyedBodyPart, 0.4f);

                SingletonSFX.Instance.PlaySFX("SFX37_body_part_destroyed");
                yield return new WaitForSecondsRealtime(0.2f);
            }
        }
    }
}
