# Bug Fixes - Ammo System

## Ngày: 2025-11-20

### ✅ Bug #1: Súng Tự Động Reload
**Vấn đề:** Súng tự động reload liên tục khi hết đạn, ngay cả khi player không nhấn nút reload.

**Nguyên nhân:** Trong phương thức `Shoot()`, dòng 574:
```csharp
if (allGuns[_selectedGun].currentAmmo <= 0) { ReloadWeapon(); return; }
```
Tự động gọi `ReloadWeapon()` mỗi khi bấn và hết đạn.

**Giải pháp:** Chỉ return mà không reload tự động:
```csharp
if (allGuns[_selectedGun].currentAmmo <= 0) { return; }
```

**File:** `PlayerController.cs` - Line 574

---

### ✅ Bug #2: Nút Reload Hiện Khi Lên Xe/Trực Thăng
**Vấn đề:** Các nút điều khiển súng (Reload, Shoot, Scope, Weapon) vẫn hiện khi player lên xe hoặc trực thăng.

**Giải pháp:** 
1. Ẩn tất cả nút súng trong `EnterVehicle()` và `EnterHelicopter()`
2. Hiện lại trong `ExitVehicle()`

**Code thêm vào:**

```csharp
// Trong EnterVehicle() và EnterHelicopter()
if (UIController.instance != null)
{
    if (UIController.instance.reloadButton != null)
        UIController.instance.reloadButton.SetActive(false);
    if (UIController.instance.shootButton != null)
        UIController.instance.shootButton.SetActive(false);
    if (UIController.instance.scopeButton != null)
        UIController.instance.scopeButton.SetActive(false);
    if (UIController.instance.weaponButton != null)
        UIController.instance.weaponButton.SetActive(false);
}

// Trong ExitVehicle()
if (photonView.IsMine)
{
    // ... existing code ...
    
    // Hiện lại các nút
    if (UIController.instance != null)
    {
        if (UIController.instance.reloadButton != null)
            UIController.instance.reloadButton.SetActive(true);
        if (UIController.instance.shootButton != null)
            UIController.instance.shootButton.SetActive(true);
        if (UIController.instance.scopeButton != null)
            UIController.instance.scopeButton.SetActive(true);
        if (UIController.instance.weaponButton != null)
            UIController.instance.weaponButton.SetActive(true);
    }
}
```

**Files:** `PlayerController.cs` - Lines 838-867, 904-928

---

## Testing Checklist

### Bug #1 - Auto Reload Fix
- [x] Bắn hết đạn → Súng dừng bắn
- [x] Không tự động reload
- [x] Nhấn R → Reload thủ công
- [x] Hết đạn dự trữ → Không reload được

### Bug #2 - UI Button Visibility
- [x] Lên xe → Nút súng ẩn
- [x] Lên trực thăng → Nút súng ẩn
- [x] Xuống xe → Nút súng hiện
- [x] Xuống trực thăng → Nút súng hiện
- [x] Multiplayer sync OK

---

## Changes Summary

| File | Lines Changed | Type |
|------|---------------|------|
| PlayerController.cs | Line 574 | Modified |
| PlayerController.cs | Lines 838-867 | Added |
| PlayerController.cs | Lines 904-928 | Modified |

**Total:** 3 changes in 1 file

---

## Status: ✅ COMPLETED

Cả 2 bugs đã được fix và test thành công!
