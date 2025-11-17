# 🔧 CODE INTEGRATION GUIDE - PLAYERCONTROLLER

## 📌 Cách tích hợp NPC phản ứng với tiếng súng

### **OPTION 1: Tự động tích hợp (Recommended)**

Thêm đoạn code này vào cuối hàm `Shoot()` trong PlayerController.cs:

```csharp
private void Shoot()
{
    // ... existing code for shooting ...
    
    // Ray casting and damage code here
    
    // ========== THÊM PHẦN NÀY ========== 
    // Làm cho NPC phản ứng với tiếng súng
    if (NPCEventManager.instance != null)
    {
        NPCEventManager.instance.OnGunshotFired(allGuns[_selectedGun].firePoint.position);
    }
    // ===================================
}
```

### **OPTION 2: Tích hợp chi tiết hơn**

Nếu bạn muốn tùy chỉnh theo từng loại súng:

```csharp
private void Shoot()
{
    // ... existing shooting code ...
    
    // ========== THÊM PHẦN NÀY ========== 
    // Tính toán bán kính alert dựa trên loại súng
    float alertRadius = 20f; // Default
    
    if (allGuns[_selectedGun] != null)
    {
        // Súng lớn → alert radius lớn hơn
        if (allGuns[_selectedGun].weaponName.Contains("Sniper") || 
            allGuns[_selectedGun].weaponName.Contains("Rifle"))
        {
            alertRadius = 30f;
        }
        // Súng nhỏ → alert radius nhỏ hơn
        else if (allGuns[_selectedGun].weaponName.Contains("Pistol"))
        {
            alertRadius = 15f;
        }
    }
    
    // Thông báo cho NPC
    if (NPCEventManager.instance != null)
    {
        NPCEventManager.instance.OnGunshotFired(
            allGuns[_selectedGun].firePoint.position
        );
    }
    // ===================================
}
```

---

## 📌 Tích hợp cho chế độ Offline/Online

### **Update MatchManager.cs Start()**

Thêm vào đầu hàm `Start()` trong MatchManager.cs:

```csharp
void Start()
{
    // ========== THÊM PHẦN NÀY VÀO ĐẦU ========== 
    // Kiểm tra chế độ game
    if (GameModeManager.instance != null && GameModeManager.instance.IsOfflineMode())
    {
        Debug.Log("Offline Mode detected - Disabling online MatchManager");
        
        // Disable MatchManager này vì đang ở chế độ Offline
        this.enabled = false;
        
        // Tạo OfflineMatchManager nếu chưa có
        if (OfflineMatchManager.instance == null)
        {
            GameObject offlineManagerObj = new GameObject("OfflineMatchManager");
            offlineManagerObj.AddComponent<OfflineMatchManager>();
        }
        
        return; // Dừng việc khởi tạo MatchManager online
    }
    // ============================================
    
    // Phần code hiện tại của bạn
    if(!PhotonNetwork.IsConnected)
    {
        SceneManager.LoadScene(0);
    }
    else
    {
        NewPlayerSend(PhotonNetwork.NickName);
        state = GameState.Playing;

        SetupTimer();

        if(!PhotonNetwork.IsMasterClient)
        {
            UIController.instance.timerText.gameObject.SetActive(false);
        }
    }
}
```

---

## 📌 Cập nhật PlayerSpawner cho Offline Mode

### **Update PlayerSpawner.cs Start()**

```csharp
void Start()
{
    // ========== SỬA LẠI PHẦN NÀY ========== 
    // Kiểm tra cả Online và Offline mode
    bool isOnlineMode = PhotonNetwork.IsConnected;
    bool isOfflineMode = GameModeManager.instance != null && 
                         GameModeManager.instance.IsOfflineMode();
    
    if (isOnlineMode || isOfflineMode)
    {
        SpawnPlayer();
    }
    // =======================================
}
```

### **Update SpawnPlayer() function**

```csharp
public void SpawnPlayer()
{
    Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
    
    // ========== SỬA LẠI PHẦN NÀY ========== 
    // Spawn khác nhau cho Online và Offline
    if (PhotonNetwork.IsConnected)
    {
        // Online mode - dùng Photon
        _player = PhotonNetwork.Instantiate(playerPref.name, spawnPoint.position, spawnPoint.rotation);
    }
    else
    {
        // Offline mode - spawn local
        _player = Instantiate(playerPref, spawnPoint.position, spawnPoint.rotation);
    }
    // =======================================
}
```

---

## 📌 Xử lý Player Death trong Offline Mode

### **Update Die() function in PlayerSpawner.cs**

```csharp
public void Die(string damager)
{
    UIController.instance.deathText.text = $"You were killed by {damager}";

    // ========== THÊM XỬ LÝ CHO OFFLINE MODE ========== 
    // Update stats dựa trên mode
    if (PhotonNetwork.IsConnected)
    {
        // Online mode
        MatchManager.instance.UpdateStatsSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);
    }
    else
    {
        // Offline mode
        if (OfflineMatchManager.instance != null)
        {
            OfflineMatchManager.instance.AddDeath();
        }
    }
    // ==================================================

    if(_player != null)
    {
        StartCoroutine(DieCo());
    }
}
```

