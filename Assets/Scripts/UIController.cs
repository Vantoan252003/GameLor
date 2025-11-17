using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    public Slider healthSlider;

    public TextMeshProUGUI ammoText;

    public TextMeshProUGUI killMessageText;

    public GameObject deathScreen;
    public TMP_Text deathText;
    public FixedJoystick joystick;


    public TMP_Text killsText;
    public TMP_Text deathsText;

    public GameObject leaderboard;
    public LeaderboardPlayer leaderboardPlayerDisplay;


    public GameObject endScreen;

    public TMP_Text timerText;

    public TextMeshProUGUI FpsText;
    private float pollingTime = 1f;
    private float time;
    private int frameCount;

    public Image damageScreen;

    public TextMeshProUGUI PingText;
    public GameObject optionsScreen;

    [Header("Vehicle Interaction UI")]
    public Button interactButton;
    public TextMeshProUGUI interactText;
    
    [Header("Vehicle Control UI")]
    public GameObject vehicleControlPanel;
    public Button gasButton;
    public Button reverseButton;
    public Button brakeButton;

    [Header("Helicopter Control UI")]
    public GameObject helicopterControlPanel;
    public Button rotateLeft;
    public Button rotateRight;
    
    [Header("Game Control Buttons")]
    public GameObject shootButton;
    public GameObject jumpButton;
    public GameObject weaponButton;
    public GameObject scopeButton;
    public GameObject reloadButton;

    void Awake()
    {
        instance = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(SimpleInput.GetButtonDown("Pause"))
        {
            ShowHideOptions();
        }

        if(optionsScreen.activeInHierarchy && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        FpsDisplay();
    }

    void FpsDisplay()
    {
        time += Time.deltaTime;

        frameCount++;

        if(time >= pollingTime)
        {
            int frameRate =  Mathf.RoundToInt(frameCount / time);
            FpsText.text = frameRate.ToString() + " FPS";

            time -= pollingTime;
            frameCount = 0;
        }

        if (PhotonNetwork.IsConnected)
        {
            int ping = PhotonNetwork.GetPing();
            PingText.text = "Ping: " + ping + " ms"; // Update teks elemen UI dengan nilai ping
        }
    }

    public void ShowHideOptions()
    {
        if(!optionsScreen.activeInHierarchy)
        {
            optionsScreen.SetActive(true);
        }
        else
        {
            optionsScreen.SetActive(false);
        }
    }

    public void ReturnToMainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
    public void ShowVehicleControls(bool show)
    {
        // Hiện/ẩn vehicle controls
        if (vehicleControlPanel != null)
        {
            vehicleControlPanel.SetActive(show);
        }
        
        // Ẩn helicopter controls khi ở xe
        if (helicopterControlPanel != null)
        {
            helicopterControlPanel.SetActive(false);
        }
        
        // Ẩn/hiện game controls (ngược lại)
        if (shootButton != null) shootButton.SetActive(!show);
        if (jumpButton != null) jumpButton.SetActive(!show);
        if (weaponButton != null) weaponButton.SetActive(!show);
        if (scopeButton != null) scopeButton.SetActive(!show);
        if (reloadButton != null) reloadButton.SetActive(!show);
    }
    
    public void ShowHelicopterControls(bool show)
    {
        // Hiện/ẩn gas và reverse (giữ nguyên)
        if (vehicleControlPanel != null)
        {
            vehicleControlPanel.SetActive(show);
        }
        
        // Ẩn brake button khi ở helicopter
        if (brakeButton != null)
        {
            brakeButton.gameObject.SetActive(!show);
        }
        
        // Hiện/ẩn helicopter rotation controls
        if (helicopterControlPanel != null)
        {
            helicopterControlPanel.SetActive(show);
        }
        
        // Ẩn/hiện game controls (ngược lại)
        if (shootButton != null) shootButton.SetActive(!show);
        if (jumpButton != null) jumpButton.SetActive(!show);
        if (weaponButton != null) weaponButton.SetActive(!show);
        if (scopeButton != null) scopeButton.SetActive(!show);
        if (reloadButton != null) reloadButton.SetActive(!show);
    }
}
