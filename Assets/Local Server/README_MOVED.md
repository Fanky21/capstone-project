# Local Server - MOVED

⚠️ **PENTING: File server telah dipindahkan!**

## Lokasi Baru

File-file server Java sekarang berada di:
```
E:\Private\Developer\capstone-project\LocalServer\
```

**BUKAN** di dalam folder `Assets/Local Server/` lagi.

## Mengapa Dipindahkan?

Unity mencoba mengkompilasi semua file `.java` yang ada di dalam folder `Assets/` saat build Android. Ini menyebabkan error karena:

1. File Java server menggunakan library yang tidak tersedia di Android build Unity
2. Server ini tidak dimaksudkan untuk di-compile oleh Unity
3. Server harus berjalan terpisah dari Unity

## Struktur Sekarang

```
capstone-project/
├── Assets/
│   ├── Script/
│   │   └── StartLocalServer.cs   ← Unity script untuk start server
│   └── Local Server/
│       └── README.md              ← File ini (dokumentasi)
│
└── LocalServer/                   ← LOKASI BARU SERVER
    ├── SimpleLocalServer.java
    ├── MoneyManager.java
    ├── CompanyDataManager.java
    ├── NewsManager.java
    ├── templates/
    ├── static/
    ├── libs/
    ├── companies.json
    └── news.json
```

## Cara Menggunakan

### 1. Compile Server (Manual)

```powershell
cd LocalServer
javac -cp "libs/*" -encoding UTF-8 MoneyManager.java CompanyDataManager.java NewsManager.java SimpleLocalServer.java
```

### 2. Run Server (Manual)

```powershell
cd LocalServer
java -cp ".;libs/*" SimpleLocalServer
```

### 3. Run dari Unity

Script `StartLocalServer.cs` sudah diupdate untuk menunjuk ke lokasi baru:

```csharp
// Otomatis mencari di: ProjectRoot/LocalServer/
public string serverPath = "LocalServer";
```

## Testing

Server akan tetap berjalan di `http://localhost:5000`

- News: http://localhost:5000/
- Markets: http://localhost:5000/markets
- API: http://localhost:5000/api/unity/health

## Build Android

Sekarang build Android akan berhasil karena file `.java` server tidak lagi di dalam folder `Assets/`.

## Notes

- Folder `Assets/Local Server/` tetap ada untuk menyimpan dokumentasi
- `StartLocalServer.cs` secara otomatis mencari server di folder `LocalServer/` (di luar Assets)
- Server tetap berfungsi sama seperti sebelumnya
- Data (companies.json, news.json) juga dipindahkan ke lokasi baru

## Troubleshooting

### Server tidak start dari Unity?
1. Pastikan folder `LocalServer/` ada di root project (sejajar dengan Assets/)
2. Cek Inspector di Unity: Server Path harus `LocalServer` (tanpa `Assets/`)
3. Pastikan file Java sudah dikompilasi

### Masih error saat build Android?
1. Pastikan tidak ada file `.java` di folder `Assets/`
2. Clean Unity project (Assets > Clean Cache)
3. Rebuild

## Dokumentasi Lengkap

Lihat file di `LocalServer/`:
- `QUICKSTART.md` - Panduan cepat
- `README.md` - Dokumentasi lengkap
- `STRUCTURE.md` - Struktur file dan routes
- `CONVERSION.md` - Detail konversi dari Flask
