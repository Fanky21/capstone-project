# FIX: "Cleartext Not Permitted" Error untuk Localhost

## 🔴 Problem
Error saat membuka `http://localhost:5000/` di webview Android:
```
Cleartext HTTP traffic not permitted
```

## ✅ SOLUSI SEDERHANA (Updated)

Saya sudah update `AndroidManifest.xml` untuk allow HTTP cleartext traffic.

### File yang Diupdate:

**Location:** `Assets/Plugins/Android/AndroidManifest.xml`

File ini sekarang menggunakan `android:usesCleartextTraffic="true"` yang:
- ✅ Mengizinkan HTTP traffic untuk semua domain (termasuk localhost)
- ✅ Kompatibel dengan Unity modern (tidak perlu file XML terpisah)
- ✅ Sederhana dan langsung

---

## 🔧 CARA MENGGUNAKAN

### Setup Sudah Selesai!

File `AndroidManifest.xml` sudah dikonfigurasi dengan benar. Yang perlu Anda lakukan:

1. **Build APK baru** (clean build recommended)
2. **Install ke Android device**
3. **Test dengan localhost server** ✅

---

## 📱 TESTING LOCALHOST SERVER

### PENTING: Gunakan IP Address, Bukan "localhost"

**Di Android Device:**
- ❌ `http://localhost:5000` → TIDAK AKAN WORK
- ✅ `http://192.168.1.100:5000` → AKAN WORK (gunakan IP PC Anda)

### Langkah Testing:

#### 1. Cari IP Address PC Anda
```bash
# Windows
ipconfig

# Lihat "IPv4 Address" di adapter WiFi yang aktif
# Contoh output: 192.168.1.100
```

#### 2. Pastikan Server Running
```bash
# Server harus running di port 5000 di PC
# Contoh: Flask, Node.js, dll
Server running on http://0.0.0.0:5000
```

#### 3. Set URL di Unity Script
```csharp
// Di Inspector atau code:
webviewURL = "http://192.168.1.100:5000/";  // Ganti dengan IP PC Anda
```

#### 4. Connect Device & PC ke Network yang Sama
- Device dan PC harus di WiFi yang sama
- Atau gunakan USB dengan port forwarding

#### 5. Test Koneksi Dulu di Browser
```
1. Buka Chrome di Android device
2. Navigate ke: http://192.168.1.100:5000
3. Jika website load → Koneksi OK ✅
4. Jika tidak load → Fix network/firewall dulu
```

#### 6. Build & Test di Unity
```
1. File → Build Settings → Android
2. Build APK
3. Install ke device
4. Run → Webview akan load localhost server! ✅
```

---

## 🔥 TROUBLESHOOTING

### 1. Koneksi Timeout / Cannot Connect

**Problem:** Device tidak bisa connect ke server PC

**Solusi:**
- ✅ Pastikan PC dan device di **WiFi yang sama**
- ✅ Check IP PC benar (gunakan `ipconfig`)
- ✅ Server benar-benar running di PC (test di browser PC dulu)
- ✅ Firewall Windows allow port 5000:
  ```
  Windows Defender Firewall → Allow an app → Add port 5000
  ```

### 2. Webview Masih Blank / Error

**Problem:** Webview tidak tampil atau error

**Solusi:**
- ✅ Build **clean APK** (delete folder `Build/` dan build ulang)
- ✅ Uninstall APK lama dari device
- ✅ Install APK baru
- ✅ Check Console log untuk error detail

### 3. "Cleartext Not Permitted" Masih Muncul

**Problem:** Error masih ada setelah update

**Solusi:**
- ✅ Pastikan `AndroidManifest.xml` sudah terupdate
- ✅ Clean build (hapus APK lama)
- ✅ Restart Unity Editor
- ✅ Build APK baru dari awal

### 4. Localhost Works di Emulator, Tidak di Device

**Problem:** Beda behavior emulator vs real device

**Solusi:**
- **Android Emulator:** Gunakan `http://10.0.2.2:5000`
  ```
  10.0.2.2 adalah special IP untuk akses host PC dari emulator
  ```
  
- **Real Device:** Gunakan IP actual PC
  ```
  http://192.168.1.100:5000 (IP WiFi PC Anda)
  ```

---

## 🌐 ALTERNATIF: Gunakan ngrok (Untuk Testing)

Jika setup network ribet, gunakan ngrok untuk expose localhost:

```bash
# Install ngrok: https://ngrok.com

# Run ngrok
ngrok http 5000

# Output:
Forwarding https://abc123.ngrok.io -> http://localhost:5000

# Gunakan URL ngrok di webview:
webviewURL = "https://abc123.ngrok.io";
```

Keuntungan:
- ✅ HTTPS (tidak perlu cleartext config)
- ✅ Works dari device manapun (tidak perlu same network)
- ✅ Public URL temporary untuk testing

---

## 🔒 KEAMANAN

### Development vs Production

**Current Setup:** `usesCleartextTraffic="true"`
- ✅ **Good for:** Development dan testing localhost
- ⚠️ **Warning:** Mengizinkan HTTP untuk **semua domain**

**Untuk Production:**

Sebaiknya gunakan HTTPS untuk server production, atau jika harus HTTP, gunakan network security config yang lebih spesifik (hanya allow localhost).

---

## 📋 CHECKLIST SEBELUM BUILD

- [ ] Server running di PC (port 5000)
- [ ] Tahu IP address PC (`ipconfig`)
- [ ] Device dan PC di WiFi yang sama
- [ ] Test connection di Chrome Android dulu
- [ ] URL di script gunakan IP PC, bukan "localhost"
- [ ] `AndroidManifest.xml` sudah terupdate
- [ ] Clean build (delete old APK)
- [ ] Firewall allow port 5000

---

## 🎯 QUICK REFERENCE

```
❌ JANGAN:
webviewURL = "http://localhost:5000";

✅ GUNAKAN:
webviewURL = "http://192.168.1.100:5000";  // IP PC Anda

✅ ATAU (Emulator):
webviewURL = "http://10.0.2.2:5000";

✅ ATAU (ngrok):
webviewURL = "https://abc123.ngrok.io";
```

---

**DONE!** 🎉  

Setelah update `AndroidManifest.xml` dan build APK baru dengan IP address yang benar, webview akan bisa load localhost server tanpa error!
