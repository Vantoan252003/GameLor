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
    [SerializeField] private float forwardSpeed = 10000f;
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
    public bool isRotatingLeft = false;  // Rotate Left button
    public bool isRotatingRight = false; // Rotate Right button
    
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
        // Chỉ cho bay khi rotor speed đủ nhanh (> 0.6)
        float minRotorSpeedToFly = 0.6f;
        
        // GAS BUTTON - Bay lên
        if (SimpleInput.GetButton("Gas") || isAscending)
        {
            // Rotor luôn tăng tốc khi nhấn Gas
            currentRotorSpeed = Mathf.Lerp(currentRotorSpeed, 1f, Time.fixedDeltaTime * 2f);
            
            // Chỉ bay lên khi rotor đủ nhanh
            if (currentRotorSpeed >= minRotorSpeedToFly)
            {
                rb.AddForce(Vector3.up * liftForce, ForceMode.Force);
            }
        }
        // REVERSE BUTTON - Hạ xuống
        else if (SimpleInput.GetButton("Reverse") || isDescending)
        {
            rb.AddForce(Vector3.down * descendForce, ForceMode.Force);
            currentRotorSpeed = Mathf.Lerp(currentRotorSpeed, 0.3f, Time.fixedDeltaTime * 2f);
        }
        else
        {
            // Hover mode - duy trì độ cao (chống lại trọng lực)
            currentRotorSpeed = Mathf.Lerp(currentRotorSpeed, 0.65f, Time.fixedDeltaTime);
            
            // Apply hover force để chống trọng lực (không bay lên, chỉ giữ độ cao)
            if (currentRotorSpeed >= minRotorSpeedToFly)
            {
                // Hover force = một phần của lift force để giữ cân bằng
                float hoverForce = liftForce * 0.7f; // 40% lift force để hover
                rb.AddForce(Vector3.up * hoverForce, ForceMode.Force);
            }
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

        Vector3 eulerAngles = transform.eulerAngles;
        float pitchAngle = eulerAngles.x > 180 ? eulerAngles.x - 360 : eulerAngles.x;
        float rollAngle = eulerAngles.z > 180 ? eulerAngles.z - 360 : eulerAngles.z;
        
        Vector3 moveDirection = Vector3.zero;
        
        if (Mathf.Abs(pitchAngle) > 2f)
        {
            float pitchFactor = Mathf.Clamp(pitchAngle / maxTiltAngle, -1f, 1f);
            moveDirection += transform.forward * pitchFactor;
        }
        
        // Di chuyển trái/phải khi nghiêng sang
        if (Mathf.Abs(rollAngle) > 2f)
        {
            float rollFactor = Mathf.Clamp(rollAngle / maxTiltAngle, -1f, 1f);
            moveDirection += -transform.right * rollFactor;
        }
        
        if (moveDirection.magnitude > 0.01f && currentRotorSpeed > 0.5f)
        {
            float speedMultiplier = 10;
            rb.AddForce(moveDirection.normalized * forwardSpeed * speedMultiplier, ForceMode.Force);
        }
        
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
        }
    }
    private void HandleRotation()
    {
        float rotationInput = 0f;
        
        // PC controls (Q/E)
        if (Input.GetKey(KeyCode.Q))
            rotationInput = -1f;
        else if (Input.GetKey(KeyCode.E))
            rotationInput = 1f;
        
        // Mobile controls (buttons)
        if (isRotatingLeft)
            rotationInput = -1f;
        else if (isRotatingRight)
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
        if (helicopterAnimator == null) return;
        float targetSpeed = 0f;
        
        if (isBeingFlown)
        {
            targetSpeed = currentRotorSpeed * maxRotorAnimationSpeed;
        }
        else
        {
            targetSpeed = 0f;
        }
        
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
    
    public void OnGasPressed()
    {
        if (!photonView.IsMine || !isBeingFlown) return;
        isAscending = true;
    }
    
    public void OnGasReleased()
    {
        if (!photonView.IsMine || !isBeingFlown) return;
        isAscending = false;
    }
    
    public void OnReversePressed()
    {
        if (!photonView.IsMine || !isBeingFlown) return;
        isDescending = true;
    }
    
    public void OnReverseReleased()
    {
        if (!photonView.IsMine || !isBeingFlown) return;
        isDescending = false;
    }
    
    // Mobile rotation control methods - Giống Q và E
    public void OnRotateLeftPressed()
    {
        if (!photonView.IsMine || !isBeingFlown) return;
        isRotatingLeft = true;
    }
    
    public void OnRotateLeftReleased()
    {
        if (!photonView.IsMine || !isBeingFlown) return;
        isRotatingLeft = false;
    }
    
    public void OnRotateRightPressed()
    {
        if (!photonView.IsMine || !isBeingFlown) return;
        isRotatingRight = true;
    }
    
    public void OnRotateRightReleased()
    {
        if (!photonView.IsMine || !isBeingFlown) return;
        isRotatingRight = false;
    }
}
