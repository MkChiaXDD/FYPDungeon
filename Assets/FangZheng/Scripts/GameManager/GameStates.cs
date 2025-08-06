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

    public float Normal_Time;
    public float StopTime;
    public GameState State;

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
        HitStopManager.Instance.ResetHitstop();
    }

    public void Pause()
    {
        State = GameState.Paused;
        HitStopManager.ActivateHitStopGlobal();
    }
}
