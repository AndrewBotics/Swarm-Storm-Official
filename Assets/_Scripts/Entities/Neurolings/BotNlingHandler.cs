using UnityEngine;

public class BotNlingHandler : NlingHandler
{
    private readonly float BotBaseHP = 250.0f;
    private readonly float BotBaseSpeed = 2.5f;
    private readonly float BotAttackRadius = 2.5f;
    private readonly float BotAttackDamage = 25f;
    private readonly float BotCooldown = 1.5f;
    private readonly float BotProjectileSpeed = 5.0f;

    protected void Awake()
    {
        EntityName = "StreamNling";
        EntityBaseHP = BotBaseHP;
        EntityMaxHP = BotBaseHP;
        EntityBaseSpeed = BotBaseSpeed;
        SetHPValue(BotBaseHP);
        EntityTeam = Constants.WILD; 
        
        NlingAttackRadius = BotAttackRadius;
        NlingAttackDamage = BotAttackDamage;
        NlingCooldown = BotCooldown;
        NlingProjectileVelocity = BotProjectileSpeed;
    }
}