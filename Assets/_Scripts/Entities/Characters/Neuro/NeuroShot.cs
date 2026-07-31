using UnityEngine;

public class NeuroShot : ProjectileHandler
{
    private float randDist = 0.7f;
    private float overshootDistance;
    
    public GameObject projectilePrefab;

    private int generation;
    private Vector3 targetPosition;
    private int currentSeed;

    public void Setup(EntityHandler neuro, Vector3 vectorData, float d, float s, float mD, float oD, int currentGeneration, int seed)
    {
        base.Setup(neuro, d, s, mD);
        
        generation = currentGeneration;
        overshootDistance = oD;
        currentSeed = seed;

        if (generation > 0)
        {
            targetPosition = vectorData;
            targetPosition.y = 0.75f; 
            
            transform.LookAt(targetPosition);
        }
        else
        {
            Vector3 travelDirection = vectorData;
            travelDirection.y = 0f; 
            travelDirection = travelDirection.normalized;

            if (travelDirection != Vector3.zero)
            {
                transform.LookAt(transform.position + travelDirection);
            }
        }
    }

    protected override void MoveProjectile()
    {
        if (generation > 0)
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
        }
        else
        {
            base.MoveProjectile();
        }
    }

    protected override void CheckMaxDistance()
    {
        if (generation > 0)
        {
            Vector2 currentFlat = new Vector2(transform.position.x, transform.position.z);
            Vector2 targetFlat = new Vector2(targetPosition.x, targetPosition.z);

            if (Vector2.Distance(currentFlat, targetFlat) <= 0.05f)
            {
                DeployNextGeneration();
                DestroyProjectile();
            }
        }
        else
        {
            base.CheckMaxDistance();
        }
    }

    private void DeployNextGeneration()
    {
        if (projectilePrefab != null)
        {
            Transform target = Constants.FindClosestTarget(transform, projectileTeam, maxDistance * 2);
            Vector3 nextVectorData; 
            
            if (target != null)
            {
                if (generation > 1)
                {
                    Random.InitState(currentSeed+generation);
                    Vector3 randomOffset = new Vector3(Random.Range(-randDist, randDist), 0, Random.Range(-randDist, randDist));
                    Vector3 randomTargetPoint = target.position + randomOffset;
                    
                    Vector3 directionToPoint = (randomTargetPoint - transform.position).normalized;
                    
                    nextVectorData = target.position + (directionToPoint * overshootDistance);
                    nextVectorData.y = transform.position.y;
                }
                else
                {
                    Vector3 dirToEnemy = target.position - transform.position;
                    dirToEnemy.y = 0f;
                    
                    if (dirToEnemy.sqrMagnitude < 0.01f) dirToEnemy = transform.forward;
                    
                    nextVectorData = dirToEnemy.normalized;
                }

                GameObject nextProj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                NeuroShot logic = nextProj.GetComponent<NeuroShot>();
                
                if (logic != null)
                {
                    logic.Setup(shooter, nextVectorData, damage, speed, maxDistance, overshootDistance, generation - 1, currentSeed);
                }

                if (base.IsServerInitialized)
                {
                    base.ServerManager.Spawn(nextProj);
                }
            }
        }
        else
        {
            Debug.LogWarning("Deploy failed: projectilePrefab is empty on " + gameObject.name);
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (!base.IsServerInitialized) return;
        EntityHandler entity = other.GetComponent<EntityHandler>();
        
        if (entity != null)
        {
            if (entity.EntityTeam != -1 && entity.EntityTeam != projectileTeam)
            {
                entity.TakeDamage(damage, shooter);
                OnHit(entity);
                // Debug.Log(entity.EntityName+": "+entity.GetHPValue());
            }
        }
    }
}