using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HelicopterController : MonoBehaviourPunCallbacks
{
    [Header("Helicopter Settings")]
    [SerializeField] private float liftForce = 30000f;  // Tăng lên để bay được!
    [SerializeField] private float descendForce = 3000f;
    [SerializeField] private float tiltForce = 30f;
    [SerializeField] private float maxTiltAngle = 45f;
    [SerializeField] private float stabilizationSpeed = 2f;
    [SerializeField] private float forwardSpeed = 3000f;
    [SerializeField] private float maxSpeed = 50f;
    [SerializeField] private float rotationSpeed = 100f;
    
    [Header("Rotor Animation Settings")]
    [SerializeField] private Animator helicopterAnimator;
    [SerializeField] private string idleAnimationName = "Helicopter_Idle";
    [SerializeField] private float maxRotorAnimationSpeed = 1f;
    [SerializeField] private float rotorSpeedUpRate = 0.5f;
    [SerializeField] private float rotorSlowDownRate = 0.3f;
    
    [Header("Player Interaction")]
    [SerializeField] private Transform pilotSeat;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask playerLayer;
    
    [Header("Camera")]
    [SerializeField] private Transform helicopterCameraPoint;
    [SerializeField] private float cameraRotationSpeed = 2f;
    
    private float cameraYaw = 0f;
    private float cameraPitch = 10f;
    private bool isBeingFlown = false;
    private PlayerController currentPilot;
    private Rigidbody rb;
    
    [Header("Audio")]
    [SerializeField] private AudioSource rotorSound;
    [SerializeField] private float minPitch = 0.5f;
    [SerializeField] private float maxPitch = 1.5f;
    
    [Header("Mobile Controls")]
    public bool isAscending = false;  // Gas button - bay lên
    public bool isDescending = false; // Reverse button - hạ xuống
    
    private FixedJoystick joystick;
    private float currentRotorSpeed = 0f;
    private Vector3 currentTilt = Vector3.zero;
    
    void Start()
    {
        // Get joystick reference
        if (UIController.instance != null)
        {
            joystick = UIController.instance.joystick;
        }
        
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 2000f;
            rb.drag = 1f;
            rb.angularDrag = 3f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.useGravity = true;
        }
        
        // Set center of mass lower for stability
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
        
        if (rotorSound != null)
        {
            rotorSound.loop = true;
            rotorSound.volume = 0f;
        }
    }
    
    void FixedUpdate()
    {
        if (photonView.IsMine && isBeingFlown)
        {
            HandleVerticalMovement();
            HandleTilt();
            HandleForwardMovement();
            HandleRotation();
            UpdateRotorSound();
            HandleCameraRotation();
        }
        
        // Always update rotors (with gradual speed changes)
        UpdateRotors();
    }
    
    private void HandleVerticalMovement()
    {
        // GAS BUTTON - Bay lên
        if (SimpleInput.GetButton("Gas") || isAscending)
        {
            rb.AddForce(Vector3.up * liftForce, ForceMode.Force);
            currentRotorSpeed = Mathf.Lerp(currentRotorSpeed, 1f, Time.fixedDeltaTime * 2f);
            Debug.Log($"Lifting! Force: {liftForce}, Velocity: {rb.velocity.y}");
        }
        // REVERSE BUTTON - Hạ xuống
        else if (SimpleInput.GetButton("Reverse") || isDescending)
        {
            rb.AddForce(Vector3.down * descendForce, ForceMode.Force);
            currentRotorSpeed = Mathf.Lerp(currentRotorSpeed, 0.3f, Time.fixedDeltaTime * 2f);
            Debug.Log($"Descending! Force: {descendForce}, Velocity: {rb.velocity.y}");
        }
        else
        {
            // Hover mode - duy trì độ cao
            currentRotorSpeed = Mathf.Lerp(currentRotorSpeed, 0.5f, Time.fixedDeltaTime);
        }
    }
    
    private void HandleTilt()
    {
        float tiltX = 0f;
        float tiltZ = 0f;
        
        // JOYSTICK để nghiêng trực thăng
        if (joystick != null)
        {
            tiltX = joystick.Vertical;  // Nghiêng trước/sau
            tiltZ = -joystick.Horizontal; // Nghiêng trái/phải
        }
        
        // Fallback keyboard
        if (tiltX == 0f && tiltZ == 0f)
        {
            tiltX = SimpleInput.GetAxis("Vertical");
            tiltZ = -SimpleInput.GetAxis("Horizontal");
        }
        
        // Apply tilt
        Vector3 targetTilt = new Vector3(tiltX * maxTiltAngle, 0f, tiltZ * maxTiltAngle);
        currentTilt = Vector3.Lerp(currentTilt, targetTilt, Time.fixedDeltaTime * tiltForce);
        
        // Rotate helicopter based on tilt
        Quaternion targetRotation = Quaternion.Euler(currentTilt.x, transform.eulerAngles.y, currentTilt.z);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * stabilizationSpeed));
    }
    
    private void HandleForwardMovement()
    {
        // Di chuyển theo hướng nghiêng
        Vector3 forwardDirection = transform.forward * currentTilt.x + transform.right * -currentTilt.z;
        
        if (forwardDirection.magnitude > 0.1f)
        {
            float currentSpeed = rb.velocity.magnitude;
            if (currentSpeed < maxSpeed)
            {
                rb.AddForce(forwardDirection.normalized * forwardSpeed, ForceMode.Force);
            }
        }
    }
    
    private void HandleRotation()
    {
        // Rotation với A/D hoặc keyboard
        float rotationInput = 0f;
        
        // Có thể dùng Q/E để xoay nếu muốn
        if (Input.GetKey(KeyCode.Q))
            rotationInput = -1f;
        else if (Input.GetKey(KeyCode.E))
            rotationInput = 1f;
        
        if (Mathf.Abs(rotationInput) > 0.1f)
        {
            float rotation = rotationInput * rotationSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0f, rotation, 0f);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
    }
    
    private void UpdateRotors()
    {
        // Điều khiển animation speed trực tiếp (không cần parameter)
        if (helicopterAnimator == null) 
        {
            Debug.LogWarning("Helicopter Animator chưa được gán!");
            return;
        }
        
        // Tính target speed dựa trên trạng thái bay
        float targetSpeed = 0f;
        
        if (isBeingFlown)
        {
            // Khi đang bay, tốc độ rotor phụ thuộc vào currentRotorSpeed (0-1)
            targetSpeed = currentRotorSpeed * maxRotorAnimationSpeed;
        }
        else
        {
            // Khi không bay, về 0
            targetSpeed = 0f;
        }
        
        // Lerp animation speed mượt mà
        float currentSpeed = helicopterAnimator.speed;
        float newSpeed;
        
        if (isBeingFlown)
        {
            // Tăng tốc dần khi vào helicopter
            newSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.fixedDeltaTime * rotorSpeedUpRate);
        }
        else
        {
            // Giảm tốc dần khi ra khỏi helicopter
            newSpeed = Mathf.Lerp(currentSpeed, 0f, Time.fixedDeltaTime * rotorSlowDownRate);
        }
        
        // Set animator speed trực tiếp
        helicopterAnimator.speed = newSpeed;
        
        Debug.Log($"Rotor Animation Speed: {newSpeed:F2} (Target: {targetSpeed:F2}, IsFlying: {isBeingFlown})");
    }
    
    private void UpdateRotorSound()
    {
        if (rotorSound != null)
        {
            rotorSound.pitch = Mathf.Lerp(minPitch, maxPitch, currentRotorSpeed);
            rotorSound.volume = Mathf.Lerp(0.3f, 1f, currentRotorSpeed);
        }
    }
    
    private void HandleCameraRotation()
    {
        if (helicopterCameraPoint == null) return;
        
        // Lấy input xoay camera
        float mouseX = SimpleInput.GetAxis("Look X");
        float mouseY = SimpleInput.GetAxis("Look Y");
        
        // Cập nhật rotation
        cameraYaw += mouseX * cameraRotationSpeed;
        cameraPitch -= mouseY * cameraRotationSpeed;
        cameraPitch = Mathf.Clamp(cameraPitch, -40f, 80f);
        
        // Apply rotation to camera point
        helicopterCameraPoint.localRotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
    }
    
    public bool CanEnterHelicopter(Vector3 playerPosition)
    {
        if (isBeingFlown) return false;
        float distance = Vector3.Distance(transform.position, playerPosition);
        return distance <= interactionDistance;
    }
    
    [PunRPC]
    public void EnterHelicopter(int playerActorNumber)
    {
        PhotonView playerView = PhotonView.Find(playerActorNumber);
        if (playerView != null)
        {
            PlayerController player = playerView.GetComponent<PlayerController>();
            if (player != null)
            {
                currentPilot = player;
                isBeingFlown = true;
                
                if (playerView.IsMine)
                {
                    player.EnterHelicopter(this);
                    photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
                    
                    // Reset camera rotation
                    cameraYaw = 0f;
                    cameraPitch = 10f;
                    
                    if (rotorSound != null && !rotorSound.isPlaying)
                    {
                        rotorSound.Play();
                    }
                }
                
                // Teleport player to pilot seat
                player.transform.position = pilotSeat.position;
                player.transform.rotation = pilotSeat.rotation;
                player.transform.SetParent(pilotSeat);
            }
        }
    }
    
    [PunRPC]
    public void ExitHelicopter(int playerActorNumber)
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
                    
                    if (rotorSound != null)
                    {
                        rotorSound.Stop();
                    }
                }
                
                currentPilot = null;
                isBeingFlown = false;
                currentRotorSpeed = 0f;
                
                // Stop movement
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
    
    public Transform GetCameraPoint()
    {
        return helicopterCameraPoint;
    }
    
    public bool IsBeingFlown()
    {
        return isBeingFlown;
    }
    
    // Mobile button control methods - Giống xe
    public void OnGasPressed()
    {
        if (!photonView.IsMine || !isBeingFlown) return;
        Debug.Log("Helicopter Gas Pressed!");
        isAscending = true;
    }
    
    public void OnGasReleased()
    {
        if (!photonView.IsMine || !isBeingFlown) return;
        Debug.Log("Helicopter Gas Released!");
        isAscending = false;
    }
    
    public void OnReversePressed()
    {
        if (!photonView.IsMine || !isBeingFlown) return;
        Debug.Log("Helicopter Reverse Pressed!");
        isDescending = true;
    }
    
    public void OnReverseReleased()
    {
        if (!photonView.IsMine || !isBeingFlown) return;
        Debug.Log("Helicopter Reverse Released!");
        isDescending = false;
    }
}
