using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script demo đơn giản để test NPC system
/// Attach vào một GameObject trong scene để test
/// </summary>
public class NPCTestHelper : MonoBehaviour
{
    [Header("Keybinds")]
    [SerializeField] private KeyCode spawnNPCKey = KeyCode.F1;
    [SerializeField] private KeyCode makeNPCsFleeKey = KeyCode.F2;
    [SerializeField] private KeyCode despawnAllKey = KeyCode.F3;
    [SerializeField] private KeyCode showInfoKey = KeyCode.F4;

    [Header("Test Settings")]
    [SerializeField] private float fleeTestRadius = 30f;

    private NPCSpawner spawner;
    private NPCEventManager eventManager;

    void Start()
    {
        spawner = FindObjectOfType<NPCSpawner>();
        eventManager = FindObjectOfType<NPCEventManager>();

        Debug.Log("=== NPC TEST HELPER ===");
        Debug.Log("F1: Spawn thêm 1 NPC");
        Debug.Log("F2: Làm NPC chạy trốn khỏi vị trí của bạn");
        Debug.Log("F3: Despawn tất cả NPC");
        Debug.Log("F4: Hiện thông tin");
        Debug.Log("=====================");
    }

    void Update()
    {
        // Spawn NPC
        if (Input.GetKeyDown(spawnNPCKey))
        {
            if (spawner != null)
            {
                GameObject npc = spawner.SpawnNPC();
                if (npc != null)
                {
                    Debug.Log($"✅ Đã spawn NPC tại: {npc.transform.position}");
                }
                else
                {
                    Debug.LogWarning("❌ Không thể spawn NPC - kiểm tra NavMesh");
                }
            }
            else
            {
                Debug.LogError("❌ Không tìm thấy NPCSpawner trong scene!");
            }
        }

        // Make NPCs flee
        if (Input.GetKeyDown(makeNPCsFleeKey))
        {
            Vector3 playerPos = Camera.main.transform.position;
            
            if (spawner != null)
            {
                spawner.MakeNPCsFleeFrom(playerPos, fleeTestRadius);
                Debug.Log($"🏃 Làm NPC trong bán kính {fleeTestRadius}m chạy trốn!");
            }
            else
            {
                Debug.LogWarning("❌ Không tìm thấy NPCSpawner");
            }
        }

        // Despawn all
        if (Input.GetKeyDown(despawnAllKey))
        {
            if (spawner != null)
            {
                spawner.DespawnAllNPCs();
                Debug.Log("🗑️ Đã despawn tất cả NPC");
            }
        }

        // Show info
        if (Input.GetKeyDown(showInfoKey))
        {
            if (spawner != null)
            {
                List<GameObject> npcs = spawner.GetActiveNPCs();
                Debug.Log($"📊 Số NPC đang hoạt động: {npcs.Count}");
                
                for (int i = 0; i < npcs.Count; i++)
                {
                    if (npcs[i] != null)
                    {
                        Debug.Log($"  NPC {i}: {npcs[i].name} at {npcs[i].transform.position}");
                    }
                }
            }

            // Check NavMesh
            UnityEngine.AI.NavMeshTriangulation triangulation = UnityEngine.AI.NavMesh.CalculateTriangulation();
            Debug.Log($"🗺️ NavMesh triangles: {triangulation.indices.Length / 3}");
            
            if (triangulation.indices.Length == 0)
            {
                Debug.LogWarning("⚠️ NavMesh chưa được bake! Hãy bake NavMesh trước.");
            }
        }
    }

    private void OnGUI()
    {
        // Hiển thị shortcuts trên screen
        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, 10, 300, 100), 
            "=== NPC TEST HELPER ===\n" +
            "F1: Spawn NPC\n" +
            "F2: Make NPCs Flee\n" +
            "F3: Despawn All\n" +
            "F4: Show Info", 
            style);

        if (spawner != null)
        {
            List<GameObject> npcs = spawner.GetActiveNPCs();
            GUI.Label(new Rect(10, 120, 200, 30), 
                $"Active NPCs: {npcs.Count}", 
                style);
        }
    }
}
