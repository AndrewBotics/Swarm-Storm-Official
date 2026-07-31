using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FishNet.Object.Prediction;
using FishNet.Transporting;

public abstract class CharacterHandler : EntityHandler
{
    [SerializeField] protected CharacterController CharController;

    // UI References
    protected JoystickHandler MoveJoy;
    protected JoystickHandler Attack1Joy;
    protected JoystickHandler Attack2Joy;
    protected JoystickHandler UltJoy;

    // Respawn
    protected float RespawnCooldown = 1.0f;
    protected float RespawnTime;
    [HideInInspector] public bool isDead = false;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            MoveJoy = Constants.MoveJoy;
            Attack1Joy = Constants.Attack1Joy;
            Attack2Joy = Constants.Attack2Joy;
            UltJoy = Constants.UltJoy;

            CameraScript camScript = Constants.MainCamera.GetComponent<CameraScript>();
            if (camScript != null)
            {
                camScript.SetTarget(this.transform);
            }
            else
            {
                Debug.LogWarning("CameraScript not found on Main Camera!");
            }
        }
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        
        if (base.IsServerInitialized || base.Owner.IsLocalClient)
        {
            base.TimeManager.OnTick += TimeManager_OnTick;
            base.TimeManager.OnPostTick += TimeManager_OnPostTick;
        }
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        if (base.TimeManager != null)
        {
            base.TimeManager.OnTick -= TimeManager_OnTick;
            base.TimeManager.OnPostTick -= TimeManager_OnPostTick;
        }
    }

    protected override void Update()
    {
        if (isDead)
        {
            if (Time.time >= RespawnTime) Respawn();
            return;
        }

        if (EntityAnimator != null && base.IsOwner)
        {
            EntityAnimator.SetFloat("Speed", MoveJoy.GetJoystickVector().magnitude);
        }
        base.Update();
    }

    private void TimeManager_OnTick()
    {
        if (isDead) return;

        if (knockbackTimer > 0)
        {
            knockbackTimer -= (float) base.TimeManager.TickDelta;
        }

        if (base.IsOwner)
        {
            Vector2 input = MoveJoy.GetJoystickVector();
            Vector3 finalMoveDir = Vector3.zero;

            if (knockbackTimer <= 0 && input != Vector2.zero)
            {
                Vector3 camForward = Constants.MainCamera.transform.forward;
                camForward.y = 0f;
                Vector3 camRight = Constants.MainCamera.transform.right;
                camRight.y = 0f;

                finalMoveDir = (camForward.normalized * input.y + camRight.normalized * input.x).normalized;
                finalMoveDir *= input.magnitude;
            }

            MoveData md = new MoveData();
            md.MoveDirection = finalMoveDir;
            MoveCharacter(md);
        }
    }

    [Replicate]
    private void MoveCharacter(MoveData md, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        if (knockbackTimer > 0)
        {
            CharController.Move(currentKnockbackVelocity * (float) base.TimeManager.TickDelta);
            return;
        }

        if (md.MoveDirection != Vector3.zero)
        {
            Vector3 scaledMovement = md.MoveDirection * GetSpeedValue() * (float)base.TimeManager.TickDelta;
            if (scaledMovement != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(scaledMovement);
            }
            CharController.Move(scaledMovement);
        }
    }

    private void TimeManager_OnPostTick()
    {
        if (base.IsServerInitialized) CreateReconcile();
    }

    public override void CreateReconcile()
    {
        ReconcileData rd = new ReconcileData();
        rd.Position = transform.position;
        rd.Rotation = transform.rotation;
        
        ReconcileCharacter(rd);
    }

    [Reconcile]
    private void ReconcileCharacter(ReconcileData rd, Channel channel = Channel.Unreliable)
    {
        transform.position = rd.Position;
        transform.rotation = rd.Rotation;
    }

    public override void TakeDamage(float amount, EntityHandler attacker)
    {
        if (isDead) return;

        base.TakeDamage(amount, attacker);
        
        if (GetHPValue() <= 0) Die();
    }

    protected override void Die()
    {
        isDead = true;
        RespawnTime = Time.time + RespawnCooldown;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (!r.gameObject.name.Contains("Telegraph")) r.enabled = false;
        }

        Canvas[] canvases = GetComponentsInChildren<Canvas>();
        foreach (Canvas c in canvases)
        {
            c.enabled = false;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            c.enabled = false;
        }

        if (EntityTelegraph != null) EntityTelegraph.ClearPreviews();
        
        Debug.Log(EntityName + " died!");
    }

    public void Respawn()
    {
        isDead = false;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            c.enabled = true;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (!r.gameObject.name.Contains("Telegraph")) r.enabled = true;
        }

        Canvas[] canvases = GetComponentsInChildren<Canvas>();
        foreach (Canvas c in canvases)
        {
            c.enabled = true;
        }

        CharController.enabled = false;
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
        CharController.enabled = true;


        SetHPValue(EntityMaxHP);
        SetHP(1.0f);
        Attack1Joy.RemoveCooldown();
        Attack2Joy.RemoveCooldown();
        UltJoy.RemoveCooldown();
        
        Debug.Log(EntityName + " has respawned!");
    }
}
