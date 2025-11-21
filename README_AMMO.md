# 🎮 Hệ Thống Đạn & Reload - FPS Multiplayer

## 📦 Tổng Quan

Hệ thống đạn hoàn chỉnh cho FPS Multiplayer game với:
- ✅ Reload Button (UI + Keyboard)
- ✅ Hộp Đạn trên Map
- ✅ Đạn Dự Trữ (Reserve Ammo)
- ✅ Multiplayer Sync (Photon)
- ✅ UI Notifications
- ✅ Auto Respawn

---

## 📁 Files Đã Tạo

### Core Scripts (Required)
```
Assets/Scripts/
├── AmmoBox.cs                    ✅ Hộp đạn logic
├── AmmoSpawnManager.cs           ✅ Quản lý spawn
├── AmmoPickupNotification.cs     ⭐ UI notification (optional)
└── PlayerController.cs           ✅ Updated with ammo system
```

### Editor Tools
```
Assets/Scripts/Editor/
└── AmmoBoxCreator.cs             ✅ Menu tạo AmmoBox nhanh
```

### Documentation
```
AMMO_SYSTEM_SUMMARY.md            📖 Tóm tắt tổng quan
AMMO_SYSTEM_SETUP.md              📖 Hướng dẫn chi tiết
AMMO_QUICK_START.md               📖 Setup nhanh 5 phút
README_AMMO.md                    📖 File này
```

---

## 🚀 Quick Start (3 Bước)

### 1️⃣ Tạo AmmoBox Prefab

```
Unity Menu → GameObject → Multiplayer FPS → Create AmmoBox
```

Hoặc thủ công:
- Create Empty → "AmmoBox"
- Add: PhotonView, BoxCollider (Trigger), AmmoBox script
- Create child Cube → Assign vào Box Model
- **QUAN TRỌNG**: Lưu vào `Assets/Resources/AmmoBox.prefab`

### 2️⃣ Đặt Hộp Đạn Trên Map

**Cách A: Đơn giản**
```
Drag AmmoBox.prefab vào Scene → Đặt nhiều vị trí
```

**Cách B: Dùng Manager (Multiplayer)**
```
1. Create Empty → "AmmoSpawnManager"
2. Add Component: AmmoSpawnManager
3. Assign AmmoBox Prefab
4. Tạo spawn points (GameObject → Multiplayer FPS → Create Ammo Spawn Point)
5. Assign spawn points vào Manager
```

### 3️⃣ Test

```
Play → Bắn → Reload (R) → Tìm hộp đạn (màu vàng) → Nhặt → Done! ✅
```

---

## 🎯 Tính Năng Chi Tiết

### 1. Reload System
- **Keyboard**: Nhấn `R`
- **UI Button**: Click Reload Button
- **Logic**: Chỉ reload khi có đạn dự trữ
- **Animation**: Tự động trigger reload animation

### 2. Ammo Box System
- **Pickup**: Tự động khi va chạm
- **Amount**: 30 đạn mặc định (configurable)
- **Respawn**: 30 giây (configurable)
- **Visual**: Quay tròn, màu vàng
- **Sync**: Photon RPC cho multiplayer

### 3. Reserve Ammo
- **Per Gun**: Mỗi súng có đạn dự trữ riêng
- **Max**: 210 đạn (configurable)
- **UI**: Hiển thị `30 / 180` (trong băng / dự trữ)
- **Persistent**: Giữ nguyên khi đổi súng

### 4. UI Notification (Optional)
- **Pickup**: Hiển thị "+30 Ammo!"
- **Animation**: Fade in/out smooth
- **Duration**: 2 giây (configurable)

---

## ⚙️ Configuration

### AmmoBox Settings
```csharp
Ammo Amount: 30         // Số đạn mỗi lần nhặt
Respawn Time: 30        // Giây
Pickup Sound: (Optional)
Pickup Effect: (Optional)
```

### PlayerController Settings
```csharp
Max Reserve Ammo: 210   // Đạn dự trữ tối đa
```

### AmmoSpawnManager Settings
```csharp
Spawn On Start: true    // Tự động spawn khi game bắt đầu
Spawn Delay: 1          // Delay trước khi spawn (giây)
Show Gizmos: true       // Hiển thị spawn points trong Scene
```

---

## 🎨 Customization Examples

### Thay đổi màu hộp đạn
```csharp
// AmmoBox Model Renderer Material
color = Color.green; // Xanh lá cho rifle ammo
color = Color.red;   // Đỏ cho pistol ammo
```

### Tăng số đạn
```csharp
// AmmoBox Inspector
ammoAmount = 50; // Thay vì 30
```

### Giảm thời gian respawn
```csharp
// AmmoBox Inspector
respawnTime = 15f; // 15 giây thay vì 30
```

### Thay đổi đạn max
```csharp
// PlayerController Inspector
maxReserveAmmo = 300; // Thay vì 210
```

---

## 🐛 Troubleshooting

### ❌ Không reload được
**Nguyên nhân**: Không có đạn dự trữ
**Fix**: Tìm và nhặt hộp đạn trên map

### ❌ Không nhặt được hộp đạn
**Nguyên nhân**: 
- Player không có Tag "Player"
- BoxCollider không phải Trigger
- Đạn dự trữ đã full

**Fix**:
1. Select Player → Inspector → Tag = "Player"
2. AmmoBox → BoxCollider → Is Trigger ✓
3. Bắn bớt đạn để có chỗ trống

### ❌ Multiplayer không sync
**Nguyên nhân**: Prefab không trong Resources folder
**Fix**: 
```
Move AmmoBox.prefab to Assets/Resources/
```

### ❌ Hộp đạn không respawn
**Nguyên nhân**: Không phải Master Client
**Fix**: Master Client sẽ tự động handle respawn

