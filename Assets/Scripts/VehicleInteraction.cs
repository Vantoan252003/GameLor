using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class VehicleInteraction : MonoBehaviourPunCallbacks
{
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask vehicleLayer;
    
    private VehicleController nearbyVehicle;
    private VehicleController currentVehicle;
    private HelicopterController nearbyHelicopter;
    private HelicopterController currentHelicopter;
    private bool isInVehicle = false;
    private bool isInHelicopter = false;
    
    private PlayerController playerController;
    
    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }
    
    void Update()
    {
        if (!photonView.IsMine) return;
        
        CheckForNearbyVehicle();
        CheckForNearbyHelicopter();
        
        if (SimpleInput.GetButtonDown("Interact"))
        {
            if (isInVehicle)
            {
                ExitVehicle();
            }
            else if (isInHelicopter)
            {
                ExitHelicopter();
            }
            else if (nearbyVehicle != null)
            {
                EnterVehicle();
            }
            else if (nearbyHelicopter != null)
            {
                EnterHelicopter();
            }
        }
        
        // Update UI
        UpdateInteractUI();
    }
    
    private void CheckForNearbyVehicle()
    {
        if (isInVehicle) return;
        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRange, vehicleLayer);
        
        nearbyVehicle = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider col in hitColliders)
        {
            VehicleController vehicle = col.GetComponent<VehicleController>();
            if (vehicle != null && vehicle.CanEnterVehicle(transform.position))
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearbyVehicle = vehicle;
                }
            }
        }
    }
    
    private void CheckForNearbyHelicopter()
    {
        if (isInHelicopter || isInVehicle) return;
        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRange, vehicleLayer);
        
        nearbyHelicopter = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider col in hitColliders)
        {
            HelicopterController helicopter = col.GetComponent<HelicopterController>();
            if (helicopter != null && helicopter.CanEnterHelicopter(transform.position))
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearbyHelicopter = helicopter;
                }
            }
        }
    }
    
    private void EnterVehicle()
    {
        if (nearbyVehicle == null) return;
        
        currentVehicle = nearbyVehicle;
        nearbyVehicle.photonView.RPC("EnterVehicle", RpcTarget.AllBuffered, photonView.ViewID);
        isInVehicle = true;
        
        // Notify PlayerController
        if (playerController != null)
        {
            playerController.SetInVehicle(true);
        }
        
        // Hide interact button, show vehicle controls
        if (UIController.instance != null)
        {
            UIController.instance.interactButton.gameObject.SetActive(false);
            UIController.instance.ShowVehicleControls(true);
            
            // Connect vehicle buttons
            ConnectVehicleButtons();
        }
    }
    
    private void ExitVehicle()
    {
        if (currentVehicle == null) return;
        
        currentVehicle.photonView.RPC("ExitVehicle", RpcTarget.AllBuffered, photonView.ViewID);
        
        // Disconnect vehicle buttons
        DisconnectVehicleButtons();
        
        currentVehicle = null;
        isInVehicle = false;
        
        // Notify PlayerController
        if (playerController != null)
        {
            playerController.SetInVehicle(false);
        }
        
        // Hide vehicle controls
        if (UIController.instance != null)
        {
            UIController.instance.ShowVehicleControls(false);
        }
    }
    
    private void ConnectVehicleButtons()
    {
        if (UIController.instance == null || currentVehicle == null) return;
        
        // Use EventTrigger for Gas button
        UnityEngine.EventSystems.EventTrigger gasEventTrigger = UIController.instance.gasButton.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (gasEventTrigger == null)
        {
            gasEventTrigger = UIController.instance.gasButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        // Clear existing listeners
        gasEventTrigger.triggers.Clear();
        
        // Add PointerDown event
        UnityEngine.EventSystems.EventTrigger.Entry gasDownEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        gasDownEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
        gasDownEntry.callback.AddListener((data) => { currentVehicle.OnGasPressed(); });
        gasEventTrigger.triggers.Add(gasDownEntry);
        
        // Add PointerUp event
        UnityEngine.EventSystems.EventTrigger.Entry gasUpEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        gasUpEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
        gasUpEntry.callback.AddListener((data) => { currentVehicle.OnGasReleased(); });
        gasEventTrigger.triggers.Add(gasUpEntry);
        
        // Use EventTrigger for Reverse button
        UnityEngine.EventSystems.EventTrigger reverseEventTrigger = UIController.instance.reverseButton.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (reverseEventTrigger == null)
        {
            reverseEventTrigger = UIController.instance.reverseButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        // Clear existing listeners
        reverseEventTrigger.triggers.Clear();
        
        // Add PointerDown event
        UnityEngine.EventSystems.EventTrigger.Entry reverseDownEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        reverseDownEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
        reverseDownEntry.callback.AddListener((data) => { currentVehicle.OnReversePressed(); });
        reverseEventTrigger.triggers.Add(reverseDownEntry);
        
        // Add PointerUp event
        UnityEngine.EventSystems.EventTrigger.Entry reverseUpEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        reverseUpEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
        reverseUpEntry.callback.AddListener((data) => { currentVehicle.OnReverseReleased(); });
        reverseEventTrigger.triggers.Add(reverseUpEntry);
        
        // Use EventTrigger for Brake button
        if (UIController.instance.brakeButton != null)
        {
            UnityEngine.EventSystems.EventTrigger brakeEventTrigger = UIController.instance.brakeButton.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (brakeEventTrigger == null)
            {
                brakeEventTrigger = UIController.instance.brakeButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }
            
            // Clear existing listeners
            brakeEventTrigger.triggers.Clear();
            
            // Add PointerDown event
            UnityEngine.EventSystems.EventTrigger.Entry brakeDownEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            brakeDownEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
            brakeDownEntry.callback.AddListener((data) => { currentVehicle.OnBrakePressed(); });
            brakeEventTrigger.triggers.Add(brakeDownEntry);
            
            // Add PointerUp event
            UnityEngine.EventSystems.EventTrigger.Entry brakeUpEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            brakeUpEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
            brakeUpEntry.callback.AddListener((data) => { currentVehicle.OnBrakeReleased(); });
            brakeEventTrigger.triggers.Add(brakeUpEntry);
        }
    }
    
    private void DisconnectVehicleButtons()
    {
        if (UIController.instance == null) return;
        
        // Clear Gas button event triggers
        UnityEngine.EventSystems.EventTrigger gasEventTrigger = UIController.instance.gasButton.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (gasEventTrigger != null)
        {
            gasEventTrigger.triggers.Clear();
        }
        
        // Clear Reverse button event triggers
        UnityEngine.EventSystems.EventTrigger reverseEventTrigger = UIController.instance.reverseButton.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (reverseEventTrigger != null)
        {
            reverseEventTrigger.triggers.Clear();
        }
        
        // Clear Brake button event triggers
        if (UIController.instance.brakeButton != null)
        {
            UnityEngine.EventSystems.EventTrigger brakeEventTrigger = UIController.instance.brakeButton.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (brakeEventTrigger != null)
            {
                brakeEventTrigger.triggers.Clear();
            }
        }
    }
    
    private void EnterHelicopter()
    {
        if (nearbyHelicopter == null) return;
        
        currentHelicopter = nearbyHelicopter;
        nearbyHelicopter.photonView.RPC("EnterHelicopter", RpcTarget.AllBuffered, photonView.ViewID);
        isInHelicopter = true;
        
        // Notify PlayerController
        if (playerController != null)
        {
            playerController.SetInVehicle(true);
        }
        
        // Hide interact button, show helicopter controls (not vehicle controls)
        if (UIController.instance != null)
        {
            UIController.instance.interactButton.gameObject.SetActive(false);
            UIController.instance.ShowHelicopterControls(true); // Dùng method riêng cho helicopter
            
            // Connect helicopter buttons
            ConnectHelicopterButtons();
        }
    }
    
    private void ExitHelicopter()
    {
        if (currentHelicopter == null) return;
        
        currentHelicopter.photonView.RPC("ExitHelicopter", RpcTarget.AllBuffered, photonView.ViewID);
        
        // Disconnect helicopter buttons
        DisconnectHelicopterButtons();
        
        currentHelicopter = null;
        isInHelicopter = false;
        
        // Notify PlayerController
        if (playerController != null)
        {
            playerController.SetInVehicle(false);
        }
        
        // Hide helicopter controls
        if (UIController.instance != null)
        {
            UIController.instance.ShowHelicopterControls(false); // Dùng method riêng
        }
    }
    
    private void ConnectHelicopterButtons()
    {
        if (UIController.instance == null || currentHelicopter == null) return;
        
        // Tìm hoặc thêm EventTrigger cho Gas button
        UnityEngine.EventSystems.EventTrigger gasEventTrigger = UIController.instance.gasButton.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (gasEventTrigger == null)
        {
            gasEventTrigger = UIController.instance.gasButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        // Xóa listeners cũ
        gasEventTrigger.triggers.Clear();
        
        // Thêm PointerDown event cho Gas
        UnityEngine.EventSystems.EventTrigger.Entry gasDownEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        gasDownEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
        gasDownEntry.callback.AddListener((data) => { 
            if (currentHelicopter != null) 
                currentHelicopter.OnGasPressed(); 
        });
        gasEventTrigger.triggers.Add(gasDownEntry);
        
        // Thêm PointerUp event cho Gas
        UnityEngine.EventSystems.EventTrigger.Entry gasUpEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        gasUpEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
        gasUpEntry.callback.AddListener((data) => { 
            if (currentHelicopter != null) 
                currentHelicopter.OnGasReleased(); 
        });
        gasEventTrigger.triggers.Add(gasUpEntry);
        
        // Tìm hoặc thêm EventTrigger cho Reverse button
        UnityEngine.EventSystems.EventTrigger reverseEventTrigger = UIController.instance.reverseButton.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (reverseEventTrigger == null)
        {
            reverseEventTrigger = UIController.instance.reverseButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        // Xóa listeners cũ
        reverseEventTrigger.triggers.Clear();
        
        // Thêm PointerDown event cho Reverse
        UnityEngine.EventSystems.EventTrigger.Entry reverseDownEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        reverseDownEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
        reverseDownEntry.callback.AddListener((data) => { 
            if (currentHelicopter != null) 
                currentHelicopter.OnReversePressed(); 
        });
        reverseEventTrigger.triggers.Add(reverseDownEntry);
        
        // Thêm PointerUp event cho Reverse
        UnityEngine.EventSystems.EventTrigger.Entry reverseUpEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        reverseUpEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
        reverseUpEntry.callback.AddListener((data) => { 
            if (currentHelicopter != null) 
                currentHelicopter.OnReverseReleased(); 
        });
        reverseEventTrigger.triggers.Add(reverseUpEntry);
        
        // Connect Rotate Left button
        if (UIController.instance.rotateLeft != null)
        {
            UnityEngine.EventSystems.EventTrigger rotateLeftTrigger = UIController.instance.rotateLeft.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (rotateLeftTrigger == null)
            {
                rotateLeftTrigger = UIController.instance.rotateLeft.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }
            
            rotateLeftTrigger.triggers.Clear();
            
            // PointerDown - Bắt đầu quay
            UnityEngine.EventSystems.EventTrigger.Entry rotateLeftDownEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            rotateLeftDownEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
            rotateLeftDownEntry.callback.AddListener((data) => {
                if (currentHelicopter != null)
                    currentHelicopter.OnRotateLeftPressed();
            });
            rotateLeftTrigger.triggers.Add(rotateLeftDownEntry);
            
            // PointerUp - Dừng quay
            UnityEngine.EventSystems.EventTrigger.Entry rotateLeftUpEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            rotateLeftUpEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
            rotateLeftUpEntry.callback.AddListener((data) => {
                if (currentHelicopter != null)
                    currentHelicopter.OnRotateLeftReleased();
            });
            rotateLeftTrigger.triggers.Add(rotateLeftUpEntry);
        }
        
        // Connect Rotate Right button
        if (UIController.instance.rotateRight != null)
        {
            UnityEngine.EventSystems.EventTrigger rotateRightTrigger = UIController.instance.rotateRight.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (rotateRightTrigger == null)
            {
                rotateRightTrigger = UIController.instance.rotateRight.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }
            
            rotateRightTrigger.triggers.Clear();
            
            // PointerDown - Bắt đầu quay
            UnityEngine.EventSystems.EventTrigger.Entry rotateRightDownEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            rotateRightDownEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
            rotateRightDownEntry.callback.AddListener((data) => {
                if (currentHelicopter != null)
                    currentHelicopter.OnRotateRightPressed();
            });
            rotateRightTrigger.triggers.Add(rotateRightDownEntry);
            
            // PointerUp - Dừng quay
            UnityEngine.EventSystems.EventTrigger.Entry rotateRightUpEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            rotateRightUpEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
            rotateRightUpEntry.callback.AddListener((data) => {
                if (currentHelicopter != null)
                    currentHelicopter.OnRotateRightReleased();
            });
            rotateRightTrigger.triggers.Add(rotateRightUpEntry);
        }
    }
    
    private void DisconnectHelicopterButtons()
    {
        if (UIController.instance == null) return;
        
        // Clear Gas button event triggers
        UnityEngine.EventSystems.EventTrigger gasEventTrigger = UIController.instance.gasButton.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (gasEventTrigger != null)
        {
            gasEventTrigger.triggers.Clear();
        }
        
        // Clear Reverse button event triggers
        UnityEngine.EventSystems.EventTrigger reverseEventTrigger = UIController.instance.reverseButton.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (reverseEventTrigger != null)
        {
            reverseEventTrigger.triggers.Clear();
        }
        
        // Clear Rotate Left button
        if (UIController.instance.rotateLeft != null)
        {
            UnityEngine.EventSystems.EventTrigger rotateLeftTrigger = UIController.instance.rotateLeft.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (rotateLeftTrigger != null)
            {
                rotateLeftTrigger.triggers.Clear();
            }
        }
        
        // Clear Rotate Right button
        if (UIController.instance.rotateRight != null)
        {
            UnityEngine.EventSystems.EventTrigger rotateRightTrigger = UIController.instance.rotateRight.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (rotateRightTrigger != null)
            {
                rotateRightTrigger.triggers.Clear();
            }
        }
    }
    
    private void UpdateInteractUI()
    {
        if (UIController.instance == null) return;
        
        if (isInVehicle)
        {
            UIController.instance.interactButton.gameObject.SetActive(true);
            UIController.instance.interactText.text = "Ra xe [F]";
        }
        else if (isInHelicopter)
        {
            UIController.instance.interactButton.gameObject.SetActive(true);
            UIController.instance.interactText.text = "Ra trực thăng [F]";
        }
        else if (nearbyHelicopter != null)
        {
            UIController.instance.interactButton.gameObject.SetActive(true);
            UIController.instance.interactText.text = "Vào trực thăng [F]";
        }
        else if (nearbyVehicle != null)
        {
            UIController.instance.interactButton.gameObject.SetActive(true);
            UIController.instance.interactText.text = "Vào xe";
        }
        else
        {
            UIController.instance.interactButton.gameObject.SetActive(false);
        }
    }
    
    public bool IsInVehicle()
    {
        return isInVehicle;
    }
    
    public VehicleController GetCurrentVehicle()
    {
        return currentVehicle;
    }
}
