# 🔧 FIX OFFLINE MODE ERROR

## ❌ LỖI GẶP PHẢI

```
DontDestroyOnLoad only works for root GameObjects or components on root GameObjects.
```

## ✅ ĐÃ SỬA

### **1. GameModeManager.cs - DontDestroyOnLoad Issue**

**Nguyên nhân:** GameObject GameModeManager có parent trong hierarchy, không phải root object.

**Giải pháp:** Tự động set parent = null trước khi gọi DontDestroyOnLoad.

```csharp
private void Awake()
{
    if (instance == null)
    {
        instance = this;
        
        // Đảm bảo GameObject là root object
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
        
        DontDestroyOnLoad(gameObject);
    }
    else
    {
        Destroy(gameObject);
    }
}
```

### **2. Launcher.cs - SelectOfflineMode() Issues**

**Nguyên nhân:** 
- Cố set `PhotonNetwork.NickName` khi Photon chưa init
- Không check null cho allMaps array

**Giải pháp:**
- Dùng PlayerPrefs thay vì PhotonNetwork cho offline mode
- Thêm null checks và error messages rõ ràng

---

## 🎯 CÁCH SETUP ĐÚNG

### **Setup GameModeManager trong Unity:**

1. **Tạo GameObject trong scene Menu:**
   ```
   - Click chuột phải trong Hierarchy
   - Create Empty
   - Đặt tên: "GameModeManager"
   - ⚠️ QUAN TRỌNG: Đảm bảo nó là ROOT object (không có parent)
   ```

2. **Add Component:**
   ```
   - Select GameModeManager object
   - Add Component → GameModeManager.cs
   ```

3. **Kiểm tra trong Hierarchy:**
   ```
   ✅ ĐÚNG:
   Hierarchy
   ├── Canvas
   ├── GameModeManager  ← ROOT level
   └── EventSystem

   ❌ SAI:
   Hierarchy
   ├── Canvas
   │   └── GameModeManager  ← Có parent = lỗi!
   └── EventSystem
   ```

### **Setup Maps Array trong Launcher:**

1. **Select Launcher GameObject trong scene Menu**

2. **Trong Inspector, tìm Launcher component:**
   ```
   Launcher (Script)
   ├── ...
   └── All Maps (Array)
       Size: 1 (hoặc nhiều hơn)
       Element 0: "TênScene1"
       Element 1: "TênScene2"
       ...
   ```

3. **Điền tên các scene game:**
   ```
   Ví dụ:
   - Element 0: "GameMap1"
   - Element 1: "GameMap2"
   - Element 2: "City"
   ```

   ⚠️ **Tên phải khớp chính xác với tên scene trong Build Settings!**

---

## 🧪 TESTING

### **Test Setup:**

1. **Kiểm tra GameModeManager:**
   ```
   - Chạy game
   - Pause game
   - Trong Hierarchy, tìm GameModeManager
   - Kiểm tra nó có chữ "(DontDestroyOnLoad)" phía sau không
   - ✅ Đúng: GameModeManager (DontDestroyOnLoad)
   ```

2. **Kiểm tra Console:**
   ```
   - Chọn Offline Mode
   - Xem Console (Ctrl+Shift+C)
   - Không có lỗi đỏ
   - Thấy log: "Game Mode đã được đặt thành: Offline"
   - Thấy log: "Loading offline map: [tên map]"
   ```

### **Test Offline Mode:**

1. Chạy game
2. Click "Offline Mode"
3. ✅ Không có lỗi "DontDestroyOnLoad"
4. ✅ Loading screen hiện "Đang tải game Offline..."
5. ✅ Game load map thành công

---

## 🐛 TROUBLESHOOTING

### **Vấn đề 1: Vẫn còn lỗi DontDestroyOnLoad**

**Giải pháp:**
```
1. Kiểm tra GameModeManager trong Hierarchy
2. Đảm bảo nó KHÔNG có parent
3. Nếu có parent, kéo nó ra ngoài root level
4. Save scene và test lại
```

### **Vấn đề 2: "Không có map nào được cấu hình"**

**Giải pháp:**
```
1. Select Launcher GameObject
2. Inspector → Launcher component
3. Tìm "All Maps" array
4. Set Size = 1 (hoặc số lượng maps bạn có)
5. Điền tên scene vào các elements
6. Đảm bảo scenes đã được add vào Build Settings
```

### **Vấn đề 3: Scene không load được**

**Giải pháp:**
```
1. File → Build Settings
2. Kiểm tra scenes trong "Scenes In Build"
3. Đảm bảo scene menu ở index 0
4. Đảm bảo scene game đã được add
5. Tên scene phải khớp với tên trong allMaps array
```

### **Vấn đề 4: GameModeManager instance = null**

**Giải pháp:**
```
1. Kiểm tra GameModeManager đã được tạo trong scene menu
2. Kiểm tra script đã được attach
3. Chạy game từ scene menu (scene 0), không chạy từ scene game
```

---

## 📝 CHECKLIST SETUP HOÀN CHỈNH

Trước khi test, kiểm tra:

### **Scene Menu:**
- [ ] GameModeManager GameObject tồn tại
- [ ] GameModeManager là ROOT object (không có parent)
- [ ] GameModeManager có component GameModeManager.cs
- [ ] Launcher có allMaps array được điền
- [ ] ModeSelectionScreen UI đã được tạo
- [ ] Buttons đã link đến SelectOfflineMode() và SelectOnlineMode()

### **Build Settings:**
- [ ] Scene Menu (index 0) đã được add
- [ ] Các scene game đã được add
- [ ] Tên scenes khớp với allMaps array

### **Scripts:**
- [ ] GameModeManager.cs đã được update
- [ ] Launcher.cs đã được update
- [ ] Không có compile errors

---

## ✅ KẾT QUẢ SAU KHI SỬA

Bạn sẽ thấy:
- ✅ Không còn lỗi DontDestroyOnLoad
- ✅ Offline mode load game thành công
- ✅ GameModeManager được giữ qua các scenes
- ✅ Console log rõ ràng những gì đang xảy ra

---

## 💡 LƯU Ý QUAN TRỌNG

1. **GameModeManager phải là ROOT object:**
   - Đừng đặt nó làm child của Canvas
   - Đừng đặt nó trong bất kỳ parent nào
   - Nó phải ở cấp độ cao nhất trong Hierarchy

2. **Luôn test từ scene Menu:**
   - Đừng chạy trực tiếp từ scene game
   - GameModeManager chỉ tồn tại nếu khởi tạo từ menu

3. **Maps array phải được điền:**
   - Ít nhất 1 map trong array
   - Tên phải chính xác 100%
   - Scene phải có trong Build Settings

---

Bây giờ hãy test lại! Offline mode sẽ hoạt động bình thường. 🚀
