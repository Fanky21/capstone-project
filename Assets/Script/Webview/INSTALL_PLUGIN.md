# INSTALASI GREE WEBVIEW PLUGIN
## Untuk Webview In-App di Android

Script `CanvasToWebview.cs` sudah diupdate untuk mendukung **webview di dalam game** menggunakan Gree WebView plugin. Webview akan ditampilkan DI DALAM canvas Unity, bukan membuka browser eksternal.

---

## ⚡ INSTALASI PLUGIN (WAJIB untuk Android)

### Cara 1: Download dari GitHub (Gratis)

1. **Download Plugin**
   - Buka: https://github.com/gree/unity-webview
   - Klik tombol hijau **"Code"** → **Download ZIP**
   - Extract file ZIP yang didownload

2. **Copy ke Unity Project**
   ```
   Dari folder yang diextract, copy folder:
   - dist/package/Assets/Plugins
   
   Paste ke:
   - e:\Private\Developer\capstone-project\Assets\
   ```
   
   Struktur akhir:
   ```
   Assets/
   ├── Plugins/
   │   ├── Android/
   │   ├── iOS/
   │   └── WebView/
   └── Script/
       └── Webview/
   ```

3. **Tunggu Unity Import**
   - Unity akan otomatis import plugin
   - Tunggu compilation selesai
   - Check Console tidak ada error

---

### Cara 2: Clone dengan Git

```bash
cd "e:\Private\Developer\capstone-project\Assets"
git clone https://github.com/gree/unity-webview.git Plugins/WebView
```

---

## 🔧 KONFIGURASI UNITY

### 1. Build Settings untuk Android

1. **File → Build Settings**
2. **Switch platform ke Android** (jika belum)
   - Pilih "Android" di list
   - Klik "Switch Platform"
   - Tunggu proses selesai

3. **Player Settings**
   - Klik "Player Settings..."
   - Tab "Other Settings"
   - **Minimum API Level**: Android 5.0 'Lollipop' (API 21) atau lebih tinggi
   - **Target API Level**: Automatic (Highest Installed) atau manual pilih

### 2. Permissions Android

Plugin sudah include permissions yang diperlukan (INTERNET), tapi pastikan di `AndroidManifest.xml`:

```xml
<uses-permission android:name="android.permission.INTERNET" />
```

---

## 📱 CARA MENGGUNAKAN

### Setup Scene (Sama seperti sebelumnya)

1. **Buat Canvas** (UI → Canvas)
2. **Tambah Raw Image** ke Canvas (untuk display image)
3. **Buat Panel** untuk webview container
4. **Attach script CanvasToWebview** ke GameObject
5. **Setup di Inspector**:
   - Canvas Image: Raw Image
   - Display Texture: Texture yang ingin ditampilkan
   - Webview URL: https://www.google.com
   - Webview Panel: Panel container

### Testing

**Di Unity Editor:**
- Webview akan buka browser eksternal (normal behavior)
- Ini hanya untuk testing
- Log akan muncul: "Editor mode: Opening in external browser..."

**Di Android Build:**
- Webview akan muncul DI DALAM game pada canvas
- Webview sesuai ukuran panel yang ditentukan
- User bisa interact dengan webview (scroll, click, dll)

---

## 🚀 BUILD KE ANDROID

1. **File → Build Settings**
2. **Pastikan platform = Android**
3. **Add Open Scenes** (tambah scene yang aktif)
4. **Build And Run** atau **Build**
5. Install APK ke Android device
6. **Test!** Webview sekarang muncul di dalam game ✅

---

## 🎯 FITUR WEBVIEW IN-APP

Dengan Gree WebView plugin yang sudah diintegrasikan:

✅ **Webview di dalam game** - Tidak buka browser eksternal  
✅ **Responsive** - Mengikuti ukuran panel yang ditentukan  
✅ **Interactive** - User bisa scroll, click, input text  
✅ **URL Navigation** - Bisa ganti URL tanpa reload scene  
✅ **Callbacks** - Log events (loaded, error, dll)  
✅ **Zoom Support** - User bisa zoom in/out  
✅ **Back/Forward** - Bisa navigasi halaman (perlu implement button)

---

## 📋 VERIFIKASI INSTALASI

### Check 1: Folder Structure
```
Assets/
└── Plugins/
    ├── Android/
    │   └── webview.aar
    └── WebView/
        └── WebViewObject.cs
```

### Check 2: No Compilation Errors
- Buka **Console** (Window → General → Console)
- Tidak ada error merah
- Jika ada error "WebViewObject not found":
  - Plugin belum terinstall dengan benar
  - Ulangi langkah instalasi

### Check 3: Script Compiled
- Buat GameObject baru
- Add Component → Ketik "CanvasToWebview"
- Script muncul dalam list ✅

---

## ⚠️ TROUBLESHOOTING

### Error: "WebViewObject not found"
**Solusi:**
1. Pastikan folder Plugins/WebView ada
2. Check file WebViewObject.cs ada di Plugins/WebView/
3. Reimport plugin: Right-click folder Plugins → Reimport

### Webview masih buka browser eksternal di Android
**Solusi:**
1. Pastikan build untuk Android, bukan Editor
2. Check Build Settings → Platform = Android
3. Actual build ke APK, bukan running di Editor

### Webview tidak muncul / blank
**Solusi:**
1. Check internet connection di device Android
2. Check URL valid (https://...)
3. Check Android API Level >= 21
4. Check Console log untuk error

### Webview position salah
**Solusi:**
1. Pastikan Canvas mode = Screen Space - Overlay
2. Adjust webview panel size/position
3. Webview akan follow panel margins

---

## 🔗 RESOURCES

- **Gree WebView GitHub**: https://github.com/gree/unity-webview
- **Documentation**: Lihat README di repo GitHub
- **Issues**: Report di GitHub jika ada masalah dengan plugin

---

## 📝 CHANGELOG

**v2.0 - Android In-App Webview Support**
- ✅ Integrasi Gree WebView plugin
- ✅ Webview render di dalam Unity canvas
- ✅ Automatic margin calculation dari panel size
- ✅ Proper cleanup on close/destroy
- ✅ Editor mode tetap buka browser (untuk testing)

---

**READY!** 🎉  
Setelah install plugin, build ke Android dan webview akan muncul di dalam game!
