using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Tower1Handler : StructureHandler
{
    private readonly float Tower1BaseHP = 10000f;
    private readonly float Tower1BaseSpeed = 0f;
    private readonly float Tower1AttackRadius = 2.5f;
    private readonly float Tower1AttackDamage = 30f;
    private float Tower1Cooldown = 1f;
    
    protected override void Start()
    {
        base.Start();
        EntityName = "Tower1";
        EntityBaseHP = Tower1BaseHP;
        EntityMaxHP = Tower1BaseHP;
        SetHPValue(Tower1BaseHP);
        EntityBaseSpeed = Tower1BaseSpeed;
        StructureAttackRadius = Tower1AttackRadius;
        StructureAttackDamage = Tower1AttackDamage;
        StructureCooldown = Tower1Cooldown;
    }
}