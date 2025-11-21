using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AmmoBox : MonoBehaviourPunCallbacks
{
    [Header("Ammo Box Settings")]
    [SerializeField] private int ammoAmount = 30; // Số đạn sẽ được thêm vào
    [SerializeField] private float respawnTime = 30f; // Thời gian respawn hộp đạn (giây)
    [SerializeField] private AudioClip pickupSound; // Âm thanh khi nhặt đạn
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject pickupEffect; // Hiệu ứng khi nhặt đạn
    [SerializeField] private GameObject boxModel; // Model của hộp đạn
    
    private bool canPickup = true;
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; // 3D sound
        audioSource.maxDistance = 10f;
        
        if (boxModel == null)
        {
            boxModel = transform.GetChild(0).gameObject;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Chỉ xử lý khi va chạm với player và player đó thuộc về local client
        if (other.CompareTag("Player") && other.GetComponent<PhotonView>().IsMine && canPickup)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // Thêm đạn cho player
                bool ammoAdded = player.AddAmmo(ammoAmount);
                
                if (ammoAdded)
                {
                    // Hiển thị notification (nếu có)
                    if (AmmoPickupNotification.instance != null)
                    {
                        AmmoPickupNotification.instance.ShowAmmoPickup(ammoAmount);
                    }
                    
                    // Gọi RPC để ẩn hộp đạn cho tất cả clients
                    photonView.RPC("PickupAmmo", RpcTarget.All);
                }
            }
        }
    }
    
    [PunRPC]
    void PickupAmmo()
    {
        canPickup = false;
        
        // Phát âm thanh
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
        
        // Hiệu ứng pickup
        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // Ẩn model
        if (boxModel != null)
        {
            boxModel.SetActive(false);
        }
        
        // Chỉ Master Client mới thực hiện respawn
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(RespawnAmmoBox());
        }
    }
    
    IEnumerator RespawnAmmoBox()
    {
        yield return new WaitForSeconds(respawnTime);
        
        // Gọi RPC để hiện lại hộp đạn cho tất cả clients
        photonView.RPC("RespawnAmmo", RpcTarget.All);
    }
    
    [PunRPC]
    void RespawnAmmo()
    {
        canPickup = true;
        
        // Hiện lại model
        if (boxModel != null)
        {
            boxModel.SetActive(true);
        }
    }
    
    // Tùy chọn: Thêm hiệu ứng quay cho hộp đạn
    void Update()
    {
        if (canPickup && boxModel != null && boxModel.activeInHierarchy)
        {
            boxModel.transform.Rotate(Vector3.up * 50f * Time.deltaTime);
        }
    }
}
