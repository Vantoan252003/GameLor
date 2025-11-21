using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Manager để spawn và quản lý tất cả hộp đạn trên map
/// Chỉ Master Client mới spawn hộp đạn để tránh duplicate
/// </summary>
public class AmmoSpawnManager : MonoBehaviourPunCallbacks
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject ammoBoxPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private float spawnDelay = 1f; // Delay trước khi spawn (giây)
    
    [Header("Debug")]
    [SerializeField] private bool showSpawnPointGizmos = true;
    [SerializeField] private Color gizmoColor = Color.yellow;
    [SerializeField] private float gizmoSize = 0.5f;
    
    private List<GameObject> spawnedAmmoBoxes = new List<GameObject>();

    void Start()
    {
        if (spawnOnStart && PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnAmmoBoxesWithDelay());
        }
    }

    IEnumerator SpawnAmmoBoxesWithDelay()
    {
        yield return new WaitForSeconds(spawnDelay);
        SpawnAllAmmoBoxes();
    }

    /// <summary>
    /// Spawn tất cả hộp đạn tại các spawn points
    /// </summary>
    public void SpawnAllAmmoBoxes()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("Only Master Client can spawn ammo boxes!");
            return;
        }

        if (ammoBoxPrefab == null)
        {
            Debug.LogError("AmmoBox Prefab is not assigned!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        // Clear danh sách cũ
        ClearSpawnedAmmoBoxes();

        // Spawn mới
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                GameObject ammoBox = PhotonNetwork.Instantiate(
                    ammoBoxPrefab.name,
                    spawnPoint.position,
                    spawnPoint.rotation
                );
                
                spawnedAmmoBoxes.Add(ammoBox);
            }
        }

        Debug.Log($"Spawned {spawnedAmmoBoxes.Count} ammo boxes.");
    }

    /// <summary>
    /// Xóa tất cả hộp đạn đã spawn
    /// </summary>
    public void ClearSpawnedAmmoBoxes()
    {
        foreach (GameObject ammoBox in spawnedAmmoBoxes)
        {
            if (ammoBox != null)
            {
                PhotonNetwork.Destroy(ammoBox);
            }
        }
        spawnedAmmoBoxes.Clear();
    }

    /// <summary>
    /// Tự động tìm tất cả spawn points trong scene
    /// Tìm các GameObject có tag "AmmoSpawnPoint"
    /// </summary>
    [ContextMenu("Auto Find Spawn Points")]
    public void AutoFindSpawnPoints()
    {
        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("AmmoSpawnPoint");
        
        if (spawnPointObjects.Length == 0)
        {
            Debug.LogWarning("No GameObjects with tag 'AmmoSpawnPoint' found! Create spawn points and tag them.");
            return;
        }

        spawnPoints = new Transform[spawnPointObjects.Length];
        for (int i = 0; i < spawnPointObjects.Length; i++)
        {
            spawnPoints[i] = spawnPointObjects[i].transform;
        }

        Debug.Log($"Found {spawnPoints.Length} spawn points.");
    }

    /// <summary>
    /// Tạo spawn points tự động theo grid pattern
    /// </summary>
    [ContextMenu("Generate Grid Spawn Points")]
    public void GenerateGridSpawnPoints()
    {
        // Xóa spawn points cũ
        if (spawnPoints != null)
        {
            foreach (Transform sp in spawnPoints)
            {
                if (sp != null)
                {
                    DestroyImmediate(sp.gameObject);
                }
            }
        }

        // Tạo grid 4x4
        List<Transform> newSpawnPoints = new List<Transform>();
        int gridSize = 4;
        float spacing = 10f;

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                GameObject spawnPoint = new GameObject($"AmmoSpawnPoint_{x}_{z}");
                spawnPoint.transform.SetParent(transform);
                spawnPoint.transform.position = new Vector3(x * spacing, 0, z * spacing);
                spawnPoint.tag = "AmmoSpawnPoint";
                newSpawnPoints.Add(spawnPoint.transform);
            }
        }

        spawnPoints = newSpawnPoints.ToArray();
        Debug.Log($"Generated {spawnPoints.Length} spawn points in a grid pattern.");
    }

    // Gizmos để visualize spawn points trong Scene view
    void OnDrawGizmos()
    {
        if (!showSpawnPointGizmos || spawnPoints == null) return;

        Gizmos.color = gizmoColor;
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Gizmos.DrawWireSphere(spawnPoint.position, gizmoSize);
                Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up * gizmoSize * 2);
            }
        }
    }

    // Callback khi Master Client thay đổi
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        
        // Nếu client này trở thành Master Client mới
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Became new Master Client. Checking ammo box spawns...");
            // Có thể thêm logic kiểm tra và spawn lại nếu cần
        }
    }
}
