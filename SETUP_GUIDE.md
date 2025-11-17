# 🎮 HƯỚNG DẪN TRIỂN KHAI HỆ THỐNG OFFLINE/ONLINE VÀ NPC

## 📚 MỤC LỤC
1. [Tổng quan hệ thống](#tổng-quan-hệ-thống)
2. [Setup Offline/Online Mode](#setup-offlineonline-mode)
3. [Setup NPC System](#setup-npc-system)
4. [Tích hợp vào PlayerController](#tích-hợp-vào-playercontroller)
5. [Cấu hình Unity Scene](#cấu-hình-unity-scene)

---

## 📋 TỔNG QUAN HỆ THỐNG

### **Các file đã tạo:**
1. ✅ **GameModeManager.cs** - Quản lý chế độ Offline/Online
2. ✅ **OfflineMatchManager.cs** - Quản lý trận đấu Offline (không timer)
3. ✅ **NPCController.cs** - Điều khiển NPC di chuyển
4. ✅ **NPCSpawner.cs** - Spawn và quản lý NPC
5. ✅ **NPCEventManager.cs** - Xử lý sự kiện ảnh hưởng đến NPC
6. ✅ **Launcher.cs (đã sửa)** - Thêm chức năng chọn mode

### **Cách hoạt động:**

#### **Offline Mode:**
- ❌ Không kết nối Photon
- ❌ Không có timer đếm ngược
- ✅ Chơi đơn với NPC
- ✅ Vào game trực tiếp

#### **Online Mode:**
- ✅ Kết nối Photon như bình thường
- ✅ Có timer và multiplayer
- ✅ Hiện 3 button: Tìm Phòng, Tạo Phòng, Thoát

---

## 🎯 SETUP OFFLINE/ONLINE MODE

### **BƯỚC 1: Tạo GameObject GameModeManager**

1. Trong scene **Menu** (scene 0 - Launcher):
   - Tạo Empty GameObject mới, đặt tên: `GameModeManager`
   - Add component: `GameModeManager.cs`
   - ✅ **Đánh dấu DontDestroyOnLoad** (script tự động làm)

### **BƯỚC 2: Cập nhật UI trong Canvas Menu**

Bạn cần tạo UI trong Unity Editor:

#### **2.1. Tạo Mode Selection Screen**
```
Canvas
└── ModeSelectionScreen (Panel)
    ├── Title Text: "CHỌN CHẾ ĐỘ CHƠI"
    ├── OfflineButton (Button)
    │   └── Text: "Chế Độ Offline"
    ├── OnlineButton (Button)
    │   └── Text: "Chế Độ Online"
    └── QuitButton (Button)
        └── Text: "Thoát Game"
```

#### **2.2. Gán sự kiện cho các Button:**

**OfflineButton:**
- OnClick() → `Launcher.SelectOfflineMode()`

**OnlineButton:**
- OnClick() → `Launcher.SelectOnlineMode()`

**QuitButton:**
- OnClick() → `Launcher.QuitGame()`

#### **2.3. Update Launcher Inspector:**

Trong Launcher component, gán:
- `Mode Selection Screen` → GameObject ModeSelectionScreen vừa tạo

### **BƯỚC 3: Thêm Back Button trong Menu Buttons**

Trong `MenuButtons` panel, thêm button "Quay Lại":
- OnClick() → `Launcher.BackToModeSelection()`

---

## 🤖 SETUP NPC SYSTEM

### **BƯỚC 1: Chuẩn bị NavMesh**

1. **Bake NavMesh cho scene game của bạn:**
   ```
   Unity Menu → Window → AI → Navigation
   ```

2. **Chọn các object là mặt đất:**
   - Inspector → Navigation Static: ✅ Tick
   
3. **Bake NavMesh:**
   - Trong Navigation window → Tab "Bake"
   - Click "Bake"

### **BƯỚC 2: Tạo NPC Prefab**

#### **2.1. Tạo NPC GameObject:**

```
NPC (GameObject)
├── Model (visual của NPC - có thể là Capsule đơn giản hoặc model 3D)
├── NavMeshAgent (Component - Auto added)
└── NPCController (Component)
```

#### **2.2. Cấu hình NPCController:**

```
NPCController Settings:
- Walk Speed: 2
- Run Speed: 5
- Wander Radius: 20
- Min Wait Time: 2
- Max Wait Time: 10
- Can Run: ✅
- Run Chance: 0.2
- Flee Distance: 10
- Animator: (gán nếu có)
```

#### **2.3. Lưu thành Prefab:**

Kéo NPC vào thư mục `Assets/Prefabs/NPCs/`

### **BƯỚC 3: Setup NPC Spawner trong Scene**

1. **Trong scene game** (không phải menu):
   - Tạo Empty GameObject: `NPCSpawner`
   - Add component: `NPCSpawner.cs`

2. **Cấu hình NPCSpawner:**

```
NPCSpawner Settings:
- NPC Prefabs: [Kéo các NPC prefab vào đây]
- Min NPC Count: 10
- Max NPC Count: 30
- Spawn Radius: 50
- Spawn On Start: ✅
- Enable Dynamic Spawn: ✅
- Respawn Interval: 30
```

### **BƯỚC 4: Setup NPC Event Manager**

1. Trong scene game, tạo GameObject: `NPCEventManager`
2. Add component: `NPCEventManager.cs`

```
NPCEventManager Settings:
- Gunshot Alert Radius: 20
- Debug Mode: ✅ (để test)
```

---

## 🔫 TÍCH HỢP VÀO PLAYERCONTROLLER

### **Bước 1: Sửa PlayerController.cs để NPC phản ứng với tiếng súng**

Thêm đoạn code này vào hàm `Shoot()` trong PlayerController.cs:

```csharp
private void Shoot()
{
    // ... existing shoot code ...

    // Thêm phần này để NPC phản ứng với tiếng súng
    if (NPCEventManager.instance != null)
    {
        NPCEventManager.instance.OnGunshotFired(transform.position);
    }
}
```

### **Bước 2: Cập nhật MatchManager để hỗ trợ cả Offline và Online**

Trong `MatchManager.cs`, thêm vào đầu hàm `Start()`:

```csharp
void Start()
{
    // Kiểm tra chế độ game
    if (GameModeManager.instance != null && GameModeManager.instance.IsOfflineMode())
    {
        // Disable MatchManager cho Offline mode
        this.enabled = false;
        
        // Tạo OfflineMatchManager thay thế
        GameObject offlineManager = new GameObject("OfflineMatchManager");
        offlineManager.AddComponent<OfflineMatchManager>();
        
        return;
    }

    // ... phần code hiện tại của Start() ...
}
```

---

## 🎨 CẤU HÌNH UNITY SCENE

### **Scene 0 (Menu/Launcher):**

```
Hierarchy:
├── Canvas
│   ├── LoadingScreen
│   ├── ModeSelectionScreen ⭐ (MỚI)
│   ├── MenuButtons
│   ├── CreateRoomScreen
│   ├── RoomBrowserScreen
│   └── ...
├── GameModeManager ⭐ (MỚI)
└── EventSystem
```

### **Scene Game (Map):**

```
Hierarchy:
├── MatchManager (hoặc sẽ chuyển sang OfflineMatchManager)
├── SpawnManager
├── PlayerSpawner
├── NPCSpawner ⭐ (MỚI)
├── NPCEventManager ⭐ (MỚI)
├── Environment (NavMesh Baked)
└── ...
```

---

## 🧪 TESTING

### **Test Offline Mode:**

1. Chạy game
2. Chọn "Chế Độ Offline"
3. ✅ Kiểm tra: Không có timer hiển thị
4. ✅ Kiểm tra: Vào game ngay không qua lobby
5. ✅ Kiểm tra: NPC spawn và đi lại

### **Test Online Mode:**

1. Chạy game
2. Chọn "Chế Độ Online"
3. ✅ Kiểm tra: Kết nối Photon
4. ✅ Kiểm tra: Hiện 3 button (Tìm/Tạo/Thoát)
5. ✅ Kiểm tra: Timer hiển thị

### **Test NPC System:**

1. Vào game (Offline hoặc Online)
2. ✅ NPC spawn tự động
3. ✅ NPC đi lại ngẫu nhiên
4. ✅ Bắn súng → NPC xung quanh chạy trốn

---

## 🐛 TROUBLESHOOTING

### **Vấn đề 1: NPC không di chuyển**
**Giải pháp:**
- Kiểm tra NavMesh đã được bake chưa
- Kiểm tra NavMeshAgent component đã được add
- Radius phải trong vùng NavMesh hợp lệ

### **Vấn đề 2: NPC không spawn**
**Giải pháp:**
- Kiểm tra NPCPrefabs đã được gán trong NPCSpawner
- Kiểm tra spawn radius không quá nhỏ
- Check console xem có lỗi không

### **Vấn đề 3: Offline mode vẫn hiện timer**
**Giải pháp:**
- Kiểm tra GameModeManager đã được tạo trong scene menu
- Kiểm tra logic trong MatchManager.Start()

### **Vấn đề 4: NPC không phản ứng với tiếng súng**
**Giải pháp:**
- Kiểm tra NPCEventManager đã được tạo trong scene
- Kiểm tra đã thêm code vào PlayerController.Shoot()

---

## 📝 GHI CHÚ QUAN TRỌNG

### **Về Lưu trữ dữ liệu:**

Game hiện tại:
- **PlayerPrefs**: Chỉ lưu tên người chơi local
- **Photon**: Lưu data online trong session (không persistent)
- **Không có database**: Data sẽ mất khi thoát game

Nếu muốn lưu progression (level, unlock, achievements):
- Cần tích hợp database (PlayFab, Firebase, hoặc custom server)
- Hoặc dùng PlayerPrefs cho offline data (dễ bị hack)

### **Về NPC System:**

NPC hiện tại:
- ✅ Di chuyển ngẫu nhiên
- ✅ Phản ứng với tiếng súng
- ❌ Không tấn công
- ❌ Không có health
- ❌ Không tương tác với vật thể

Để nâng cấp thành NPC như GTA:
- Thêm system cho xe cộ (NPC lái xe)
- Thêm animation phong phú hơn
- Thêm variety (nhiều loại NPC khác nhau)
- Thêm interaction system

---

## 🚀 NEXT STEPS

Sau khi setup xong, bạn có thể:

1. **Thêm nhiều loại NPC:**
   - NPC đi bộ
   - NPC lái xe
   - NPC ngồi/đứng
   - NPC cảnh sát (phản ứng khi bắn)

2. **Cải thiện AI:**
   - NPC tránh vật cản
   - NPC tương tác với nhau
   - NPC có lịch trình (patrol route)

3. **Thêm Vehicle cho NPC:**
   - NPC spawn xe và lái
   - NPC dừng đèn đỏ
   - Traffic system

4. **Save System:**
   - Lưu progress
   - Unlock weapons
   - Achievements

---

## 📞 HỖ TRỢ

Nếu gặp lỗi khi setup, hãy check:
1. Console log trong Unity
2. NavMesh đã bake đúng chưa
3. Các reference đã được gán trong Inspector chưa
4. Scripts đã được compile không lỗi

Chúc bạn thành công! 🎉
