using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;

public class Constants : NetworkBehaviour
{
    // Object constants
    public static Camera MainCamera { get; private set; }
    public static JoystickHandler MoveJoy { get; private set; }
    public static JoystickHandler Attack1Joy { get; private set; }
    public static JoystickHandler Attack2Joy { get; private set; }
    public static JoystickHandler UltJoy { get; private set; }

    // Prefab constants
    public GameObject topNlingPrefab;
    public GameObject junNlingPrefab;
    public GameObject botNlingPrefab;
    public GameObject tower1Prefab;
    public GameObject tower2Prefab;
    public GameObject eyePrefab;

    // Value constants
    public static readonly int WILD = 0;
    public static readonly int TEAM1 = 1;
    public static readonly int TEAM2 = 2;

    // Location constants
    public static Vector3 TOP1 = new Vector3(30f, 0f, -12.5f);
    public static Vector3 TOP2 = new Vector3(8.75f, 0f, -16.25f);
    public static Vector3 TOP3 = new Vector3(-8.75f, 0f, -16.25f);
    public static Vector3 TOP4 = new Vector3(-30f, 0f, -12.5f);
    public static Vector3 TOP5 = new Vector3(0f, 0f, -17.5f);
    public static Vector3 STREAMBOSS = new Vector3(0f, 0f, -12.5f);
    public static Vector3[] TOP = new Vector3[]{TOP1, TOP2, TOP3, TOP4, TOP5, STREAMBOSS};

    public static Vector3 BOT1 = new Vector3(30f, 0f, 12.5f);
    public static Vector3 BOT2 = new Vector3(8.75f, 0f, 16.25f);
    public static Vector3 BOT3 = new Vector3(-8.75f, 0f, 16.25f);
    public static Vector3 BOT4 = new Vector3(-30f, 0f, 12.5f);
    public static Vector3 BOT5 = new Vector3(0f, 0f, 17.5f);
    public static Vector3 PROJECTSBOSS = new Vector3(0f, 0f, 12.5f);
    public static Vector3[] BOT = new Vector3[]{BOT1, BOT2, BOT3, BOT4, BOT5, PROJECTSBOSS};

    public static Vector3 JUN1 = new Vector3(22.5f, 0f, -7.5f);
    public static Vector3 JUN2 = new Vector3(22.5f, 0f, 7.5f);
    public static Vector3 JUN3 = new Vector3(-22.5f, 0f, -7.5f);
    public static Vector3 JUN4 = new Vector3(-22.5f, 0f, 7.5f);
    public static Vector3 JUN5 = new Vector3(17.5f, 0f, -5f);
    public static Vector3 JUN6 = new Vector3(17.5f, 0f, 5f);
    public static Vector3 JUN7 = new Vector3(-17.5f, 0f, -5f);
    public static Vector3 JUN8 = new Vector3(-17.5f, 0f, 5f);
    public static Vector3 JUNGLEBOSS = new Vector3(0f, 0f, 0f);
    public static Vector3[] JUN = new Vector3[]{JUN1, JUN2, JUN3, JUN4, JUN5, JUN6, JUN7, JUN8, JUNGLEBOSS};

    public static Vector3 TEALEYE = new Vector3(28.75f, 0f, 0f);
    public static Vector3 TEALTOWERA = new Vector3(25f, 0f, -15f);
    public static Vector3 TEALTOWERB = new Vector3(12.5f, 0f, -16.25f);
    public static Vector3 TEALTOWERC = new Vector3(25f, 0f, 15f);
    public static Vector3 TEALTOWERD = new Vector3(12.5f, 0f, 16.25f);
    public static Vector3[] TEALPOS = new Vector3[]{TEALEYE, TEALTOWERA, TEALTOWERB, TEALTOWERC, TEALTOWERD};

    public static Vector3 CRIMEYE = new Vector3(-28.75f, 0f, 0f);
    public static Vector3 CRIMTOWERA = new Vector3(-25f, 0f, -15f);
    public static Vector3 CRIMTOWERB = new Vector3(-12.5f, 0f, -16.25f);
    public static Vector3 CRIMTOWERC = new Vector3(-25f, 0f, 15f);
    public static Vector3 CRIMTOWERD = new Vector3(-12.5f, 0f, 16.25f);
    public static Vector3[] CRIMPOS = new Vector3[]{CRIMEYE, CRIMTOWERA, CRIMTOWERB, CRIMTOWERC, CRIMTOWERD};

