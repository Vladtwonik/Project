﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleAnimations : MonoBehaviour
{
    public Animator anim;
    StateManager states;

    public float attackRate = .3f; //с какой скоростью мы можем атаковать, короче - скорость атаки
    public AttacksBase[] attacks = new AttacksBase[2];

    void Start()
    {
        states = GetComponent<StateManager>();
        anim = GetComponentInChildren<Animator>();
    }

    void FixedUpdate()
    {
        states.dontMove = anim.GetBool("DontMove");

        anim.SetBool("TakesHit", states.gettingHit);
        anim.SetBool("OnAir", !states.onGround);
        anim.SetBool("Crouch", states.crouch);

        float movement = Mathf.Abs(states.horizontal);
        anim.SetFloat("Movement", movement);

        if(states.vertical < 0) //КАК РАБОТАЕТ VERTICAL И HORIZONTAL, посмотреть еще раз
        {
            states.crouch = true;
        }
        else
        {
            states.crouch = false;
        }

        HandleAttacks();
    }

    void HandleAttacks() //если хочешь добавить варианты атак, то их необходимо добавлять здесь
    {
        if(states.canAttack)
        {
            if(states.attack1)
            {
                attacks[0].attack = true;
                attacks[0].attackTimer = 0;
                attacks[0].timesPressed++;
            }

            if(attacks[0].attack)
            {
                attacks[0].attackTimer += Time.deltaTime;

                if(attacks[0].attackTimer > attackRate || attacks[0].timesPressed >= 3)
                {
                    attacks[0].attack = false;
                    attacks[0].attackTimer = 0;
                    attacks[0].timesPressed++;
                }
            }

            if(states.attack2)
            {
                attacks[1].attack = true;
                attacks[1].attackTimer = 0;
                attacks[1].timesPressed++;
            }

            if (attacks[1].attack)
            {
                attacks[1].attackTimer += Time.deltaTime;

                if (attacks[1].attackTimer > attackRate || attacks[0].timesPressed >= 3)
                {
                    attacks[1].attack = false;
                    attacks[1].attackTimer = 0;
                    attacks[1].timesPressed++;
                }
            }
        }

        anim.SetBool("Attack1", attacks[0].attack);
        anim.SetBool("Attack2", attacks[1].attack);
    }

    public void JumpAnim()
    {
        anim.SetBool("Attack1", false);
        anim.SetBool("Attack2", false);
        anim.SetBool("Jump", true);
        StartCoroutine(CloseBoolInAnim("Jump"));
    }

    IEnumerator CloseBoolInAnim(string name)
    {
        yield return new WaitForSeconds(0.5f);
        anim.SetBool(name, false);
    }
}

[System.Serializable]
public class AttacksBase
{
    public bool attack;
    public float attackTimer;
    public int timesPressed;
}