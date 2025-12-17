# Unity Build Error - AndroidManifest Fix

## ✅ SOLUSI FINAL (Updated)

Error `NullReferenceException` di `UnityWebViewPostprocessBuild.cs` terjadi karena plugin memerlukan AndroidManifest yang di-generate oleh Unity, bukan custom manifest.

### Yang Sudah Dilakukan:

1. ✅ **Hapus custom AndroidManifest.xml**
   - File custom manifest sudah dihapus
   - Unity akan generate manifest yang proper saat build

2. ✅ **Plugin akan auto-configure cleartext**
   - Gree WebView plugin punya support built-in untuk cleartext traffic
   - Line 97-101 di UnityWebViewPostprocessBuild.cs menangani ini

---

## 🔧 ENABLE CLEARTEXT TRAFFIC (Correct Way)

### Opsi 1: Define Scripting Symbol (Recommended)

Untuk enable HTTP localhost support, tambahkan scripting define symbol:

**Cara 1: Via Player Settings UI**
```
1. Edit → Project Settings
2. Player → Other Settings
3. Scroll ke "Scripting Define Symbols"
4. Tambah: UNITYWEBVIEW_ANDROID_USES_CLEARTEXT_TRAFFIC
5. Click Apply
```

**Cara 2: Via Script (Paste ke Editor folder)**

Create file: `Assets/Editor/WebViewDefines.cs`

```csharp
#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public class WebViewDefines
{
    static WebViewDefines()
    {
        var buildTargetGroup = BuildTargetGroup.Android;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        
        if (!defines.Contains("UNITYWEBVIEW_ANDROID_USES_CLEARTEXT_TRAFFIC"))
        {
            defines += ";UNITYWEBVIEW_ANDROID_USES_CLEARTEXT_TRAFFIC";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
            UnityEngine.Debug.Log("Added UNITYWEBVIEW_ANDROID_USES_CLEARTEXT_TRAFFIC define");
        }
    }
}
#endif
```

### Opsi 2: Biar Unity Generate (Tanpa Cleartext)

Jika hanya untuk HTTPS:
- Tidak perlu setting apapun
- Langsung build

---

## 📱 TESTING LOCALHOST

**Penting:** Setelah enable cleartext traffic define symbol, gunakan **IP address PC**, bukan "localhost":

```
❌ SALAH:
http://localhost:5000

✅ BENAR:
http://192.168.1.100:5000  (IP PC Anda)
```

**Check IP PC:**
```bash
# Windows Command Prompt
ipconfig

# Lihat "IPv4 Address"
```

---

## 🚀 BUILD STEPS

1. **Enable Cleartext (jika perlu localhost):**
   - Tambah define symbol: `UNITYWEBVIEW_ANDROID_USES_CLEARTEXT_TRAFFIC`
   - Atau create `WebViewDefines.cs` di Assets/Editor/

2. **Refresh Unity:**
   - Ctrl+R atau Assets → Refresh
   - Wait for compilation

3. **Build APK:**
   - File → Build Settings → Android
   - Build And Run
   - Error seharusnya hilang! ✅

4. **Test:**
   - Webview akan load di dalam game
   - HTTP localhost akan work (jika define symbol sudah ditambah)

---

## 🐛 TROUBLESHOOTING

### Error masih muncul
1. Pastikan AndroidManifest.xml custom sudah dihapus
2. Restart Unity Editor
3. Clean build (delete Build/ folder)
4. Build ulang

### Cleartext masih tidak work
1. Check define symbol sudah ditambah: `UNITYWEBVIEW_ANDROID_USES_CLEARTEXT_TRAFFIC`
2. Rebuild APK (define symbol harus ada sebelum build)
3. Gunakan IP address, bukan localhost

### Webview tidak muncul
1. Check Gree WebView plugin terinstall dengan benar
2. Check file WebViewPlugin.aar ada di Assets/Plugins/Android/
3. Check Console untuk error lain

---

## 📋 VERIFICATION CHECKLIST

- [ ] Custom AndroidManifest.xml sudah dihapus
- [ ] Define symbol `UNITYWEBVIEW_ANDROID_USES_CLEARTEXT_TRAFFIC` ditambah (jika perlu localhost)
- [ ] Unity di-refresh (Ctrl+R)
- [ ] No compilation errors
- [ ] Build platform = Android
- [ ] URL menggunakan IP address PC, bukan "localhost"

---

## 📝 SUMMARY

**Kesalahan Sebelumnya:**
- ❌ Custom AndroidManifest.xml terlalu sederhana
- ❌ Tidak kompatibel dengan Gree WebView plugin

**Solusi:**
- ✅ Hapus custom manifest, biar Unity generate
- ✅ Plugin auto-handle manifest modifications
- ✅ Gunakan define symbol untuk enable cleartext

---

**Ready!** 🎉

Plugin akan otomatis configure AndroidManifest saat buil. Tinggal tambah define symbol (kalau perlu localhost) dan build!