    public static GameObject[] TOWERPREFABS;
    private HashSet<Vector3> respawningPositions = new HashSet<Vector3>();
    

    private void Awake()
    {
        MainCamera = Camera.main;
        MoveJoy = GameObject.FindWithTag("MoveJoy").GetComponent<JoystickHandler>();
        Attack1Joy = GameObject.FindWithTag("Attack1Joy").GetComponent<JoystickHandler>();
        Attack2Joy = GameObject.FindWithTag("Attack2Joy").GetComponent<JoystickHandler>();
        UltJoy = GameObject.FindWithTag("UltJoy").GetComponent<JoystickHandler>();
        TOWERPREFABS = new GameObject[]{eyePrefab, tower2Prefab, tower1Prefab, tower2Prefab, tower1Prefab};
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        foreach (Vector3 pos in TOP){
            GameObject go = Instantiate(topNlingPrefab, pos, Quaternion.identity);
            base.ServerManager.Spawn(go);
        }

        foreach (Vector3 pos in JUN)
        {
            GameObject go = Instantiate(junNlingPrefab, pos, Quaternion.identity);
            base.ServerManager.Spawn(go);
        }
        
        foreach (Vector3 pos in BOT)
        {
            GameObject go = Instantiate(botNlingPrefab, pos, Quaternion.identity);
            base.ServerManager.Spawn(go);
        }

        

        for (int i = 0; i<5; i++){
            CreateTower(TOWERPREFABS[i], TEALPOS[i], TEAM1);
            CreateTower(TOWERPREFABS[i], CRIMPOS[i], TEAM2);
        }
    }

    private void CreateTower(GameObject prefab, Vector3 pos, int team)
    {
        GameObject go = Instantiate(prefab, pos, Quaternion.identity);
        StructureHandler sh = go.GetComponent<StructureHandler>();
        sh.SetTeam(team);
        base.ServerManager.Spawn(go);
    }

    private void Update()
    {
        if (!base.IsServerInitialized) return;
        foreach (Vector3 pos in TOP){
            Vector3 pos2 = new Vector3(pos.x, 0.75f, pos.z);
            if (!respawningPositions.Contains(pos) && isEmptyAtVector3(pos2))
            {
                StartCoroutine(RespawnWithDelay(30f, topNlingPrefab, pos));
            }
        }
        foreach (Vector3 pos in JUN)
        {
            Vector3 pos2 = new Vector3(pos.x, 0.75f, pos.z);
            if (!respawningPositions.Contains(pos) && isEmptyAtVector3(pos2))
            {
                StartCoroutine(RespawnWithDelay(30f, junNlingPrefab, pos));
            }
        }
        foreach (Vector3 pos in BOT)
        {
            Vector3 pos2 = new Vector3(pos.x, 0.75f, pos.z);
            if (!respawningPositions.Contains(pos) && isEmptyAtVector3(pos2))
            {
                StartCoroutine(RespawnWithDelay(30f, botNlingPrefab, pos));
            }
        }
    }

