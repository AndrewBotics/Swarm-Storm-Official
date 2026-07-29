using UnityEngine;
using FishNet.Object;
public abstract class ProjectileHandler : NetworkBehaviour
{
    [HideInInspector] public int projectileTeam;
    protected float damage;
    protected float speed;
    protected float maxDistance;
    protected Vector3 startPosition;

    public virtual void Setup(int team, float dmg, float spd, float maxDist)
    {
        projectileTeam = team;
        damage = dmg;
        speed = spd;
        maxDistance = maxDist;
        
        transform.position = new Vector3(transform.position.x, 0.75f, transform.position.z);
        startPosition = transform.position;
    }

    protected virtual void Start()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in renderers)
        {
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }
    }

    protected virtual void Update()
    {
        MoveProjectile();
        CheckMaxDistance();
    }

    protected virtual void MoveProjectile()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    protected virtual void CheckMaxDistance()
    {
        if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            DestroyProjectile();
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        EntityHandler entity = other.GetComponent<EntityHandler>();
        
        if (entity != null)
        {
            if (entity.EntityTeam != -1 && entity.EntityTeam != projectileTeam)
            {
                entity.TakeDamage(damage, projectileTeam);
                OnHit(entity);
                DestroyProjectile();
            }
        }
    }

    // override to apply unique effects (e.g., slows, stuns, or branching spawns)
    protected virtual void OnHit(EntityHandler target)
    {
    }

    protected virtual void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}