### ❌ UI không hiển thị đúng
**Nguyên nhân**: UIController.ammoText chưa assign
**Fix**: 
```
UIController Inspector → Assign Ammo Text (TextMeshProUGUI)
```

---

## 📊 API Reference

### PlayerController

#### `bool AddAmmo(int amount)`
Thêm đạn vào dự trữ của súng hiện tại
```csharp
// Example
if (player.AddAmmo(30)) {
    Debug.Log("Added 30 ammo!");
}
```

#### `void ReloadWeapon()`
Nạp đạn từ dự trữ vào băng đạn
```csharp
// Called automatically khi nhấn R hoặc Reload Button
```

### AmmoBox

#### `void PickupAmmo()` [PunRPC]
Xử lý pickup, được gọi qua RPC
```csharp
// Called automatically khi player va chạm
```

### AmmoSpawnManager

#### `void SpawnAllAmmoBoxes()`
Spawn tất cả hộp đạn tại spawn points
```csharp
// Example
ammoSpawnManager.SpawnAllAmmoBoxes();
```

#### `void ClearSpawnedAmmoBoxes()`
Xóa tất cả hộp đạn đã spawn
```csharp
// Example - useful for reset round
ammoSpawnManager.ClearSpawnedAmmoBoxes();
```

### AmmoPickupNotification (Optional)

#### `void ShowAmmoPickup(int ammoAmount)`
Hiển thị thông báo nhặt đạn
```csharp
// Example
AmmoPickupNotification.instance.ShowAmmoPickup(30);
```

#### `void ShowCustomNotification(string message)`
Hiển thị thông báo tùy chỉnh
```csharp
// Example
AmmoPickupNotification.instance.ShowCustomNotification("Max Ammo!");
```

---

## 🔧 Advanced Usage

### Tạo nhiều loại hộp đạn

```csharp
// Trong AmmoBox.cs, thêm:
public enum AmmoType { Rifle, Pistol, Shotgun }
[SerializeField] private AmmoType ammoType;

// Trong PlayerController.cs, update AddAmmo:
public bool AddAmmo(int amount, AmmoType type) {
    // Logic cho từng loại đạn
}
```

### Spawn hộp đạn động

```csharp
// Trong GameManager
void SpawnAmmoAtPosition(Vector3 position) {
    if (PhotonNetwork.IsMasterClient) {
        PhotonNetwork.Instantiate("AmmoBox", position, Quaternion.identity);
    }
}
```

### Track statistics

```csharp
// Thêm vào PlayerController
private int totalAmmoPickedUp = 0;
private int reloadCount = 0;

public void AddAmmo(int amount) {
    totalAmmoPickedUp += amount;
    // ... rest of code
}
```

---

## 🎓 Best Practices

### 1. Prefab Organization
```
Assets/Resources/
├── AmmoBox.prefab           ✅ Must be here for Photon
├── AmmoBox_Rifle.prefab     
├── AmmoBox_Pistol.prefab    
└── AmmoBox_Special.prefab   
```

### 2. Layer Setup
```
Create separate layer for AmmoBox
Player layer should collide with AmmoBox layer
```

### 3. Testing Workflow
```
1. Test single player first
2. Test multiplayer với 2 clients
3. Test Master Client disconnect scenario
4. Test với nhiều hộp đạn cùng lúc
```

### 4. Performance
```
- Dùng Object Pooling nếu spawn/despawn nhiều
- Limit số hộp đạn active (max 20-30)
- Use LOD cho model nếu cần
```

---

## 📈 Roadmap (Future Features)

### Phase 1 ✅ (Complete)
- [x] Basic reload system
- [x] Ammo box pickup
- [x] Reserve ammo
- [x] Multiplayer sync
- [x] UI notification

### Phase 2 🚧 (Planned)
- [ ] Multiple ammo types
- [ ] Golden ammo boxes (bonus)
- [ ] Ammo crates (larger pickup)
- [ ] Drop ammo on death
- [ ] Ammo trading between players

### Phase 3 💡 (Ideas)
- [ ] Crafting system (combine ammo)
- [ ] Ammo perks/buffs
- [ ] Limited ammo game mode
- [ ] Ammo economy system

---

## 🤝 Contributing

Nếu bạn thêm tính năng mới hoặc fix bugs:
1. Test kỹ càng (single + multiplayer)
2. Update documentation
3. Commit với message rõ ràng

---

## 📞 Support

**Issues?** Check troubleshooting section trong:
- `AMMO_QUICK_START.md` - Quick fixes
- `AMMO_SYSTEM_SETUP.md` - Detailed setup
- `AMMO_SYSTEM_SUMMARY.md` - Overview

**Still stuck?**
- Check Unity Console for errors
- Verify all GameObjects have correct Tags
- Test in single player first

---

## 📝 Changelog

### Version 1.0 (2025-01-20)
- ✅ Initial release
- ✅ Reload button support
- ✅ Ammo box system
- ✅ Reserve ammo system
- ✅ Multiplayer sync
- ✅ UI notification
- ✅ Spawn manager
- ✅ Editor tools

---

## 📄 License

Part of FPS Multiplayer project
Free to modify and extend

---

## 🎉 Credits

**Developed by**: Van Toan
**Date**: January 20, 2025
**Unity Version**: 2020.3+
**Photon Version**: PUN 2

---

**🎮 Happy Gaming! Enjoy your new ammo system! 🚀**

---

## Quick Links

📖 [Detailed Setup Guide](./AMMO_SYSTEM_SETUP.md)
⚡ [Quick Start (5 min)](./AMMO_QUICK_START.md)
📊 [System Summary](./AMMO_SYSTEM_SUMMARY.md)
