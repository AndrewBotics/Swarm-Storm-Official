using UnityEngine;
using FishNet.Object;

public abstract class NlingHandler : EntityHandler
{
    [SerializeField] protected UnityEngine.AI.NavMeshAgent NlingAgent;

    // Base targeting
    protected Transform targetBase;
    
    // Attack Variables
    protected float NlingAttackRadius;
    protected float NlingAttackDamage;
    protected float NlingCooldown;
    protected float NlingProjectileVelocity = 5.0f; 
    
    public GameObject projectilePrefab;
    public Transform firePoint;
    
    protected uint nextAttackTick = 0;

    public override void OnStartServer()
    {
        base.OnStartServer();
        base.TimeManager.OnTick += TimeManager_OnTick;
        
        UpdateTargetBase();
        
        if (NlingAgent != null)
        {
            NlingAgent.speed = GetSpeedValue();
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (base.TimeManager != null)
        {
            base.TimeManager.OnTick -= TimeManager_OnTick;
        }
    }

    protected override void Update() 
    {
        base.Update();
    }

    private void TimeManager_OnTick()
    {
        if (GetHPValue() <= 0) return;

        if (knockbackTimer > 0)
        {
            knockbackTimer -= (float)base.TimeManager.TickDelta;
            
            if (NlingAgent != null)
            {
                NlingAgent.isStopped = true;
                NlingAgent.Move(currentKnockbackVelocity * (float)base.TimeManager.TickDelta);
            }
            return; 
        }

        ExecuteAILogic();
    }

    protected virtual void ExecuteAILogic()
    {
        Transform target = Constants.FindClosestTarget(transform, EntityTeam, NlingAttackRadius);

        if (target != null)
        {
            if (NlingAgent != null) NlingAgent.isStopped = true;

            if (base.TimeManager.Tick >= nextAttackTick)
            {
                FireProjectile(target);
                nextAttackTick = base.TimeManager.Tick + (uint)base.TimeManager.TimeToTicks(NlingCooldown);
            }
        }
        else
        {
            MoveTowardBase();
        }
    }

    private void FireProjectile(Transform target)
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        HomingProjectileHandler homingLogic = proj.GetComponent<HomingProjectileHandler>();
        if (homingLogic != null)
        {
            homingLogic.Setup(this, target, GetAttackValue(NlingAttackDamage), NlingProjectileVelocity);
        }
        
        base.ServerManager.Spawn(proj);
    }

    public override void TakeDamage(float amount, EntityHandler attacker)
    {
        if (GetHPValue() <= 0) return;

        if (attacker != null && attacker is StructureHandler)
        {
            amount *= 0.7f;
        }

        base.TakeDamage(amount, attacker);

        if (GetHPValue() <= 0)
        {
            if (EntityTeam == Constants.WILD && attacker != null) 
            {
                ConvertToTeam(attacker.EntityTeam);
            }
            else 
            {
                Die();
            }
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
        base.ApplyKnockback(direction, distance*0.5f, duration);
    }

    protected void ConvertToTeam(int newTeam)
    {
        SetTeam(newTeam);
        SetHPValue(EntityMaxHP);
        SetHP(1.0f);

        UpdateTargetBase();
    }

    protected void UpdateTargetBase()
    {
        if (EntityTeam == Constants.WILD)
        {
            targetBase = null;
            return;
        }

        targetBase = Constants.FindNextLaneTarget(transform, EntityTeam);
    }

    protected void MoveTowardBase()
    {
        if (NlingAgent != null && targetBase != null)
        {
            NlingAgent.isStopped = false;
            NlingAgent.SetDestination(targetBase.position);
        }
    }
}