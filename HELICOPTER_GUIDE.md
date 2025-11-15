# 🚁 HƯỚNG DẪN THÊM TRỰC THĂNG VÀO GAME

## 📝 TỔNG QUAN

Hệ thống trực thăng hoạt động tương tự xe, nhưng có vật lý bay và điều khiển khác:
- **Gas Button**: Bay lên
- **Reverse Button**: Hạ xuống  
- **Joystick**: Nghiêng trực thăng (trước/sau/trái/phải)
- **Q/E**: Xoay trực thăng (tùy chọn)
- **Vuốt màn hình**: Xoay camera

---

## ✅ BƯỚC 1: TẠO TRỰC THĂNG TRONG UNITY

### 1.1. Tạo GameObject cơ bản

```
Hierarchy → Create Empty → Đặt tên "Helicopter"
```

### 1.2. Thêm Components

**Trên Helicopter object:**
1. Add Component → **Rigidbody**
   - Mass: 2000
   - Drag: 1
   - Angular Drag: 3
   - Use Gravity: ✓
   - Interpolation: Interpolate
   - Collision Detection: Continuous Dynamic

2. Add Component → **Photon View**
   - Ownership Transfer: Takeover
   - Synchronization: Unreliable On Change

3. Add Component → **HelicopterController** (script đã tạo)

4. Add Component → **Box Collider** (cho body trực thăng)
   - Điều chỉnh size phù hợp với model

---

### 1.3. Tạo cấu trúc Hierarchy

```
Helicopter
├── Body (Cube hoặc model trực thăng)
│   └── Visual mesh của body
├── MainRotor (Empty GameObject)
│   └── MainRotorMesh (Plane hoặc model cánh quạt chính)
├── TailRotor (Empty GameObject)
│   └── TailRotorMesh (Plane hoặc model cánh quạt đuôi)
├── PilotSeat (Empty Transform)
├── ExitPoint (Empty Transform)
└── CameraPoint (Empty Transform)
```

#### Chi tiết từng phần:

**Body:**
- Position: (0, 0, 0)
- Scale: (2, 1, 4) - Hoặc theo model
- Thêm Cube tạm hoặc import model trực thăng

**MainRotor (Cánh quạt chính - trên đầu):**
- Create Empty → Position: (0, 2, 0)
- Rotation: (0, 0, 0)
- Child object: MainRotorMesh
  - Create → 3D Object → Plane
  - Scale: (2, 1, 0.2) - Làm thành cánh quạt
  - Rotation: (90, 0, 0)

**TailRotor (Cánh quạt đuôi):**
- Create Empty → Position: (0, 0.5, -3)
- Rotation: (0, 0, 0)
- Child object: TailRotorMesh
  - Create → 3D Object → Plane
  - Scale: (0.5, 1, 0.1)
  - Rotation: (0, 0, 0)

**PilotSeat:**
- Create Empty → Position: (0, 0.5, 0)
- Đây là nơi player ngồi

**ExitPoint:**
- Create Empty → Position: (-3, 0, 0)
- Vị trí player xuất hiện khi ra khỏi trực thăng

**CameraPoint:**
- Create Empty → Position: (0, 3, -6)
- Rotation: (15, 0, 0)
- Camera sẽ nhìn từ phía sau và trên trực thăng

---

### 1.4. Cấu hình HelicopterController

Chọn **Helicopter** object, trong Inspector:

**Helicopter Settings:**
- Lift Force: 5000
- Descend Force: 3000
- Tilt Force: 30
- Max Tilt Angle: 45
- Stabilization Speed: 2
- Forward Speed: 3000
- Max Speed: 50
- Rotation Speed: 100

**Rotor Settings:**
- Main Rotor: Kéo MainRotor transform vào
- Tail Rotor: Kéo TailRotor transform vào
- Main Rotor Speed: 1000
- Tail Rotor Speed: 2000

**Player Interaction:**
- Pilot Seat: Kéo PilotSeat vào
- Exit Point: Kéo ExitPoint vào
- Interaction Distance: 3
- Player Layer: Chọn layer Player

**Camera:**
- Helicopter Camera Point: Kéo CameraPoint vào
- Camera Rotation Speed: 2

---

### 1.5. Tạo Layer cho Helicopter

1. Top-right Inspector → Layers → Add Layer
2. Tạo layer mới: **"Vehicle"** (nếu chưa có, dùng chung với xe)
3. Set layer của **Helicopter** object → Vehicle

---

### 1.6. Tạo Prefab

