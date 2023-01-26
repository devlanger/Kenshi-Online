using System;
using System.Collections;
using System.Collections.Generic;
using Kenshi.Utils;
using StarterAssets;
using StarterAssets.CombatStates;
using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    public enum DamageType
    {
        AOE = 0,
        DIRECT = 1
    }

    public bool damageOwner = true;
    public DamageType type = DamageType.DIRECT;
    public AttackState.DamageData.HitType hitType = AttackState.DamageData.HitType.heavy;
    public ushort damage = 25;
    public float distance = 3;
    public float throwBack = 10;
    private TriggerCollisionHandler collisionHandler;
    private Collider collisionTarget;

    private void Awake()
    {
        collisionHandler = GetComponent<TriggerCollisionHandler>();
    }


    public void Trigger(Collider other)
    {
        if (!GameServer.IsServer)
        {
            return;
        }
        
        collisionTarget = other;
        
        switch (type)
        {
            case DamageType.AOE:
                AoeDamage();
                break;
            case DamageType.DIRECT:
                DirectDamage();
                break;
        }
    }

    private void DirectDamage()
    {
        Debug.Log($"Direct damage {collisionTarget}");
        if (collisionTarget != null && collisionTarget.TryGetComponent<Player>(out var player))
        {
            if (!damageOwner && player.NetworkId == collisionHandler.owner.NetworkId)
            {
                return;
            }
            
            TriggerHit(player);
        }
    }

    private void AoeDamage()
    {
        var players = PlayerUtils.GetPlayersAtPos(transform.position, distance);
        foreach (var player in players)
        {
            if (!damageOwner && player.NetworkId == collisionHandler.owner.NetworkId)
            {
                continue;
            }
            
            TriggerHit(player);
        }
    }

    private void TriggerHit(Player player)
    {
        CombatController.Instance.HitSingleTarget(new AttackState.DamageData
        {
            attacker = collisionHandler.owner,
            hitTarget = player,
            direction = (player.transform.position - transform.position).normalized * throwBack,
            damage = damage,
            hitType = hitType
        });
    }

}
