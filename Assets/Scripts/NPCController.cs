using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class NPCController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float wanderRadius = 20f;
    [SerializeField] private float minWaitTime = 2f;
    [SerializeField] private float maxWaitTime = 10f;

    [Header("Behavior Settings")]
    [SerializeField] private bool canRun = true;
    [SerializeField] private float runChance = 0.2f; 
    [SerializeField] private float fleeDistance = 10f; 
    [Header("Animation (Optional)")]
    [SerializeField] private Animator animator;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private bool isWaiting = false;
    private bool isFleeing = false;
    private float currentSpeed;

    private enum NPCState
    {
        Idle,
        Walking,
        Running,
        Fleeing
    }

    private NPCState currentState = NPCState.Idle;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        
     
        currentSpeed = walkSpeed;
        agent.speed = currentSpeed;

        StartCoroutine(WanderRoutine());
    }

    void Update()
    {
        // Cập nhật animation nếu có
        UpdateAnimation();

        // Kiểm tra xem đã đến đích chưa
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                if (currentState != NPCState.Idle && !isWaiting && !isFleeing)
                {
                    SetState(NPCState.Idle);
                }
            }
        }
    }

    /// <summary>
    /// Routine di chuyển ngẫu nhiên
    /// </summary>
    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            if (!isFleeing && !isWaiting)
            {
            
                Vector3 randomPoint = GetRandomPointInRadius(startPosition, wanderRadius);
                
                if (randomPoint != Vector3.zero)
                {
            
                    bool shouldRun = canRun && Random.value < runChance;
                    
                    if (shouldRun)
                    {
                        SetState(NPCState.Running);
                        agent.speed = runSpeed;
                    }
                    else
                    {
                        SetState(NPCState.Walking);
                        agent.speed = walkSpeed;
                    }

                    agent.SetDestination(randomPoint);
   
                    yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);
                    
                    isWaiting = true;
                    SetState(NPCState.Idle);
                    float waitTime = Random.Range(minWaitTime, maxWaitTime);
                    yield return new WaitForSeconds(waitTime);
                    isWaiting = false;
                }
            }
            
            yield return null;
        }
    }

    private Vector3 GetRandomPointInRadius(Vector3 origin, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += origin;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        return Vector3.zero;
    }

    public void FleeFrom(Vector3 dangerPosition)
    {
        if (!isFleeing)
        {
            StartCoroutine(FleeRoutine(dangerPosition));
        }
    }

    private IEnumerator FleeRoutine(Vector3 dangerPosition)
    {
        isFleeing = true;
        SetState(NPCState.Fleeing);
        agent.speed = runSpeed * 1.2f; // Chạy nhanh hơn khi sợ hãi

        Vector3 fleeDirection = (transform.position - dangerPosition).normalized;
        Vector3 fleePosition = transform.position + fleeDirection * fleeDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleePosition, out hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
       
            yield return new WaitForSeconds(5f);
        }

        isFleeing = false;
        agent.speed = walkSpeed;
    }

    private void SetState(NPCState newState)
    {
        currentState = newState;
    }


    private void UpdateAnimation()
    {
        if (animator != null)
        {
    
            float speed = agent.velocity.magnitude;
            
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsWalking", currentState == NPCState.Walking);
            animator.SetBool("IsRunning", currentState == NPCState.Running || currentState == NPCState.Fleeing);
            animator.SetBool("IsIdle", currentState == NPCState.Idle);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ bán kính di chuyển
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, wanderRadius);
        
        // Vẽ đường đi
        if (Application.isPlaying && agent != null && agent.hasPath)
        {
            Gizmos.color = Color.green;
            Vector3[] path = agent.path.corners;
            for (int i = 0; i < path.Length - 1; i++)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
            }
        }
    }
}