1. Kéo **Helicopter** từ Hierarchy vào `Assets/Prefabs/`
2. Xóa Helicopter khỏi Hierarchy (sẽ spawn lại sau)

---

## ✅ BƯỚC 2: SETUP UI (Dùng chung với xe!)

**UI Controls đã có sẵn từ xe:**
- Gas Button (Bay lên cho trực thăng)
- Reverse Button (Hạ xuống cho trực thăng)
- Joystick (Nghiêng trực thăng)

**Không cần tạo UI mới!** Buttons này sẽ hoạt động cho cả xe và trực thăng.

---

## ✅ BƯỚC 3: KẾT NỐI UI VỚI HELICOPTER

### 3.1. Trong UIController

Đã setup sẵn từ xe, không cần thay đổi gì!

### 3.2. Kết nối Event Triggers

**GasButton** (đã setup cho xe):
- Event Trigger → Pointer Down → Kéo **Helicopter prefab** → `HelicopterController.OnGasPressed`
- Event Trigger → Pointer Up → Kéo **Helicopter prefab** → `HelicopterController.OnGasReleased`

**ReverseButton** (đã setup cho xe):
- Event Trigger → Pointer Down → Kéo **Helicopter prefab** → `HelicopterController.OnReversePressed`
- Event Trigger → Pointer Up → Kéo **Helicopter prefab** → `HelicopterController.OnReverseReleased`

**Lưu ý:** Buttons này sẽ trigger methods của object gần nhất (xe hoặc trực thăng). Script tự động detect!

---

## ✅ BƯỚC 4: ĐẶT TRỰC THĂNG VÀO SCENE

1. Kéo **Helicopter prefab** từ `Assets/Prefabs/` vào Scene
2. Đặt vị trí ở nơi dễ tiếp cận (không quá cao khỏi mặt đất lúc đầu)
3. Position khuyến nghị: Y = 1 (hơi nổi trên mặt đất)

---

## ✅ BƯỚC 5: THÊM VISUALS (TÙY CHỌN)

### 5.1. Tạo Body đẹp hơn

Thay vì Cube, bạn có thể:
- Import model trực thăng từ Asset Store
- Hoặc tạo bằng primitive objects:
  - Body: Capsule (thân)
  - Cockpit: Sphere (buồng lái)
  - Tail: Cylinder (đuôi)

### 5.2. Màu sắc

- Tạo Materials cho body
- Gán màu cho rotors (màu đen hoặc xám)

### 5.3. Thêm Audio (Optional)

1. Import rotor sound (âm thanh cánh quạt)
2. Trên Helicopter:
   - Add Component → Audio Source
   - Audio Clip: Kéo rotor sound vào
   - Loop: ✓
   - Spatial Blend: 1 (3D sound)
3. Trong HelicopterController:
   - Kéo Audio Source vào field "Rotor Sound"

---

## 🎮 CÁCH ĐIỀU KHIỂN TRỰC THĂNG

### Desktop:
- **W/S hoặc Gas/Reverse buttons**: Bay lên/xuống
- **A/D hoặc Joystick trái/phải**: Nghiêng trái/phải
- **W/S hoặc Joystick trên/dưới**: Nghiêng trước/sau
- **Q/E**: Xoay trái/phải
- **Chuột**: Xoay camera
- **F**: Vào/ra trực thăng

### Mobile:
- **Gas Button (giữ)**: Bay lên
- **Reverse Button (giữ)**: Hạ xuống
- **Joystick**: Nghiêng trực thăng
- **Vuốt màn hình**: Xoay camera
- **F hoặc Interact Button**: Vào/ra

---

## 🔧 VẬT LÝ TRỰC THĂNG

### Nguyên lý hoạt động:

1. **Lift (Bay lên)**: AddForce Vector3.up
2. **Descend (Hạ xuống)**: AddForce Vector3.down
3. **Tilt (Nghiêng)**: Rotate helicopter theo joystick input
4. **Forward Movement**: Di chuyển theo hướng nghiêng
5. **Stabilization**: Tự động ổn định khi không nhấn nút

### Tweaking Settings:

**Trực thăng bay quá nhanh:**
- Giảm Lift Force (3000-4000)
- Giảm Forward Speed (2000)

**Trực thăng khó điều khiển:**
- Giảm Tilt Force (20-25)
- Giảm Max Tilt Angle (30-35)
- Tăng Stabilization Speed (3-4)

**Trực thăng rơi nhanh:**
- Tăng Lift Force
- Giảm Rigidbody Mass (1500)