    private bool isEmptyAtVector3(Vector3 position)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, 0.25f);
        return hitColliders.Length==0;
    }

    IEnumerator RespawnWithDelay(float delay, GameObject prefab, Vector3 pos)
    {
        respawningPositions.Add(pos);
        yield return new WaitForSeconds(delay);
        GameObject go = Instantiate(prefab, pos, Quaternion.identity);
        base.ServerManager.Spawn(go);
        respawningPositions.Remove(pos);
    }

    private static Collider[] targetColliders = new Collider[50];

    public static Transform FindClosestTarget(Transform searcherTransform, int searcherTeam, float radius, bool targetEnemies = true, bool targetAllies = false, bool requireMissingHP = false, bool targetWild = true)
    {
        int count = Physics.OverlapSphereNonAlloc(searcherTransform.position, radius, targetColliders);
        Transform bestTarget = null;
        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < count; i++)
        {
            Collider hit = targetColliders[i];
            
            EntityHandler eHandler = hit.GetComponentInParent<EntityHandler>();
            if (eHandler == null) continue;
            if (eHandler.gameObject == searcherTransform.gameObject) continue;
            if (hit.GetComponentInParent<ProjectileHandler>() != null) continue;

            int targetTeam = eHandler.EntityTeam;

            if (targetTeam != -1)
            {
                if (!targetWild && targetTeam == WILD) continue;

                bool isAlly = (targetTeam == searcherTeam);
                bool isEnemy = !isAlly;

                if ((targetEnemies && isEnemy) || (targetAllies && isAlly))
                {
                    if (requireMissingHP && eHandler.GetHPPercent() >= 1.0f) continue;

                    float distanceToTarget = Vector3.Distance(searcherTransform.position, hit.transform.position);
                    if (distanceToTarget < closestDistance)
                    {
                        closestDistance = distanceToTarget;
                        bestTarget = hit.transform;
                    }
                }
            }
        }

        return bestTarget;
    }

    public static Transform FindWeakestTarget(Transform searcherTransform, int searcherTeam, float radius, bool targetEnemies = true, bool targetAllies = false, bool requireMissingHP = false, bool targetWild = true)
    {
        int count = Physics.OverlapSphereNonAlloc(searcherTransform.position, radius, targetColliders);
        Transform bestTarget = null;
        
        float weakestPercent = 1.01f; 

        for (int i = 0; i < count; i++)
        {
            Collider hit = targetColliders[i];
            
            EntityHandler eHandler = hit.GetComponentInParent<EntityHandler>();
            if (eHandler == null) continue;
            if (eHandler.gameObject == searcherTransform.gameObject) continue;
            if (hit.GetComponentInParent<ProjectileHandler>() != null) continue;
            
            int targetTeam = eHandler.EntityTeam;

            if (targetTeam != -1)
            {
                if (!targetWild && targetTeam == WILD) continue;

                bool isAlly = (targetTeam == searcherTeam);
                bool isEnemy = !isAlly;

                if ((targetEnemies && isEnemy) || (targetAllies && isAlly))
                {
                    float hpPercent = eHandler.GetHPPercent();
                    
                    if (requireMissingHP && hpPercent >= 1.0f) continue;

                    if (hpPercent < weakestPercent)
                    {
                        weakestPercent = hpPercent;
                        bestTarget = hit.transform;
                    }
                }
            }
        }

        return bestTarget;
    }

    public static Transform FindNextLaneTarget(Transform searcherTransform, int searcherTeam)
    {
        Vector3[] enemyPositions;
        if (searcherTeam == TEAM1) enemyPositions = CRIMPOS;
        else if (searcherTeam == TEAM2) enemyPositions = TEALPOS;
        else return null;

        bool isTopLane = searcherTransform.position.z < 0;

        // these magic numbers come from CRIMPOS and TEALPOS
        // Index 0: Eye
        // Index 1: Top Inner Tower (Tower 2)
        // Index 2: Top Outer Tower (Tower 1)
        // Index 3: Bot Inner Tower (Tower 2)
        // Index 4: Bot Outer Tower (Tower 1)
        
        int outerTowerIndex = isTopLane ? 2 : 4;
        int innerTowerIndex = isTopLane ? 1 : 3;
        int eyeIndex = 0;
        
        // Check outer tower
        Transform target = GetAliveStructureAt(enemyPositions[outerTowerIndex], searcherTeam);
        if (target != null) return target;

        // Check inner tower if outer is deadge
        target = GetAliveStructureAt(enemyPositions[innerTowerIndex], searcherTeam);
        if (target != null) return target;

        // Default to Eye if both towers in a lane are deadge
        return GetAliveStructureAt(enemyPositions[eyeIndex], searcherTeam);
    }

    private static Transform GetAliveStructureAt(Vector3 position, int searcherTeam)
    {
        int count = Physics.OverlapSphereNonAlloc(position, 1.0f, targetColliders);
        
        for (int i = 0; i < count; i++)
        {
            StructureHandler structure = targetColliders[i].GetComponentInParent<StructureHandler>();
            if (structure != null && structure.EntityTeam != searcherTeam && structure.GetHPValue() > 0)
            {
                return structure.transform;
            }
        }
        return null;
    }
}
