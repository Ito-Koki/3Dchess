using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip selectSound;  // ‹î‚ð‘I‘ð‚µ‚½Žž
    [SerializeField] private AudioClip placeSound;   // ‹î‚ð’u‚¢‚½Žž

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySelect()
    {
        audioSource.PlayOneShot(selectSound);
    }

    public void PlayPlace()
    {
        audioSource.PlayOneShot(placeSound);
    }
}