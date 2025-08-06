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
    }

    public void AddPauseStuff()
    {
        ++AmountPause;
    }

    public void RemovePauseStuff()
    {
        --AmountPause;
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
