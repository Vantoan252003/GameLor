using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Script optional để hiển thị thông báo khi nhặt đạn
/// Attach vào UI Canvas
/// </summary>
public class AmmoPickupNotification : MonoBehaviour
{
    public static AmmoPickupNotification instance;

    [Header("UI References")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    private Coroutine currentNotification;

    void Awake()
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

    void Start()
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Hiển thị thông báo nhặt đạn
    /// </summary>
    public void ShowAmmoPickup(int ammoAmount)
    {
        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
        }
        
        currentNotification = StartCoroutine(ShowNotification($"+{ammoAmount} Ammo!"));
    }

    /// <summary>
    /// Hiển thị thông báo custom
    /// </summary>
    public void ShowCustomNotification(string message)
    {
        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
        }
        
        currentNotification = StartCoroutine(ShowNotification(message));
    }

    IEnumerator ShowNotification(string message)
    {
        if (notificationPanel == null || notificationText == null)
        {
            yield break;
        }

        // Set text
        notificationText.text = message;
        notificationPanel.SetActive(true);

        // Fade in
        CanvasGroup canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
        }

        float fadeInDuration = 0.3f;
        for (float t = 0; t < fadeInDuration; t += Time.deltaTime)
        {
            float alpha = fadeInCurve.Evaluate(t / fadeInDuration);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Display
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        float fadeOutDuration = 0.5f;
        for (float t = 0; t < fadeOutDuration; t += Time.deltaTime)
        {
            float alpha = fadeOutCurve.Evaluate(t / fadeOutDuration);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        canvasGroup.alpha = 0f;

        notificationPanel.SetActive(false);
    }
}
