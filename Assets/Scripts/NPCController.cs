using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controller cho NPC (dân thường đi lại như trong GTA)
/// Sử dụng NavMesh để di chuyển
/// </summary>
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
    [SerializeField] private float runChance = 0.2f; // 20% khả năng chạy
    [SerializeField] private float fleeDistance = 10f; // Khoảng cách chạy trốn khi nghe tiếng súng

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
        
        // Đặt tốc độ ban đầu
        currentSpeed = walkSpeed;
        agent.speed = currentSpeed;

        // Bắt đầu di chuyển ngẫu nhiên
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
                // Chọn điểm đích ngẫu nhiên
                Vector3 randomPoint = GetRandomPointInRadius(startPosition, wanderRadius);
                
                if (randomPoint != Vector3.zero)
                {
                    // Quyết định đi bộ hay chạy
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
                    
                    // Đợi đến khi đến nơi
                    yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);
                    
                    // Nghỉ một lúc tại điểm đích
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

    /// <summary>
    /// Lấy điểm ngẫu nhiên trong bán kính
    /// </summary>
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

    /// <summary>
    /// NPC chạy trốn khỏi vị trí nguy hiểm
    /// </summary>
    public void FleeFrom(Vector3 dangerPosition)
    {
        if (!isFleeing)
        {
            StartCoroutine(FleeRoutine(dangerPosition));
        }
    }

    /// <summary>
    /// Routine chạy trốn
    /// </summary>
    private IEnumerator FleeRoutine(Vector3 dangerPosition)
    {
        isFleeing = true;
        SetState(NPCState.Fleeing);
        agent.speed = runSpeed * 1.2f; // Chạy nhanh hơn khi sợ hãi

        // Tính hướng chạy trốn (ngược lại với nguồn nguy hiểm)
        Vector3 fleeDirection = (transform.position - dangerPosition).normalized;
        Vector3 fleePosition = transform.position + fleeDirection * fleeDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleePosition, out hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            
            // Đợi đến khi chạy xong
            yield return new WaitForSeconds(5f);
        }

        isFleeing = false;
        agent.speed = walkSpeed;
    }

    /// <summary>
    /// Đặt trạng thái của NPC
    /// </summary>
    private void SetState(NPCState newState)
    {
        currentState = newState;
    }

    /// <summary>
    /// Cập nhật animation dựa trên trạng thái
    /// </summary>
    private void UpdateAnimation()
    {
        if (animator != null)
        {
            // Tính tốc độ di chuyển
            float speed = agent.velocity.magnitude;
            
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsWalking", currentState == NPCState.Walking);
            animator.SetBool("IsRunning", currentState == NPCState.Running || currentState == NPCState.Fleeing);
            animator.SetBool("IsIdle", currentState == NPCState.Idle);
        }
    }

    /// <summary>
    /// Vẽ Gizmos để debug
    /// </summary>
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
