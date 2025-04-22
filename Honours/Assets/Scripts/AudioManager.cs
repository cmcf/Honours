using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] AudioSource sfxSource;

    [Header("Weapon SFX")]
    public AudioClip shotgunFire;
    public AudioClip pistolFire;
    public AudioClip poisonFire;

    [Header("Enemy SFX")]
    public AudioClip enemyHit;
    public AudioClip playerHit;

    public AudioClip pickup;
    public AudioClip explosion;

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

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }
}
