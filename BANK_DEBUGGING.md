# Bank Server Debugging Guide

## Issue: localhost:5001 tidak menampilkan apa-apa

### ✅ Changes Made:

1. **Enhanced Logging di BankServer.java**
   - Added emoji icons untuk easy spotting: 📥 📂 ✅ ❌
   - Log setiap request dengan detail
   - Log file loading process
   - Return HTML error page instead of JSON for easier debugging

2. **Fixed API_BASE di bank.js**
   - Changed from: `const API_BASE = 'http://localhost:5001';`
   - Changed to: `const API_BASE = window.location.origin;`
   - Reason: Prevents CORS issues, works from any IP

3. **Fixed POST Data Handling**
   - Added proper parseBody() for NanoHTTPD
   - Support both JSON and form-encoded data
   - Better error messages

4. **Added Test Page**
   - Created `/test.html` for quick server verification
   - Auto-tests Balance API on page load
   - Visual buttons to test all APIs

## 🧪 Testing Steps:

### Step 1: Build & Install APK
```bash
# Di Unity: Build APK
# Install ke Android device
```

### Step 2: Check Logs
```bash
# Di Android Studio atau adb logcat
adb logcat | grep -i "BankServer"
```

Expected logs when accessing http://localhost:5001:
```
BankServer: ✅ Bank Server started on port 5001
BankServer: 📥 REQUEST: GET /
BankServer: Serving index.html from server/bank/index.html
BankServer: 📂 Attempting to open: server/bank/index.html
BankServer: ✅ File loaded successfully, size: XXXX chars
```

### Step 3: Test Pages

#### Option A: Test Page (Simple)
```
http://localhost:5001/test
atau
http://[YOUR_IP]:5001/test
```

Should show:
- ✅ Green "Working!" message
- Auto-test balance API
- Buttons to test other APIs

#### Option B: Main Bank Page
```
http://localhost:5001/
```

Should show:
- Bank interface with 2 tabs
- Balance display at top
- Deposit & Loan forms

### Step 4: Check API Endpoints

Test APIs manually:
```
http://localhost:5001/api/balance
http://localhost:5001/api/deposits
http://localhost:5001/api/loans
http://localhost:5001/api/stats
```

Should return JSON data.

## 🔍 Common Issues & Solutions:

### Issue 1: White/Blank Page
**Symptom:** Page loads but nothing displayed

**Check:**
1. View page source (Ctrl+U) - Is HTML there?
2. Open browser console (F12) - Any JS errors?
3. Check Network tab - Are CSS/JS files loading?

**Logcat should show:**
```
BankServer: 📥 REQUEST: GET /
BankServer: 📥 REQUEST: GET /static/css/bank.css
BankServer: 📥 REQUEST: GET /static/js/bank.js
```

**Fix:**
- If HTML loads but blank: JS error, check console
- If 404 on static files: Path issue, check folder structure

### Issue 2: Error Page Displayed
**Symptom:** HTML page shows Java exception

**Check logcat for:**
```
BankServer: ❌ Error serving request
BankServer: Error message: [error details]
BankServer: Error class: [exception type]
```

**Common errors:**
- `FileNotFoundException`: File not in assets
- `JSONException`: API data format issue
- `NullPointerException`: MoneyManager not initialized

### Issue 3: API Returns Error
**Symptom:** API call fails with 500 error

**Check:**
1. Is BankDataManager initialized?
2. Is MoneyManager shared correctly?
3. Check logcat for Java exceptions

### Issue 4: Can't Access from Browser
**Symptom:** Connection refused or timeout

**Check:**
1. Is server actually started?
   ```
   adb logcat | grep "✅ Bank Server started"
   ```

2. Is port 5001 accessible?
   ```
   adb shell netstat -an | grep 5001
   ```

3. Try from phone's browser first (http://localhost:5001)
4. Then try from PC using phone's IP

5. Firewall might be blocking port 5001

## 📂 File Structure Verification:

Files should be at:
```
Assets/
  StreamingAssets/
    server/
      bank/
        index.html          ✓
        test.html           ✓
        static/
          css/
            bank.css        ✓
          js/
            bank.js         ✓
```

When APK is built, these go to Android assets:
```
server/bank/index.html
server/bank/test.html
server/bank/static/css/bank.css
server/bank/static/js/bank.js
```

## 🛠️ Debug Checklist:

- [ ] BankServer.java compiled without errors
- [ ] Files exist in Assets/StreamingAssets/server/bank/
- [ ] APK rebuilt after code changes
- [ ] APK installed on device (not old version)
- [ ] Server started successfully (check logcat)
- [ ] Port 5001 listening (check netstat)
- [ ] Test page loads (http://localhost:5001/test)
- [ ] Balance API returns JSON (http://localhost:5001/api/balance)
- [ ] Main page loads (http://localhost:5001/)
- [ ] CSS loads (check Network tab)
- [ ] JS loads (check Network tab)
- [ ] No JS errors in console

## 🚀 Quick Fix Commands:

### Clean & Rebuild
```bash
# In Unity
1. Assets > Reimport All
2. File > Build Settings > Build
3. Install new APK
```

### Force Restart Server
```csharp
// In Unity script
StartLocalServer.Instance.RestartServer();
```

### Check Server Status
```csharp
// In Unity script
string status = StartLocalServer.Instance.GetServerStatus();
// Should show: "Both Running (5000 + 5001)"
```

## 📱 Testing from Device Browser:

1. Open Chrome on Android device
2. Go to: `http://localhost:5001/test`
3. Should see green success message
4. Click API test buttons
5. All should return JSON data

## 💻 Testing from PC Browser:

1. Get device IP: `adb shell ip addr show wlan0`
2. Open browser on PC
3. Go to: `http://[DEVICE_IP]:5001/test`
4. Should see same test page
5. If timeout: Check firewall/network

## 📊 Expected Behavior:

### First Access (/)
```
BankServer: 📥 REQUEST: GET /
BankServer: Serving index.html
BankServer: 📂 Attempting to open: server/bank/index.html
BankServer: ✅ File loaded successfully, size: XXXX chars
BankServer: 📥 REQUEST: GET /static/css/bank.css
BankServer: 📂 Attempting to open: server/bank/static/css/bank.css
BankServer: ✅ File loaded successfully
BankServer: 📥 REQUEST: GET /static/js/bank.js
BankServer: 📂 Attempting to open: server/bank/static/js/bank.js
BankServer: ✅ File loaded successfully
BankServer: 📥 REQUEST: GET /api/balance
```

### API Call (/api/balance)
```
BankServer: 📥 REQUEST: GET /api/balance
[Returns JSON with balance]
```

## ⚠️ Important Notes:

1. **Must rebuild APK** after any Java code changes
2. **Must reimport** after adding new files to StreamingAssets
3. **Clear app data** if behavior seems cached
4. **Check logcat** for all debugging
5. **Test page first** before testing main page

## 🎯 Success Criteria:

✅ http://localhost:5001/test shows "Working!"
✅ Balance API button returns JSON
✅ http://localhost:5001/ shows bank interface
✅ Deposit tab displays form
✅ Loan tab displays info
✅ Balance shows at top
✅ No console errors in browser F12

If all above pass, server is working correctly!
