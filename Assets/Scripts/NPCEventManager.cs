using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager để xử lý các sự kiện trong game ảnh hưởng đến NPC
/// Ví dụ: tiếng súng làm NPC chạy trốn
/// </summary>
public class NPCEventManager : MonoBehaviour
{
    public static NPCEventManager instance;

    [Header("Sound Alert Settings")]
    [SerializeField] private float gunshotAlertRadius = 20f; // Bán kính NPC nghe thấy tiếng súng
    [SerializeField] private bool debugMode = false;

    private NPCSpawner npcSpawner;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Tìm NPC Spawner trong scene
        npcSpawner = FindObjectOfType<NPCSpawner>();
        
        if (npcSpawner == null && debugMode)
        {
            Debug.LogWarning("NPCEventManager: Không tìm thấy NPCSpawner trong scene");
        }
    }

    /// <summary>
    /// Gọi khi có tiếng súng - làm NPC xung quanh chạy trốn
    /// </summary>
    /// <param name="shotPosition">Vị trí bắn súng</param>
    public void OnGunshotFired(Vector3 shotPosition)
    {
        if (npcSpawner != null)
        {
            npcSpawner.MakeNPCsFleeFrom(shotPosition, gunshotAlertRadius);
        }

        if (debugMode)
        {
            Debug.Log($"Gunshot at {shotPosition} - NPCs within {gunshotAlertRadius}m are fleeing!");
            
            // Vẽ sphere debug trong scene view
            Debug.DrawLine(shotPosition, shotPosition + Vector3.up * 5f, Color.red, 2f);
        }
    }

    /// <summary>
    /// Gọi khi có vụ nổ - tác động mạnh hơn tiếng súng
    /// </summary>
    public void OnExplosion(Vector3 explosionPosition, float radius)
    {
        if (npcSpawner != null)
        {
            npcSpawner.MakeNPCsFleeFrom(explosionPosition, radius);
        }

        if (debugMode)
        {
            Debug.Log($"Explosion at {explosionPosition} with radius {radius}m");
        }
    }

    /// <summary>
    /// Vẽ debug gizmos
    /// </summary>
    private void OnDrawGizmos()
    {
        if (debugMode)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            // Có thể vẽ thêm gizmos nếu cần
        }
    }
}
