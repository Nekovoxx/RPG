using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneSkill_Controller : MonoBehaviour
{
    private SpriteRenderer sr;
    private Animator anim;

    private Player player;
    [SerializeField] private float colorLoosingSpeed;

    private float cloneTimer;
    [SerializeField] private Transform attackCheak;
    [SerializeField] private float attackCheakRiadius = 0.8f;
    private Transform closestEnemy;


    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        cloneTimer -= Time.deltaTime;

        if (cloneTimer < 0)
        {
            sr.color = new Color(1, 1, 1, sr.color.a - (Time.deltaTime * colorLoosingSpeed));

            if(sr.color.a <= 0)
                Destroy(gameObject);
        }
    }

    public void SetupClone(Transform _newTransform, float _cloneDuration, bool _canAttack, Player _player)
    {
        player = _player;

        if (_canAttack)
            anim.SetInteger("AttackNumber", Random.Range(1, 3));


        transform.position = new Vector2( _newTransform.position.x,_newTransform.position.y - 1.4f);
        cloneTimer = _cloneDuration;

        FaceClosestTarget();
    }

    private void AnimationTrigger()
    {
        cloneTimer = -0.1f;
    }
    private void AttackTrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attackCheak.position, attackCheakRiadius);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                EnemyStats _target = hit.GetComponent<EnemyStats>();

                if (_target != null)
                    player.stats.DoDamage(_target);

                ItemData_Equipment weaponData = Inventory.instance.GetEquipment(EquipmentType.Weapon);

                if (weaponData != null)
                    weaponData.Effect(_target.transform);
            }
        }
    }
    private void FaceClosestTarget()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 25);

        float closestDistance = Mathf.Infinity;

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                float distanceToEnemy = Vector2.Distance(transform.position, hit.transform.position);

                if (distanceToEnemy < closestDistance)
                {
                    closestDistance = distanceToEnemy;
                    closestEnemy = hit.transform;
                }
            }
        }

        if (closestEnemy != null)
        {
            if (transform.position.x > closestEnemy.position.x)
                transform.Rotate(0, 180, 0);
        }
    }
    private void OnDrawGizmos()
    {
        if (attackCheak == null)
            return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            attackCheak.position,
            attackCheakRiadius
        );
    }
}
