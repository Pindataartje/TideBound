using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
public class AiBehavoir : MonoBehaviour
{
    [Header("AI Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float detectionRange = 10f;
    public float chaseRange = 15f;
    public float attackRange = 2f;
    public float patrolRadius = 10f;

    [Header("References")]
    public Transform player;
    public TMP_Text stateText; // Reference to TMP Text UI element

    private NavMeshAgent agent;
    private Vector3 patrolTarget;
    private bool isChasing = false;
    private bool isAttacking = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        SetNewPatrolTarget();
        agent.speed = walkSpeed;
        UpdateStateText("Patrolling");
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            Attack();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            StartChasing();
        }
        else if (distanceToPlayer > chaseRange && isChasing)
        {
            StopChasing();
        }

        if (!isChasing && agent.remainingDistance < 1f)
        {
            SetNewPatrolTarget();
        }
    }

    void StartChasing()
    {
        if (!isChasing)
        {
            isChasing = true;
            agent.speed = runSpeed;
            UpdateStateText("Chasing");
        }
        agent.SetDestination(player.position);
    }

    void StopChasing()
    {
        isChasing = false;
        agent.speed = walkSpeed;
        SetNewPatrolTarget();
        UpdateStateText("Patrolling");
    }

    void Attack()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            Debug.Log("Hit");
            UpdateStateText("Attacking");
            Invoke("ResetAttack", 1f);
        }
    }

    void ResetAttack()
    {
        isAttacking = false;
    }

    void SetNewPatrolTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            patrolTarget = hit.position;
            agent.SetDestination(patrolTarget);
        }
        UpdateStateText("Patrolling");
    }

    void UpdateStateText(string state)
    {
        if (stateText != null)
        {
            stateText.text = "AI State: " + state;
        }
    }
}
