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

    [System.NonSerialized] protected readonly SyncVar<int> CurrentState = new SyncVar<int>();
    [System.NonSerialized] protected readonly SyncVar<float> TimeRemaining = new SyncVar<float>();

    [Header("Character Prefabs")]
    public GameObject neuroPrefab;
    
    [Header("Wild Prefabs")]
    public GameObject streamNling;
    public GameObject projectsNling;
    public GameObject jungleNling;

    [Header("Boss Prefabs")]
    public GameObject sponsorPrefab; 
    public GameObject raidPrefab; 
    public GameObject integrationPrefab; 
    public GameObject musicVideoPrefab; 
    public GameObject subathonPrefab; 

    // Game progression states
    private List<GameObject> randomizedStreamBosses;
    private List<GameObject> randomizedProjectBosses;
    private bool laneBossesSpawned1 = false;
    private bool laneBossesSpawned2 = false;
    private bool jungleBossSpawned = false;
    private readonly float laneBossesTime1 = 450f; 
    private readonly float laneBossesTime2 = 300f; 
    private readonly float jungleBossTime = 150f; 

    // Game locations
    Vector3 t1TopSpawn = new Vector3(30f, 0f, -6.25f);
    Vector3 t1JunSpawn = new Vector3(33.75f, 0f, 0f);
    Vector3 t1BotSpawn = new Vector3(30f, 0f, 6.25f);
    Vector3 t2TopSpawn = new Vector3(-30f, 0f, -6.25f);
    Vector3 t2JunSpawn = new Vector3(-33.75f, 0f, 0f);
    Vector3 t2BotSpawn = new Vector3(-30f, 0f, 6.25f);
    Quaternion topQuaternion = Quaternion.Euler(0f, 180f, 0f);
    Quaternion junQuaternion = Quaternion.Euler(0f, -90f, 0f);
    Quaternion botQuaternion = Quaternion.Euler(0f, 0f, 0f);

    private Queue<NetworkObject> unassignedCharacters = new Queue<NetworkObject>();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        CurrentState.Value = WAITING;
        TimeRemaining.Value = 600f;
        
        base.TimeManager.OnTick += TimeManager_OnTick;
        base.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;

        randomizedStreamBosses = new List<GameObject>{sponsorPrefab, raidPrefab};
        randomizedProjectBosses = new List<GameObject>{integrationPrefab, musicVideoPrefab};
        
        ShuffleList(randomizedStreamBosses);
        ShuffleList(randomizedProjectBosses);

        InitializeMatch(neuroPrefab, neuroPrefab, neuroPrefab, neuroPrefab, neuroPrefab, neuroPrefab);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (base.TimeManager != null)
        {
            base.TimeManager.OnTick -= TimeManager_OnTick;
        }
        if (base.ServerManager != null)
        {
            base.ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
        }
    }

    public void InitializeMatch(GameObject t1p1, GameObject t1p2, GameObject t1p3, GameObject t2p1, GameObject t2p2, GameObject t2p3)
    {
        if (CurrentState.Value != WAITING) return;
        
        SpawnAndQueue(t1p1, t1TopSpawn, topQuaternion, Constants.TEAM1);
        SpawnAndQueue(t1p2, t1JunSpawn, junQuaternion, Constants.TEAM1);
        SpawnAndQueue(t1p3, t1BotSpawn, botQuaternion, Constants.TEAM1);
        SpawnAndQueue(t2p1, t2TopSpawn, topQuaternion, Constants.TEAM2);
        SpawnAndQueue(t2p2, t2JunSpawn, junQuaternion, Constants.TEAM2);
        SpawnAndQueue(t2p3, t2BotSpawn, botQuaternion, Constants.TEAM2); 
        
        CurrentState.Value = PLAYING;
        Debug.Log("Match started");  
    }

    private void SpawnAndQueue(GameObject prefab, Vector3 pos, Quaternion rot, int team)
    {
        if (prefab == null) return;

        GameObject go = Instantiate(prefab, pos, rot);
        EntityHandler eh = go.GetComponent<EntityHandler>();
        if (eh != null) eh.SetTeam(team);
        base.ServerManager.Spawn(go);
        unassignedCharacters.Enqueue(go.GetComponent<NetworkObject>());
    }

    private void ServerManager_OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            if (unassignedCharacters.Count > 0)
            {
                NetworkObject nobj = unassignedCharacters.Dequeue();
                nobj.GiveOwnership(conn);
                Debug.Log($"Assigned character to client {conn.ClientId}");
            }
            else
            {
                Debug.Log("match is full so cant give character sorry");
            }
        }
    }

    private void TimeManager_OnTick()
    {
        if (CurrentState.Value != PLAYING) return;
        TimeRemaining.Value -= (float) base.TimeManager.TickDelta;
        
        if (TimeRemaining.Value <= laneBossesTime1 && !laneBossesSpawned1)
        {
            SpawnLaneBosses(randomizedStreamBosses[0], randomizedProjectBosses[0]);
            laneBossesSpawned1 = true;
        }
        else if (TimeRemaining.Value <= laneBossesTime2 && !laneBossesSpawned2)
        {
            SpawnLaneBosses(randomizedStreamBosses[1], randomizedProjectBosses[1]);
            laneBossesSpawned2 = true;
        }
        else if (TimeRemaining.Value <= jungleBossTime && !jungleBossSpawned)
        {
            SpawnJungleBoss(subathonPrefab);
            jungleBossSpawned = true;
        }
        else if (TimeRemaining.Value <= 0f)
        {
            EndMatch();
        }
    }

    private void SpawnLaneBosses(GameObject streamBoss, GameObject projectBoss)
    {
        Debug.Log("lane bosses spawned");
        if (streamBoss != null)
        {
            GameObject sb = Instantiate(streamBoss, Constants.STREAMBOSS, Quaternion.identity);
            base.ServerManager.Spawn(sb);
        }
        if (projectBoss != null)
        {
            GameObject pb = Instantiate(projectBoss, Constants.PROJECTSBOSS, Quaternion.identity);
            base.ServerManager.Spawn(pb);
        }
    }
    
    private void SpawnJungleBoss(GameObject jungleBoss)
    {
        Debug.Log("jungle boss spawned");
        if (jungleBoss != null)
        {
            GameObject jb = Instantiate(jungleBoss, Constants.JUNGLEBOSS, Quaternion.identity);
            base.ServerManager.Spawn(jb);
        }
    }
    
    public void EndMatch()
    {
        if (CurrentState.Value == ENDED) return;
        CurrentState.Value = ENDED;
        TimeRemaining.Value = 0f;
        Debug.Log("match over!");
    }

    private void ShuffleList(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            GameObject temp = list[i];
            int r = Random.Range(i, list.Count);
            list[i] = list[r];
            list[r] = temp;
        }
    }
}