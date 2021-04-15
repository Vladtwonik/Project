using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public float attackDamage = 20;

    void Start(){

    }
    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            Attack();
        }
    }

    void Attack(){
        //Анимация атаки
        animator.SetTrigger("Attack");

        //Обнаружение врагов в радиусе удара
        Vector2 ap = attackPoint.position;
        Collider2D[] hitEnemys = Physics2D.OverlapCircleAll(ap, attackRange, enemyLayers);
           

        //Нанести урон
        foreach(Collider2D enemy in hitEnemys){
            Debug.Log("Knight hit " + enemy.name);
            enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
        }
    }

    void OnDrawGizmosSelected(){
        if (attackPoint == null){
            return;
        }

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
