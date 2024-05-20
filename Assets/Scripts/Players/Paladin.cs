using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Cinemachine;

public class Paladin : MonoBehaviour {

    [SerializeField] public Transform m_positionAfterAbyss;
    [SerializeField] public int m_currentNoLives;

    [SerializeField] GameObject m_playerTwo;
    [SerializeField] GameObject m_slideDust;
    [SerializeField] GameObject m_snowflake;
    [SerializeField] GameObject m_playerSpawnEffect;
    [SerializeField] GameObject m_invincibilityStars;
    [SerializeField] float      m_speed;
    [SerializeField] float      m_jumpForce;
    [SerializeField] float      m_rollForce;
    [SerializeField] float      m_wallSlidingSpeed;
    [SerializeField] float      m_immortalTime;
    [SerializeField] bool       m_noBlood = false;

    public  Rigidbody2D         m_body2d;
    private Animator            m_animator;
    private SensorHeroKnight    m_groundSensor;
    private SensorHeroKnight    m_wallSensorR1;
    private SensorHeroKnight    m_wallSensorR2;
    private SensorHeroKnight    m_wallSensorL1;
    private SensorHeroKnight    m_wallSensorL2;

    public bool                 m_rolling = false;
    public bool                 m_grounded = false;
    private bool                m_isWallSliding = false;
    private bool                m_isBlocking = false;
    private int                 m_facingDirection = 1;
    private int                 m_currentAttack = 0;
    private int                 m_noJumps = 1;
    private float               m_timeSinceAttack = 0.0f;
    private float               m_delayToIdle = 0.0f;
    private float               m_rollDuration = 8.0f / 14.0f;
    private float               m_rollCurrentTime;

    public Image[] arrayHP;

    public StaminaUI staminaUI;
    public int maxStamina = 10;
    public int currentStamina;

    public float staminaTime;
    private float currentStaminaTime = 0f;
    public bool immortal;

    public PhysicsMaterial2D material2D;
    public PhysicsMaterial2D material2DNoFriction;

    public float wallForceX;
    public float wallForceY;
    public float buddyForceX;
    public float buddyForceY;
    public float wallTime;

    Vector2 moveInput;

    private float movementDirection = 1;
    private float jumpForceHelp;
    private float rollForceHelp;

    private int noShieldUsage = 0;
    private int noPlayers;

    private bool attacking = false;
    private bool isWallJumping = false;
    private bool staminaMethodCalled = false;
    private bool alwaysJump = false;
    private bool dead = false;
    private bool frozen = false;
    private bool fallenInAbyss = false;

    private string playerNumber = "1";
    private string sceneToEnter = "";

    private readonly int unfreezeNumber = 14;
    private int currentUnfreezeNumber = 0;

