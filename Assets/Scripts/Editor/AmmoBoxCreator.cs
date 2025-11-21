using UnityEngine;
using UnityEditor;
using Photon.Pun;

#if UNITY_EDITOR
/// <summary>
/// Script hỗ trợ tạo AmmoBox Prefab nhanh chóng trong Unity Editor
/// Menu: GameObject > Multiplayer FPS > Create AmmoBox
/// </summary>
public class AmmoBoxCreator : MonoBehaviour
{
    [MenuItem("GameObject/Multiplayer FPS/Create AmmoBox", false, 10)]
    static void CreateAmmoBox(MenuCommand menuCommand)
    {
        // Tạo parent GameObject
        GameObject ammoBox = new GameObject("AmmoBox");
        GameObjectUtility.SetParentAndAlign(ammoBox, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(ammoBox, "Create " + ammoBox.name);
        Selection.activeObject = ammoBox;

        // Thêm PhotonView
        PhotonView photonView = ammoBox.AddComponent<PhotonView>();
        photonView.OwnershipTransfer = OwnershipOption.Fixed;
        photonView.Synchronization = ViewSynchronization.Off;

        // Thêm BoxCollider (Trigger)
        BoxCollider boxCollider = ammoBox.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        boxCollider.size = new Vector3(1f, 1f, 1f);

        // Thêm AmmoBox script
        AmmoBox ammoBoxScript = ammoBox.AddComponent<AmmoBox>();

        // Tạo Model (Cube)
        GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cube);
        model.name = "Model";
        model.transform.SetParent(ammoBox.transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localScale = Vector3.one * 0.5f;

        // Remove collider từ model (vì parent đã có)
        DestroyImmediate(model.GetComponent<Collider>());

        // Tạo material màu vàng cho hộp đạn
        Material ammoMaterial = new Material(Shader.Find("Standard"));
        ammoMaterial.color = new Color(1f, 0.8f, 0f); // Màu vàng
        model.GetComponent<Renderer>().material = ammoMaterial;

        // Assign model vào script (dùng SerializedObject để tránh lỗi)
        SerializedObject serializedObject = new SerializedObject(ammoBoxScript);
        SerializedProperty boxModelProperty = serializedObject.FindProperty("boxModel");
        boxModelProperty.objectReferenceValue = model;
        serializedObject.ApplyModifiedProperties();

        Debug.Log("AmmoBox created successfully! Configure the AmmoBox component settings and save as Prefab.");
    }

    [MenuItem("GameObject/Multiplayer FPS/Create Ammo Spawn Point", false, 11)]
    static void CreateAmmoSpawnPoint(MenuCommand menuCommand)
    {
        // Tạo spawn point (empty GameObject)
        GameObject spawnPoint = new GameObject("AmmoSpawnPoint");
        GameObjectUtility.SetParentAndAlign(spawnPoint, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(spawnPoint, "Create " + spawnPoint.name);
        Selection.activeObject = spawnPoint;

        // Thêm icon để dễ nhìn trong Scene view
        #if UNITY_EDITOR
        var icon = EditorGUIUtility.IconContent("sv_icon_dot3_pix16_gizmo").image as Texture2D;
        var egu = typeof(EditorGUIUtility);
        var flags = System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
        var args = new object[] { spawnPoint, icon };
        var setIcon = egu.GetMethod("SetIconForObject", flags, null, new System.Type[] { typeof(UnityEngine.Object), typeof(Texture2D) }, null);
        setIcon.Invoke(null, args);
        #endif

        Debug.Log("Ammo Spawn Point created! Position it where you want ammo boxes to spawn.");
    }
}
#endif
