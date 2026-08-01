using UnityEngine;

public class NeuroBeam : ProjectileHandler
{
    private uint tickRateTicks;
    private uint endTick;
    private uint nextDamageTick;
    
    private uint destroyTick;
    private bool isStopping = false;

    private Vector3 extents;
    private Transform casterTransform;
    private Vector3 positionOffset;
    
    private ParticleSystem beamParticles;

    public void Setup(NeuroHandler neuro, float totalDamage, float duration, int ticks, float width, float range, Transform caster)
    {
        base.Setup(neuro, totalDamage / ticks, 0f, range);
        
        extents = new Vector3(width / 2f, 2f, range / 2f);
        casterTransform = caster;
        positionOffset = transform.position - caster.position;
        
        beamParticles = GetComponent<ParticleSystem>();
        
        if (neuro.IsServerInitialized)
        {
            tickRateTicks = (uint)neuro.TimeManager.TimeToTicks(duration / ticks);
            endTick = neuro.TimeManager.Tick + (uint)neuro.TimeManager.TimeToTicks(duration);
            nextDamageTick = neuro.TimeManager.Tick;
        }
    }

    protected override void MoveProjectile()
    {
        if (casterTransform != null)
        {
            transform.position = casterTransform.position + positionOffset;
            transform.position = new Vector3(transform.position.x, 0.75f, transform.position.z);
        }
    }

    protected override void CheckMaxDistance()
    {
        if (endTick == 0) return; 

        if (!isStopping && base.TimeManager.Tick >= endTick)
        {
            isStopping = true;
            if (beamParticles != null) beamParticles.Stop();
            
            destroyTick = base.TimeManager.Tick + (uint)base.TimeManager.TimeToTicks(0.5f);
        }
        else if (isStopping && base.TimeManager.Tick >= destroyTick)
        {
            DestroyProjectile();
        }
        else if (!isStopping && base.TimeManager.Tick >= nextDamageTick)
        {
            TickDamage();
            nextDamageTick += tickRateTicks;
        }
    }

    private void TickDamage()
    {
        Vector3 center = transform.position + (transform.forward * (maxDistance / 2f));
        Collider[] hits = Physics.OverlapBox(center, extents, transform.rotation);

        foreach (Collider hit in hits)
        {
            if (hit is CapsuleCollider) 
            {
                EntityHandler eHandler = hit.GetComponent<EntityHandler>();
                if (eHandler != null)
                {
                    if (eHandler.EntityTeam != -1 && eHandler.EntityTeam != projectileTeam)
                    {
                        eHandler.TakeDamage(damage, shooter);
                    }
                }
            }
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        
    }
}