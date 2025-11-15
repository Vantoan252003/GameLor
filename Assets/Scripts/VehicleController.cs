using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class VehicleController : MonoBehaviourPunCallbacks
{
    [Header("Vehicle Settings")]
    [SerializeField] private float motorForce = 1500f;
    [SerializeField] private float brakeForce = 3000f;
    [SerializeField] private float maxSteerAngle = 30f;
    [SerializeField] private float maxSpeed = 150f;
    
    [Header("Wheels")]
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;
    
    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;
    
    [Header("Player Interaction")]
    [SerializeField] private Transform driverSeat;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask playerLayer;
    
    [Header("Camera")]
    [SerializeField] private Transform vehicleCameraPoint;
    [SerializeField] private float cameraRotationSpeed = 2f;
    private float cameraYaw = 0f;
    private float cameraPitch = 0f;
    
    private float currentSteerAngle;
    private float currentBrakeForce;
    private bool isBeingDriven = false;
    private PlayerController currentDriver;
    private Rigidbody rb;
    
    [Header("Audio")]
    [SerializeField] private AudioSource engineSound;
    [SerializeField] private float minPitch = 0.5f;
    [SerializeField] private float maxPitch = 2f;
    
    [Header("Mobile Controls")]
    public bool isAccelerating = false;
    public bool isReversing = false;
    public bool isBraking = false;
    public bool isGasPressed = false; // Gas button
    
    private FixedJoystick joystick;
    
    void Start()
    {
        if (UIController.instance != null)
        {
            joystick = UIController.instance.joystick;
        }
        
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 1500f;
            rb.drag = 0.5f;
            rb.angularDrag = 1.0f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        
        // Fix xe nhảy - đặt center of mass thấp
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
        
        if (engineSound != null)
        {
            engineSound.loop = true;
            engineSound.volume = 0f;
        }
    }
    
    void FixedUpdate()
    {
        if (photonView.IsMine && isBeingDriven)
        {
            HandleMotor();
            HandleSteering();
            UpdateWheels();
            UpdateEngineSound();
            HandleCameraRotation();
        }
    }
    
    private void HandleCameraRotation()
    {
        if (vehicleCameraPoint == null) return;
        
        // Lấy input xoay camera
        float mouseX = SimpleInput.GetAxis("Look X");
        float mouseY = SimpleInput.GetAxis("Look Y");
        
        // Cập nhật rotation
        cameraYaw += mouseX * cameraRotationSpeed;
        cameraPitch -= mouseY * cameraRotationSpeed;
        cameraPitch = Mathf.Clamp(cameraPitch, -30f, 60f); // Giới hạn góc nhìn lên/xuống
        
        // Apply rotation to camera point
        vehicleCameraPoint.localRotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
    }
    
    private void HandleMotor()
    {
        float verticalInput = 0f;
        
        // GAS BUTTON - Đi thẳng
        if (SimpleInput.GetButton("Gas") || isGasPressed)
        {
            verticalInput = 1f;
        }
        // REVERSE BUTTON - Lùi
        else if (SimpleInput.GetButton("Reverse") || isReversing)
        {
            verticalInput = -1f;
        }
        // Fallback keyboard W/S
        else
        {
            verticalInput = SimpleInput.GetAxis("Vertical");
        }
            
        float currentSpeed = rb.velocity.magnitude * 3.6f; // Convert to km/h
        
        if (currentSpeed < maxSpeed)
        {
            frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
            frontRightWheelCollider.motorTorque = verticalInput * motorForce;
        }
        else
        {
            frontLeftWheelCollider.motorTorque = 0;
            frontRightWheelCollider.motorTorque = 0;
        }
        
        // Brake từ keyboard hoặc mobile button
        currentBrakeForce = (SimpleInput.GetButton("Brake") || isBraking) ? brakeForce : 0f;
        ApplyBraking();
    }
    
    private void ApplyBraking()
    {
        frontRightWheelCollider.brakeTorque = currentBrakeForce;
        frontLeftWheelCollider.brakeTorque = currentBrakeForce;
        rearLeftWheelCollider.brakeTorque = currentBrakeForce;
        rearRightWheelCollider.brakeTorque = currentBrakeForce;
    }
    
    private void HandleSteering()
    {
        float horizontalInput = 0f;
        
        // JOYSTICK để lái (chỉ trái/phải)
        if (joystick != null)
        {
            horizontalInput = joystick.Horizontal;
        }
        
        // Fallback keyboard A/D
        if (horizontalInput == 0f)
        {
            horizontalInput = SimpleInput.GetAxis("Horizontal");
        }
        
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }
    
    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }
    
    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }
    
    private void UpdateEngineSound()
    {
        if (engineSound != null)
        {
            float speedNormalized = rb.velocity.magnitude / (maxSpeed / 3.6f);
            engineSound.pitch = Mathf.Lerp(minPitch, maxPitch, speedNormalized);
            engineSound.volume = Mathf.Lerp(0.3f, 1f, speedNormalized);
        }
    }
    
    public bool CanEnterVehicle(Vector3 playerPosition)
    {
        if (isBeingDriven) return false;
        float distance = Vector3.Distance(transform.position, playerPosition);
        return distance <= interactionDistance;
    }
    
    [PunRPC]
    public void EnterVehicle(int playerActorNumber)
    {
        PhotonView playerView = PhotonView.Find(playerActorNumber);
        if (playerView != null)
        {
            PlayerController player = playerView.GetComponent<PlayerController>();
            if (player != null)
            {
                currentDriver = player;
                isBeingDriven = true;
                
                if (playerView.IsMine)
                {
                    player.EnterVehicle(this);
                    photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
                    
                    // Reset camera rotation
                    cameraYaw = 0f;
                    cameraPitch = 10f; // Initial pitch
                    
                    if (engineSound != null && !engineSound.isPlaying)
                    {
                        engineSound.Play();
                    }
                }
                
                // Teleport player to driver seat
                player.transform.position = driverSeat.position;
                player.transform.rotation = driverSeat.rotation;
                player.transform.SetParent(driverSeat);
            }
        }
    }
    
    [PunRPC]
    public void ExitVehicle(int playerActorNumber)
    {
        PhotonView playerView = PhotonView.Find(playerActorNumber);
        if (playerView != null)
        {
            PlayerController player = playerView.GetComponent<PlayerController>();
            if (player != null)
            {
                player.transform.SetParent(null);
                player.transform.position = exitPoint.position;
                player.transform.rotation = exitPoint.rotation;
                
                if (playerView.IsMine)
                {
                    player.ExitVehicle();
                    
                    if (engineSound != null)
                    {
                        engineSound.Stop();
                    }
                }
                
                currentDriver = null;
                isBeingDriven = false;
                
                // Stop the vehicle
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
    
    public Transform GetCameraPoint()
    {
        return vehicleCameraPoint;
    }
    
    public bool IsBeingDriven()
    {
        return isBeingDriven;
    }
    
    // Mobile button control methods
    public void OnGasPressed()
    {
        isGasPressed = true;
    }
    
    public void OnGasReleased()
    {
        isGasPressed = false;
    }
    
    public void OnReversePressed()
    {
        isReversing = true;
    }
    
    public void OnReverseReleased()
    {
        isReversing = false;
    }
    
    public void OnBrakePressed()
    {
        isBraking = true;
    }
    
    public void OnBrakeReleased()
    {
        isBraking = false;
    }
}
