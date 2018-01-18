using UnityEngine;
public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource correctSound;

    public static AudioManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public static void PlayCorrectSound()
    {
        Instance.correctSound.Stop();
        Instance.correctSound.Play();
    }
}
