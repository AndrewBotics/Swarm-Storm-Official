using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public abstract class StructureHandler : EntityHandler
{
    protected float StructureAttackRadius;
    protected float StructureAttackVelocity = 5.0f;
    protected float StructureAttackDamage;
    protected float StructureCooldown = 1.5f;

    public GameObject projectilePrefab;
    public Transform firePoint;
    protected float nextAttackTime = 0f;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        if (!base.IsServerInitialized) return;
        HomingAttackCheck();
    }

    protected void HomingAttackCheck()
    {
        // Default structure behavior:
        // if enemies exist, attack the weakest close one.
        // else if friendlies exist, heal the weakest close one.
        if (Time.time < nextAttackTime) return;
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
        nextAttackTime = Time.time + StructureCooldown;
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
        // structures cant be knocked back silly
        return;
    }
}