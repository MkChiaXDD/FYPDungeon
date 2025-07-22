using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

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

    [Header("Player")]
    [SerializeField] private GameObject Player;
    [SerializeField] private float Hearing_Distance;

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

    public void PlaySFX(string clipName , GameObject obj , bool DoesDitanceEffect = true)
    {
        if (sfxDict.ContainsKey(clipName))
        {
            sfxSource.clip = sfxDict[clipName];

            float distance = (Player.transform.position - obj.transform.position).magnitude;

            if (DoesDitanceEffect && distance > Hearing_Distance)
            {
 
                return;
            }

            float volume = DoesDitanceEffect ? Mathf.Clamp01(1 - (distance / Hearing_Distance)) : 1f;

            sfxSource.PlayOneShot(sfxDict[clipName]);
            sfxSource.Play();
            Debug.LogWarning("this is played" + clipName);
        }
        else
        {
            Debug.LogWarning("SFX clip " + clipName + " not found!");
        }
    }
}