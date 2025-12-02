# Quick Start Guide

Get the Local Trading Server running in 5 minutes!

## Prerequisites

✅ **Java JDK 8 or higher** - [Download here](https://www.oracle.com/java/technologies/downloads/)
✅ **Unity 2020.3 or higher** (if using Unity integration)

## Step-by-Step Setup

### 1️⃣ Download Dependencies (1 minute)

Open PowerShell in the `Assets/Local Server` directory and run:

```powershell
.\download-deps.ps1
```

This will automatically download:
- AndroidAsync library
- JSON library

**Alternative:** Download manually from [DEPENDENCIES.md](DEPENDENCIES.md)

---

### 2️⃣ Build the Server (30 seconds)

```powershell
.\build.ps1
```

Or use the batch file:
```cmd
build.bat
```

✅ You should see: "Build Successful!"

---

### 3️⃣ Unity Integration (1 minute)

1. Open your Unity project
2. Find `StartLocalServer.cs` in `Assets/Script/`
3. Drag it onto any GameObject in your scene
4. In the Inspector, configure:
   - ✅ Auto Start: **Enabled**
   - ✅ Server Port: **5000**
   - ✅ Server Path: **Local Server**

---

### 4️⃣ Test It! (30 seconds)

**Run your Unity scene**

Check the Unity Console for:
```
Local Trading Server starting on port 5000...
Server is running at http://localhost:5000
```

**Open your browser:**
```
http://localhost:5000
```

You should see the trading interface! 🎉

---

## Verify Everything Works

### Test 1: Web Interface
- Open http://localhost:5000
- You should see the LCN Exchange homepage
- Click on a stock to see details

### Test 2: API Endpoint
- Open http://localhost:5000/api/companies
- You should see JSON data with all companies

### Test 3: Unity Integration
Create a test script:

```csharp
using UnityEngine;

public class TestServer : MonoBehaviour
{
    void Start()
    {
        StartLocalServer server = FindObjectOfType<StartLocalServer>();
        
        // Check money
        StartCoroutine(server.CheckMoney((success, money) => {
            if (success) {
                Debug.Log($"Current money: ${money}");
            }
        }));
        
        // Add money
        StartCoroutine(server.AddMoney(1000, (success, message) => {
            Debug.Log($"Add money: {message}");
        }));
    }
}
```

---

## Common Issues & Quick Fixes

### ❌ "Java not found"
**Fix:** Install JDK from https://www.oracle.com/java/technologies/downloads/

### ❌ "Missing dependencies"
**Fix:** Run `download-deps.ps1` or see [DEPENDENCIES.md](DEPENDENCIES.md)

### ❌ "Port 5000 in use"
**Fix:** Change port in StartLocalServer.cs Inspector

### ❌ "Build failed"
**Fix:** Make sure all JAR files are in `libs/` folder

### ❌ "Server won't start in Unity"
**Fix:** Check Unity Console for errors. Verify Java is in system PATH.

---

## Usage Examples

### Check Server Status (C#)
```csharp
StartLocalServer server = GetComponent<StartLocalServer>();
bool running = server.IsServerRunning();
string status = server.GetServerStatus();
Debug.Log($"Server Status: {status}");
```

### Add Money (C#)
```csharp
StartCoroutine(server.AddMoney(5000, (success, message) => {
    if (success) {
        Debug.Log("Money added successfully!");
    }
}));
```

### Get All Companies (JavaScript/Browser)
```javascript
fetch('http://localhost:5000/api/companies')
    .then(response => response.json())
    .then(data => console.log(data));
```

### Execute Trade (JavaScript/Browser)
```javascript
fetch('http://localhost:5000/api/trade', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
        action: 'buy',
        ticker: 'LET',
        shares: 10
    })
})
.then(response => response.json())
.then(data => console.log(data));
```

---

## What's Next?

- 📖 Read [README.md](README.md) for detailed documentation
- 🔄 See [CONVERSION.md](CONVERSION.md) for Flask comparison
- 🛠️ Check [DEPENDENCIES.md](DEPENDENCIES.md) for manual setup
- 🚀 Explore the API endpoints

---

## File Structure

```
Assets/Local Server/
├── LocalServer.java           ← Main server
├── MoneyManager.java          ← Money management
├── CompanyDataManager.java    ← Company data
├── NewsManager.java           ← News handling
├── index.html                 ← Web interface
├── app.js                     ← Frontend logic
├── style.css                  ← Styling
├── companies.json             ← Company data
├── news.json                  ← News articles
├── build.ps1                  ← Build script
├── download-deps.ps1          ← Dependency downloader
├── libs/                      ← JAR dependencies
│   ├── androidasync-3.1.0.jar
│   └── json-20210307.jar
└── bin/                       ← Compiled classes
```

---

## Quick Commands Reference

### PowerShell Commands
```powershell
# Download dependencies
.\download-deps.ps1

# Build server
.\build.ps1

# Run server manually (optional)
java -cp "bin;libs/*" com.trading.localserver.LocalServer
```

### Batch Commands
```cmd
# Build server
build.bat
```

### API Testing (curl)
```bash
# Get companies
curl http://localhost:5000/api/companies

# Check health
curl http://localhost:5000/api/unity/health

# Get money
curl http://localhost:5000/api/unity/money/check

# Add money
curl -X POST http://localhost:5000/api/unity/money/add ^
     -H "Content-Type: application/json" ^
     -d "{\"amount\":1000}"
```

---

## Success Checklist

- ✅ Java installed and verified
- ✅ Dependencies downloaded to `libs/`
- ✅ Server compiled successfully
- ✅ StartLocalServer.cs added to Unity scene
- ✅ Server starts when scene loads
- ✅ Web interface accessible at localhost:5000
- ✅ API endpoints responding correctly

---

## Need Help?

1. Check Unity Console for error messages
2. Review [README.md](README.md) for detailed docs
3. Verify all dependencies are installed
4. Make sure Java is in system PATH
5. Test manually: `java -version`

---

**🎉 You're all set! Happy trading!**
