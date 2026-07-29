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
    
    // player assignment is not implemented, so I will assume the only character on the map is the player
    public bool isPlayer = true;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            MoveJoy = Constants.MoveJoy;
            Attack1Joy = Constants.Attack1Joy;
            Attack2Joy = Constants.Attack2Joy;
            UltJoy = Constants.UltJoy;

            CameraScript camScript = Camera.main.GetComponent<CameraScript>();
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

    protected override void Update()
    {
        if (isDead)
        {
            if (Time.time >= RespawnTime) Respawn();
            return;
        }

        if (base.IsOwner)
        {
            Vector2 input = MoveJoy.GetJoystickVector();
            Vector3 finalMoveDir = Vector3.zero;

            if (input != Vector2.zero)
            {
                Vector3 camForward = Camera.main.transform.forward;
                camForward.y = 0f;
                Vector3 camRight = Camera.main.transform.right;
                camRight.y = 0f;

                finalMoveDir = (camForward.normalized * input.y + camRight.normalized * input.x).normalized;
                finalMoveDir *= input.magnitude;
            }

            MoveData md = new MoveData();
            md.MoveDirection = finalMoveDir;
            MoveCharacter(md);
        }

        if (EntityAnimator != null && base.IsOwner)
        {
            EntityAnimator.SetFloat("Speed", MoveJoy.GetJoystickVector().magnitude);
        }
        base.Update();
    }

    [Replicate]
    private void MoveCharacter(MoveData md, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
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

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (base.IsServerInitialized)
        {
            base.TimeManager.OnPostTick += TimeManager_OnPostTick;
        }
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        if (base.TimeManager != null)
        {
            base.TimeManager.OnPostTick -= TimeManager_OnPostTick;
        }
    }

    private void TimeManager_OnPostTick()
    {
        CreateReconcile();
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

    public override void TakeDamage(float amount, int attackerTeam)
    {
        if (isDead) return;

        base.TakeDamage(amount, attackerTeam);
        
        if (EntityCurrentHP <= 0) Die();
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

    protected override IEnumerator KnockbackRoutine(Vector3 dir, float dist, float dur)
    {
        float timer = 0f;
        float speed = dist / dur;
        while (timer < dur)
        {
            if (CharController != null && CharController.enabled)
            {
                CharController.Move(dir * speed * Time.deltaTime);
            }
            timer += Time.deltaTime;
            yield return null;
        }
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


        EntityCurrentHP = EntityMaxHP;
        SetHP(1.0f);
        Attack1Joy.RemoveCooldown();
        Attack2Joy.RemoveCooldown();
        UltJoy.RemoveCooldown();
        
        Debug.Log(EntityName + " has respawned!");
    }
}
