using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý trận đấu cho chế độ Offline (không có timer, không có Photon)
/// </summary>
public class OfflineMatchManager : MonoBehaviour
{
    public static OfflineMatchManager instance;

    [Header("Player Stats")]
    public int playerKills = 0;
    public int playerDeaths = 0;

    [Header("Game Settings")]
    public int killsToWin = 10;
    public bool showStats = true;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Khởi tạo UI
        if (UIController.instance != null)
        {
            // Ẩn timer trong chế độ Offline
            if (UIController.instance.timerText != null)
            {
                UIController.instance.timerText.gameObject.SetActive(false);
            }
        }

        Debug.Log("Offline Match Manager đã khởi động - Không có timer");
    }

    void Update()
    {
        // Hiển thị bảng xếp hạng khi nhấn phím
        if(SimpleInput.GetButton("Leaderboard"))
        {
            ShowStats();
        }
        else
        {
            HideStats();
        }
    }

    /// <summary>
    /// Cập nhật số kills của người chơi
    /// </summary>
    public void AddKill()
    {
        playerKills++;
        UpdateStatsDisplay();
        
        // Kiểm tra điều kiện thắng
        if (playerKills >= killsToWin)
        {
            EndGame();
        }
    }

    /// <summary>
    /// Cập nhật số deaths của người chơi
    /// </summary>
    public void AddDeath()
    {
        playerDeaths++;
        UpdateStatsDisplay();
    }

    /// <summary>
    /// Cập nhật hiển thị thống kê
    /// </summary>
    private void UpdateStatsDisplay()
    {
        if (UIController.instance != null && UIController.instance.killsText != null)
        {
            UIController.instance.killsText.text = $"Kills: {playerKills}";
            UIController.instance.deathsText.text = $"Deaths: {playerDeaths}";
        }
    }

    /// <summary>
    /// Hiển thị bảng thống kê
    /// </summary>
    private void ShowStats()
    {
        if (showStats && UIController.instance != null && UIController.instance.leaderboard != null)
        {
            UIController.instance.leaderboard.SetActive(true);
        }
    }

    /// <summary>
    /// Ẩn bảng thống kê
    /// </summary>
    private void HideStats()
    {
        if (UIController.instance != null && UIController.instance.leaderboard != null)
        {
            UIController.instance.leaderboard.SetActive(false);
        }
    }

    /// <summary>
    /// Kết thúc trò chơi
    /// </summary>
    private void EndGame()
    {
        Debug.Log("Game Over - Offline Mode");
        
        if (UIController.instance != null && UIController.instance.endScreen != null)
        {
            UIController.instance.endScreen.SetActive(true);
        }

        // Có thể thêm logic restart hoặc quay về menu
        StartCoroutine(ReturnToMenuAfterDelay(5f));
    }

    /// <summary>
    /// Quay về menu sau một khoảng thời gian
    /// </summary>
    private IEnumerator ReturnToMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(0); // Load scene menu
    }

    /// <summary>
    /// Reset stats
    /// </summary>
    public void ResetStats()
    {
        playerKills = 0;
        playerDeaths = 0;
        UpdateStatsDisplay();
    }
}
