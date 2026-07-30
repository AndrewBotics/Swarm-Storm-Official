using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public abstract class EntityHandler : NetworkBehaviour
{
    // UI References
    protected Camera MainCamera;
    [SerializeField] protected Animator EntityAnimator;
    [SerializeField] protected Image EntityHPBar;
    protected Telegraph EntityTelegraph;
    protected Vector3 spawnPosition;
    protected Quaternion spawnRotation;

    // Stats
    [HideInInspector] public string EntityName;
    protected float EntityBaseHP;
    protected float EntityBaseSpeed;
    [HideInInspector] public int EntityTeam;

    // Instance States
    protected int EntityLevel = 1;
    protected float EntityHPMultiplier = 1.0f;
    protected float EntityAttackMultiplier = 1.0f;
    protected readonly SyncVar<float> EntityCurrentHP = new SyncVar<float>();
    protected float EntityMaxHP;

    // Statuses
    protected Vector3 currentKnockbackVelocity;
    protected float knockbackTimer;

    protected virtual void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        MainCamera = Constants.MainCamera;
        EntityTelegraph = GetComponent<Telegraph>();
        SetHP(1.0f);
    }

    protected virtual void Update()
    {
        SetHP(GetHPValue()/EntityMaxHP);
    }

    public void SetTeam(int team)
    {
        EntityTeam = team;
    }

    protected void SetHPValue(float f)
    {
        EntityCurrentHP.Value = f;
    }

    public float GetHPValue()
    {
        return EntityCurrentHP.Value;
    }

    public void ChangeHPValue(float f)
    {
        SetHPValue(GetHPValue()+f);
        if (GetHPValue()>EntityMaxHP) SetHPValue(EntityMaxHP);
    }

    public float GetHPPercent()
    {
        return GetHPValue()/EntityMaxHP;
    }

    public float GetAttackValue(float basePower){
        return basePower * EntityAttackMultiplier;
    }

    public float GetDefenseValue(){
        return 1.0f;
    }

    public float GetSpeedValue(){
        return EntityBaseSpeed;
    }

    public float GetMaxHP()
    {
        return EntityMaxHP; 
    }

    public void LevelUp(){
        EntityLevel++;
        EntityHPMultiplier += 0.15f;
        float _minus = EntityMaxHP - GetHPValue();
        EntityMaxHP = EntityBaseHP * EntityHPMultiplier;
        SetHPValue(EntityMaxHP - _minus);
        SetHP(GetHPValue()/EntityMaxHP);

        EntityAttackMultiplier += 0.1f;
    }

    protected void SetHP(float percent)
    {
        if (percent>1) percent = 1;
        if (percent<0) percent = 0;
        EntityHPBar.fillAmount = percent;
    }

    protected void SetHP(float currentHP, float maxHP)
    {
        SetHP(currentHP/maxHP);
    }

    public virtual void TakeDamage(float amount, int attackerTeam)
    {
        ChangeHPValue(-amount/GetDefenseValue()); 
        
        SetHP(GetHPValue()/EntityMaxHP);
        //Debug.Log(EntityName +" took "+amount+" damage!");
    }

    public virtual void Heal(float amount)
    {
        ChangeHPValue(amount);
        SetHP(GetHPValue()/EntityMaxHP);
    }

    protected abstract void Die();

    public virtual void ApplyKnockback(Vector3 direction, float distance, float duration)
    {
        if (duration <= 0) return;
        currentKnockbackVelocity = direction * (distance/duration);
        knockbackTimer = duration;
    }
}
