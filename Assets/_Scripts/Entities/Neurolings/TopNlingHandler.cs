using UnityEngine;

public class TopNlingHandler : NlingHandler
{
    private readonly float TopBaseHP = 500.0f;
    private readonly float TopBaseSpeed = 2.5f;
    private readonly float TopAttackRadius = 2.5f;
    private readonly float TopAttackDamage = 7.5f;
    private readonly float TopCooldown = 1f;
    private readonly float TopProjectileSpeed = 5.0f;

    protected void Awake()
    {
        EntityName = "StreamNling";
        EntityBaseHP = TopBaseHP;
        EntityMaxHP = TopBaseHP;
        EntityBaseSpeed = TopBaseSpeed;
        SetHPValue(TopBaseHP);
        EntityTeam = Constants.WILD; 
        
        NlingAttackRadius = TopAttackRadius;
        NlingAttackDamage = TopAttackDamage;
        NlingCooldown = TopCooldown;
        NlingProjectileVelocity = TopProjectileSpeed;
    }
}