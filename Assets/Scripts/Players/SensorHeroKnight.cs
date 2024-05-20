using UnityEngine;

public class SensorHeroKnight : MonoBehaviour {

    private float m_DisableTimer;
    private int m_ColCount = 0; 

    /// <summary>
    /// When enabled, call this method
    /// </summary>
    private void OnEnable()
    {
        m_ColCount = 0;
    }

    /// <summary>
    /// Get state from the sensor.
    /// NOTE: Currently, m_DisableTimer is not used anywhere in the project
    /// </summary>
    /// <returns>True if sensor is detecting ground</returns>
    public bool State()
    {
        if (m_DisableTimer > 0)
        {
            return false;
        }
        return m_ColCount > 0;
    }

    /// <summary>
    /// Add number by one if sensor detects collider with tag Ground
    /// </summary>
    /// <param name="other">Collider from another gameobject</param>
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground")) 
        { 
            m_ColCount++;
        }
    }

    /// <summary>
    /// Decrease number by one if sensor detects collider with tag Ground
    /// </summary>
    /// <param name="other">Collider from another gameobject</param>
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            m_ColCount--;
        }
    }

    // Update is called once per frame
    void Update()
    {
        m_DisableTimer -= Time.deltaTime;
    }

    /// <summary>
    /// When disabled, call this method
    /// </summary>
    /// <param name="duration">Duration of being disabled</param>
    public void Disable(float duration)
    {
        m_DisableTimer = duration;
    }

    /// <summary>
    /// If paladin was standing on icicle while icicle was destroyed, decrease m_ColCount
    /// </summary>
    /// <param name="shouldCallAgain">True if method should call itself for double check for m_colCount, false if not</param>
    public void NotStayingOnIcicle(bool shouldCallAgain = false)
    {
        m_ColCount = 0;

        if (shouldCallAgain)
        {
            Invoke(nameof(NotStayingOnIcicle), 0.4f);
        }
    }
}
