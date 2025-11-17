# 🎨 HƯỚNG DẪN TẠO UI MODE SELECTION

## ❌ VẤN ĐỀ: UI Mode Selection không xuất hiện

**Nguyên nhân:** Chưa tạo UI hoặc chưa gán vào Launcher Inspector.

---

## ✅ CÁCH TẠO UI MODE SELECTION (5 PHÚT)

### **BƯỚC 1: Tạo UI Panel**

1. **Mở scene Menu** (scene 0 - nơi có Launcher)

2. **Tìm Canvas trong Hierarchy:**
   ```
   Hierarchy
   └── Canvas
       ├── LoadingScreen
       ├── MenuButtons
       ├── CreateRoomScreen
       └── ... (các UI khác)
   ```

3. **Tạo Panel mới:**
   ```
   - Click chuột phải vào Canvas
   - UI → Panel
   - Đặt tên: "ModeSelectionScreen"
   ```

---

### **BƯỚC 2: Thiết kế UI**

#### **2.1. Tạo Title Text:**
```
- Click chuột phải vào ModeSelectionScreen
- UI → Text - TextMeshPro
- Đặt tên: "TitleText"
```

**Cấu hình TitleText:**
- Text: "CHỌN CHẾ ĐỘ CHƠI"
- Font Size: 48
- Alignment: Center
- Color: White
- Position: Top center của Panel

#### **2.2. Tạo Offline Button:**
```
- Click chuột phải vào ModeSelectionScreen
- UI → Button - TextMeshPro
- Đặt tên: "OfflineButton"
```

**Cấu hình OfflineButton:**
- Text child → Text: "🎮 CHẾ ĐỘ OFFLINE"
- Font Size: 32
- Width: 400
- Height: 80
- Position: Center (Y = 50)

**Gán sự kiện OnClick:**
```
- Select OfflineButton
- Inspector → Button component
- OnClick() → Click dấu "+"
- Kéo GameObject "Launcher" vào ô trống
- Dropdown chọn: Launcher → SelectOfflineMode()
```

#### **2.3. Tạo Online Button:**
```
- Duplicate OfflineButton (Ctrl+D)
- Đặt tên: "OnlineButton"
```

**Cấu hình OnlineButton:**
- Text: "🌐 CHẾ ĐỘ ONLINE"
- Position: Center (Y = -50)

**Gán sự kiện OnClick:**
```
- OnClick() → Launcher → SelectOnlineMode()
```

#### **2.4. Tạo Quit Button (Optional):**
```
- Duplicate OnlineButton
- Đặt tên: "QuitButton"
```

**Cấu hình QuitButton:**
- Text: "❌ THOÁT"
- Position: Bottom (Y = -150)
- OnClick() → Launcher → QuitGame()

---

### **BƯỚC 3: Gán vào Launcher**

1. **Select GameObject "Launcher" trong Hierarchy**

2. **Trong Inspector, tìm component "Launcher (Script)":**
   ```
   Launcher (Script)
   ├── Loading Screen
   ├── Credit Screen
   ├── Make Room Panel
   ├── Loading Text
   ├── Mode Selection Screen ← ⚠️ Gán ở đây!
   ├── Menu Buttons
   └── ...
   ```

3. **Kéo GameObject "ModeSelectionScreen" vào ô "Mode Selection Screen"**

4. **Save Scene** (Ctrl+S)

---

### **BƯỚC 4: Ẩn các Panel khác ban đầu**

Đảm bảo các panel sau bị ẩn khi bắt đầu:

```
Trong Hierarchy, select và TẮT các GameObject sau (uncheck):
- LoadingScreen ❌
- MenuButtons ❌
- CreateRoomScreen ❌
- RoomScreen ❌
- RoomBrowserScreen ❌
- NameInputScreen ❌
- ErrorScreen ❌
- CreditScreen ❌
- MakeRoomPanel ❌

CHỈ BẬT:
- ModeSelectionScreen ✅ (hoặc để code tự bật)
```

---

## 🎨 LAYOUT MẪU

```
┌─────────────────────────────────────┐
│                                     │
│      CHỌN CHẾ ĐỘ CHƠI              │
│                                     │
│   ┌───────────────────────────┐   │
│   │  🎮 CHẾ ĐỘ OFFLINE        │   │
│   └───────────────────────────┘   │
│                                     │
│   ┌───────────────────────────┐   │
│   │  🌐 CHẾ ĐỘ ONLINE         │   │
│   └───────────────────────────┘   │
│                                     │
│   ┌───────────────────────────┐   │
│   │  ❌ THOÁT                  │   │
│   └───────────────────────────┘   │
│                                     │
└─────────────────────────────────────┘
```

---

