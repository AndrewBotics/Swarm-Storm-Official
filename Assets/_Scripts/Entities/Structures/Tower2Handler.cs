using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Tower2Handler : StructureHandler
{
    private readonly float Tower2BaseHP = 20000f;
    private readonly float Tower2BaseSpeed = 0f;
    private readonly float Tower2AttackRadius = 3.125f;
    private readonly float Tower2AttackDamage = 75f;

    protected override void Start()
    {
        base.Start();
        EntityName = "Tower2";
        EntityBaseHP = Tower2BaseHP;
        EntityMaxHP = Tower2BaseHP;
        SetHPValue(Tower2BaseHP);
        EntityBaseSpeed = Tower2BaseSpeed;
        StructureAttackRadius = Tower2AttackRadius;
        StructureAttackDamage = Tower2AttackDamage;
    }
}