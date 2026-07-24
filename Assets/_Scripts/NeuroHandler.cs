using UnityEngine;

public class NeuroHandler : CharacterHandler
{
    public GameObject attack1Prefab;
    public GameObject attack2Prefab;
    public GameObject ultPrefab;

    private readonly float NeuroBaseHP = 700.0f;
    private readonly float NeuroBaseSpeed = 3.0f;

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
        EntityCurrentHP = NeuroBaseHP;
        EntityBaseSpeed = NeuroBaseSpeed;
        EntityTeam = Constants.TEAM1;

        Attack1Joy.OnJoystickReleased += HandleAttack1Release;
        Attack2Joy.OnJoystickReleased += HandleAttack2Release;
        UltJoy.OnJoystickReleased += HandleUltRelease;
    }
   
    protected override void Update()
    {
        base.Update();

        if (CameraScript.CurrentPlayerMode == EntityName)
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
        if (CameraScript.CurrentPlayerMode != EntityName) return;
        if (releaseVector == Vector2.zero && holdTime > JoystickHandler.autoAimTime)
        {
            Debug.Log("attack cancelled");
            return;
        }

        Vector3 aimDirection = Vector3.zero;

        if (holdTime <= JoystickHandler.autoAimTime) 
        {
            Transform target = Constants.FindClosestTarget(transform, EntityTeam, 10f, true, false);
            if (target != null)
            {
                aimDirection = (target.position - transform.position).normalized;
                aimDirection.y = 0f;
            }
            else 
            {
                return; 
            }
        }
        else
        {
            Vector3 camForward = MainCamera.transform.forward;
            camForward.y = 0f;
            Vector3 camRight = MainCamera.transform.right;
            camRight.y = 0f;
            
            aimDirection = (camForward.normalized * releaseVector.y + camRight.normalized * releaseVector.x).normalized;
        }

        FireAttack1(aimDirection);
        Attack1Joy.AddCooldown(NeuroAttack1Cooldown);
    }

    private void SpawnProjectile(Vector3 startPos, Vector3 endPos, float damage)
    {
        if (attack1Prefab != null)
        {
            GameObject proj = Instantiate(attack1Prefab, startPos, Quaternion.identity);
            NeuroShot logic = proj.GetComponent<NeuroShot>();
            if (logic != null)
            {
                logic.Setup(endPos, EntityTeam, damage, NeuroAttack1Speed, NeuroAttack1Range2, NeuroAttack1Overshoot, 1);
            }
        }
    }

    private void FireAttack1(Vector3 aimDirection)
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

        SpawnProjectile(leftStart, leftEnd, totalDamage);
        SpawnProjectile(baseStart, baseEnd, totalDamage);
        SpawnProjectile(rightStart, rightEnd, totalDamage);
    }

    private void HandleAttack2Release(Vector2 releaseVector, float holdTime)
    {
        if (CameraScript.CurrentPlayerMode != EntityName) return;
        if (releaseVector == Vector2.zero && holdTime > JoystickHandler.autoAimTime)
        {
            Debug.Log("attack cancelled");
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
            else 
            {
                return; 
            }
        }
        else
        {
            Vector3 camForward = MainCamera.transform.forward;
            camForward.y = 0f;
            Vector3 camRight = MainCamera.transform.right;
            camRight.y = 0f;
            
            aimDirection = (camForward.normalized * releaseVector.y + camRight.normalized * releaseVector.x).normalized;
        }

        FireAttack2(aimDirection);
        Attack2Joy.AddCooldown(NeuroAttack2Cooldown);
    }

    private void FireAttack2(Vector3 aimDirection)
    {
        float startOffset = 0.5f;
        Vector3 startPos = transform.position + (aimDirection * startOffset);
        
        if (attack2Prefab != null)
        {
            GameObject beamObj = Instantiate(attack2Prefab, startPos, Quaternion.LookRotation(aimDirection));
            
            // beamObj.transform.SetParent(this.transform);

            NeuroBeam logic = beamObj.GetComponent<NeuroBeam>();
            if (logic != null)
            {
                logic.Setup(EntityTeam, GetAttackValue(NeuroAttack2BaseDamage), NeuroAttack2Duration, NeuroAttack2Ticks, NeuroAttack2Width, NeuroAttack2Range, transform);
            }
        }
    }

    private void HandleUltRelease(Vector2 releaseVector, float holdTime)
    {
        if (CameraScript.CurrentPlayerMode != EntityName) return;
        if (releaseVector == Vector2.zero && holdTime > JoystickHandler.autoAimTime)
        {
            Debug.Log("attack cancelled");
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
            else 
            {
                return; 
            }
        }
        else
        {
            Vector3 camForward = MainCamera.transform.forward;
            camForward.y = 0f;
            Vector3 camRight = MainCamera.transform.right;
            camRight.y = 0f;
            
            aimDirection = (camForward.normalized * releaseVector.y + camRight.normalized * releaseVector.x).normalized;
        }

        FireUlt(aimDirection);
        UltJoy.AddCooldown(NeuroUltCooldown);
    }

    private void FireUlt(Vector3 aimDirection)
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
                logic.Setup(endPos, EntityTeam, GetAttackValue(NeuroUltBaseDamage), NeuroUltSpeed, NeuroUltRange2, NeuroUltOvershoot, NeuroUltCount);
            }
        }
    }
}
