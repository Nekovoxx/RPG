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
    public Sun_Skill sun { get; private set; }
    public Awakening_Skill awakening { get; private set; }
    public Invisibility_Skill invisibility { get; private set; }


    private void Awake()
    {
        if(instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;

        preciseDodge = GetComponent<PreciseDodge_Skill>();
        sun = GetComponent<Sun_Skill>();
        awakening = GetComponent<Awakening_Skill>();
        invisibility = GetComponent<Invisibility_Skill>();

        if (preciseDodge == null)
            preciseDodge = gameObject.AddComponent<PreciseDodge_Skill>();

        if (awakening == null)
            awakening = gameObject.AddComponent<Awakening_Skill>();

        if (invisibility == null)
            invisibility = gameObject.AddComponent<Invisibility_Skill>();
    }

    private void Start()
    {
        dash = GetComponent<Dash_Skill>();
        sword = GetComponent<Sword_Skill>();    
        parry = GetComponent<Parry_Skill>();

        if (preciseDodge == null)
            preciseDodge = GetComponent<PreciseDodge_Skill>();

        sun = GetComponent<Sun_Skill>();
        awakening = GetComponent<Awakening_Skill>();
        invisibility = GetComponent<Invisibility_Skill>();
    }
}
