using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    protected override void Start()
    {
        base.Start();
        MoveJoy = Constants.MoveJoy;
        Attack1Joy = Constants.Attack1Joy;
        Attack2Joy = Constants.Attack2Joy;
        UltJoy = Constants.UltJoy;
    }

    protected override void Update()
    {
        if (isDead)
        {
            if (Time.time >= RespawnTime) Respawn();
            return;
        }

        if (CameraScript.CurrentPlayerMode == EntityName)
        {
            Vector2 MovementAmount = MoveJoy.GetJoystickVector();
            
            if (MovementAmount != Vector2.zero) 
            {
                Vector3 moveDir = MoveJoy.GetCameraRelativeDirection(); 
                Vector3 scaledMovement = moveDir * MovementAmount.magnitude * GetSpeedValue() * Time.deltaTime;

                if (scaledMovement != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(scaledMovement);
                }
                CharController.Move(scaledMovement);
            }

            if (EntityAnimator != null)
            {
                EntityAnimator.SetFloat("Speed", MovementAmount.magnitude);
            }
        }
        base.Update();
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
