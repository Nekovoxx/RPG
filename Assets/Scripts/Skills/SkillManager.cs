using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager instance;
    public Dash_Skill dash { get; private set; }
    public Sword_Skill sword { get; private set; }
    public Parry_Skill parry { get; private set; }  
    public PreciseDodge_Skill preciseDodge { get; private set; }


    private void Awake()
    {
        if(instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;

        preciseDodge = GetComponent<PreciseDodge_Skill>();

        if (preciseDodge == null)
            preciseDodge = gameObject.AddComponent<PreciseDodge_Skill>();
    }

    private void Start()
    {
        dash = GetComponent<Dash_Skill>();
        sword = GetComponent<Sword_Skill>();    
        parry = GetComponent<Parry_Skill>();

        if (preciseDodge == null)
            preciseDodge = GetComponent<PreciseDodge_Skill>();
    }
}
