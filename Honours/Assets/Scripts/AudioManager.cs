using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource musicSource;

    [Header("Weapon SFX")]
    public AudioClip shotgunFire;
    public AudioClip pistolFire;
    public AudioClip beamFire;
    public AudioClip poisonFire;
    public AudioClip iceFire;

    [Header("Enemy SFX")]
    public AudioClip enemyHit;
    public AudioClip iceHitEffect;
    public AudioClip fireBall;
    public AudioClip fireAppear;
    public AudioClip fireDeath;
    public AudioClip bossSwitch;
    public AudioClip pantherProjectile;

    [Header("Player SFX")]
    public AudioClip playerHit;
    public AudioClip dashSFX;
    public AudioClip switchSFX;
    public AudioClip playerDeath;
    public AudioClip wolfBite;
    public AudioClip knifeSpawn;
    public AudioClip knifeThrow;

    [Header("Music")]
    public AudioClip backgroundMusic;
    public AudioClip bossMusic;

    void Awake()
    {
        // If there is already an instance of AudioManager, destroy this one
        if (Instance != null)
        {
            Destroy(gameObject);  // Destroy the duplicate instance
            return;
        }

        Instance = this;

        PlayMusic(backgroundMusic);
    }

    public void PlaySFX(AudioClip clip, float volume = 0.8f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}
