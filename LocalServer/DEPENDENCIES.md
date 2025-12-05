# Dependency Download Instructions

To run the Local Trading Server, you need to download the following JAR files and place them in the `libs/` folder.

## Required Dependencies

### 1. AndroidAsync by Koush

**What it is:** Asynchronous HTTP server library for Android/Java

**Download Options:**

**Option A - Maven Central:**
1. Visit: https://mvnrepository.com/artifact/com.koushikdutta.async/androidasync
2. Click on version 3.1.0 (or latest)
3. Download the JAR file
4. Place in `Assets/Local Server/libs/`

**Option B - GitHub Releases:**
1. Visit: https://github.com/koush/AndroidAsync
2. Look for releases or build from source
3. Place the JAR in `Assets/Local Server/libs/`

**Option C - Direct Maven Link:**
```
https://repo1.maven.org/maven2/com/koushikdutta/async/androidasync/3.1.0/androidasync-3.1.0.jar
```

### 2. JSON Library (org.json)

**What it is:** JSON parsing and generation library for Java

**Download Options:**

**Option A - Maven Central:**
1. Visit: https://mvnrepository.com/artifact/org.json/json
2. Click on version 20210307 (or latest)
3. Download the JAR file
4. Place in `Assets/Local Server/libs/`

**Option B - Direct Maven Link:**
```
https://repo1.maven.org/maven2/org/json/json/20210307/json-20210307.jar
```

**Option C - GitHub:**
1. Visit: https://github.com/stleary/JSON-java
2. Download releases
3. Place in `Assets/Local Server/libs/`

## Quick Download (PowerShell)

If you have PowerShell, you can use these commands to download automatically:

```powershell
# Navigate to Local Server directory
cd "Assets/Local Server"

# Create libs directory
New-Item -ItemType Directory -Force -Path "libs"

# Download AndroidAsync
Invoke-WebRequest -Uri "https://repo1.maven.org/maven2/com/koushikdutta/async/androidasync/3.1.0/androidasync-3.1.0.jar" -OutFile "libs/androidasync-3.1.0.jar"

# Download JSON library
Invoke-WebRequest -Uri "https://repo1.maven.org/maven2/org/json/json/20210307/json-20210307.jar" -OutFile "libs/json-20210307.jar"

Write-Host "Dependencies downloaded successfully!" -ForegroundColor Green
```

## Verification

After downloading, your directory structure should look like:

```
Assets/Local Server/
├── libs/
│   ├── androidasync-3.1.0.jar
│   └── json-20210307.jar
├── LocalServer.java
├── MoneyManager.java
├── CompanyDataManager.java
├── NewsManager.java
└── ... (other files)
```

## Build After Download

Once dependencies are downloaded:

1. **Using PowerShell:**
   ```powershell
   cd "Assets/Local Server"
   .\build.ps1
   ```

2. **Using Batch:**
   ```cmd
   cd "Assets\Local Server"
   build.bat
   ```

3. **Manual Compilation:**
   ```bash
   javac -cp "libs/*" -d bin *.java
   ```

## Troubleshooting

### "Cannot find symbol" errors
- Make sure both JAR files are in the `libs/` folder
- Verify JAR files are not corrupted (re-download if needed)

### "Class not found" errors at runtime
- Ensure JAR files are included in classpath
- Check that StartLocalServer.cs is finding the JAR files

### Download fails
- Check internet connection
- Try alternative download options
- Download manually from Maven Central website

## Alternative: Using Maven or Gradle

If you're familiar with Maven or Gradle, you can add these dependencies:

**Maven:**
```xml
<dependencies>
    <dependency>
        <groupId>com.koushikdutta.async</groupId>
        <artifactId>androidasync</artifactId>
        <version>3.1.0</version>
    </dependency>
    <dependency>
        <groupId>org.json</groupId>
        <artifactId>json</artifactId>
        <version>20210307</version>
    </dependency>
</dependencies>
```

**Gradle:**
```gradle
dependencies {
    implementation 'com.koushikdutta.async:androidasync:3.1.0'
    implementation 'org.json:json:20210307'
}
```

## Support

If you continue to have issues downloading dependencies:
1. Check the README.md for more information
2. Verify your Java installation: `java -version`
3. Ensure you have internet access
4. Try manual download from Maven Central website
