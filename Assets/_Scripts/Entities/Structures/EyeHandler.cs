using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EyeHandler : StructureHandler
{
    // aka the nexus from league, kill this as the win condition
    // or just do more damage before time is up
    private readonly float EyeBaseHP = 50000f;
    private readonly float EyeBaseSpeed = 0f;
    private readonly float EyeAttackRadius = 4.375f;
    private readonly float EyeAttackDamage = 200f;
    
    protected override void Start()
    {
        base.Start();
        EntityName = "Eye";
        EntityBaseHP = EyeBaseHP;
        EntityMaxHP = EyeBaseHP;
        SetHPValue(EyeBaseHP);
        EntityBaseSpeed = EyeBaseSpeed;
        StructureAttackRadius = EyeAttackRadius;
        StructureAttackDamage = EyeAttackDamage;
    }
}