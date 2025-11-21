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
    
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float respawnTime = 5f;
    [SerializeField] private float respawnRadius = 50f; // Khoảng cách xa để respawn
    
    [Header("Animation (Optional)")]
    [SerializeField] private Animator animator;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private bool isWaiting = false;
    private bool isFleeing = false;
    private float currentSpeed;
    private float currentHealth;
    private bool isDead = false;

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
        
        // Cải thiện obstacle avoidance
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.avoidancePriority = 50;
        
        currentSpeed = walkSpeed;
        agent.speed = currentSpeed;
        currentHealth = maxHealth;

        StartCoroutine(WanderRoutine());
    }

    void Update()
    {
        // CHỈ cập nhật khi NPC còn sống và agent đang active
        if (isDead || !agent.enabled) return;

        // Cập nhật animation
        UpdateAnimation();

        // Kiểm tra xem đã đến đích chưa - CHỈ khi ĐANG DI CHUYỂN
        if ((currentState == NPCState.Walking || currentState == NPCState.Running) && 
            !agent.pathPending && 
            agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.1f)
            {
                if (!isWaiting && !isFleeing)
                {
                    SetState(NPCState.Idle);
                }
            }
        }
    }

    /// <summary>
    /// Routine di chuyển ngẫu nhiên - CHỈ ĐI BỘ
    /// </summary>
    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            if (!isFleeing && !isWaiting && !isDead)
            {
            
                Vector3 randomPoint = GetRandomPointInRadius(startPosition, wanderRadius);
                
                if (randomPoint != Vector3.zero)
                {
                    // CHỈ ĐI BỘ - không chạy ngẫu nhiên
                    SetState(NPCState.Walking);
                    agent.speed = walkSpeed;

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

    /// <summary>
    /// NPC nhận sát thương
    /// </summary>
    public void TakeDamage(float damage, Vector3 damageSource, int killerActorNumber = -1)
    {
        if (isDead) return;

        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die(killerActorNumber);
        }
        else
        {
            // Còn sống thì bỏ chạy
            FleeFrom(damageSource);
        }
    }

    /// <summary>
    /// NPC chết
    /// </summary>
    private void Die(int killerActorNumber = -1)
    {
        isDead = true;
        SetState(NPCState.Idle);
        
        // Dừng NavMeshAgent
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        
        // Cộng điểm kill cho người bắn (chỉ trong multiplayer)
        if (killerActorNumber > 0 && MatchManager.instance != null)
        {
            MatchManager.instance.UpdateStatsSend(killerActorNumber, 0, 1);
        }
        
        gameObject.SetActive(false);

        // Respawn sau vài giây
        Invoke("Respawn", respawnTime);
    }

    private void Respawn()
    {
        Vector3 respawnPoint = GetRandomPointInRadius(startPosition, respawnRadius);
        
        if (respawnPoint != Vector3.zero)
        {
            transform.position = respawnPoint;
        }
        else
        {
            transform.position = startPosition;
        }

        // Reset trạng thái
        currentHealth = maxHealth;
        isDead = false;
        agent.isStopped = false;
        gameObject.SetActive(true);

        SetState(NPCState.Idle);
        isFleeing = false;
        isWaiting = false;
    }

    private void SetState(NPCState newState)
    {
        if (currentState != newState)
        {
            Debug.Log($"[{gameObject.name}] NPC State Change: {currentState} -> {newState}");
            currentState = newState;
        }
    }


    private void UpdateAnimation()
    {
        if (animator != null && agent.enabled)
        {
            float speed = agent.velocity.magnitude;
            
            if (speed > 0.01f)
            {
                Debug.Log($"[{gameObject.name}] Speed: {speed:F2}, State: {currentState}, HasPath: {agent.hasPath}, IsStopped: {agent.isStopped}");
            }
            
            animator.SetFloat("Speed", speed);
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
