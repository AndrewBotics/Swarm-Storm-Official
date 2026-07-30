using UnityEngine;
using System.Collections;

public class NeuroBeam : ProjectileHandler
{
    private float tickRate;
    private Vector3 extents;
    private Transform casterTransform;
    private Vector3 positionOffset;
    
    private ParticleSystem beamParticles;

    public void Setup(int team, float totalDamage, float duration, int ticks, float width, float range, Transform caster)
    {
        base.Setup(team, totalDamage / ticks, 0f, range);
        
        tickRate = duration / ticks;
        extents = new Vector3(width / 2f, 2f, range / 2f);
        casterTransform = caster;
        positionOffset = transform.position - caster.position;
        
        beamParticles = GetComponent<ParticleSystem>();
        
        StartCoroutine(BeamRoutine(duration));
    }

    protected override void Update()
    {
        if (casterTransform != null)
        {
            transform.position = casterTransform.position + positionOffset;
            transform.position = new Vector3(transform.position.x, 0.75f, transform.position.z);
        }
    }

    private IEnumerator BeamRoutine(float duration)
    {
        float endTime = Time.time + duration;
        
        while (Time.time < endTime)
        {
            TickDamage();
            yield return new WaitForSeconds(tickRate);
        }
        
        if (beamParticles != null)
        {
            beamParticles.Stop(); 
        }

        yield return new WaitForSeconds(0.5f);
        
        DestroyProjectile();
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
                        eHandler.TakeDamage(damage, projectileTeam);
                    }
                }
            }
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        
    }
}