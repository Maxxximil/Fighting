using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class AttackController : NetworkBehaviour
{
    public Transform AttackPoint;
    public LayerMask DamageableLayerMask;
    public float Damage;
    public float AttackRadius;

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isOwned)
        {
            _animator = GetComponent<Animator>();

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                Attack();
                _animator.SetTrigger("Attack");
            }
        }

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(AttackPoint.position, AttackRadius);
    }
    public void Attack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(AttackPoint.position, AttackRadius, DamageableLayerMask);
        if (enemies.Length > 0)
        {
            Debug.Log("DealDamage");
            //for (int i = 0; i < enemies.Length; i++)
            //{
            //    //enemies[i].GetComponent<>().TakeDamage(Damage);
            //}
        }
    }
}
