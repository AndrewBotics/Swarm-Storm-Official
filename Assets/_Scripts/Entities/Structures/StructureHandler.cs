using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FishNet.Object;

public abstract class StructureHandler : EntityHandler
{
    protected float StructureAttackRadius;
    protected float StructureAttackVelocity = 5.0f;
    protected float StructureAttackDamage;
    protected float StructureCooldown = 1.5f;

    public GameObject projectilePrefab;
    public Transform firePoint;
    
    protected uint nextAttackTick = 0;

    protected override void Start()
    {
        base.Start();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        base.TimeManager.OnTick += TimeManager_OnTick;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (base.TimeManager != null)
        {
            base.TimeManager.OnTick -= TimeManager_OnTick;
        }
    }

    private void TimeManager_OnTick()
    {
        HomingAttackCheck();
    }

    protected void HomingAttackCheck()
    {
        if (base.TimeManager.Tick < nextAttackTick) return;
        
        Transform target = Constants.FindClosestTarget(transform, EntityTeam, StructureAttackRadius, targetWild: false);

        if (target != null)
        {
            FireProjectile(target, GetAttackValue(StructureAttackDamage));
        }
        else {
            target = Constants.FindClosestTarget(transform, EntityTeam, StructureAttackRadius, targetEnemies: false, targetAllies: true, requireMissingHP: true);
            if (target != null)
            {
                FireProjectile(target, StructureAttackDamage);
            }
        }
    }

    private void FireProjectile(Transform target, float damage)
    {
        nextAttackTick = base.TimeManager.Tick + (uint)base.TimeManager.TimeToTicks(StructureCooldown);
        
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        HomingProjectileHandler homingLogic = proj.GetComponent<HomingProjectileHandler>();
        if (homingLogic != null)
        {
            homingLogic.Setup(this, target, damage, StructureAttackVelocity);
        }
        base.ServerManager.Spawn(proj);
    }

    public override void TakeDamage(float amount, EntityHandler attacker)
    {
        if (GetHPValue() <= 0) return;

        if (attacker != null && attacker is NlingHandler)
        {
            amount *= 2.0f;
        }

        base.TakeDamage(amount, attacker);

        if (GetHPValue() <= 0)
        {
            Die();
        }
    }

    protected override void Die()
    {
        if (base.IsServerInitialized)
        {
            base.ServerManager.Despawn(gameObject);
        }
    }

    public override void ApplyKnockback(Vector3 direction, float distance, float duration)
    {
        return;
    }
}