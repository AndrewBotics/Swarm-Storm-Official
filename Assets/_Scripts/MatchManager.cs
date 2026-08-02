using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using FishNet.Transporting;
using System.Collections.Generic;

public class MatchManager : NetworkBehaviour
{
    public static MatchManager Instance;
    
    // Match State constants
    public static readonly int WAITING = 0;
    public static readonly int PLAYING = 1;
    public static readonly int ENDED = 2;

    public int CurrentState = WAITING;
    public float TimeRemaining = 600f; // 10:00 in seconds

    [Header("Character Prefabs")]
    public GameObject neuroPrefab;
    
    [Header("Wild Prefabs")]
    public GameObject streamNling;
    public GameObject projectsNling;
    public GameObject jungleNling;

    [Header("Boss Prefabs")]
    public GameObject sponsorPrefab; // early/mid stream boss
    public GameObject raidPrefab; // early/mid stream boss
    public GameObject integrationPrefab; // early/mid projects boss
    public GameObject musicVideoPrefab; // early/mid projects boss
    public GameObject subathonPrefab; // late jungle boss

    private bool laneBossesSpawned1 = false;
    private bool laneBossesSpawned2 = false;
    private bool jungleBossSpawned = false;
    private readonly float laneBossesTime1 = 450f; // Spawned 2:30 into the game
    private readonly float laneBossesTime2 = 300f; // Spawned 5:00 into the game
    private readonly float jungleBossTime = 150f; // Spawned 7:30 into the game
}
