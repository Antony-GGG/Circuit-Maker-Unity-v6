using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] AudioSource BGMusic;
    [SerializeField] AudioSource SFX;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip clickSFX;
    public AudioClip connectionSFX;
}
