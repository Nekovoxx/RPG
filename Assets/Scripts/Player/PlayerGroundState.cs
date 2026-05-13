using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerGroundState : PlayerState
{
    public PlayerGroundState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Mouse1) && HasNoSword())
            stateMachine.ChangeState(player.aimSword);

        if (Input.GetKeyDown(KeyCode.Q))
            stateMachine.ChangeState(player.counterAttack);

        if(Input.GetKeyDown(KeyCode.Mouse0)) 
            stateMachine.ChangeState(player.primaryAttack);

        if(!player.IsGroundDetected())
            stateMachine.ChangeState(player.airState);

        if (Input.GetKeyDown(KeyCode.Space)&& player.IsGroundDetected()) 
            stateMachine.ChangeState(player.jumpState);

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (Inventory.instance.CanUseFlask())
            {
                stateMachine.ChangeState(player.healState);
            }
            else
            {
                Debug.Log("不能使用药瓶（没装备或在冷却中）");
            }
        }
    }

    private bool HasNoSword()
    {
        if(!player.sword)
        {
            return true;
        }

        player.sword.GetComponent<Sword_Skill_Contorller>().ReturnSword(); 
        return false ;
    }
}
