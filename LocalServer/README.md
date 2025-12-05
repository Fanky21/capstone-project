# Local Trading Server (AndroidAsync)

This is a conversion of the Flask trading server to AndroidAsync (by Koush) for use in Unity projects.

## Overview

The Local Server provides HTTP endpoints for:
- Stock market data (companies, prices, news)
- Trading functionality (buy/sell stocks)
- Portfolio management
- Money management (Unity integration)
- Real-time price updates

## Architecture

### Java Components

1. **LocalServer.java**
   - Main HTTP server using AndroidAsync
   - Routes all HTTP requests
   - Manages server lifecycle
   - Port: 5000 (default)

2. **MoneyManager.java**
   - Global cash management
   - Portfolio tracking (shares owned)
   - Buy/sell operations

3. **CompanyDataManager.java**
   - Loads and manages company data from companies.json
   - Updates stock prices periodically
   - Calculates market statistics

4. **NewsManager.java**
   - Manages news articles
   - Active news (latest 10) and archived news

### Static Files

- **index.html** - Main web interface
- **app.js** - Frontend JavaScript logic
- **style.css** - Styling
- **companies.json** - Company data with prices
- **news.json** - News articles

## Setup Instructions

### Prerequisites

1. **Java Development Kit (JDK) 8 or higher**
   - Download from: https://www.oracle.com/java/technologies/downloads/
   - Verify installation: `java -version`

2. **AndroidAsync Library (by Koush)**
   - Download from: https://github.com/koush/AndroidAsync
   - Or use Maven coordinates: `com.koushikdutta.async:androidasync:3.1.0`

3. **JSON Library**
   - Download org.json from: https://mvnrepository.com/artifact/org.json/json
   - Or use: `org.json:json:20210307`

### Installation

1. **Download Required Libraries**
   
   Place the following JAR files in `Assets/Local Server/libs/`:
   - `androidasync-3.1.0.jar`
   - `json-20210307.jar`

2. **Compile Java Files**

   Run the build script:
   ```powershell
   # Windows PowerShell
   cd "Assets/Local Server"
   .\build.ps1
   ```

   Or compile manually:
   ```bash
   javac -cp "libs/*" -d bin *.java
   ```

3. **Add StartLocalServer.cs to Unity Scene**
   
   - Attach `StartLocalServer.cs` to a GameObject in your scene
   - Configure settings in the Inspector:
     - Server Port: 5000 (default)
     - Auto Start: true
     - Server Path: "Local Server"

## Usage

### Starting the Server

The server starts automatically when the Unity scene loads (if Auto Start is enabled).

You can also control it programmatically:

```csharp
StartLocalServer serverManager = GetComponent<StartLocalServer>();

// Start server
serverManager.StartServer();

// Stop server
serverManager.StopServer();

// Restart server
serverManager.RestartServer();

// Check status
bool isRunning = serverManager.IsServerRunning();
string status = serverManager.GetServerStatus();
```

### Using Unity API

```csharp
StartLocalServer server = GetComponent<StartLocalServer>();

// Check money
StartCoroutine(server.CheckMoney((success, money) => {
    if (success) {
        Debug.Log($"Current money: ${money}");
    }
}));

// Add money
StartCoroutine(server.AddMoney(1000, (success, message) => {
    Debug.Log(success ? message : "Failed to add money");
}));

// Subtract money
StartCoroutine(server.SubtractMoney(500, (success, message) => {
    Debug.Log(success ? message : "Failed to subtract money");
}));

// Set money
StartCoroutine(server.SetMoney(5000, (success, message) => {
    Debug.Log(success ? message : "Failed to set money");
}));
```

## API Endpoints

### Stock Market Endpoints

- `GET /api/companies` - Get all companies
- `GET /api/company/{ticker}` - Get specific company
- `GET /api/news` - Get latest news
- `GET /api/news/old` - Get archived news
- `GET /api/portfolio` - Get portfolio with details
- `GET /api/cash` - Get current cash
- `GET /api/stats` - Get market statistics
- `POST /api/trade` - Execute trade (buy/sell)
- `POST /api/update-prices` - Update all prices
- `POST /api/reset` - Reset portfolio

### Unity Integration Endpoints

- `GET /api/unity/health` - Health check
- `GET /api/unity/money/check` - Check current money
- `POST /api/unity/money/add` - Add money
- `POST /api/unity/money/subtract` - Subtract money
- `POST /api/unity/money/set` - Set money to specific amount

### General Money Management

- `GET /api/money/get` - Get current money
- `POST /api/money/add` - Add money
- `POST /api/money/subtract` - Subtract money

## Web Interface

Access the web interface at: `http://localhost:5000`

Features:
- View all companies and stock prices
- Real-time price updates
- Buy and sell stocks
- View portfolio
- Track profit/loss
- Read news articles

## Background Tasks

The server automatically:
- Updates stock prices every 30 seconds
- Maintains price history (7 days)
- Simulates market movements (-5% to +5% per update)

## Differences from Flask Version

1. **Server Framework**: AndroidAsync instead of Flask
2. **Language**: Java instead of Python
3. **News Generation**: Simplified (no AI generation in this version)
4. **Session Management**: Uses global state (no sessions)
5. **CORS**: Built into response handling
6. **Static File Serving**: Direct file serving from filesystem

## Troubleshooting

### Server Won't Start

1. Check Java installation: `java -version`
2. Verify JAR files are in `libs/` folder
3. Check Unity Console for error messages
4. Ensure port 5000 is not in use

### Can't Access Web Interface

1. Check server status in Unity Inspector
2. Verify server is running: Visit `http://localhost:5000/api/unity/health`
3. Check firewall settings

### Compilation Errors

1. Ensure all dependencies are in `libs/` folder
2. Check Java version compatibility
3. Verify package structure matches file paths

## File Structure

```
Assets/Local Server/
├── LocalServer.java          # Main server
├── MoneyManager.java          # Money & portfolio management
├── CompanyDataManager.java    # Company data management
├── NewsManager.java           # News management
├── index.html                 # Web interface
├── app.js                     # Frontend logic
├── style.css                  # Styling
├── companies.json             # Company data
├── news.json                  # News articles
├── build.ps1                  # Build script
├── README.md                  # This file
└── libs/                      # JAR dependencies
    ├── androidasync-3.1.0.jar
    └── json-20210307.jar
```

## License

Converted from Flask application for Unity integration.
AndroidAsync library by Koush: https://github.com/koush/AndroidAsync

## Support

For issues or questions, check:
- Unity Console for error messages
- Server logs in terminal
- API response messages
