using UnityEngine;

public class ConnectionSound : MonoBehaviour
{
    public AudioSource connectionSoundEffect;

    private void Start()
    {
        connectionSoundEffect = GetComponent<AudioSource>();

        connectionSoundEffect.volume = 1f;
    }
}
