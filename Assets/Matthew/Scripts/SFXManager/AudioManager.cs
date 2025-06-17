using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{

    public static AudioManager Instance;
    public Sound[] bgm, sfx;
    public AudioSource bgmSource, sfxSource;

    private void Awake()
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
    private void Start()
    {
        PlayMusic("NatureBGM");
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(bgm, x => x.name == name);

        if (s == null)
        {
            Debug.Log("Sound not found");
        }
        else
        {          
            bgmSource.clip = s.clip;
            bgmSource.Play();
        }
    }

    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfx, x => x.name == name);
        if(s == null)
        {
            Debug.Log("Sound not found");
        }
        else
        {
            //make each sound more varied
            RandomisePitch();
            sfxSource.PlayOneShot(s.clip);
        }


    }

    public void PlayOneShotSFX(string name)
    {
        Sound s = Array.Find(sfx, x => x.name == name);
        
        if ( s == null)
        {
            Debug.Log("no sound");
        }
        else
        {
            
            //make each sound more varied
            RandomisePitch();
            sfxSource.Play();
        }

    }

    public void StopBGM()
    {
        bgmSource.Stop();
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

    public void RandomisePitch()
    {
        sfxSource.pitch = UnityEngine.Random.Range(1f, 1.5f);
    }
}
