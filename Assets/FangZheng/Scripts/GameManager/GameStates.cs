using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamStates : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        Paused
    }

    private readonly float Normal_Time = 1;
    private readonly float PausedTime = 0;
    public GameState State;
    private int AmountPause;
    public static GamStates instance;
    public SoundManager _SoundManager;
    public PlayerMovement PlayerMovement;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (_SoundManager == null)
        {
            _SoundManager = FindFirstObjectByType<SoundManager>();
        }

        if (PlayerMovement == null)
        {
            PlayerMovement = FindAnyObjectByType<PlayerMovement>();
        }
    }

    public void Play()
    {
        State = GameState.Playing;

        Time.timeScale = Normal_Time;


    }

    public void Pause()
    {
        State = GameState.Paused;

        Time.timeScale = PausedTime;

        _SoundManager.PauseBGMSound();
        _SoundManager.PauseSFXSound();
        PlayerMovement.audiosource.enabled = false;
        //_SoundManager.SFXVolume(0);


    }

    public void AddPauseStuff()
    {
        ++AmountPause;

        
    }

    public void RemovePauseStuff()
    {
        --AmountPause;

        if (AmountPause <= 0)
        {
            //Change this with start
            //SoundManager.Instance.StopBGM();
            //SoundManager.Instance.StopSFX();
            _SoundManager.ResumeBGMSound();
            //PlayerMovement.audiosource.enabled = true;
            //_SoundManager.SFXVolume(100);
        }
    }

    private void Update()
    {
        if (AmountPause <= 0)
        {
            Play();
        }
        else
        {
            Pause();
        }
    }
}
