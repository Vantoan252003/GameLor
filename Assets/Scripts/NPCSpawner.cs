using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Prefabs")]
    [SerializeField] private GameObject[] npcPrefabs; // Danh sách prefab NPC khác nhau
    
    [Header("Spawn Settings")]
    [SerializeField] private int minNPCCount = 10;
    [SerializeField] private int maxNPCCount = 30;
    [SerializeField] private float spawnRadius = 50f;
    [SerializeField] private bool spawnOnStart = true;
    
    [Header("Spawn Areas (Optional)")]
    [SerializeField] private Transform[] spawnPoints; // Các điểm spawn cụ thể
    [SerializeField] private bool useSpawnPoints = false;

    [Header("Dynamic Spawn")]
    [SerializeField] private bool enableDynamicSpawn = true;
    [SerializeField] private float respawnInterval = 30f; // Spawn thêm NPC sau mỗi 30 giây
    
    private List<GameObject> spawnedNPCs = new List<GameObject>();
    private int targetNPCCount;

    void Start()
    {
        if (npcPrefabs == null || npcPrefabs.Length == 0)
        {
            Debug.LogError("NPCSpawner: Chưa gán prefab NPC!");
            return;
        }

        targetNPCCount = Random.Range(minNPCCount, maxNPCCount + 1);

        if (spawnOnStart)
        {
            SpawnInitialNPCs();
        }

        if (enableDynamicSpawn)
        {
            StartCoroutine(DynamicSpawnRoutine());
        }
    }
    private void SpawnInitialNPCs()
    {
        for (int i = 0; i < targetNPCCount; i++)
        {
            SpawnNPC();
        }

        Debug.Log($"Đã spawn {targetNPCCount} NPC");
    }

    public GameObject SpawnNPC()
    {
        Vector3 spawnPosition = GetSpawnPosition();
        
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogWarning("Không tìm được vị trí spawn hợp lệ cho NPC");
            return null;
        }

        // Chọn prefab ngẫu nhiên
        GameObject npcPrefab = npcPrefabs[Random.Range(0, npcPrefabs.Length)];
        
        // Tạo NPC
        GameObject npc = Instantiate(npcPrefab, spawnPosition, Quaternion.identity);
        npc.transform.parent = transform; // Đặt làm con của spawner để dễ quản lý
        
        spawnedNPCs.Add(npc);
        
        return npc;
    }

    private Vector3 GetSpawnPosition()
    {
        if (useSpawnPoints && spawnPoints != null && spawnPoints.Length > 0)
        {
            // Sử dụng spawn points đã định sẵn
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            return spawnPoint.position;
        }
        else
        {
            // Spawn ngẫu nhiên xung quanh spawner
            Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
            randomDirection += transform.position;
            randomDirection.y = transform.position.y; // Giữ nguyên độ cao

            // Kiểm tra xem vị trí có hợp lệ không (có NavMesh)
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, spawnRadius, UnityEngine.AI.NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Routine spawn động - spawn thêm NPC nếu số lượng giảm
    /// </summary>
    private IEnumerator DynamicSpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(respawnInterval);

            // Xóa các NPC đã bị destroy khỏi list
            spawnedNPCs.RemoveAll(npc => npc == null);

            // Spawn thêm nếu số lượng ít hơn target
            int currentCount = spawnedNPCs.Count;
            if (currentCount < targetNPCCount)
            {
                int toSpawn = Mathf.Min(5, targetNPCCount - currentCount); // Spawn tối đa 5 mỗi lần
                for (int i = 0; i < toSpawn; i++)
                {
                    SpawnNPC();
                }
                
                Debug.Log($"Dynamic spawn: Thêm {toSpawn} NPC. Tổng: {spawnedNPCs.Count}");
            }
        }
    }

    /// <summary>
    /// Despawn tất cả NPC
    /// </summary>
    public void DespawnAllNPCs()
    {
        foreach (GameObject npc in spawnedNPCs)
        {
            if (npc != null)
            {
                Destroy(npc);
            }
        }
        
        spawnedNPCs.Clear();
        Debug.Log("Đã despawn tất cả NPC");
    }

    /// <summary>
    /// Lấy danh sách NPC đang hoạt động
    /// </summary>
    public List<GameObject> GetActiveNPCs()
    {
        spawnedNPCs.RemoveAll(npc => npc == null);
        return spawnedNPCs;
    }

    /// <summary>
    /// Làm cho tất cả NPC chạy trốn khỏi một vị trí
    /// </summary>
    public void MakeNPCsFleeFrom(Vector3 dangerPosition, float radius)
    {
        foreach (GameObject npc in spawnedNPCs)
        {
            if (npc != null)
            {
                float distance = Vector3.Distance(npc.transform.position, dangerPosition);
                
                if (distance <= radius)
                {
                    NPCController controller = npc.GetComponent<NPCController>();
                    if (controller != null)
                    {
                        controller.FleeFrom(dangerPosition);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Vẽ Gizmos
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Vẽ bán kính spawn
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // Vẽ các spawn points
        if (useSpawnPoints && spawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 1f);
                }
            }
        }
    }
}
