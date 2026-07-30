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
        if (GetHPValue() <= 0) return;

        base.TakeDamage(amount, attackerTeam);

        if (GetHPValue() <= 0)
        {
            if (EntityTeam==Constants.WILD) ConvertToTeam(attackerTeam);
            else Die();
        }
    }

    protected override void Die()
    {
        Destroy(gameObject);
    }

    public override void ApplyKnockback(Vector3 direction, float distance, float duration)
    {
        base.ApplyKnockback(direction, distance*0.5f, duration);
    }

    protected void ConvertToTeam(int newTeam)
    {
        EntityTeam = newTeam;
        SetHPValue(EntityMaxHP);
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