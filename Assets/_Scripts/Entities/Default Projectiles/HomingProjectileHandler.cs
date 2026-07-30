using UnityEngine;
using FishNet.Object;

public class HomingProjectileHandler : ProjectileHandler
{
    protected Transform target;
    
    private float acceleration = 0.5f;
    private float maxSpeed = 10f;

    protected override void Update()
    {
        
    }

    public void Setup(int team, Transform newTarget, float dmg, float spd)
    {
        projectileTeam = team;
        target = newTarget;
        damage = dmg;
        speed = spd;
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
        MoveProjectile();
    }

    protected override void MoveProjectile()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            DestroyProjectile();
            return;
        }

        CharacterHandler charTarget = target.GetComponent<CharacterHandler>();
        if (charTarget != null && charTarget.isDead)
        {
            DestroyProjectile();
            return;
        }

        float delta = (float)base.TimeManager.TickDelta;

        speed += acceleration * delta;
        if (speed > maxSpeed) speed = maxSpeed;

        Vector3 direction = (target.position - transform.position).normalized;
        transform.LookAt(target);
        
        transform.position += direction * speed * delta;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (!base.IsServerInitialized) return;
        if (other.transform != target) return;
        
        EntityHandler entity = other.GetComponent<EntityHandler>();
        if (entity != null)
        {
            if (entity.EntityTeam != -1 && entity.EntityTeam != projectileTeam)
            {
                entity.TakeDamage(damage, projectileTeam);
                OnHit(entity);
                DestroyProjectile();
            }
            else if (entity.EntityTeam != -1 && entity.EntityTeam==projectileTeam)
            {
                entity.Heal(damage);
            }
        }
    }
}