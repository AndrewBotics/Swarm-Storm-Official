using UnityEngine;
using FishNet.Object;

public class NeuroHandler : CharacterHandler
{
    public GameObject attack1Prefab;
    public GameObject attack2Prefab;
    public GameObject ultPrefab;

    private readonly float NeuroBaseHP = 700.0f;
    private readonly float NeuroBaseSpeed = 3f;

    // Attack Local Variables
    private readonly float NeuroAttack1Range = 4f;
    private readonly float NeuroAttack1Range2 = 4f;
    private readonly float NeuroAttack1Overshoot = 1f;
    private readonly float NeuroAttack1Width = 0.25f;
    private readonly float NeuroAttack1Spacing = 0.5f;
    private readonly float NeuroAttack1Cooldown = 2f;
    private readonly float NeuroAttack1BaseDamage = 20f;
    private readonly float NeuroAttack1Speed = 10f;

    private readonly float NeuroAttack2Range = 4f;
    private readonly float NeuroAttack2Width = 1f;
    private readonly float NeuroAttack2Cooldown = 10f;
    private readonly float NeuroAttack2BaseDamage = 75f; 
    private readonly float NeuroAttack2Duration = 3f;
    private readonly int NeuroAttack2Ticks = 15;

    private readonly float NeuroUltRange = 4f;
    private readonly float NeuroUltWidth = 0.5f;
    private readonly float NeuroUltRange2 = 5f;
    private readonly float NeuroUltOvershoot = 2f;
    private readonly float NeuroUltCooldown = 40f;
    private readonly float NeuroUltBaseDamage = 35f;
    private readonly float NeuroUltSpeed = 15f;
    private readonly int NeuroUltCount = 10;

