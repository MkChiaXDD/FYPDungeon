using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private List<AudioClip> musicClips = new List<AudioClip>();

    [Header("SFX Clips")]
    [SerializeField] private List<AudioClip> sfxClips = new List<AudioClip>();

    [Header("Player")]
    [SerializeField] private GameObject Player;
    [SerializeField] private float Hearing_Distance;

    private Dictionary<string, AudioClip> musicDict;
    private Dictionary<string, AudioClip> sfxDict;

    [SerializeField] private string starterBGMName;


    private float pauseTime; //store position where pause

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDictionaries();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayMusic(starterBGMName);
    }

    private void InitializeDictionaries()
    {
        musicDict = new Dictionary<string, AudioClip>();
        foreach (AudioClip clip in musicClips)
        {
            if (!musicDict.ContainsKey(clip.name))
            {
                musicDict.Add(clip.name, clip);
            }
        }

        sfxDict = new Dictionary<string, AudioClip>();
        foreach (AudioClip clip in sfxClips)
        {
            if (!sfxDict.ContainsKey(clip.name))
            {
                sfxDict.Add(clip.name, clip);
            }
        }
    }

    public void PlayMusic(string clipName, bool loop = true)
    {
        if (musicDict.ContainsKey(clipName))
        {
            bgmSource.clip = musicDict[clipName];
            bgmSource.loop = loop;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning("Music clip " + clipName + " not found!");
        }
    }

    public void StopMusic()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    public void PlaySFX(string clipName, GameObject obj, bool DoesDistanceEffect = true)
    {
        if (sfxDict.ContainsKey(clipName))
        {
            if (DoesDistanceEffect)
            {
                if (obj == null) return;
                if (Player == null) return;

                float distance = (Player.transform.position - obj.transform.position).magnitude;

                if (distance > Hearing_Distance)
                    return;

                float volume = Mathf.Clamp01(1 - (distance / Hearing_Distance));
                sfxSource.volume = volume;
            }
            else
            {
                sfxSource.volume = 1f;
            }

            sfxSource.PlayOneShot(sfxDict[clipName]);
        }
        else
        {
            Debug.LogWarning("SFX clip " + clipName + " not found!");
        }
    }

    public void PlaySFX(string clipName)
    {
        if (sfxDict.ContainsKey(clipName))
        {
            sfxSource.PlayOneShot(sfxDict[clipName]);
        }
        else
        {
            Debug.LogWarning("SFX clip " + clipName + " not found!");
        }
    }

    public void PlayVariationSFX(string clipName)
    {
        if (!sfxDict.ContainsKey(clipName))
        {
            Debug.LogWarning("SFX clip " + clipName + " not found!");
            return;
        }


        RandomisePitch();
        sfxSource.PlayOneShot(sfxDict[clipName]);
        ResetPitch();
    }


    //Toggle between pause and resume for sfx
    public void TogglePauseSFX()
    {
        if (sfxSource == null) return;

        if (sfxSource.isPlaying)
        {
            PauseSFXSound();
        }
        else
        {
            ResumeSFXSound();
        }
    }

    // Toggle between pause and resume for bgm
    public void TogglePauseBGM()
    {
        if (bgmSource == null) return;

        if (bgmSource.isPlaying)
        {
            PauseBGMSound();
        }
        else
        {
            ResumeBGMSound();
        }
    }

    // Call this to pause the sound
    public void PauseBGMSound()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            pauseTime = bgmSource.time;
            bgmSource.Pause();
        }
    }

    // Call this to resume the sound from where it was paused
    public void ResumeBGMSound()
    {
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.time = pauseTime;
            bgmSource.Play();
        }
    }

    // Call this to pause the sound
    public void PauseSFXSound()
    {
        if (sfxSource != null && sfxSource.isPlaying)
        {
            pauseTime = sfxSource.time;
            sfxSource.Pause();
        }
    }

    // Call this to resume the sound from where it was paused
    public void ResumeSFXSound()
    {
        if (sfxSource != null && !sfxSource.isPlaying)
        {
            sfxSource.time = pauseTime;
            sfxSource.Play();
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void StopSFX()
    {
        sfxSource.Stop();
    }

    public void ToggleMusic()
    {
        bgmSource.mute = !bgmSource.mute;
    }
    public void ToggleSFX()
    {
        sfxSource.mute = !sfxSource.mute;
    }

    public void MusicVolume(float volume)
    {
        bgmSource.volume = volume;
    }
    public void SFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
    public float GetMusicVolume()
    {
        return bgmSource.volume;
    }
    public float GetSFXVolume()
    {
        return sfxSource.volume;
    }

    public void RandomisePitch(float minRange = 0.9f, float maxRange = 1.1f)
    {
        sfxSource.pitch = Random.Range(minRange, maxRange);
    }

    public void ResetPitch()
    {
        sfxSource.pitch = 1;
    }
}