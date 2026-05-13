using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clone_Skill : MonoBehaviour
{

    private Player player;

    [Header("Clone Info")]
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private float cloneDuration;
    [Space]
    [SerializeField] private bool canAttack;

    private void Awake()
    {
        player = FindObjectOfType<Player>();
    }
    public void CreateClone(Transform _clonePosition)
    {
        GameObject newClone = Instantiate(clonePrefab);

        newClone.GetComponent<CloneSkill_Controller>().SetupClone(_clonePosition, cloneDuration, canAttack, player);
    }
}