    protected override void Start()
    {
        base.Start();
        EntityName = "NeuroPlayer";
        EntityBaseHP = NeuroBaseHP;
        EntityMaxHP = NeuroBaseHP;
        EntityBaseSpeed = NeuroBaseSpeed;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        SetHPValue(NeuroBaseHP);
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (base.IsOwner)
        {
            Attack1Joy.OnJoystickReleased += HandleAttack1Release;
            Attack2Joy.OnJoystickReleased += HandleAttack2Release;
            UltJoy.OnJoystickReleased += HandleUltRelease;
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (base.IsOwner)
        {
            if (Attack1Joy != null) Attack1Joy.OnJoystickReleased -= HandleAttack1Release;
            if (Attack2Joy != null) Attack2Joy.OnJoystickReleased -= HandleAttack2Release;
            if (UltJoy != null) UltJoy.OnJoystickReleased -= HandleUltRelease;
        }
    }
   
    protected override void Update()
    {
        base.Update();

        if (base.IsOwner)
        {
            EntityTelegraph.ClearPreviews();
            if (Attack1Joy.GetJoystickVector() != Vector2.zero)
            {
                Attack1Preview(Attack1Joy.GetCameraRelativeDirection());
            }

            if (Attack2Joy.GetJoystickVector() != Vector2.zero)
            {
                Attack2Preview(Attack2Joy.GetCameraRelativeDirection());
            }

            if (UltJoy.GetJoystickVector() != Vector2.zero)
            {
                UltPreview(UltJoy.GetCameraRelativeDirection());
            }
        }
    }

    private void Attack1Preview(Vector3 aimDirection)
    {
        Vector3 rightDirection = Vector3.Cross(Vector3.up, aimDirection).normalized;
        float forwardOffset = (NeuroAttack1Range / 2f);
        
        Vector3 baseCenter = transform.position + (aimDirection * forwardOffset);
        
        baseCenter.y = Telegraph.offset; 

        Vector3 rightCenter = baseCenter + (rightDirection * NeuroAttack1Spacing);
        Vector3 leftCenter = baseCenter - (rightDirection * NeuroAttack1Spacing);

        EntityTelegraph.DrawFilledRectangle(leftCenter, NeuroAttack1Width, NeuroAttack1Range, aimDirection);
        EntityTelegraph.DrawFilledRectangle(baseCenter, NeuroAttack1Width, NeuroAttack1Range, aimDirection);
        EntityTelegraph.DrawFilledRectangle(rightCenter, NeuroAttack1Width, NeuroAttack1Range, aimDirection);
    }

   private void Attack2Preview(Vector3 aimDirection)
    {
        float forwardOffset = (NeuroAttack2Range / 2f);
        Vector3 center = transform.position + (aimDirection * forwardOffset);
        center.y = Telegraph.offset;
        EntityTelegraph.DrawFilledRectangle(center, NeuroAttack2Width, NeuroAttack2Range, aimDirection);
    }

    private void UltPreview(Vector3 aimDirection)
    {
        float forwardOffset = (NeuroUltRange / 2f);
        Vector3 center = transform.position + (aimDirection * forwardOffset);
        center.y = Telegraph.offset;
        EntityTelegraph.DrawFilledRectangle(center, NeuroUltWidth, NeuroUltRange, aimDirection);
    }

    private void HandleAttack1Release(Vector2 releaseVector, float holdTime)
    {
        if (!base.IsOwner) return;
        
        if (releaseVector == Vector2.zero && holdTime > JoystickHandler.autoAimTime)
        {
            return;
        }

        Vector3 aimDirection = Vector3.zero;

        if (holdTime <= JoystickHandler.autoAimTime) 
        {
            Transform target = Constants.FindClosestTarget(transform, EntityTeam, 10f);
            if (target != null)
            {
                aimDirection = (target.position - transform.position).normalized;
                aimDirection.y = 0f;
            }
            else return; 
        }
        else
        {
            Vector3 camForward = Constants.MainCamera.transform.forward;
            camForward.y = 0f;
            Vector3 camRight = Constants.MainCamera.transform.right;
            camRight.y = 0f;
            
            aimDirection = (camForward.normalized * releaseVector.y + camRight.normalized * releaseVector.x).normalized;
        }

        int randomSeed = Random.Range(int.MinValue, int.MaxValue);
        ServerFireAttack1(aimDirection, randomSeed);
        Attack1Joy.AddCooldown(NeuroAttack1Cooldown);
    }

    [ServerRpc]
    private void ServerFireAttack1(Vector3 aimDirection, int seed)
    {
        Vector3 rightDirection = Vector3.Cross(Vector3.up, aimDirection).normalized;
        float startOffset = 0.5f;

        Vector3 baseStart = transform.position + (aimDirection * startOffset);
        baseStart.y = Telegraph.offset;

        Vector3 rightStart = baseStart + (rightDirection * NeuroAttack1Spacing);
        Vector3 leftStart = baseStart - (rightDirection * NeuroAttack1Spacing);

        Vector3 baseEnd = baseStart + (aimDirection * NeuroAttack1Range);
        Vector3 rightEnd = rightStart + (aimDirection * NeuroAttack1Range);
        Vector3 leftEnd = leftStart + (aimDirection * NeuroAttack1Range);

        float totalDamage = GetAttackValue(NeuroAttack1BaseDamage);

        SpawnProjectile(leftStart, leftEnd, totalDamage, seed);
        SpawnProjectile(baseStart, baseEnd, totalDamage, seed);
        SpawnProjectile(rightStart, rightEnd, totalDamage, seed);
    }

    private void SpawnProjectile(Vector3 startPos, Vector3 endPos, float damage, int seed)
    {
        if (attack1Prefab != null)
        {
            GameObject proj = Instantiate(attack1Prefab, startPos, Quaternion.identity);
            NeuroShot logic = proj.GetComponent<NeuroShot>();
            if (logic != null)
            {
                logic.Setup(this, endPos, damage, NeuroAttack1Speed, NeuroAttack1Range2, NeuroAttack1Overshoot, 1, seed);
            }
            
            base.ServerManager.Spawn(proj);
        }
    }

    private void HandleAttack2Release(Vector2 releaseVector, float holdTime)
    {
        if (!base.IsOwner) return;
        if (releaseVector == Vector2.zero && holdTime > JoystickHandler.autoAimTime) return;

        Vector3 aimDirection = Vector3.zero;

        if (holdTime <= JoystickHandler.autoAimTime) 
        {
            Transform target = Constants.FindClosestTarget(transform, EntityTeam, 10f);
            if (target != null)
            {
                aimDirection = (target.position - transform.position).normalized;
                aimDirection.y = 0f;
            }
            else return; 
        }
        else
        {
            Vector3 camForward = Constants.MainCamera.transform.forward;
            camForward.y = 0f;
            Vector3 camRight = Constants.MainCamera.transform.right;
            camRight.y = 0f;
            
            aimDirection = (camForward.normalized * releaseVector.y + camRight.normalized * releaseVector.x).normalized;
        }

        ServerFireAttack2(aimDirection);
        Attack2Joy.AddCooldown(NeuroAttack2Cooldown);
    }

    [ServerRpc]
    private void ServerFireAttack2(Vector3 aimDirection)
    {
        float startOffset = 0.5f;
        Vector3 startPos = transform.position + (aimDirection * startOffset);
        
        if (attack2Prefab != null)
        {
            GameObject beamObj = Instantiate(attack2Prefab, startPos, Quaternion.LookRotation(aimDirection));
            NeuroBeam logic = beamObj.GetComponent<NeuroBeam>();
            if (logic != null)
            {
                logic.Setup(this, GetAttackValue(NeuroAttack2BaseDamage), NeuroAttack2Duration, NeuroAttack2Ticks, NeuroAttack2Width, NeuroAttack2Range, transform);
            }
            
            base.ServerManager.Spawn(beamObj);
        }
    }

    private void HandleUltRelease(Vector2 releaseVector, float holdTime)
    {
        if (!base.IsOwner) return;
        if (releaseVector == Vector2.zero && holdTime > JoystickHandler.autoAimTime) return;

        Vector3 aimDirection = Vector3.zero;

        if (holdTime <= JoystickHandler.autoAimTime) 
        {
            Transform target = Constants.FindClosestTarget(transform, EntityTeam, 10f);
            if (target != null)
            {
                aimDirection = (target.position - transform.position).normalized;
                aimDirection.y = 0f;
            }
            else return; 
        }
        else
        {
            Vector3 camForward = Constants.MainCamera.transform.forward;
            camForward.y = 0f;
            Vector3 camRight = Constants.MainCamera.transform.right;
            camRight.y = 0f;
            
            aimDirection = (camForward.normalized * releaseVector.y + camRight.normalized * releaseVector.x).normalized;
        }

        int randomSeed = Random.Range(int.MinValue, int.MaxValue);
        ServerFireUlt(aimDirection, randomSeed);
        UltJoy.AddCooldown(NeuroUltCooldown);
    }

    [ServerRpc]
    private void ServerFireUlt(Vector3 aimDirection, int seed)
    {
        Vector3 rightDirection = Vector3.Cross(Vector3.up, aimDirection).normalized;
        float startOffset = 0.5f;

        Vector3 startPos = transform.position + (aimDirection * startOffset);
        startPos.y = Telegraph.offset;

        Vector3 endPos = startPos + (aimDirection * NeuroUltRange);

        if (ultPrefab != null)
        {
            GameObject proj = Instantiate(ultPrefab, startPos, Quaternion.identity);
            NeuroShot logic = proj.GetComponent<NeuroShot>();
            if (logic != null)
            {
                logic.Setup(this, endPos, GetAttackValue(NeuroUltBaseDamage), NeuroUltSpeed, NeuroUltRange2, NeuroUltOvershoot, NeuroUltCount, seed);
            }
            
            base.ServerManager.Spawn(proj);
        }
    }
}