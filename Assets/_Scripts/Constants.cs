using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Constants : MonoBehaviour
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
    public GameObject teamTowerPrefab;

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

    /*
    public static Vector3 ONEBASE = new Vector3(28.75f, 1.25f, 0f);
    public static Vector3 ONETOWER1 = new Vector3(25f, 1.25f, -15f);
    public static Vector3 ONETOWER2 = new Vector3(12.5f, 1.25f, -16.25f);
    public static Vector3 ONETOWER3 = new Vector3(25f, 1.25f, 15f);
    public static Vector3 ONETOWER4 = new Vector3(12.5f, 1.25f, 16.25f);
    public static Vector3[] ONE = new Vector3[]{ONEBASE, ONETOWER1, ONETOWER2, ONETOWER3, ONETOWER4};

    public static Vector3 TWOBASE = new Vector3(-28.75f, 1.25f, 0f);
    public static Vector3 TWOTOWER1 = new Vector3(-25f, 1.25f, -15f);
    public static Vector3 TWOTOWER2 = new Vector3(-12.5f, 1.25f, -16.25f);
    public static Vector3 TWOTOWER3 = new Vector3(-25f, 1.25f, 15f);
    public static Vector3 TWOTOWER4 = new Vector3(-12.5f, 1.25f, 16.25f);
    public static Vector3[] TWO = new Vector3[]{TWOBASE, TWOTOWER1, TWOTOWER2, TWOTOWER3, TWOTOWER4};
    */

    private HashSet<Vector3> respawningPositions = new HashSet<Vector3>();
    

    private void Awake()
    {
        MainCamera = Camera.main;
        MoveJoy = GameObject.FindWithTag("MoveJoy").GetComponent<JoystickHandler>();
        Attack1Joy = GameObject.FindWithTag("Attack1Joy").GetComponent<JoystickHandler>();
        Attack2Joy = GameObject.FindWithTag("Attack2Joy").GetComponent<JoystickHandler>();
        UltJoy = GameObject.FindWithTag("UltJoy").GetComponent<JoystickHandler>();
    }

    private void Start()
    {
        foreach (Vector3 pos in TOP){
            Instantiate(topNlingPrefab, pos, Quaternion.identity);
        }

        foreach (Vector3 pos in JUN)
        {
            Instantiate(junNlingPrefab, pos, Quaternion.identity);
        }
        
        foreach (Vector3 pos in BOT)
        {
            Instantiate(botNlingPrefab, pos, Quaternion.identity);
        }

        /*

        foreach (Vector3 pos in ONE)
        {
            GameObject go = Instantiate(teamTowerPrefab, pos, Quaternion.identity);
            TowerHandler th = go.GetComponent<TowerHandler>();
            th.Team = TEAM1;
        }

        foreach (Vector3 pos in TWO)
        {
            GameObject go = Instantiate(teamTowerPrefab, pos, Quaternion.identity);
            TowerHandler th = go.GetComponent<TowerHandler>();
            th.Team = TEAM2;
        }
        */
    }

    private void Update()
    {
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
        Instantiate(prefab, pos, Quaternion.identity);
    }

    public static Transform FindClosestTarget(Transform searcherTransform, int searcherTeam, float radius, bool targetEnemies = true, bool targetAllies = false)
    {
        Collider[] hitColliders = Physics.OverlapSphere(searcherTransform.position, radius);
        Transform bestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hitColliders)
        {
            if (hit is CapsuleCollider)
            {
                if (hit.transform == searcherTransform) continue;

                int targetTeam = -1;

                EntityHandler eHandler = hit.GetComponent<EntityHandler>();
                if (eHandler != null)
                {
                    targetTeam = eHandler.EntityTeam;
                }

                if (targetTeam != -1)
                {
                    bool isAlly = (targetTeam == searcherTeam);
                    bool isEnemy = !isAlly;

                    if ((targetEnemies && isEnemy) || (targetAllies && isAlly))
                    {
                        float distanceToTarget = Vector3.Distance(searcherTransform.position, hit.transform.position);
                        if (distanceToTarget < closestDistance)
                        {
                            closestDistance = distanceToTarget;
                            bestTarget = hit.transform;
                        }
                    }
                }
            }
        }

        return bestTarget;
    }
}
