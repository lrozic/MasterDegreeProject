using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChainedUndead : AbstractBoss
{
    [SerializeField] List<GameObject> remainingChainsList;
    [SerializeField] GameObject floatingIslands;
    [SerializeField] GameObject skeletonLeft;
    [SerializeField] GameObject skeletonRight;
    [SerializeField] GameObject fireDestroyedBodyPart;
    [SerializeField] GameObject deathFirePosition;
    [SerializeField] GameObject sceneManager;
    [SerializeField] GameObject head;
    [SerializeField] GameObject torso;
    [SerializeField] Transform darkMatterSpawnPosition;

    private readonly Dictionary<string,bool> bodyPartsDictionary = new();

    // Start is called before the first frame update
    private void Start()
    {
        PlayerPrefs.SetString("sceneName", SceneManager.GetActiveScene().name);
        StartCoroutine(IncreaseMusicVolume());

        rigidBody2d = GetComponent<Rigidbody2D>();
        heroes = new List<GameObject>();
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    public override void MinusHealth(int layer)
    {       
    }

    /// <summary>
    /// Adding name of the body part to the list.
    /// False means that the body part is not destroyed.
    /// </summary>
    /// <param name="bodyPart">Name of body part</param>
    public void AddBodyPartToDictionary(string bodyPart) 
    {
        bodyPartsDictionary.Add(bodyPart, false);
    }

    /// <summary>
    /// Changes status of body part to true in dictionary, which means it is destroyed
    /// </summary>
    /// <param name="bodyPart">Name of the body part</param>
    public void ChangeBodyPartStatusInDictionary(string bodyPart) 
    {
        bodyPartsDictionary[bodyPart] = true;
        CheckIfInvincibilityStatusNeedsChange(bodyPart);
    }

    /// <summary>
    /// Checks which body part needs to change from invincible true to false.
    /// Depends on given param.
    /// </summary>
    /// <param name="bodyPart">Name of the body part</param>
    private void CheckIfInvincibilityStatusNeedsChange(string bodyPart) 
    {
        switch (bodyPart) 
        {
            case "LowerRightLeg":
                ChangeBodyPartInvincibility("UpperRightLeg");
                break;
            case "LowerLeftLeg":
                ChangeBodyPartInvincibility("UpperLeftLeg");
                break;
            case "LowerLeftArm":
                ChangeBodyPartInvincibility("UpperLeftArm");
                break;
            case "LowerRightArm":
                ChangeBodyPartInvincibility("UpperRightArm");
                break;
            case "Stomach":
                ChangeBodyPartInvincibility("Heart");
                break;
            case "Heart":
                Death(true);
                break;
        }   

        // If every other body part is destroyed except heart, change the invincibility
        // of the stomach to true
        if (bodyPartsDictionary.Where(x => x.Value == true).Count() == 8)
        {
            ChangeBodyPartInvincibility("Stomach");
        }

        // Spawn floating islands after the legs are destroyed
        if (bodyPartsDictionary.Where(x => x.Value == true).Count() == 2)
        {
            SingletonSFX.Instance.PlaySFX("SFX26_teleport");
            floatingIslands.SetActive(true);
        }
    }

    /// <summary>
    /// Change the body part invincibility from true to false
    /// </summary>
    /// <param name="bodyPartToChange">Body part that no longer needs to be invincible</param>
    private void ChangeBodyPartInvincibility(string bodyPartToChange) 
    {
        GameObject.Find(bodyPartToChange).GetComponent<BodyPart>().ChangeInvincibilityStatus();
    }

    /// <summary>
    /// Called when player comes near Chained Undead. 
    /// Summon skeletons.
    /// Overrides method from AbstractBoss class.
    /// </summary>
    public override void StartAttacking()
    {
        timerCountDown.GetComponent<TimeCountDown>().countTime = true;
        SingletonSFX.Instance.PlaySFX("SFX45_chained_undead_start_roar");

        if (GameObject.Find("PlayerKnight1") != null)
        {
            heroes.Add(GameObject.Find("PlayerKnight1"));
        }

        if (GameObject.Find("PlayerKnight2") != null)
        {
            heroes.Add(GameObject.Find("PlayerKnight2"));
            GameObject.Find("RespawnController").GetComponent<ConcreteSubject>().DecreaseRespawnTime();
        }

        skeletonLeft.SetActive(true);
        skeletonRight.SetActive(true);
    }

    /// <summary>
    /// Calls a virtual method from the abstract class and then uses specialized code for death.
    /// </summary>
    /// <param name="saveToDb">True if time and player name should be saved in database, false if not</param>
    protected override void Death(bool saveToDb)
    {
        base.Death(saveToDb);

        head.GetComponent<Animator>().SetTrigger("Defeated");
        head.GetComponent<PolygonCollider2D>().enabled = false;
        torso.GetComponent<PolygonCollider2D>().enabled = false;
        GameObject.Find("RespawnController").GetComponent<ConcreteSubject>().IncreaseRespawnTime();

        foreach (var enemy in FindObjectsOfType<AbstractEnemy>())
        {
            try
            {
                enemy.destroyed = true;
                enemy.Death();
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        Destroy(skeletonLeft, 2f);
        Destroy(skeletonRight, 2f);

        SingletonSFX.Instance.PlaySFX("SFX46_chained_undead_death_roar");

        SingletonSFX.Instance.PlaySFX("SFX63_boss_big_damage");
        Instantiate(darkMatterEffects, darkMatterSpawnPosition);

        StartCoroutine(PlayDeathFireAnimation());
        StartCoroutine(RemoveRemainingChains());

        sceneManager.GetComponent<SceneLoader>().LoadSceneCoroutine(12f, "HubWorld");

        Destroy(gameObject, 10f);
    }

    /// <summary>
    /// Instantiates fire around destroyed Chained Undead's head and torso.
    /// It also plays certain SFX.
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayDeathFireAnimation()
    {
        int numberOfFireDestroyed = 50;
        yield return new WaitForSecondsRealtime(4f);

        for (int i = 0; i < numberOfFireDestroyed; i++)
        {
            int randomPlacementX = (int)UnityEngine.Random.Range(-10, 12);
            int randomPlacementY = (int)UnityEngine.Random.Range(-10, 20);

            var clonedFireDestroyedBodyPart = Instantiate(fireDestroyedBodyPart, 
                new Vector3(
                    deathFirePosition.transform.position.x + randomPlacementX,
                    deathFirePosition.transform.position.y + randomPlacementY,
                    deathFirePosition.transform.position.z),
                    Quaternion.identity);

            Destroy(clonedFireDestroyedBodyPart, 0.4f);

            SingletonSFX.Instance.PlaySFX("SFX37_body_part_destroyed");
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    /// <summary>
    /// Removes chains that are left attached to head and torso
    /// </summary>
    /// <returns></returns>
    private IEnumerator RemoveRemainingChains() 
    {
        yield return new WaitForSecondsRealtime(6.5f);
        SingletonSFX.Instance.PlaySFX("SFX44_steel_chain_rattle");

        foreach(var chain in remainingChainsList)
        {
            float chainVelocityX = chain.transform.rotation.eulerAngles.z / 10;
            if (chain.transform.rotation.eulerAngles.z > 90)
            {
                chainVelocityX *= -1;
            }
            chain.GetComponent<Rigidbody2D>().velocity = new Vector2(chainVelocityX, 25f);
        }
        GetComponent<Rigidbody2D>().gravityScale = 3f;
    }
}
