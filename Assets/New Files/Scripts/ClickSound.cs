using UnityEngine;

public class ClickSound : MonoBehaviour
{
    public AudioSource clickSoundEffect;

    private void Start()
    {
        clickSoundEffect = GetComponent<AudioSource>();

        clickSoundEffect.volume = 1f;
    }
}
