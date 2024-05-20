using UnityEngine;

public class SingletonSFX : MonoBehaviour
{
    public static SingletonSFX Instance { get; private set; }

    private AudioClip resourceSFX;
    private AudioSource sourceOfSFX;

    // Called before Start() method
    // Destroy a duplicate instance if another one already exists
    private void Awake()
    {
        if (Instance != null 
            && Instance != this)
        {
            Destroy(this);
        }
        else {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    // Gets audio source component
    private void Start()
    {
        sourceOfSFX = GetComponent<AudioSource>();
        sourceOfSFX.loop = false;
    }

    // Plays one time SFX
    public void PlaySFX(string name)
    {
        sourceOfSFX.pitch = 1f;
        sourceOfSFX.volume = 1f;

        resourceSFX = Resources.Load<AudioClip>(name);
        sourceOfSFX.PlayOneShot(resourceSFX);
    }
}