## 🎨 MẪU THIẾT KẾ ĐƠN GIẢN NHẤT

Nếu muốn nhanh, chỉ cần:

### **Vertical Layout:**
```
ModeSelectionScreen (Panel - Màu nền tối)
├── TitleText (TextMeshPro - Căn giữa trên)
├── OfflineButton (Button - to, màu xanh lá)
├── OnlineButton (Button - to, màu xanh dương)
└── QuitButton (Button - nhỏ hơn, màu đỏ)
```

### **Quick Settings:**

**ModeSelectionScreen:**
- Image Color: RGBA(0, 0, 0, 200) - Đen trong suốt

**Buttons:**
- OfflineButton Color: Green
- OnlineButton Color: Blue
- QuitButton Color: Red

---

## 🧪 KIỂM TRA

### **1. Trong Unity Editor:**

```
✅ ModeSelectionScreen tồn tại trong Canvas
✅ ModeSelectionScreen có 2-3 buttons
✅ Các buttons có OnClick events gán đúng
✅ Launcher có reference đến ModeSelectionScreen
✅ Các panel khác đang ẩn
```

### **2. Chạy game:**

```
✅ ModeSelectionScreen xuất hiện đầu tiên
✅ Click Offline → Load game offline
✅ Click Online → Hiện loading và connect Photon
✅ Không có lỗi trong Console
```

---

## 🐛 TROUBLESHOOTING

### **Vấn đề 1: UI không hiện**

**Kiểm tra:**
```
1. ModeSelectionScreen đã được gán vào Launcher chưa?
   → Inspector → Launcher → Mode Selection Screen
   
2. ModeSelectionScreen có đang active không?
   → Trong Hierarchy, check checkbox bên cạnh tên
   
3. Canvas Scaler settings đúng không?
   → Canvas → Canvas Scaler → UI Scale Mode: Scale With Screen Size
```

### **Vấn đề 2: Click button không có phản ứng**

**Kiểm tra:**
```
1. Button có EventSystem trong scene?
   → Hierarchy phải có GameObject "EventSystem"
   
2. Button OnClick event đã gán đúng?
   → Button → Inspector → OnClick() → Launcher → Function
   
3. Launcher GameObject có đang active không?
```

### **Vấn đề 3: UI bị che hoặc nhỏ lắm**

**Kiểm tra:**
```
1. Canvas Render Mode:
   → Canvas → Canvas component → Render Mode: Screen Space - Overlay
   
2. Rect Transform của ModeSelectionScreen:
   → Anchor: Stretch cả 4 hướng
   → Left, Right, Top, Bottom: 0, 0, 0, 0
   
3. Canvas Scaler:
   → Reference Resolution: 1920x1080
```

---

## 📸 HÌNH ẢNH THAM KHẢO

### **Hierarchy nên trông như thế này:**

```
Canvas
├── ModeSelectionScreen ← Active ✅
│   ├── TitleText
│   ├── OfflineButton
│   ├── OnlineButton
│   └── QuitButton
├── LoadingScreen ← Inactive ❌
├── MenuButtons ← Inactive ❌
├── CreateRoomScreen ← Inactive ❌
└── ... (các panel khác đều inactive)
```

### **Inspector của Launcher:**

```
Launcher (Script)
├── Loading Screen: LoadingScreen (GameObject)
├── Mode Selection Screen: ModeSelectionScreen (GameObject) ← Quan trọng!
├── Menu Buttons: MenuButtons (GameObject)
├── All Maps:
│   Size: 1
│   Element 0: "YourMapName"
└── ...
```

---

## ⚡ QUICK SETUP (CHO NGƯỜI VỘI)

Nếu bạn đang vội, làm theo 5 bước này:

1. **Canvas → Right click → UI → Panel** → Đặt tên "ModeSelectionScreen"
2. **ModeSelectionScreen → Right click → UI → Button** → Đặt tên "OfflineButton"
3. **Duplicate button** → Đặt tên "OnlineButton"
4. **OfflineButton → OnClick()** → Launcher.SelectOfflineMode
5. **OnlineButton → OnClick()** → Launcher.SelectOnlineMode
6. **Launcher Inspector** → Kéo ModeSelectionScreen vào ô "Mode Selection Screen"

✅ **XONG!**

---

## 💡 TIP PRO

### **Sao chép từ MenuButtons hiện có:**

Nếu bạn đã có `MenuButtons` panel đẹp rồi:

1. Duplicate MenuButtons → Đổi tên "ModeSelectionScreen"
2. Xóa các button cũ
3. Thêm 2 buttons: Offline và Online
4. Gán OnClick events
5. Done!

---

Sau khi làm xong, **chạy game** và bạn sẽ thấy màn hình chọn mode xuất hiện! 🎉
