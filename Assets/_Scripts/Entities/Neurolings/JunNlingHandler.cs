using UnityEngine;

public class JunNlingHandler : NlingHandler
{
    private readonly float JunBaseHP = 375.0f;
    private readonly float JunBaseSpeed = 3.75f;
    private readonly float JunAttackRadius = 2.5f;
    private readonly float JunAttackDamage = 2f;
    private readonly float JunCooldown = 0.25f;
    private readonly float JunProjectileSpeed = 5.0f;

    protected void Awake()
    {
        EntityName = "StreamNling";
        EntityBaseHP = JunBaseHP;
        EntityMaxHP = JunBaseHP;
        EntityBaseSpeed = JunBaseSpeed;
        SetHPValue(JunBaseHP);
        EntityTeam = Constants.WILD; 
        
        NlingAttackRadius = JunAttackRadius;
        NlingAttackDamage = JunAttackDamage;
        NlingCooldown = JunCooldown;
        NlingProjectileVelocity = JunProjectileSpeed;
    }
}