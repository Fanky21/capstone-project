# Local Server - Struktur File

## Struktur Folder

```
Assets/Local Server/
├── SimpleLocalServer.java     # Main HTTP server
├── MoneyManager.java           # Money & portfolio management
├── CompanyDataManager.java     # Company data management
├── NewsManager.java            # News management
│
├── templates/                  # HTML templates (seperti Flask)
│   ├── news.html              # Halaman news (route: /)
│   ├── markets.html           # Halaman markets (route: /markets)
│   └── index.html             # (tidak digunakan, hanya backup)
│
├── static/                     # Static files (seperti Flask)
│   ├── css/
│   │   └── style.css          # Stylesheet utama
│   └── js/
│       ├── app.js             # JavaScript untuk markets
│       └── news.js            # JavaScript untuk news
│
├── companies.json              # Data perusahaan
├── news.json                   # Data berita
│
└── libs/                       # JAR dependencies
    └── json-20210307.jar      # JSON library
```

## Routes

### Halaman Web
- `GET /` → Menampilkan `templates/news.html` (Halaman News)
- `GET /markets` → Menampilkan `templates/markets.html` (Halaman Markets)

### Static Files
- `GET /static/css/style.css` → CSS file
- `GET /static/js/app.js` → JavaScript untuk markets
- `GET /static/js/news.js` → JavaScript untuk news

### API Endpoints
- `GET /api/companies` - Semua perusahaan
- `GET /api/company/{ticker}` - Detail perusahaan
- `GET /api/news` - Berita terbaru
- `GET /api/portfolio` - Portfolio user
- `GET /api/cash` - Cash saat ini
- `POST /api/trade` - Execute trade
- `POST /api/update-prices` - Update harga
- `POST /api/reset` - Reset portfolio
- `GET /api/stats` - Statistik market

### Unity Integration API
- `GET /api/unity/health` - Health check
- `GET /api/unity/money/check` - Cek money
- `POST /api/unity/money/add` - Tambah money
- `POST /api/unity/money/subtract` - Kurangi money
- `POST /api/unity/money/set` - Set money

## Perbedaan dengan Flask

### Flask (Original)
```python
@app.route('/')
def index():
    return render_template('news.html')

@app.route('/markets')
def markets():
    return render_template('markets.html')
```

### Java HttpServer (Converted)
```java
// Root route serves news.html
server.createContext("/", createTemplateHandler("news.html"));

// Markets route
server.createContext("/markets", createTemplateHandler("markets.html"));
```

## Template Processing

Server secara otomatis mengganti Flask template syntax:

**Flask:**
```html
<link rel="stylesheet" href="{{ url_for('static', filename='css/style.css') }}">
<script src="{{ url_for('static', filename='js/app.js') }}"></script>
```

**Diubah menjadi:**
```html
<link rel="stylesheet" href="/static/css/style.css">
<script src="/static/js/app.js"></script>
```

## Cara Menjalankan

### Compile
```bash
javac -cp "libs/*" -encoding UTF-8 MoneyManager.java CompanyDataManager.java NewsManager.java SimpleLocalServer.java
```

### Run
```bash
java -cp ".;libs/*" SimpleLocalServer
```

### Akses Web Interface
- News: http://localhost:5000/
- Markets: http://localhost:5000/markets

## Testing

Test API endpoints:
```powershell
# Health check
Invoke-WebRequest http://localhost:5000/api/unity/health

# Get companies
Invoke-WebRequest http://localhost:5000/api/companies

# Get news
Invoke-WebRequest http://localhost:5000/api/news
```

## Notes

- Server menggunakan Java HttpServer bawaan (tidak perlu AndroidAsync)
- Hanya memerlukan 1 dependency: json-20210307.jar
- Kompatibel 100% dengan Flask API endpoints
- Template syntax Flask otomatis dikonversi
- Port default: 5000 (sama seperti Flask)
