# Canvas to Webview Script - ANDROID IN-APP WEBVIEW

## ✨ UPDATE v2.1 - LOCALHOST SUPPORT!
✅ Script mendukung **webview di dalam game** untuk Android  
✅ **FIX: HTTP localhost support** - Bisa load http://localhost:5000/  

## File yang Tersedia
- **CanvasToWebview.cs** - Script utama dengan support Gree WebView (Android in-app)
- **WebviewExample.cs** - Contoh penggunaan script
- **INSTALL_PLUGIN.md** - ⚠️ **WAJIB BACA!** Panduan install Gree WebView plugin
- **FIX_LOCALHOST.md** - 🔧 **Solusi "Cleartext not permitted"** untuk localhost

## 🆕 FIX LOCALHOST HTTP

Jika Anda perlu akses **http://localhost:5000/** atau local server, file konfigurasi sudah dibuat:

**Files Created:**
- `Assets/Plugins/Android/res/xml/network_security_config.xml`
- `Assets/Plugins/Android/AndroidManifest.xml` (updated)

**Allowed HTTP Domains:**
- ✅ localhost
- ✅ 127.0.0.1
- ✅ 10.0.2.2 (Android Emulator)
- ✅ 192.168.x.x (Local network - sesuaikan di config)

**Baca:** `FIX_LOCALHOST.md` untuk detail lengkap!

## ⚡ QUICK START (Android)

### STEP 1: Install Plugin (WAJIB!)
**Baca file: INSTALL_PLUGIN.md**

Singkatnya:
1. Download: https://github.com/gree/unity-webview
2. Copy folder `dist/package/Assets/Plugins` ke project
3. Tunggu Unity import

### STEP 2: Setup Scene
1. Buat Canvas (UI → Canvas)
2. Tambah Raw Image
3. Buat Panel untuk webview container (full screen)
4. Attach script **CanvasToWebview**
5. Setup Inspector:
   - Canvas Image → Raw Image
   - Display Texture → Your texture
   - Webview URL → **http://192.168.1.xxx:5000/** (gunakan IP PC!)
   - Webview Panel → Panel

### STEP 3: Localhost Setup
Jika pakai localhost server:
- ❌ **Jangan** gunakan `localhost` di Android device
- ✅ **Gunakan** IP address PC: `http://192.168.1.100:5000`
- Check IP: `ipconfig` (Windows) atau `ifconfig` (Mac/Linux)

### STEP 4: Build ke Android
1. File → Build Settings
2. Switch Platform ke Android  
3. Build And Run
4. ✅ Webview load localhost server di dalam game!

## 🎯 Fitur

### Di Android Build:
- ✅ Webview render di dalam Unity canvas
- ✅ Support **HTTP localhost/local server**
- ✅ Interactive (scroll, click, input)
- ✅ Tidak buka browser eksternal

### Di Unity Editor:
- ⚠️ Buka browser eksternal (testing mode)
- ✅ Build ke Android = in-app webview

## 📱 Platform Support

| Platform | In-App | HTTP Localhost |
|----------|--------|----------------|
| **Android Build** | ✅ YES | ✅ YES |
| **Unity Editor** | ❌ NO | ⚠️ Browser |
| **iOS** | ✅ YES* | ✅ YES* |

*iOS perlu konfigurasi tambahan

## ⚠️ PENTING untuk Localhost!

1. **Gunakan IP Address di Device:**
   ```
   ❌ http://localhost:5000
   ✅ http://192.168.1.100:5000  (IP PC Anda)
   ```

2. **Device & PC di Network yang Sama:**
   - Same WiFi network
   - Atau USB dengan port forwarding

3. **Check Firewall:**
   - Allow port 5000 di Windows Firewall
   - Test di browser device dulu

## 🔧 Troubleshooting

### "Cleartext not permitted" error
✅ **SOLVED!** Config sudah dibuat di:
- `Assets/Plugins/Android/res/xml/network_security_config.xml`
- Build APK baru dan error hilang

### Localhost tidak connect
- Gunakan IP address, bukan "localhost"
- Check server running di port 5000
- Device & PC same network
- Test: buka IP di Chrome Android dulu

### Detail troubleshooting
Baca **FIX_LOCALHOST.md** untuk solusi lengkap!

## 📖 Dokumentasi

- **INSTALL_PLUGIN.md** - Install Gree WebView
- **FIX_LOCALHOST.md** - Fix localhost HTTP support
- **README.txt** (ini) - Quick reference

## 🎉 Ready!

Dengan setup ini, webview akan:
1. ✅ Muncul di dalam game (in-app)
2. ✅ Bisa load HTTP localhost server
3. ✅ Full interactive di Android device

**Build ke Android dan test localhost server Anda!**
