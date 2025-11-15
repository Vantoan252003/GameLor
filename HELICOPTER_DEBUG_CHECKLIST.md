# ✅ CHECKLIST - Kiểm Tra Helicopter Setup

## Đã Fix: ForceMode.Force
- ❌ **Trước**: `rb.AddForce(Vector3.up * liftForce * Time.fixedDeltaTime, ForceMode.Force)`
- ✅ **Sau**: `rb.AddForce(Vector3.up * liftForce, ForceMode.Force)`

**Lý do**: ForceMode.Force đã tự động nhân với Time.fixedDeltaTime bên trong Unity, nên không cần nhân thêm lần nữa!

---

## 🔍 Kiểm Tra Trong Unity Editor:

### 1. **Helicopter GameObject - Rigidbody Settings**:
```
Inspector → Rigidbody:
✓ Mass: 2000 (hoặc tối thiểu 1000)
✓ Drag: 1
✓ Angular Drag: 3
✓ Use Gravity: ✓ CHECKED (QUAN TRỌNG!)
✓ Is Kinematic: ✗ UNCHECKED
✓ Interpolation: Interpolate
✓ Collision Detection: Continuous Dynamic
```

### 2. **HelicopterController Settings**:
```
Inspector → HelicopterController:
✓ Lift Force: 5000 (tối thiểu = Mass * 10)
  → Nếu Mass = 2000, Lift Force ≥ 20000 mới bay được!
  → Thử tăng lên 25000 để test
  
✓ Descend Force: 3000
✓ Tilt Force: 30
✓ Max Tilt Angle: 45
✓ Forward Speed: 3000
✓ Max Speed: 50
```

### 3. **Constraints Check**:
```
Inspector → Rigidbody → Constraints:
✓ Freeze Position: ALL UNCHECKED (X, Y, Z phải di chuyển được)
✓ Freeze Rotation: Có thể check X và Z nếu muốn (nhưng Y phải unchecked)
```

---

## 🧪 Test Đơn Giản:

### Trong Unity Editor Play Mode:

1. **Test Gravity**:
   - Vào helicopter
   - KHÔNG nhấn gì
   - → Helicopter phải rơi xuống (có gravity)

2. **Test Lift Force**:
   - Nhấn giữ Gas button
   - Xem Console log: `Lifting! Force: 5000, Velocity: 0.5`
   - → Velocity.y phải tăng dần (dương số)
   - → Nếu vẫn âm = Force không đủ mạnh!

3. **Calculate Minimum Lift Force**:
   ```
   Minimum Lift = Mass × Gravity × 1.2
   
   Ví dụ:
   Mass = 2000
   Gravity = 9.81
   → Minimum Lift = 2000 × 9.81 × 1.2 = 23,544
   
   → Set Lift Force = 25000 để có thừa một chút!
   ```

---

## 🛠️ Nếu Vẫn Không Bay:

### Debug Steps:

1. **Mở Console (Ctrl+Shift+C)**
2. **Nhấn Gas button**
3. **Kiểm tra logs**:

```
✓ "Helicopter Gas Pressed!" → Button hoạt động
✓ "Lifting! Force: 5000, Velocity: -2.5" → Force đang apply

Nếu Velocity âm và không tăng lên:
→ Lift Force quá nhỏ!
→ Mass quá lớn!
→ Hoặc có Constraints freezing position Y
```

### Quick Fix:

**Trong Unity Editor (không cần sửa code):**

1. Chọn Helicopter GameObject
2. Inspector → HelicopterController
3. Thay đổi:
   - **Lift Force**: 25000 (thay vì 5000)
   - **Rigidbody Mass**: 1500 (giảm từ 2000)
4. Test lại

---

## 📊 Giải Thích Physics:

### Tại sao cần Lift Force lớn?

```
Trọng lực kéo xuống: Mass × Gravity = 2000 × 9.81 = 19,620 N (Newton)

Để bay lên, cần:
Lift Force > Trọng lực
Lift Force > 19,620

Nên đặt: Lift Force = 25,000 (dư 25%)
```

### Tại sao bỏ Time.fixedDeltaTime?

```
ForceMode.Force = Continuous force over time
→ Unity tự động nhân với Time.fixedDeltaTime

Trước: liftForce × Time.fixedDeltaTime × Time.fixedDeltaTime
      = 5000 × 0.02 × 0.02 = 2 N (quá nhỏ!)

Sau:  liftForce × Time.fixedDeltaTime (Unity tự động)
      = 5000 × 0.02 = 100 N/frame
      = 5000 N/giây (đủ mạnh!)
```

---

## ✅ Action Items Cho Bạn:

1. [ ] Kiểm tra Rigidbody → Use Gravity = ✓
2. [ ] Kiểm tra Rigidbody → Is Kinematic = ✗
3. [ ] Kiểm tra Constraints → Freeze Position Y = ✗
4. [ ] Tăng Lift Force lên 25000
5. [ ] Test lại và xem Console log
6. [ ] Báo lại Velocity number trong log

Cho tôi biết trong Console log bạn thấy số Velocity bao nhiêu khi nhấn Gas nhé!
