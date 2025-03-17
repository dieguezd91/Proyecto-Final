using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Settings")]
    public float moveSpeed = 3f;
    public float detectionDistance = 10f;
    public float attackDistance = 1.5f;
    public float damage = 10f;
    public float attackCooldown = 1f;

    [Header("State")]
    public bool canAttack = true;
    public bool chasingPlayer = false;

    private Rigidbody2D rb;
    private Vector2 direction;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    void Update()
    {
        if (player == null)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionDistance)
        {
            chasingPlayer = true;

            direction = (player.position - transform.position).normalized;

            if (distanceToPlayer <= attackDistance && canAttack)
            {
                Attack();
            }
        }
        else
        {
            chasingPlayer = false;
            direction = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        if (chasingPlayer)
        {
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);

            if (direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                rb.rotation = angle;
            }
        }
    }

    void Attack()
    {
        Debug.Log("Enemy attacking player!");

        LifeController playerHealth = player.GetComponent<LifeController>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        StartCoroutine(AttackCooldown());
    }

    IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}