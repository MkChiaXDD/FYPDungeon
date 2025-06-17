using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonerMiniController : Enemy
{
    public enum State { Idle, Summon, RunAround }
    private State currentState;

    [Header("References")]
    public Transform player;
    [SerializeField] ParticleSystem SummonVFX;

    [Header("Summon Settings")]
    public List<GameObject> summonEnemyPrefabs;
    public int amountToSummon = 3;
    public float summonRadius = 5f;
    public float summonCooldown = 5f;
    private float summonTimer;

    [Header("Detection")]
    public float detectionRange = 10f;

    [Header("RunAround Settings")]
    public float runDuration = 2f;
    public float moveSpeed = 3f;
    public float changeDirectionInterval = 1f;
    private float runTimer;
    private float directionTimer;
    private Vector3 moveDirection;

    protected override void Awake()
    {
        base.Awake();
        currentState = State.Idle;
        summonTimer = summonCooldown;

        if (player == null && GameObject.FindGameObjectWithTag("Player"))
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                HandleIdle();
                break;

            case State.Summon:
                HandleSummon();
                break;

            case State.RunAround:
                HandleRunAround();
                break;
        }
    }

    void HandleIdle()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= detectionRange)
        {
            currentState = State.Summon;
        }
    }

    void HandleSummon()
    {
        summonTimer -= Time.deltaTime;
        if (summonTimer <= 0f)
        {
            SummonEnemies();
            summonTimer = summonCooldown;
            runTimer = runDuration;
            directionTimer = 0f;
            PickNewDirection();
            currentState = State.RunAround;
        }
        PlaySummonVFX();
    }

    void HandleRunAround()
    {
        runTimer -= Time.deltaTime;
        directionTimer -= Time.deltaTime;

        // Change direction occasionally
        if (directionTimer <= 0f)
        {
            PickNewDirection();
            directionTimer = changeDirectionInterval;
        }

        // Move in direction
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if (runTimer <= 0f)
        {
            currentState = State.Summon;
        }
    }

    void PickNewDirection()
    {
        Vector2 random2D = Random.insideUnitCircle.normalized;
        moveDirection = new Vector3(random2D.x, 0f, random2D.y);
    }

    void SummonEnemies()
    {
        for (int i = 0; i < amountToSummon; i++)
        {
            if (summonEnemyPrefabs.Count == 0) return;

            GameObject prefab = summonEnemyPrefabs[Random.Range(0, summonEnemyPrefabs.Count)];
            Vector2 offset = Random.insideUnitCircle * summonRadius;
            Vector3 spawnPos = transform.position + new Vector3(offset.x, 0, offset.y);

            Instantiate(prefab, spawnPos, Quaternion.identity);
        }

        Debug.Log($"[Summoner] Spawned {amountToSummon} enemies.");
    }

    private void PlaySummonVFX()
    {
        SummonVFX.Play();      
    }
}