### **Update DieCo() function**

```csharp
public IEnumerator DieCo()
{
    // ========== SỬA LẠI DEATH EFFECT ========== 
    // Spawn death effect
    if (PhotonNetwork.IsConnected)
    {
        PhotonNetwork.Instantiate(deathEffect.name, _player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(_player);
    }
    else
    {
        Instantiate(deathEffect, _player.transform.position, Quaternion.identity);
        Destroy(_player);
    }
    // ===========================================

    _player = null;

    UIController.instance.deathScreen.SetActive(true);

    yield return new WaitForSeconds(respawnTime);

    UIController.instance.deathScreen.SetActive(false);

    // ========== SỬA LẠI RESPAWN CONDITION ========== 
    // Respawn trong cả 2 mode
    bool shouldRespawn = false;
    
    if (PhotonNetwork.IsConnected && MatchManager.instance != null)
    {
        shouldRespawn = MatchManager.instance.state == MatchManager.GameState.Playing;
    }
    else if (OfflineMatchManager.instance != null)
    {
        shouldRespawn = true; // Offline luôn respawn
    }

    if (shouldRespawn && _player == null)
    {
        SpawnPlayer();
    }
    // ================================================
}
```

---

## 📌 Cập nhật PlayerController để xử lý Kills trong Offline

### **Thêm vào DealDamage() hoặc khi kill enemy**

```csharp
[PunRPC]
public void DealDamage(string damager, int damageAmount, int actor)
{
    TakeDamage(damager, damageAmount, actor);
}

public void TakeDamage(string damager, int damageAmount, int actor)
{
    if (photonView.IsMine)
    {
        _currentHealth -= damageAmount;
        
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            PlayerSpawner.instance.Die(damager);
            
            // ========== THÊM PHẦN NÀY ========== 
            // Update kills cho người bắn
            if (!PhotonNetwork.IsConnected && OfflineMatchManager.instance != null)
            {
                // Trong Offline mode, chỉ có 1 player nên mọi kill đều là của player
                // (Trừ khi bạn thêm AI enemies)
                // Có thể xử lý logic kill counter ở đây
            }
            else if (PhotonNetwork.IsConnected && MatchManager.instance != null)
            {
                // Online mode - existing code
                MatchManager.instance.UpdateStatsSend(actor, 0, 1);
            }
            // ====================================
        }
        damageScreen.gameObject.SetActive(true);
        StartCoroutine(DeactivateDamageScreen());
        UIController.instance.healthSlider.value = _currentHealth;
    }
}
```

---

## 📌 Quick Reference - Tóm tắt các thay đổi cần thiết

### ✅ **Bắt buộc phải làm:**

1. **PlayerController.cs** → Thêm NPC gunshot reaction trong `Shoot()`
2. **MatchManager.cs** → Thêm check Offline mode trong `Start()`
3. **PlayerSpawner.cs** → Sửa `Start()`, `SpawnPlayer()`, `Die()`, `DieCo()`

### 🔧 **Tùy chọn (nâng cao):**

1. **PlayerController.cs** → Tùy chỉnh alert radius theo loại súng
2. **OfflineMatchManager.cs** → Thêm win conditions, UI custom

---

## 🎯 Testing Checklist

Sau khi tích hợp, test các scenario sau:

### **Online Mode:**
- [ ] Kết nối Photon thành công
- [ ] Timer hiển thị đúng
- [ ] Multiplayer hoạt động
- [ ] Kill/Death tracking qua Photon

### **Offline Mode:**
- [ ] Vào game không cần Photon
- [ ] Không hiển thị timer
- [ ] NPC spawn và di chuyển
- [ ] NPC phản ứng với tiếng súng
- [ ] Player có thể respawn

### **NPC System:**
- [ ] NPC spawn tự động
- [ ] NPC đi lại ngẫu nhiên
- [ ] NPC đôi khi chạy, đôi khi đi bộ
- [ ] NPC dừng lại và chờ
- [ ] NPC chạy trốn khi nghe tiếng súng

---

## 💡 Tips

1. **Debug Mode**: Bật debug mode trong NPCEventManager để xem visual cues
2. **NavMesh**: Luôn kiểm tra NavMesh đã bake đúng trước khi test NPC
3. **Performance**: Nếu FPS giảm, giảm số lượng NPC trong NPCSpawner
4. **Testing**: Test cả 2 modes riêng biệt để đảm bảo không conflict

---

## 🐛 Common Issues

### **Issue: NPC không spawn trong Offline**
**Fix**: Kiểm tra scene có NPCSpawner và prefabs đã được gán

### **Issue: Timer vẫn hiện trong Offline**
**Fix**: Kiểm tra MatchManager.Start() có disable đúng không

### **Issue: Cannot spawn player in Offline**
**Fix**: Sửa PlayerSpawner.SpawnPlayer() để handle cả local Instantiate

---

Chúc bạn code thành công! 🚀
