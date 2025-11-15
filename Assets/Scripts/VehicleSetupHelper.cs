using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper script để setup xe nhanh chóng trong Unity Editor
/// Attach script này vào GameObject xe và click "Setup Vehicle" button
/// </summary>
public class VehicleSetupHelper : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Quick Setup - Click button below")]
    [SerializeField] private GameObject bodyObject;
    [SerializeField] private float wheelRadius = 0.35f;
    [SerializeField] private float wheelWidth = 0.3f;
    [SerializeField] private float wheelbaseLength = 2.4f;
    [SerializeField] private float wheelbaseWidth = 1.6f;

    public void SetupBasicVehicle()
    {
        Debug.Log("Setting up vehicle...");

        // Add Rigidbody if not exists
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 1500f;
            rb.drag = 0.1f;
            rb.angularDrag = 0.5f;
            Debug.Log("✓ Added Rigidbody");
        }

        // Add PhotonView if not exists
        var photonView = GetComponent<Photon.Pun.PhotonView>();
        if (photonView == null)
        {
            photonView = gameObject.AddComponent<Photon.Pun.PhotonView>();
            Debug.Log("✓ Added PhotonView");
        }

        // Add VehicleController if not exists
        VehicleController vehicleController = GetComponent<VehicleController>();
        if (vehicleController == null)
        {
            vehicleController = gameObject.AddComponent<VehicleController>();
            Debug.Log("✓ Added VehicleController");
        }

        // Create wheels
        CreateWheel("WheelFL", new Vector3(-wheelbaseWidth / 2, -0.4f, wheelbaseLength / 2), vehicleController);
        CreateWheel("WheelFR", new Vector3(wheelbaseWidth / 2, -0.4f, wheelbaseLength / 2), vehicleController);
        CreateWheel("WheelRL", new Vector3(-wheelbaseWidth / 2, -0.4f, -wheelbaseLength / 2), vehicleController);
        CreateWheel("WheelRR", new Vector3(wheelbaseWidth / 2, -0.4f, -wheelbaseLength / 2), vehicleController);

        // Create driver seat
        Transform driverSeat = transform.Find("DriverSeat");
        if (driverSeat == null)
        {
            GameObject seatObj = new GameObject("DriverSeat");
            seatObj.transform.SetParent(transform);
            seatObj.transform.localPosition = new Vector3(0, 0.5f, 0);
            Debug.Log("✓ Created DriverSeat");
        }

        // Create exit point
        Transform exitPoint = transform.Find("ExitPoint");
        if (exitPoint == null)
        {
            GameObject exitObj = new GameObject("ExitPoint");
            exitObj.transform.SetParent(transform);
            exitObj.transform.localPosition = new Vector3(-2f, 0, 0);
            Debug.Log("✓ Created ExitPoint");
        }

        // Create camera point
        Transform cameraPoint = transform.Find("CameraPoint");
        if (cameraPoint == null)
        {
            GameObject camObj = new GameObject("CameraPoint");
            camObj.transform.SetParent(transform);
            camObj.transform.localPosition = new Vector3(0, 2.5f, -5f);
            camObj.transform.localRotation = Quaternion.Euler(10, 0, 0);
            Debug.Log("✓ Created CameraPoint");
        }

        // Set layer to Vehicle
        int vehicleLayer = LayerMask.NameToLayer("Vehicle");
        if (vehicleLayer == -1)
        {
            Debug.LogWarning("⚠ Layer 'Vehicle' doesn't exist. Please create it manually.");
        }
        else
        {
            gameObject.layer = vehicleLayer;
            Debug.Log("✓ Set layer to Vehicle");
        }

        Debug.Log("✅ Vehicle setup complete! Don't forget to assign references in VehicleController.");
    }

    private void CreateWheel(string name, Vector3 localPosition, VehicleController controller)
    {
        Transform existingWheel = transform.Find(name);
        if (existingWheel != null)
        {
            Debug.Log($"Wheel {name} already exists, skipping...");
            return;
        }

        // Create wheel parent
        GameObject wheelObj = new GameObject(name);
        wheelObj.transform.SetParent(transform);
        wheelObj.transform.localPosition = localPosition;

        // Add wheel collider
        WheelCollider wheelCollider = wheelObj.AddComponent<WheelCollider>();
        wheelCollider.mass = 20f;
        wheelCollider.radius = wheelRadius;
        wheelCollider.wheelDampingRate = 0.25f;
        wheelCollider.suspensionDistance = 0.3f;
        wheelCollider.forceAppPointDistance = 0;

        JointSpring spring = wheelCollider.suspensionSpring;
        spring.spring = 35000f;
        spring.damper = 4500f;
        spring.targetPosition = 0.5f;
        wheelCollider.suspensionSpring = spring;

        WheelFrictionCurve forwardFriction = wheelCollider.forwardFriction;
        forwardFriction.stiffness = 1f;
        wheelCollider.forwardFriction = forwardFriction;

        WheelFrictionCurve sidewaysFriction = wheelCollider.sidewaysFriction;
        sidewaysFriction.stiffness = 1f;
        wheelCollider.sidewaysFriction = sidewaysFriction;

        // Create visual wheel
        GameObject wheelMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wheelMesh.name = "WheelMesh";
        wheelMesh.transform.SetParent(wheelObj.transform);
        wheelMesh.transform.localPosition = Vector3.zero;
        wheelMesh.transform.localRotation = Quaternion.Euler(0, 0, 90);
        wheelMesh.transform.localScale = new Vector3(wheelRadius * 2, wheelWidth / 2, wheelRadius * 2);

        // Remove collider from visual mesh
        Destroy(wheelMesh.GetComponent<Collider>());

        Debug.Log($"✓ Created {name}");
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(VehicleSetupHelper))]
public class VehicleSetupHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        VehicleSetupHelper helper = (VehicleSetupHelper)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Setup Vehicle", GUILayout.Height(40)))
        {
            helper.SetupBasicVehicle();
        }

        GUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "Click 'Setup Vehicle' to automatically create:\n" +
            "• Rigidbody\n" +
            "• PhotonView\n" +
            "• VehicleController\n" +
            "• 4 Wheels with colliders\n" +
            "• Driver seat, exit point, camera point",
            MessageType.Info);
    }
}
#endif
