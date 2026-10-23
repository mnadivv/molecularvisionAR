# Panduan Integrasi Unity AR - Molecular Vision AR (Monorepo)

Repositori ini telah dikonfigurasi sebagai **Monorepo (Flutter + Unity 6 Vuforia)**. Baik aplikasi Flutter maupun proyek Unity berada dalam satu repositori yang rapi, ringan, dan siap di-commit ke GitHub.

---

## 📁 Struktur Direktori Repositori
```text
molecularvisionAR/
├── lib/               # Source code Flutter (UI, Logika, Materi, Kuis)
├── android/           # Konfigurasi Gradle & Android Native
├── assets/            # Aset gambar, ikon, dan data materi
└── unity/
    └── MVAR/          # Proyek Unity 6 (Vuforia Engine AR)
        ├── Assets/
        │   └── Scripts/
        │       └── UnityAndroidReceiver.cs  # Script jembatan penerima Intent
        ├── Packages/
        └── ProjectSettings/
```

> **Catatan GitHub:** File [`.gitignore`](file:///c:/MVAR/molecularvisionAR/.gitignore) sudah disetting untuk mengabaikan folder cache Unity yang berat (`Library/`, `Temp/`, `Logs/`, `UserSettings/`). Jadi repository GitHub tetap ringan (< 50 MB) dan aman dari batas ukuran file 100 MB.

---

## 🚀 Panduan untuk Teman / Rekan Tim yang Meng-clone Repositori

Jika teman Anda meng-clone repositori ini dari GitHub:

### 1. Menjalankan Aplikasi Flutter
```bash
git clone <URL_REPOSITORY>
cd molecularvisionAR
flutter pub get
flutter run
```
Aplikasi Flutter akan langsung ter-build dan berjalan lancar di HP Android mereka.

---

### 2. Membuka & Mengedit Proyek Unity di Unity Hub
1. Buka **Unity Hub**.
2. Klik tombol **Add** > **Add project from disk**.
3. Pilih folder:
   ```text
   molecularvisionAR/unity/MVAR
   ```
4. Buka dengan versi **Unity 6 (6000.5.3f1)** atau versi Unity 6 LTS yang setara.
5. Saat pertama kali dibuka, Unity akan secara otomatis mengunduh & menyusun kembali folder `Library/` (ini normal dan membutuhkan waktu 1-3 menit).

---

### 3. Export Proyek Unity ke Flutter Embed (Super Ringan & Terkompresi)
Proyek ini menggunakan **`flutter_embed_unity`**, di mana Unity diexport sebagai modul Android library (`android/unityLibrary`):

#### Cara 1: Menggunakan Script Otomatis (Direkomendasikan)
Cukup jalankan script PowerShell berikut dari terminal root repository:
```powershell
.\export_unity_to_flutter.ps1
```
Script ini akan otomatis:
- Menjalankan Unity 6 dalam mode headless/batchmode.
- Mengaktifkan kompresi penuh **LZ4HC (High Compression)**.
- Menjalankan **`ShaderBuildStripper`** untuk membuang shader yang tidak terpakai (Sentis, Lightmaps, Fog, Cascaded Shadows, dsb.).
- Menerapkan **IL2CPP OptimizeSize** dan **Managed Stripping High** sehingga ukuran build minimal.
- Mengekspor langsung ke folder `android/unityLibrary`.

#### Cara 2: Dari Menu Unity Editor
1. Buka Unity Editor dengan proyek `unity/MVAR`.
2. Pada menu bar atas, klik **Flutter Embed**:
   - Pilih **Export Direct to Android unityLibrary (Compressed LZ4HC)** untuk export langsung.
   - Atau pilih **Export project to flutter app (Android)** lalu arahkan folder ke:
     ```text
     <lokasi_repo>/android/unityLibrary
     ```
3. Tunggu hingga proses export selesai. Folder `android/unityLibrary` akan otomatis terisi dan siap dikompilasi oleh Flutter.

---


## 🔄 Cara Kerja Interaksi Antara Flutter dan Unity

1. Pengguna membuka aplikasi Flutter dan memilih topik kimia (contoh: *Ikatan Kimia*).
2. Pengguna menekan tombol **"Luncurkan Kamera AR Unity"** di Flutter.
3. Flutter memanggil [`UnityLauncherService`](file:///c:/MVAR/molecularvisionAR/lib/services/unity_launcher_service.dart) via Android Intent:
   ```text
   topic_id = "ikatan_kimia"
   topic_title = "Ikatan Kimia"
   ```
4. HP secara otomatis berpindah membuka aplikasi Unity Vuforia:
   - Script `UnityAndroidReceiver.cs` membaca parameter `topic_id`.
   - Objek molekul 3D Vuforia untuk materi tersebut otomatis aktif.
5. Ketika siswa selesai melakukan praktikum AR, menekan tombol **Kembali** di Unity akan menjalankan `currentActivity.Call("finish")` yang langsung menutup Unity dan membawa pengguna kembali ke layar Flutter.

---

## 🏷️ Daftar ID Topik Materi Kimia yang Dikirim ke Unity
| No | Judul Materi | `topic_id` Intent |
|---|---|---|
| 1 | Ikatan Kimia | `ikatan_kimia` |
| 2 | Termokimia | `termokimia` |
| 3 | Kalorimetri | `kalorimetri` |
| 4 | Laju Reaksi | `laju_reaksi` |
| 5 | Asam Basa | `asam_basa` |
| 6 | Hidrolisis | `hidrolisis` |
| 7 | Hasil Kali Kelarutan | `hasil_kali_kelarutan` |
| 8 | Kesetimbangan Kimia | `kesetimbangan_kimia` |
| 9 | Sel Volta | `sel_volta` |
| 10 | Elektrolisis | `elektrolisis` |
| 11 | Hidrokarbon | `hidrokarbon` |
| 12 | Daur Ulang Polimer | `daur_ulang_polimer` |
