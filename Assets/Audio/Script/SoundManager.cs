using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private List<AudioClip> musicClips = new List<AudioClip>();

    [Header("SFX Clips")]
    [SerializeField] private List<AudioClip> sfxClips = new List<AudioClip>();

    private Dictionary<string, AudioClip> musicDict;
    private Dictionary<string, AudioClip> sfxDict;

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
            musicSource.clip = musicDict[clipName];
            musicSource.loop = loop;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("Music clip " + clipName + " not found!");
        }
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
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
}