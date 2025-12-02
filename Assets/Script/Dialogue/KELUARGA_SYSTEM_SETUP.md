# Panduan Setup Sistem Keluarga Money Request

## Overview
Sistem ini memungkinkan keluarga pemain meminta uang secara berkala (10-15 menit). Pemain dapat menerima atau menolak dengan tombol. Jika menolak, hutang akan menjadi 2x lipat.

## File yang Dibuat/Dimodifikasi

1. **DialogueManager.cs** - Diperbarui
   - Menambah support untuk choice buttons (Terima/Tolak)
   - Menambah method `StartDialogueWithChoices()`
   - Menambah button container dan prefab

2. **KeluargaManager.cs** - Dibuat
   - Mengelola jadwal permintaan uang (random 10-15 menit)
   - Mengelola debt multiplier (1x, 2x, 4x, dll)
   - Callback untuk handle pilihan pemain

3. **PlayerMoneyManager.cs** - Dibuat
   - Mengelola uang pemain
   - Method `AddMoney()`, `RemoveMoney()`, `GetMoney()`

## Setup di Unity

### Step 1: Update DialogueManager GameObject
1. Select GameObject dengan component `DialogueManager`
2. Di Inspector, assign field baru:
   - **Choice Button Prefab**: Buat button prefab dengan TextMeshProUGUI sebagai child
   - **Choice Container**: Transform untuk menyimpan buttons (biasanya Panel/HorizontalLayoutGroup)
   - **Continue Button**: Button untuk lanjut dialog

### Step 2: Create Choice Button Prefab
1. Create Panel di Canvas untuk dialog
2. Create 2 Button sebagai children (untuk template)
3. Setup sebagai Prefab
4. Assign ke DialogueManager's "Choice Button Prefab"

### Step 3: Setup PlayerMoneyManager
1. Create GameObject baru di scene bernama "PlayerMoneyManager"
2. Add component `PlayerMoneyManager`
3. Set initial money di Inspector

### Step 4: Setup KeluargaManager
1. Create GameObject baru di scene bernama "KeluargaManager"
2. Add component `KeluargaManager`
3. Di Inspector, setup:
   - **Min Request Interval**: 10 (menit)
   - **Max Request Interval**: 15 (menit)
   - **Money Amount**: 10000 (jumlah yang diminta)
   - **Keluarga Character**: Buat DialogueCharacter dengan nama "Keluarga"

## How It Works

### Timeline
1. **Start**: Keluarga dijadwalkan meminta uang dalam 10-15 menit
2. **Request**: Dialog muncul dengan pilihan Terima/Tolak
3. **Accept**: Uang berkurang, hutang reset ke 1x
4. **Refuse**: Hutang menjadi 2x lipat, request dijadwalkan ulang
5. **Loop**: Kembali ke step 1

### Money Flow
- Awal: Pemain diminta Rp10.000
- Tolak 1x: Berikutnya diminta Rp20.000 (10.000 x 2)
- Tolak 2x: Berikutnya diminta Rp40.000 (10.000 x 4)
- Tolak 3x: Berikutnya diminta Rp80.000 (10.000 x 8)
- Dan seterusnya...
- Jika terima: Hutang reset, mulai dari Rp10.000 lagi

## Dialog Format

### Request Dialog
```
[Keluarga]: Nak, keluarga butuh uang sebesar Rp10.000.
[Keluarga]: Bisakah kamu memberikannya?
[Buttons]: [Terima] [Tolak]
```

## Customization

### Edit Dialog Keluarga
Di `KeluargaManager.cs`, method `MakeMoneyRequest()`:
```csharp
DialogueLine line1 = new DialogueLine
{
    character = keluargaCharacter,
    utama_Dialogue = true,
    line = "Custom dialog sini" // Edit text di sini
};
```

### Edit Jadwal Request
Di Inspector KeluargaManager:
- **Min Request Interval**: Ubah dari 10
- **Max Request Interval**: Ubah dari 15

### Edit Jumlah Uang
Di Inspector KeluargaManager:
- **Money Amount**: Ubah dari 10000

## Testing

1. Play Scene
2. Tunggu 10-15 detik (waktu game, bukan real-time)
3. Dialog keluarga muncul
4. Click "Terima" atau "Tolak"
5. Uang akan berkurang (jika terima) atau hutang menjadi 2x (jika tolak)
6. Repeat

## Debug

Di Console, Anda akan melihat log:
```
Pemain menerima memberikan Rp10000
Uang berkurang: -Rp10000. Total: Rp40000

atau

Pemain menolak - Utang menjadi 2x lipat!
```

## Notes

- Waktu interval dalam game time, bukan real-time
- Untuk test cepat, ubah min/max request interval menjadi lebih kecil (e.g., 0.5 menit)
- Jika uang tidak cukup saat menerima, dialog khusus akan muncul dan hutang tetap sama
- Debt multiplier bersifat persistent selama game session

## Future Enhancements

1. Save/Load debt status
2. Tambah character dialogues yang variatif
3. Sound effects untuk accept/refuse
4. UI showing current debt
5. Animation untuk button choice
