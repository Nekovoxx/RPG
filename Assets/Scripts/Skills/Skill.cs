using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    public  float cooldown;
    protected float cooldownTimer;

    protected Player player;
    protected virtual  void Start()
    {
        player = PlayerManager.instance.player;
    }
    protected virtual void Update()
    {
        cooldownTimer -= Time.deltaTime;  
    }

    public bool IsReady() => cooldownTimer <= 0;
    public float CooldownRemaining => Mathf.Max(cooldownTimer, 0);

    protected void StartCooldown() => cooldownTimer = cooldown;

    public virtual bool CanUseSkill()
    {
        if(IsReady())
        {
            UseSkill();
            StartCooldown();
            return true;
        }
        Debug.Log("技能正在冷却");
        return false;
        
    }

    public virtual void UseSkill()
    {

    }

    

}
