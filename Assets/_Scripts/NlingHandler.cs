using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public abstract class NlingHandler : EntityHandler
{
    [SerializeField] protected UnityEngine.AI.NavMeshAgent NlingAgent;

    // Base targeting
    protected Transform targetBase;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void TakeDamage(float amount, int attackerTeam)
    {
        if (EntityCurrentHP <= 0) return;

        base.TakeDamage(amount, attackerTeam);

        if (EntityCurrentHP <= 0)
        {
            if (EntityTeam==Constants.WILD) ConvertToTeam(attackerTeam);
            else Die();
        }
    }

    protected override void Die()
    {
        Destroy(gameObject);
    }

    protected override IEnumerator KnockbackRoutine(Vector3 dir, float dist, float dur)
    {
        float timer = 0f;
        float speed = dist / dur;
        while (timer < dur)
        {
            if (NlingAgent != null && NlingAgent.isActiveAndEnabled)
            {
                NlingAgent.Move(dir * speed * Time.deltaTime);
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

    protected void ConvertToTeam(int newTeam)
    {
        EntityTeam = newTeam;
        EntityCurrentHP = EntityMaxHP;
        SetHP(1.0f);

        UpdateTargetBase();

        Debug.Log(EntityName + " has been converted to Team " + newTeam + "!");
    }

    protected void UpdateTargetBase()
    {
        string enemyTag = null;
        if (EntityTeam == Constants.TEAM1) enemyTag = "Team2";
        else if (EntityTeam == Constants.TEAM2) enemyTag = "Team1";

        if (enemyTag == null)
        {
            targetBase = null;
            return;
        }

        GameObject[] bases = GameObject.FindGameObjectsWithTag(enemyTag);
        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject b in bases)
        {
            float distance = Vector3.Distance(transform.position, b.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = b.transform;
            }
        }

        targetBase = closest;
    }

    protected void MoveTowardBase()
    {
        if (NlingAgent != null && targetBase != null)
        {
            NlingAgent.SetDestination(targetBase.position);
        }
    }
}