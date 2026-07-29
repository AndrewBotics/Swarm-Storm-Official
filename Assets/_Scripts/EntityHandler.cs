using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FishNet.Object;

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
    protected string EntityName;
    protected float EntityBaseHP;
    protected float EntityBaseSpeed;
    [HideInInspector] public int EntityTeam;

    // Instance States
    protected int EntityLevel = 1;
    protected float EntityHPMultiplier = 1.0f;
    protected float EntityAttackMultiplier = 1.0f;
    protected float EntityCurrentHP;
    protected float EntityMaxHP;

    // Statuses
    // protected float EntityAttackBoost = 1.0f;

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
        SetHP(EntityCurrentHP/EntityMaxHP);
    }

    protected float GetHPValue(){
        return EntityCurrentHP;
    }

    protected float GetAttackValue(float basePower){
        return basePower * EntityAttackMultiplier;
    }

    protected float GetDefenseValue(){
        return 1.0f;
    }

    protected float GetSpeedValue(){
        return EntityBaseSpeed;
    }

    public float GetMaxHP()
    {
        return EntityMaxHP; 
    }

    public void LevelUp(){
        EntityLevel++;
        EntityHPMultiplier += 0.15f;
        float _minus = EntityMaxHP - EntityCurrentHP;
        EntityMaxHP = EntityBaseHP * EntityHPMultiplier;
        EntityCurrentHP = EntityMaxHP - _minus;
        SetHP(EntityCurrentHP/EntityMaxHP);

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
        EntityCurrentHP -= amount / GetDefenseValue(); 
        
        SetHP(EntityCurrentHP / EntityMaxHP);
    }

    protected abstract void Die();

    public virtual void ApplyKnockback(Vector3 direction, float distance, float duration)
    {
        StartCoroutine(KnockbackRoutine(direction, distance, duration));
    }

    protected abstract IEnumerator KnockbackRoutine(Vector3 dir, float dist, float dur);
}
