using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_SkeletonAnimationTriggers : MonoBehaviour
{
    private Enemy_Skeleton enemy => GetComponentInParent<Enemy_Skeleton>();


    private void Awake()
    {

    }

    private void AnimationTrigger()
    {
        enemy.AnimationFinishTrigger();
    }

    private void AttackTrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(enemy.attackCheak.position, enemy.attackCheakRidus);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Player>() != null)
            {
                Player player = hit.GetComponent<Player>();

                if (player.TryStartPreciseDodge(enemy.transform))
                    return;

                PlayerStats target = hit.GetComponent<PlayerStats>();
                enemy.stats.DoDamage(target);
            }
        }

    }

  

    private IEnumerator CloseAttackWindowAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Enemy.isAnyEnemyAttacking = false;
    }

    private void OpenCounterWindow() => enemy.OpenCounterAttackWindow();
    private void CloseCounterWindow() => enemy.CloseCounterAttackWindow();
}