    // Method is called when the script is first loaded. Called before Start()
    private void Awake()
    {
        noPlayers = PlayerPrefs.GetInt("noPlayers"); 

        if (GameObject.Find("PlayerKnight1") == null)
        {
            gameObject.name = "PlayerKnight1";

            if (noPlayers == 2)
            {
                m_playerTwo.SetActive(true);
            }

            if (!SceneManager.GetActiveScene().name.Equals("SidusIstar") 
                && !SceneManager.GetActiveScene().name.Equals("GlacialOverlord")) 
            {
                GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>().Follow = transform;
            }
        }
        else
        {
            gameObject.name = "PlayerKnight2";
            playerNumber = "2";

            CameraScreenSize();
        }

        //Not the most efficient code, it is done with dragging UI Gameobject and with SerializeField 
        Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => 
        g.name == $"StaminaBarP{playerNumber}").SetActive(true);

        Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g =>
        g.name == $"Hearts_P{playerNumber}").SetActive(true);

        staminaUI = GameObject.Find($"StaminaBarP{playerNumber}").GetComponent<StaminaUI>();
        staminaUI.SetPlayerGameObject(gameObject);

        arrayHP = new Image[] 
        { 
            GameObject.Find("Heart_P" + playerNumber + "_1").GetComponent<Image>(),
            GameObject.Find("Heart_P" + playerNumber + "_2").GetComponent<Image>(),
            GameObject.Find("Heart_P" + playerNumber + "_3").GetComponent<Image>(),
            GameObject.Find("Heart_P" + playerNumber + "_4").GetComponent<Image>(),
            GameObject.Find("Heart_P" + playerNumber + "_5").GetComponent<Image>()
        };

        jumpForceHelp = m_jumpForce;
        rollForceHelp = m_rollForce;

        immortal = true;
        Invoke(nameof(InvincibilityTime), m_immortalTime);
    }

    /// <summary>
    /// Check based on scene if there is a need to make camera screen bigger if multiplayer
    /// </summary>
    private void CameraScreenSize()
    {
        var activeScene = SceneManager.GetActiveScene();
        var sceneName = activeScene.name;

        switch (sceneName)
        {
            case "FernBehemothArena":
                GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize += 8;
                break;
            case "ChainedUndead":
                GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize += 10;
                break;
        }
    }

    // Method for getting input from various devices and Player Input component - Jump
    public void JumpInput(InputAction.CallbackContext ctx) 
    {
        if (ctx.performed) 
        {
            try
            {
                JumpAction();
            }
            catch (NullReferenceException Ex)
            {
                Debug.Log(Ex.ToString());
            }
        }
    }

    // Method for getting input from various devices and Player Input component - Shield
    public void ShieldInput(InputAction.CallbackContext ctx) 
    {
        if (ctx.performed)
        {
            BlockingAction();
        }
        else if (ctx.canceled)
        {
            UnblockingAction();
        }
    }

    // Method for getting input from various devices and Player Input component - Roll
    public void RollInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            RollingAction();
        }
    }

    // Method for getting input from various devices and Player Input component - Attack
    public void AttackInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            AttackAction();
        }
    }

    // Method for getting input from various devices and Player Input component - Movement
    public void MovementInput(InputAction.CallbackContext ctx) 
    {
        if (ctx.performed)
        {
            moveInput = ctx.ReadValue<Vector2>() * movementDirection;
        }
        else if (ctx.canceled)
        {
            moveInput = Vector2.zero;
        }
    }

    // Method for getting input from various devices and Player Input component - Do actions (enter arena)
    public void ActionInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (!sceneToEnter.Equals("") 
                && gameObject.name.Equals("PlayerKnight1"))
            {
                SingletonSFX.Instance.PlaySFX("SFX66_open_door");
                GameObject.Find("BlackScreen").GetComponent<Animator>().SetTrigger("Appear");

                StartCoroutine(LowerMusicVolume());
                StartCoroutine(LoadScene(sceneToEnter));
            }
        }
    }

    // Method for getting input from various devices and Player Input component - Quit scene
    public void QuitInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            GameObject.Find("BlackScreen").GetComponent<Animator>().SetTrigger("Appear");

            string sceneToLoad = "HubWorld";
            var sceneManager = SceneManager.GetActiveScene();

            if (sceneManager.name.Equals("HubWorld"))
            {
                sceneToLoad = "TitleScreen";
            }

            StartCoroutine(LowerMusicVolume());
            StartCoroutine(LoadScene(sceneToLoad));
        }     
    }

    // Method for getting input from various devices and Player Input component - Retry
    public void RetryInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            var sceneManager = SceneManager.GetActiveScene();

            if (!sceneManager.name.Equals("HubWorld"))
            {
                GameObject.Find("BlackScreen").GetComponent<Animator>().SetTrigger("Appear");

                StartCoroutine(LowerMusicVolume());
                StartCoroutine(LoadScene(sceneManager.name));
            }
        }
    }

    // Method for getting inputs from jumping, rolling and attacking in order to unfreeze
    public void UnfreezeInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed
            && frozen)
        {
            float moveToUnfreeze = (currentUnfreezeNumber % 2 == 0) ? -0.3f : 0.3f;
            transform.position = new Vector2(
                transform.position.x + moveToUnfreeze, 
                transform.position.y);

            currentUnfreezeNumber++;
            if (currentUnfreezeNumber >= unfreezeNumber)
            {
                currentUnfreezeNumber = 0;
                EnableWalkJumpActions();
                Freeze(false);
            }
        }
    }

    /// <summary>
    /// Method used to invert player's movement direction. Used in boss battle Psychic Psycho
    /// </summary>
    public void InvertMovementDirection() 
    {
        movementDirection *= -1;
    }

    /// <summary>
    /// Enabling actions if knight is not dead
    /// </summary>
    public void OnEnable()
    {
        if (!dead)
        {
            GetComponent<PlayerInput>().ActivateInput();
        }
        if (!frozen)
        {
            GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        }
    }

    /// <summary>
    /// Disabling actions
    /// <summary>
    public void OnDisable()
    {
        GetComponent<PlayerInput>().DeactivateInput();
    }

    /// <summary>
    /// Enable walking and jumping. Used in Psychic Psycho battle
    /// </summary>
    public void EnableWalkJumpActions()
    {
        movementDirection = 1;
        m_jumpForce = jumpForceHelp;
        m_rollForce = rollForceHelp;
    }

    /// <summary>
    /// Disable walking, jumping and rolling
    /// </summary>
    /// <param name="movementDirection">Movement direction speed</param>
    public void DisableWalkJump(float movementDirection = 0)
    {
        this.movementDirection = movementDirection;
        m_jumpForce = 0;
        m_rollForce = 0;
    }

    // Start is called before the first frame update
    void Start ()
    {
        //SingletonSFX.Instance.playSFX("SFX17_neo-ridley_scream");

        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<SensorHeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensorR1").GetComponent<SensorHeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensorR2").GetComponent<SensorHeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensorL1").GetComponent<SensorHeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensorL2").GetComponent<SensorHeroKnight>();        

        currentStamina = maxStamina;       
    }

    // Update is called once per frame
    void Update()
    {
        CheckStamina();

        m_timeSinceAttack += Time.deltaTime;
        RollingBlockingTime();
        SwapSpriteDirection(moveInput.x);
        CheckIfWallSliding(moveInput.x);
    }

    /// <summary>
    /// Method for checking ground for animation and for adding 1 for total number of jumps.
    /// Also used when player is always jumping because of the effect from Psychic Psycho.
    /// </summary>
    private void CheckGround()
    {
        if (m_body2d.velocity.y > 0.5f)
        {
            m_animator.SetBool("Jumping", true);
        }
        else
        {
            m_animator.SetBool("Jumping", false);
        }

        if (m_grounded)
        {
            m_noJumps = 1;
           
            if (alwaysJump) 
            {
                JumpAction();
            }
        }
    }

    /// <summary>
    /// Method for counting time for stamina to be regenerated.
    /// Starts coroutine for stamina regeneration. 
    /// </summary>
    private void CheckStamina()
    {
        if (!m_isBlocking 
            && currentStaminaTime <= staminaTime)
        {
            currentStaminaTime += Time.deltaTime;
        }

        if (!staminaMethodCalled
            && currentStamina < 10)
        {
            staminaMethodCalled = true;
            StartCoroutine(StaminaRegeneration());
        }
    }

    /// <summary>
    /// Method for stamina regeneration. Starts after staminaTime while stamina is below 10 units
    /// </summary>
    private IEnumerator StaminaRegeneration()
    {
        while (currentStamina < 10)
        {
            yield return new WaitForSeconds(0.5f);

            if (currentStaminaTime > staminaTime)
            {
                currentStamina += 1;
            }
        }

        staminaMethodCalled = false;
    }

    /// <summary>
    /// Called when hopping on another player
    /// </summary>
    public void BuddyHopping()
    {
        if (!dead
            && !isWallJumping
            && !m_grounded)
        {
            MovementForceOnAction(buddyForceX, buddyForceY);
        }
    }

    /// <summary>
    /// Transparent player colors when invincible
    /// </summary>
    private IEnumerator TransparentColourInvincibility()
    {
        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < 20; i++)
        {
            if ((i % 2) == 0)
            {
                GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.3f);
            }
            else
            {
                GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    /// <summary>
    /// Freeze or unfreeze player by disabling inputs and changing color to blue
    /// </summary>
    /// <param name="freeze">True if player is frozen, false if not</param>
    public void Freeze(bool freeze)
    {
        frozen = freeze;
        GetComponent<SpriteRenderer>().color = frozen ? new Color(0.2216981f, 0.4079851f, 1f) : new Color(1f, 1f, 1f);
        m_animator.enabled = !frozen;

        for (int i = 0; i < 5; i++)
        {
            int randomSnowflakePositionX = UnityEngine.Random.Range(-2, 2);
            int randomSnowflakePositionY = UnityEngine.Random.Range(-1, 4);

            GameObject clonedSnowflake = Instantiate(
                m_snowflake,
                new Vector2(
                    transform.position.x + randomSnowflakePositionX,
                    transform.position.y + randomSnowflakePositionY),
                Quaternion.identity);

            Destroy(clonedSnowflake, 1f);
        }

        SingletonSFX.Instance.PlaySFX("SFX58_shooting_ice");
    }

    /// <summary>
    /// Method based on jump movement.
    /// Checks if paladin is allowed to jump.
    /// </summary>
    public void JumpAction()
    {
        // Ordinary jump
        if (!m_isBlocking 
            && m_noJumps > 0 
            && !m_rolling 
            && !m_isWallSliding 
            && Time.timeScale != 0f) 
        {
            SoundAnimationJump();

            m_grounded = false;
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            m_noJumps--;
        }
        // Jump when grounded and near wall
        else if (m_grounded && m_isWallSliding) 
        {
            SoundAnimationJump();

            m_grounded = false;
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);          
        }

        // Jump when wall sliding
        if (m_isWallSliding 
            && !isWallJumping 
            && !m_grounded) 
        {
            MovementForceOnAction();
        }
    }

    /// <summary>
    /// Additional method for setting animation for jumping and playing jump SFX
    /// </summary>
    private void SoundAnimationJump() 
    {
        if (m_jumpForce != 0)
        {
            SingletonSFX.Instance.PlaySFX("SFX16_jump_player");
        }

        m_animator.SetBool("Grounded", m_grounded);
        m_animator.SetTrigger("Jump");
    }

    /// <summary>
    /// Method that applies force to the player after specific action (jumping from wall, jumping on another player)
    /// </summary>
    /// <param name="forceMethodX">Strength of force X</param>
    /// <param name="forceMethodY">Strength of force Y</param>
    private void MovementForceOnAction(float wallForceMethodX = 0, float wallForceMethodY = 0) 
    {
        m_animator.SetBool("WallSlide", false);
        m_animator.SetTrigger("Jump");

        SingletonSFX.Instance.PlaySFX("SFX16_jump_player");

        m_grounded = false;
        m_animator.SetBool("Grounded", m_grounded);

        isWallJumping = true;
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);
        GetComponent<SpriteRenderer>().flipX = !GetComponent<SpriteRenderer>().flipX;

        if (GetComponent<SpriteRenderer>().flipX)
        {
            RotateChildren(180);
        }
        else
        {
            RotateChildren(0);
        }

        // If given wallForceMethodX is empty, it means that paladin must normally jump from the wall,
        // otherwise it means that he is jumping on the shoulder from another player
        if (wallForceMethodX == 0) 
        {
            wallForceMethodX = wallForceX;
            wallForceMethodY = wallForceY;
        }
            
        Invoke(nameof(SetWallJumpingToFalse), wallTime);
        m_body2d.velocity = new Vector2(wallForceMethodX * -m_facingDirection, wallForceMethodY);
    }

    /// <summary>
    /// Method for for counting time for rolling invincibility
    /// </summary>
    private void RollingBlockingTime()
    {
        if (!attacking)
        {
            if (m_rolling) 
            { 
            m_rollCurrentTime += Time.deltaTime;
            }

            if (m_rollCurrentTime > m_rollDuration)
            {
                m_rolling = false;
                m_rollCurrentTime = 0;
            }
        }
    }

    /// <summary>
    /// Method which starts a blocking (shielding) action
    /// </summary>
    private void BlockingAction()
    {
        // Block - shield
        if (!m_rolling 
            && !m_isBlocking 
            && currentStamina >= 3
            && !frozen)
        {
            SingletonSFX.Instance.PlaySFX("SFX12_shield_up");
            currentStaminaTime = 0f;
            currentStamina -= 3;
            m_isBlocking = true;

            transform.GetChild(10).GetComponent<BoxCollider2D>().enabled = true;

            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
            noShieldUsage++;

            StartCoroutine(ShieldingEnumerator(noShieldUsage));
        }
    }

    /// <summary>
    /// Called when player doesn't hold a button anymore
    /// </summary>
    private void UnblockingAction() 
    {
        m_isBlocking = false;
        m_animator.SetBool("IdleBlock", false);
        transform.GetChild(10).GetComponent<BoxCollider2D>().enabled = false;
    }

    /// <summary>
    /// Called when player is rolling
    /// </summary>
    private void RollingAction() 
    {
        if (!m_rolling 
            && !m_isWallSliding 
            && m_grounded 
            && currentStamina >= 4
            && !frozen)
        {
            currentStamina -= 4;
            currentStaminaTime = 0f;
            m_rolling = true;

            SingletonSFX.Instance.PlaySFX("SFX10_heavy_roll");
            m_animator.SetTrigger("Roll");
            m_body2d.velocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.velocity.y);
        }
    }

    /// <summary>
    /// Method which disables shielding if characters shields himself for too long
    /// </summary>
    /// <param name="noShieldInMethod">Used for checking if current click on shielding is from the same method</param>
    /// <returns></returns>
    private IEnumerator ShieldingEnumerator(int noShieldInMethod)
    {
        yield return new WaitForSeconds(staminaTime);

        if (noShieldUsage == noShieldInMethod 
            && m_isBlocking) 
        {
            m_isBlocking = false;
            m_animator.SetBool("IdleBlock", false);
            transform.GetChild(10).GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    /// <summary>
    /// Method for activating attack collider and animation.
    /// There are three attack combos.
    /// Loops back to the first combo after the third combo attack 
    /// or if the measured interval of time since the last attack is too large.
    /// </summary>
    private void AttackAction()
    {      
        if (!m_isBlocking 
            && m_timeSinceAttack > 0.25f 
            && !m_rolling 
            && !m_isWallSliding
            && !frozen)
        {
            m_currentAttack++;

            if (m_currentAttack > 3
                || m_timeSinceAttack > 1.0f)
            {
                m_currentAttack = 1;
            }

            m_animator.SetTrigger("Attack" + m_currentAttack);
            SingletonSFX.Instance.PlaySFX("SFX" + (6 + m_currentAttack).ToString() + "_sword_combo_" + m_currentAttack);

            StartCoroutine(CanAttackAgain(m_currentAttack));

            m_timeSinceAttack = 0.0f;
        }    
    }

    /// <summary>
    /// Method which sets certain 2D polygon collider active for a short period of time based on attack combo
    /// </summary>
    /// <param name="attackNo">Current number from 3 different attacks in a row</param>
    /// <returns></returns>
    private IEnumerator CanAttackAgain(int attackNo)
    {
        attacking = true;
        float timeForDisable = 0.15f;

        if (attackNo == 3)
        {
            timeForDisable = 0.25f;
        }

        GameObject child = transform.GetChild(5 + attackNo).gameObject;
        yield return new WaitForSeconds(0.1f);

        // Can't attack if hurt animation is playing
        if (!m_animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt"))
        {      
            child.SetActive(true);
        }
        yield return new WaitForSeconds(timeForDisable);

        child.SetActive(false);
        attacking = false;
    }

    /// <summary>
    /// Method based on hurt and death animation.
    /// It also does other necessary actions after players death, 
    /// like switching the camera to the other player if he is still alive.
    /// </summary>
    private void HurtDeathAnimation()
    {
        // Death
        if (m_currentNoLives <= 0)
        {
            if (!dead)
            {
                SingletonSFX.Instance.PlaySFX("SFX15_blood_splash");
                SingletonSFX.Instance.PlaySFX("SFX14_player_dead_scream");
                dead = true;

                gameObject.transform.GetChild(11).GetComponent<BoxCollider2D>().transform.gameObject.SetActive(false);
                gameObject.GetComponent<BoxCollider2D>().enabled = false;

                OnDisable();
                ChangeCameraOrScene();
                EnemiesFocusOnAnotherPlayer();

                GameObject.Find($"StaminaBarP{playerNumber}").transform.gameObject.SetActive(false);

                m_animator.SetBool("noBlood", m_noBlood);
                m_animator.SetTrigger("Death");
                m_body2d.velocity = new Vector2(0, 0);
            }
        }
        // Hurt
        else
        {
            SingletonSFX.Instance.PlaySFX("SFX13_player_hurt");
            m_animator.SetTrigger("Hurt");

            if (playerNumber.Equals("2"))
            {
                GameObject clonedInvincibilityStars = Instantiate(
                    m_invincibilityStars,
                    new Vector2(
                        transform.position.x,
                        transform.position.y),
                    Quaternion.identity);

                clonedInvincibilityStars.transform.parent = transform;
                Destroy(clonedInvincibilityStars, m_immortalTime);
            }

            StartCoroutine(TransparentColourInvincibility());
        }
    }

    /// <summary>
    /// Method for changing the camrea to other player if there are more than two players.
    /// Method also checks if both players don't have any more lives, for which the black screen
    /// appears and the game over scene loads
    /// </summary>
    private void ChangeCameraOrScene()
    {
        if (noPlayers == 2)
        {
            var otherPlayerHealth = m_playerTwo.GetComponent<Paladin>().m_currentNoLives;

            if (otherPlayerHealth > 0)
            {
                EnemiesFocusOnAnotherPlayer();

                if (name.Equals("PlayerKnight1")
                    && !SceneManager.GetActiveScene().name.Equals("SidusIstar")
                    && !SceneManager.GetActiveScene().name.Equals("GlacialOverlord"))
                {
                    GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>().Follow = m_playerTwo.transform;
                }
            }
            else
            {
                GameObject.Find("BlackScreen").GetComponent<Animator>().SetTrigger("Appear");

                StartCoroutine(LowerMusicVolume());
                StartCoroutine(LoadScene("GameOver"));
            }
        }
        else
        {
            GameObject.Find("BlackScreen").GetComponent<Animator>().SetTrigger("Appear");

            StartCoroutine(LowerMusicVolume());
            StartCoroutine(LoadScene("GameOver"));
        }
    }

    /// <summary>
    /// When this player dies, then tell enemies to focus their attacks on 
    /// another player if he is still alive
    /// </summary>
    private void EnemiesFocusOnAnotherPlayer()
    {
        var otherPlayerHealth = m_playerTwo.GetComponent<Paladin>().m_currentNoLives;

        if (otherPlayerHealth > 0)
        {
            int playerStillAlive;

            if (name.Equals("PlayerKnight1"))
            {
                playerStillAlive = 2;
            }
            else
            {
                playerStillAlive = 1;
            }

            foreach (var boss in FindObjectsOfType<AbstractBoss>())
            {
                try
                {
                    boss.GetComponent<AbstractBoss>().ChangeFocusToAnotherPlayer(playerStillAlive);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            if (SceneManager.GetActiveScene().name.Equals("ChainedUndead"))
            {
                foreach (var enemy in FindObjectsOfType<AbstractEnemy>())
                {
                    try
                    {
                        enemy.GetComponent<AbstractEnemy>().ChangeFocusToAnotherPlayer(playerStillAlive);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }
    }

    /// <summary>
    /// FixedUpdate - useful when dealing with physics
    /// </summary>
    private void FixedUpdate()
    {
        float inputX = moveInput.x;

        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);

        CheckGround();

        PutMaterial();
        LandingFallingCheck();
        
        PaladinMovement(inputX);
        RunIdleAnimation(inputX);
    }

    /// <summary>
    /// Movement based on rigidbody velocity. Also checks if paladin is rolling or blocking
    /// </summary>
    /// <param name="inputX">Input direction from player</param>
    private void PaladinMovement(float inputX)
    {
        // Move
        if (!m_rolling 
            && !m_isBlocking 
            && !isWallJumping)
        {
            m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);
        }

        // If blocking, then paladin can't move
        if (m_isBlocking)
        {
            m_body2d.velocity = new Vector2(0, m_body2d.velocity.y);
        }
    }

    /// <summary>
    /// Set m_isWallSliding to true if both wall sensors on one side detect wall set bool for animation.
    /// Change velocity of paladin if player is holding a button towards wall.
    /// </summary>
    /// <param name="inputX">Input direction from player</param>
    private void CheckIfWallSliding(float inputX)
    {
        if ((m_wallSensorR1.State() 
                && m_wallSensorR2.State() 
                && !gameObject.GetComponent<SpriteRenderer>().flipX) 
            || (m_wallSensorL1.State() 
                && m_wallSensorL2.State() 
                && gameObject.GetComponent<SpriteRenderer>().flipX))
        {
            m_isWallSliding = true;           
            m_animator.SetBool("WallSlide", m_isWallSliding);
            AE_SlideDust();
        }
        else 
        {
            m_isWallSliding = false;
            m_animator.SetBool("WallSlide", m_isWallSliding);
        }

        if ((m_wallSensorR1.State() 
                && m_wallSensorR2.State() 
                && inputX > 0) 
            || m_wallSensorL1.State() 
            && m_wallSensorL2.State() 
            && inputX < 0 
            && !m_grounded) 
        {
            m_noJumps = 0;
            AE_SlideDust();
            m_body2d.velocity = new Vector2(
                m_body2d.velocity.x, 
                Mathf.Clamp(
                    m_body2d.velocity.y, 
                    -m_wallSlidingSpeed, 
                    float.MaxValue));
        }               
    }

    /// <summary>
    /// Set wall jumping to false after certain amount of time
    /// </summary>
    private void SetWallJumpingToFalse() {
        isWallJumping = false;
    }

    /// <summary>
    /// Run and idle animation conditions and settings
    /// </summary>
    /// <param name="inputX">Input direction from player</param>
    private void RunIdleAnimation(float inputX)
    {
        if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }
        else if (!isWallJumping)
        {
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
            {
                m_animator.SetInteger("AnimState", 0);
            }
        }
    }

    /// <summary>
    /// Swap sprite direction depending on walk direction - input.GetAxis() (i.e. inputX)
    /// </summary>
    /// <param name="inputX">Input direction from player</param>
    private void SwapSpriteDirection(float inputX)
    {
        if (inputX > 0 
            && !m_rolling 
            && !isWallJumping)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
            RotateChildren(0);
        }
        else if (inputX < 0 
            && !m_rolling 
            && !isWallJumping)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
            RotateChildren(180);
        }
    }

    /// <summary>
    /// Checking if knight is falling on the ground based on sensor detection of the ground
    /// </summary>
    private void LandingFallingCheck()
    {
        if (!m_grounded 
            && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        if (m_grounded 
            && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }
    }

    /// <summary>
    /// Putting proper material on Rigidbody 2D:
    /// <para>-> material2D used for flour</para>
    /// <para>-> material2DNoFriction used for walls</para>
    /// </summary>
    private void PutMaterial()
    {
        if ((m_wallSensorR1.State() 
            && m_wallSensorR2.State()) 
            || (m_wallSensorL1.State() 
                && m_wallSensorL2.State()) 
            || !m_grounded)
        {
            m_body2d.sharedMaterial = material2DNoFriction;
        }
        else
        {
            m_body2d.sharedMaterial = material2D;
        }
    }

    /// <summary>
    /// Takes a certain amount of health from the player
    /// </summary>
    public void DecreaseHealth() 
    {
        try
        {
            if (frozen)
            {
                currentUnfreezeNumber = 0;
                EnableWalkJumpActions();
                Freeze(false);
            }

            immortal = true;
            arrayHP[m_currentNoLives - 1].enabled = false;
            m_currentNoLives--;
            HurtDeathAnimation();

            Invoke(nameof(InvincibilityTime), m_immortalTime);
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    /// <summary>
    /// Method being invoked for a certain duration, that is, duration of immortality after being hit
    /// </summary>
    private void InvincibilityTime()
    {
        immortal = false;
    }

    /// <summary>
    /// Rotate the child gameobjects in order to face the direction in the same direction as player
    /// </summary>
    /// <param name="number">0 for rotating to the right, 180 for rotating to the left</param>
    private void RotateChildren(int number) {
        transform.GetChild(6).gameObject.transform.rotation = Quaternion.Euler(0, number, 0);
        transform.GetChild(7).gameObject.transform.rotation = Quaternion.Euler(0, number, 0);
        transform.GetChild(8).gameObject.transform.rotation = Quaternion.Euler(0, number, 0);
        transform.GetChild(10).gameObject.transform.rotation = Quaternion.Euler(0, number, 0);
    }

    /// <summary>
    /// Force knight to always jump. Used in Psychic Psycho battle
    /// </summary>
    public void AlwaysJumpMode()
    {
        alwaysJump = !alwaysJump;

        if (alwaysJump) 
        {
            m_jumpForce *= 1.5f;
            return;
        }

        m_jumpForce = jumpForceHelp;
    }

    /// <summary>
    /// Set the name of arena if player is in front of the door of the arena.
    /// If player is falling into the abyss, then call method for needed actions for it.
    /// </summary>
    /// <param name="collision">Door of the arena</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Door"))
        {
            sceneToEnter = collision.gameObject.name;
        }

        if (collision.transform.CompareTag("Abyss")
            && !fallenInAbyss)
        {
            fallenInAbyss = true;
            StartCoroutine(FallingInAbyss());
        }
    }

    /// <summary>
    /// Delete the name of the arena
    /// </summary>
    /// <param name="collision">Door of the arena</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Door"))
        {
            sceneToEnter = "";
        }
    }

    /// <summary>
    /// Disable player's input, set velocity to 0, decrease health if not in hubworld
    /// and spawn back at a specific position
    /// </summary>
    /// <returns></returns>
    private IEnumerator FallingInAbyss()
    {
        OnDisable();
        m_body2d.velocity = Vector2.zero;
        yield return new WaitForSecondsRealtime(1f);

        var sceneManager = SceneManager.GetActiveScene();
        if (!sceneManager.name.Equals("HubWorld"))
        {
            immortal = false; 
            DecreaseHealth();
        }

        yield return new WaitForSecondsRealtime(1f);

        if (m_positionAfterAbyss != null
            && m_currentNoLives > 0)
        {
            transform.position = new Vector2 (m_positionAfterAbyss.position.x, m_positionAfterAbyss.position.y);
            MovementForceOnAction(0.1f, 8f);

            SingletonSFX.Instance.PlaySFX("SFX67_spawn.stars");

            GameObject clonedSpawnEffect = Instantiate(
                m_playerSpawnEffect,
                new Vector2(
                    transform.position.x,
                    transform.position.y),
                Quaternion.identity);

            Destroy(clonedSpawnEffect, 2f);

            OnEnable();
        }

        yield return new WaitForSecondsRealtime(0.5f);
        fallenInAbyss = false;
    }

    /// <summary>
    /// Loads scene after some time passes
    /// </summary>
    /// <param name="sceneToLoad">Scene which will be loaded</param>
    /// <returns></returns>
    private IEnumerator LoadScene(string sceneToLoad)
    {
        yield return new WaitForSecondsRealtime(1.25f);

        SceneManager.LoadScene(sceneToLoad);
    }

    /// <summary>
    /// Lower the general sound of everything in the scene
    /// </summary>
    /// <returns></returns>
    private IEnumerator LowerMusicVolume()
    {
        while (AudioListener.volume > 0)
        {
            AudioListener.volume -= 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    /// <summary>
    /// Animation Events - Called in slide animation
    /// </summary>
    void AE_SlideDust()
    {
        Vector3 spawnPosition;

        if (m_facingDirection == 1)
        {
            spawnPosition = m_wallSensorR2.transform.position;
        }
        else
        {
            spawnPosition = m_wallSensorL2.transform.position;
        }

        if (m_slideDust != null)
        {
            GameObject dust = Instantiate(
                m_slideDust, 
                spawnPosition, 
                gameObject.transform.localRotation) 
            as GameObject;
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }
}
