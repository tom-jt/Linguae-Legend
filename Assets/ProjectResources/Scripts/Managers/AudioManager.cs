using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static int currentGamePlaylistIndex = 0;
    public static int currentBossPlaylistIndex = 0;

    [HideInInspector]
    public float newMasterVolume;

    [Header("Assignments")]
    [SerializeField]
    private AudioSource backgroundMusicSource;
    [SerializeField]
    private GameObject sfxPrefab;

    [Header("Cosmetics")]
    public int maxSimultaneousSfxCount = 5;

    [Header("Background Music")]
    public AudioClip[] startMenuPlaylist;
    public AudioClip[] gamePlaylist;
    public AudioClip[] bossPlaylist;
    public AudioClip winGame;
    public AudioClip loseGame;

    [Header("Sound Effects")]
    public AudioClip sceneTransition;
    public AudioClip buttonHover;
    public AudioClip buttonClicked;
    public AudioClip boxSelect;
    public AudioClip damage;
    public AudioClip heal;
    public AudioClip boxReveal;
    public AudioClip boxNotReveal;
    public AudioClip boxDelete;
    public AudioClip popupMsg;
    public AudioClip bannerSlide;
    public AudioClip overlaySlideDown;
    public AudioClip overlaySlideUp;
    public AudioClip lootSlide;
    public AudioClip bossStage;
    public AudioClip lantern;
    public AudioClip map;
    public AudioClip feast;
    public AudioClip sword;
    public AudioClip wand;
    public AudioClip rune;
    public AudioClip teleport;

    private void Awake()
    {
        backgroundMusicSource.ignoreListenerVolume = true;
    }

    public void ChangeMasterVolume(float newValue)
    {
        newMasterVolume = newValue;
        AudioListener.volume = newValue;
    }

    public float GetMasterVolume() => AudioListener.volume;

    public void ChangeBackgroundMusicVolume(float newValue) => backgroundMusicSource.volume = newValue;
    public float GetBackgroundMusicVolume() => backgroundMusicSource.volume;
    
    public void ChangeBackgroundMusic(AudioClip newBgm)
    {
        backgroundMusicSource.clip = newBgm;
        backgroundMusicSource.Play();
    }

    public void CreateSfxInstance(AudioClip sfx)
    {
        if ((sfx) && (transform.childCount < maxSimultaneousSfxCount))
        {
            AudioSource createdSfx = Instantiate(sfxPrefab, transform).GetComponent<AudioSource>();
            createdSfx.clip = sfx;
            createdSfx.Play();
        }
    }
}
