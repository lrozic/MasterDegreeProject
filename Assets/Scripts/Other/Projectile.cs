using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] GameObject hitBoxGameobject;
    [SerializeField] Transform rotationTo;
    [SerializeField] GameObject particleEffect;

    private Rigidbody2D rigidBody2DParent;
    private Transform transformParent;
    private GameObject playerGameObject;

    private bool parried;
    private bool remainAtShield;
    private float addPositionNumber;

    // For flying ice crystals
    private float timeCount = 0f;
    private bool onTheGround;
    private bool leftSpan = false;

    /// <summary>
    /// Velocity of projectile after being parried on X axis
    /// </summary>
    public float MultipliedVelocityX { get; set; }

    /// <summary>
    /// Velocity of projectile after being parried on Y axis
    /// </summary>
    public float MultipliedVelocityY { get; set; }

    /// <summary>
    /// Used for Glacial Overlord. Set that the crystal should begin to fall and rotate if true
    /// </summary>
    public bool ShouldRotate { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        rigidBody2DParent = GetComponentInParent<Rigidbody2D>();
        transformParent = GetComponentInParent<Transform>();

        parried = false;
        remainAtShield = false;
        onTheGround = false;

        ShouldRotate = false;

        addPositionNumber = 0;
    }

    // Update is called once per frame
    void Update()
    {
        transformParent = GetComponentInParent<Transform>();

        if (transform.parent.name.Contains("Yellow"))
        {
            StarTurnWithPaladin();
            return;
        }

        if (transform.parent.name.Contains("FlyingIce"))
        {
            IceFallRotate();
        }

    }

    /// <summary>
    /// When paladin turns around, so should the star that is parried and in front of him
    /// </summary>
    private void StarTurnWithPaladin()
    {
        if (parried
            && playerGameObject != null
            && remainAtShield)
        {
            if (playerGameObject.transform.parent.GetComponent<SpriteRenderer>().flipX == true)
            {
                transform.parent.position = new Vector3(
                        playerGameObject.transform.parent.position.x - addPositionNumber,
                        playerGameObject.transform.parent.position.y + addPositionNumber - 1,
                        playerGameObject.transform.parent.position.z);
            }
            else if (playerGameObject.transform.parent.GetComponent<SpriteRenderer>().flipX == false)
            {
                transform.parent.position = new Vector3(
                        playerGameObject.transform.parent.position.x + addPositionNumber,
                        playerGameObject.transform.parent.position.y + addPositionNumber - 1,
                        playerGameObject.transform.parent.position.z);
            }
        }
    }

    /// <summary>
    /// Ice falls and rotates after flying for a short period of time
    /// </summary>
    private void IceFallRotate()
    {
        if (!parried && ShouldRotate)
        {
            rotationTo.rotation = Quaternion.Euler(
            0,
            0,
            leftSpan ? 220 : 180);

            transform.parent.rotation = Quaternion.Slerp(
               transform.parent.rotation,
               rotationTo.rotation,
               timeCount / 80);

            rigidBody2DParent.velocity = new Vector2(rigidBody2DParent.velocity.x, -timeCount * 6);
            timeCount += Time.deltaTime;
        }
    }

    /// <summary>
    /// If projectile hits shield, it will not hurt paladin. In that case, it deactivates hitbox,
    /// where hitbox is actually a collider from projectile's main gameobject and adds velocity 
    /// to projectile, resulting in going to opposite direction from initial direction
    /// </summary>
    /// <param name="collision">Collider from another gameobject, usually shield or sword</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Need to refactor immediately
        if (transform.parent.name.Contains("FlyingIce"))
        {
            CheckIceCrystalParry(collision);
            IceTouchGround(collision);
        }
        else 
        {
            CheckNormalProjectileParry(collision);
        }

        LaunchStar(collision);
    }

    /// <summary>
    /// Swing sword at parried object to launch it (star from Sidus Istar)
    /// </summary>
    /// <param name="collision">Collider from another gameobject, needs to be a sword</param>
    private void LaunchStar(Collider2D collision)
    {
        if (collision.gameObject.layer == 16
            && parried
            && remainAtShield
            && transform.parent.name.Contains("Yellow"))
        {
            remainAtShield = false;
            SingletonSFX.Instance.PlaySFX("SFX49_launch_star");
            transform.gameObject.layer = 19;

            if (playerGameObject.transform.parent.position.x <= transform.parent.position.x)
            {
                rigidBody2DParent.velocity = new Vector2(100f, 0f);
            }
            else
            {
                rigidBody2DParent.velocity = new Vector2(-100f, 0f);
            }
        }
    }

    /// <summary>
    /// Check if flying crystal ice and shield's rotation are alligned well for parry.
    /// IMPORTANT: This method is similar to the previous one, you must refactor it sooner than later.
    /// </summary>
    /// <param name="collision">Collider from another gameobject - usually shield</param>
    private void CheckIceCrystalParry(Collider2D collision)
    {
        if (((collision.transform.rotation.eulerAngles.y == 180
                    && leftSpan)
                || (collision.transform.rotation.eulerAngles.y == 0
                    && !leftSpan))
            && collision.gameObject.layer == 17
            && parried == false)
            {
                parried = true;
                transform.gameObject.tag = "ParriedObject";
                transform.gameObject.layer = 19;
                hitBoxGameobject.transform.GetComponent<PolygonCollider2D>().enabled = false;

                Instantiate(
                    particleEffect,
                    new Vector2(
                        transformParent.position.x,
                        transformParent.position.y),
                    Quaternion.identity);

            Instantiate(particleEffect, transformParent);
                SingletonSFX.Instance.PlaySFX("SFX30_shield_parry_tink");
                Destroy(transform.parent.gameObject, 0.05f);
        }
    }


    /// If projectile hits shield, it will not hurt paladin. In that case, it deactivates hitbox,
    /// where hitbox is actually a collider from projectile's main gameobject and adds velocity 
    /// to projectile, resulting in going to opposite direction from initial direction
    /// </summary>
    /// <param name="collision">Collider from another gameobject, usually shield or sword</param>
    private void CheckNormalProjectileParry(Collider2D collision)
    {
        if (((collision.transform.rotation.eulerAngles.y == 180
                && transformParent.rotation.eulerAngles.y == 180)
                || (collision.transform.rotation.eulerAngles.y == 0
                && transformParent.rotation.eulerAngles.y == 0))
            && collision.gameObject.layer == 17
            && parried == false)
        {
            parried = true;
            transform.gameObject.tag = "ParriedObject";
            transform.gameObject.layer = 19;
            hitBoxGameobject.transform.GetComponent<PolygonCollider2D>().enabled = false;

            if (transform.parent.name.Contains("Yellow"))
            {
                ChangeStarSettings(collision);
            }
            else if (transform.parent.name.Contains("IceCrystalTurquoise"))
            {
                ChangeIceCrystalSettings();
            }
            else
            {
                ParryOrdinaryFlyingProjectile();
            }
        }
    }

    /// <summary>
    /// Parrying yellow flying star.
    /// Changing it's settings and values.
    /// </summary>
    /// <param name="collision">Collision from shield</param>
    private void ChangeStarSettings(Collider2D collision)
    {
        SingletonSFX.Instance.PlaySFX("SFX48_parried_star");

        transform.gameObject.tag = "ParriedObject";
        transform.gameObject.layer = 0;

        rigidBody2DParent.velocity = new Vector2(0f, 0f);
        playerGameObject = collision.gameObject;
        addPositionNumber = 4;
        remainAtShield = true;
    }

    /// <summary>
    /// Parrying ice crystal projectile.
    /// Changing it's settings and values.
    /// </summary>
    private void ChangeIceCrystalSettings()
    {
        SingletonSFX.Instance.PlaySFX("SFX30_shield_parry_tink");

        rigidBody2DParent.velocity = new Vector2(
            rigidBody2DParent.velocity.x * MultipliedVelocityX,
            MultipliedVelocityY);

        transform.parent.transform.rotation = Quaternion.Euler(
            transform.parent.transform.rotation.x,
            transform.parent.transform.rotation.y,
            transform.parent.transform.rotation.y == 0 ? -30 : -22);
    }

    /// <summary>
    /// Parrying ordinary flying projectile.
    /// Changing it's settings and values.
    /// </summary>
    private void ParryOrdinaryFlyingProjectile()
    {
        SingletonSFX.Instance.PlaySFX("SFX30_shield_parry_tink");

        rigidBody2DParent.velocity = new Vector2(
            rigidBody2DParent.velocity.x * MultipliedVelocityX,
            MultipliedVelocityY);

        transform.parent.transform.rotation = Quaternion.Euler(
            transform.parent.transform.rotation.x,
            transform.parent.transform.rotation.y == 0 ? 180 : 0,
            -105f);
    }

    /// <summary>
    /// When flying ice crystals thrusts the ground, it will remain there for a certain amount of time
    /// </summary>
    /// <param name="collision">Ground</param>
    private void IceTouchGround(Collider2D collision)
    {
        if (collision.gameObject.layer == 6 && !onTheGround)
        {
            // See if onTheGround is necessary
            onTheGround = true;
            ShouldRotate = false;
            GetComponent<PolygonCollider2D>().enabled = false;

            rigidBody2DParent.velocity = Vector2.zero;
            SingletonSFX.Instance.PlaySFX("SFX56_ice_crystal_grounded");

            int randomDestroyTime = Random.Range(2, 4);
            Destroy(transform.parent.gameObject, randomDestroyTime);
        }
    }

    /// <summary>
    /// Time that needs to pass in order for crystal to start falling and rotating
    /// </summary>
    /// <param name="time">Time to pass</param>
    /// <returns></returns>
    public IEnumerator TimeBeforeFalling(float time, bool leftSpawn)
    {
        leftSpan = leftSpawn;
        yield return new WaitForSecondsRealtime(time);

        ShouldRotate = true;
    }
}