**Trực thăng quay không mượt:**
- Tăng Angular Drag (4-5)
- Giảm Rotation Speed (50-80)

---

## 🐛 TROUBLESHOOTING

### Trực thăng không bay:
- Check Rigidbody có Use Gravity ✓
- Check Lift Force đủ lớn (>= 5000)
- Check Gas button đã kết nối đúng không

### Trực thăng lật ngược:
- Center of Mass quá cao
- Thêm code: `rb.centerOfMass = new Vector3(0, -0.5f, 0);`
- Tăng Angular Drag

### Cánh quạt không quay:
- Check MainRotor và TailRotor đã gán trong Inspector
- Check Rotor Speed > 0
- Check prefab structure đúng chưa

### Camera không xoay:
- Check Look X và Look Y trong SimpleInput
- Tăng Camera Rotation Speed

### Button không hiện khi gần trực thăng:
- Check Layer của Helicopter = Vehicle
- Check Interaction Distance đủ lớn (3)
- Check VehicleInteraction script có trên Player

### Multiplayer không sync:
- Check PhotonView có Ownership Transfer = Takeover
- Check Transform được observe trong PhotonView
- Add Photon Transform View component

---

## 🎯 SO SÁNH XE VS TRỰC THĂNG

| Feature | Xe 🚗 | Trực Thăng 🚁 |
|---------|------|----------------|
| **Gas Button** | Đi thẳng | Bay lên |
| **Reverse** | Lùi | Hạ xuống |
| **Joystick** | Lái trái/phải | Nghiêng tất cả hướng |
| **Vật lý** | Wheel Colliders | Rigidbody + Forces |
| **Di chuyển** | Trên mặt đất | Trong không trung |
| **Stabilization** | Friction | Auto stabilize |
| **UI Controls** | Dùng chung | Dùng chung |

---

## 🚀 TÍNH NĂNG NÂNG CAO (OPTIONAL)

### 1. Thêm Landing Gear (Càng đáp)
```csharp
// Trong HelicopterController.cs
[SerializeField] private GameObject landingGear;

void Update() {
    if (isBeingFlown && rb.velocity.magnitude > 1f) {
        landingGear.SetActive(false); // Rút càng khi bay
    } else {
        landingGear.SetActive(true); // Thả càng khi đáp
    }
}
```

### 2. Thêm Altitude Display (Hiện độ cao)
```csharp
// UI Text hiển thị độ cao
float altitude = transform.position.y;
altitudeText.text = $"Altitude: {altitude:F0}m";
```

### 3. Thêm Health cho trực thăng
- Tạo health system tương tự player
- Damage khi va chạm mạnh
- Crash khi health = 0

### 4. Thêm Passenger Seats (Ghế phụ)
- Nhiều người có thể ngồi cùng
- Chỉ pilot điều khiển
- Passengers có thể bắn súng

### 5. Thêm Weapons
- Rockets
- Machine guns
- Targeting system

---

## ✅ CHECKLIST HOÀN THÀNH

Trước khi test, đảm bảo:
- [ ] Helicopter prefab đã tạo với đầy đủ components
- [ ] Rigidbody settings đúng (Mass: 2000, Drag: 1, Angular Drag: 3)
- [ ] MainRotor và TailRotor đã tạo và gán
- [ ] PilotSeat, ExitPoint, CameraPoint đã tạo và gán
- [ ] Layer = Vehicle
- [ ] PhotonView có Ownership Transfer = Takeover
- [ ] HelicopterController có tất cả references
- [ ] UI buttons đã kết nối (nếu cần riêng cho helicopter)
- [ ] Helicopter prefab lưu vào Assets/Prefabs/
- [ ] Đã spawn vào scene và test

---

## 🎉 KẾT LUẬN

Bây giờ bạn có:
✅ Hệ thống trực thăng hoàn chỉnh với vật lý bay
✅ Điều khiển smooth với joystick + buttons
✅ Camera xoay 360°
✅ Multiplayer sync qua Photon
✅ Cánh quạt quay realtime
✅ Audio động cơ (optional)
✅ Tương thích với hệ thống xe hiện có

**Chúc bạn bay vui vẻ! 🚁✨**

---

## 📞 HỖ TRỢ THÊM

Nếu cần thêm tính năng:
- Multiple helicopters (Apache, Black Hawk, etc.)
- Advanced flight physics
- Combat systems
- Formation flying
- Rescue missions

Chỉ cần hỏi! 😊
