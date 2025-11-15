# Hướng Dẫn Setup Helicopter (Trực Thăng)

## 1. Setup Helicopter GameObject

### A. Thêm Components vào Helicopter:
1. **Rigidbody**:
   - Mass: 2000
   - Drag: 1
   - Angular Drag: 3
   - Use Gravity: ✓
   - Interpolation: Interpolate
   - Collision Detection: Continuous Dynamic

2. **HelicopterController Script**:
   - Lift Force: 5000
   - Descend Force: 3000
   - Tilt Force: 30
   - Max Tilt Angle: 45
   - Forward Speed: 3000
   - Max Speed: 50
   - Rotation Speed: 100

3. **PhotonView**:
   - Observed Components: Add Transform
   - Synchronization: Unreliable On Change

4. **Collider** (BoxCollider hoặc MeshCollider):
   - Đảm bảo có collider để phát hiện va chạm

### B. Setup Animator cho Helicopter:

1. **Thêm Animator Component**:
   - Tạo Animator Controller mới (ví dụ: `HelicopterAnimator`)
   - Gán vào Animator component

2. **Trong Animator Controller**:
   - Tạo animation `Helicopter_Idle` (rotor quay)
   - **QUAN TRỌNG**: Animation này phải có rotor quay

3. **Setup Animation Speed Parameter**:
   - Trong Animator window, mở Parameters tab
   - Thêm Float parameter tên: **`RotorSpeed`**
   - Default value: 0

4. **Trong HelicopterController Inspector**:
   - **Helicopter Animator**: Kéo Animator component vào đây
   - **Idle Animation Name**: "Helicopter_Idle" (hoặc tên animation của bạn)
   - **Max Rotor Animation Speed**: 1.0 (tốc độ max khi bay)
   - **Rotor Speed Up Rate**: 0.5 (tốc độ tăng dần)
   - **Rotor Slow Down Rate**: 0.3 (tốc độ giảm dần)

### C. Setup Child Objects:

1. **Pilot Seat** (Empty GameObject):
   - Position: Vị trí ghế lái
   - Gán vào field "Pilot Seat" trong HelicopterController

2. **Exit Point** (Empty GameObject):
   - Position: Vị trí người chơi sẽ xuất hiện khi ra khỏi helicopter
   - Nên đặt bên cạnh cửa helicopter
   - Gán vào field "Exit Point" trong HelicopterController

3. **Camera Point** (Empty GameObject):
   - Position: Phía sau và trên cao helicopter
   - Rotation: Nhìn về phía trước
   - Gán vào field "Helicopter Camera Point" trong HelicopterController

### D. Layer và Tag:
- Layer: Chọn layer mà VehicleInteraction sẽ detect (thường là "Vehicle")
- Tag: (tùy chọn)

## 2. Cách Hoạt Động của Animation

### Thay đổi so với trước:
- **TRƯỚC**: Code quay transform của Main_Rotor và Tail_Rotor trực tiếp
- **BÂY GIỜ**: Dùng Animator.speed để điều khiển tốc độ animation

### Animation Flow:
```
Người chơi vào helicopter
    ↓
isBeingFlown = true
    ↓
UpdateRotors() tăng animation speed dần dần
    ↓
animator.speed = 0 → 1.0 (trong vài giây)
    ↓
Rotor quay nhanh dần (do animation)
    ↓
Người chơi nhấn Gas/Reverse
    ↓
currentRotorSpeed thay đổi theo input
    ↓
Animation speed điều chỉnh theo
    ↓
Người chơi ra khỏi helicopter
    ↓
isBeingFlown = false
    ↓
Animation speed giảm dần về 0
    ↓
Rotor chậm dần và dừng
```

## 3. Setup UI Buttons trong Inspector

### A. Trong UIController:
Đảm bảo các buttons này được gán:
- **Gas Button**: Button để bay lên
- **Reverse Button**: Button để hạ xuống
- **Brake Button**: (không dùng cho helicopter, chỉ dùng cho xe)

