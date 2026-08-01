using UnityEngine;
using FishNet.Object;

public class HomingProjectileHandler : ProjectileHandler
{
    protected Transform target;
    
    private float acceleration = 2.5f;
    private float maxSpeed = 20f;

    public void Setup(EntityHandler ent, Transform newTarget, float dmg, float spd)
    {
        base.Setup(ent, dmg, spd, 100f);
        target = newTarget;
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

        EntityHandler eHandler = target.GetComponent<EntityHandler>();
        Vector3 targetPos = (eHandler != null) ? eHandler.CenterPoint : target.position;

        Vector3 direction = (targetPos - transform.position).normalized;
        
        transform.LookAt(targetPos);
        
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
                entity.TakeDamage(damage, shooter);
                OnHit(entity);
                DestroyProjectile();
            }
            else if (entity.EntityTeam != -1 && entity.EntityTeam==projectileTeam)
            {
                entity.Heal(damage);
                OnHit(entity);
                DestroyProjectile();
            }
        }
    }
}