using UnityEngine;

/// <summary>
/// Script đơn giản để test xe nhanh mà không cần Photon
/// Dùng cho testing local trước khi implement multiplayer
/// </summary>
public class SimpleVehicleTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WheelCollider[] frontWheels;
    [SerializeField] private WheelCollider[] rearWheels;
    [SerializeField] private Transform[] wheelMeshes;
    
    [Header("Settings")]
    [SerializeField] private float motorForce = 1500f;
    [SerializeField] private float brakeForce = 3000f;
    [SerializeField] private float maxSteerAngle = 30f;
    
    private float currentSteerAngle;
    private float currentBrakeForce;
    
    void Update()
    {

        float motor = Input.GetAxis("Vertical") * motorForce;
        float steering = Input.GetAxis("Horizontal") * maxSteerAngle;
        currentBrakeForce = Input.GetKey(KeyCode.Space) ? brakeForce : 0f;
        foreach (WheelCollider wheel in frontWheels)
        {
            wheel.steerAngle = steering;
            wheel.brakeTorque = currentBrakeForce;
        }
        
        foreach (WheelCollider wheel in rearWheels)
        {
            wheel.motorTorque = motor;
            wheel.brakeTorque = currentBrakeForce;
        }
        UpdateWheelMeshes();
    }
    
    private void UpdateWheelMeshes()
    {
        WheelCollider[] allWheels = new WheelCollider[frontWheels.Length + rearWheels.Length];
        frontWheels.CopyTo(allWheels, 0);
        rearWheels.CopyTo(allWheels, frontWheels.Length);
        
        for (int i = 0; i < allWheels.Length && i < wheelMeshes.Length; i++)
        {
            if (wheelMeshes[i] != null)
            {
                Vector3 pos;
                Quaternion rot;
                allWheels[i].GetWorldPose(out pos, out rot);
                wheelMeshes[i].position = pos;
                wheelMeshes[i].rotation = rot;
            }
        }
    }
}