### B. KHÔNG CẦN setup Event Triggers thủ công!
- VehicleInteraction sẽ tự động thêm EventTrigger vào buttons
- Khi vào helicopter, nó sẽ connect buttons
- Khi ra khỏi helicopter, nó sẽ disconnect buttons

## 4. Testing và Debug

### A. Kiểm tra trong Console:
Khi vào helicopter, bạn sẽ thấy các log:
```
Connecting helicopter buttons...
Added EventTrigger to Gas button
Added EventTrigger to Reverse button
Helicopter buttons connected. Gas triggers: 2, Reverse triggers: 2
```

### B. Khi nhấn Gas/Reverse buttons:
```
Gas button pointer down triggered
Helicopter Gas Pressed!
Gas button pointer up triggered
Helicopter Gas Released!
```

### C. Nếu không thấy logs:
1. Kiểm tra UIController có gán đúng Gas/Reverse buttons không
2. Kiểm tra buttons có trong Canvas và được enable không
3. Kiểm tra EventSystem có trong scene không

## 5. Troubleshooting

### Vấn đề: Rotor không quay
**Giải pháp**:
- Kiểm tra Animator Controller có animation "Helicopter_Idle" không
- Kiểm tra animation có được assign vào Animator không
- Kiểm tra parameter "RotorSpeed" có trong Animator không
- Kiểm tra field "Helicopter Animator" trong HelicopterController đã được gán chưa

### Vấn đề: Gas/Reverse buttons không hoạt động
**Giải pháp**:
- Kiểm tra Console có log "Connecting helicopter buttons..." không
- Kiểm tra buttons có EventSystem để nhận input không
- Thử nhấn F12 trong Unity Editor và xem logs
- Kiểm tra photonView.IsMine = true (chỉ owner mới điều khiển được)

### Vấn đề: Helicopter không bay
**Giải pháp**:
- Kiểm tra Rigidbody có Use Gravity = true không
- Kiểm tra Lift Force đủ lớn không (phải > mass * 9.81)
- Kiểm tra isBeingFlown = true khi vào helicopter
- Kiểm tra isAscending/isDescending được set đúng không

## 6. Photon Multiplayer Setup

### PhotonView Settings:
- **Ownership Transfer**: Takeover
- **Synchronization**: Unreliable On Change
- **Observed Components**: 
  - Transform (để sync vị trí/rotation)
  - Rigidbody (để sync physics)

### RPC Methods trong HelicopterController:
- `EnterHelicopter(int playerActorNumber)` - Được gọi khi player vào
- `ExitHelicopter(int playerActorNumber)` - Được gọi khi player ra

## 7. Animation Best Practices

### Tạo Animation "Helicopter_Idle":
1. Chọn Main_Rotor object
2. Window → Animation → Animation
3. Create New Clip → đặt tên "Helicopter_Idle"
4. Thêm rotation keyframes:
   - Frame 0: Rotation Y = 0
   - Frame 60: Rotation Y = 360
5. Set Loop Time = true
6. Tương tự cho Tail_Rotor (rotation X)

### Animation Controller Setup:
1. Tạo State "Idle" với animation "Helicopter_Idle"
2. Set as Default State
3. KHÔNG CẦN transitions - chỉ cần 1 state
4. Speed được điều khiển bằng code (animator.speed)

---

## Kết Luận

Với setup này:
- ✅ Rotor quay được điều khiển bằng animation (không phải code)
- ✅ Tốc độ rotor tăng/giảm dần một cách mượt mà
- ✅ Gas/Reverse buttons tự động kết nối khi vào helicopter
- ✅ Multiplayer sync hoạt động đúng với Photon PUN
- ✅ Debug logs giúp dễ dàng troubleshoot

Nếu có vấn đề, hãy kiểm tra Console logs trước tiên!
