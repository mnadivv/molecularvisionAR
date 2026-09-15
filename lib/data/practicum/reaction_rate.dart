import '../../models/material_item.dart';
import '../../models/practicum.dart';
import '../../models/practicum_step.dart';
import '../../models/tool_item.dart';

final reactionRatePracticum = Practicum(
  id: "reaction_rate",

  title: "Laju Reaksi",

  description:
      "Praktikum untuk mengetahui pengaruh konsentrasi terhadap laju reaksi menggunakan Augmented Reality.",

  objective:
      "Peserta didik mampu memahami pengaruh konsentrasi terhadap laju reaksi melalui simulasi praktikum interaktif.",

  estimatedMinutes: 15,

  difficulty: 2,

  tools: [

    ToolItem(
      id: "beaker",
      name: "Gelas Kimia",
      image: "assets/tools/beaker.png",
      description: "Wadah utama pencampuran larutan.",
      category: "Wadah",
    ),

    ToolItem(
      id: "pipette",
      name: "Pipet Tetes",
      image: "assets/tools/pipette.png",
      description: "Mengambil dan meneteskan larutan.",
      category: "Alat Ukur",
    ),

    ToolItem(
      id: "balance",
      name: "Neraca Digital",
      image: "assets/tools/balance.png",
      description: "Menimbang massa zat.",
      category: "Alat Ukur",
    ),

    ToolItem(
      id: "volumetric_flask",
      name: "Labu Ukur",
      image: "assets/tools/flask.png",
      description: "Menyiapkan larutan dengan volume tertentu.",
      category: "Wadah",
    ),

    ToolItem(
      id: "stirrer",
      name: "Batang Pengaduk",
      image: "assets/tools/stirrer.png",
      description: "Mengaduk larutan.",
      category: "Pengaduk",
    ),

    ToolItem(
      id: "spray",
      name: "Botol Semprot",
      image: "assets/tools/spray.png",
      description: "Menyemprot aquades.",
      category: "Pembersih",
    ),

    ToolItem(
      id: "petri",
      name: "Cawan Petri",
      image: "assets/tools/petri.png",
      description: "Tempat sementara bahan.",
      category: "Wadah",
    ),

    ToolItem(
      id: "balloon",
      name: "Balon",
      image: "assets/tools/balloon.png",
      description: "Mengamati gas hasil reaksi.",
      category: "Pendukung",
    ),

  ],

  materials: [

    MaterialItem(
      id: "water",
      name: "Aquades",
      formula: "H₂O",
      image: "assets/materials/water.png",
      description: "Pelarut utama.",
    ),

    MaterialItem(
      id: "nacl",
      name: "Natrium Klorida",
      formula: "NaCl",
      image: "assets/materials/nacl.png",
      description: "Sampel utama percobaan.",
    ),

    MaterialItem(
      id: "naoh",
      name: "Natrium Hidroksida",
      formula: "NaOH",
      image: "assets/materials/naoh.png",
      description: "Larutan basa.",
    ),

    MaterialItem(
      id: "hcl",
      name: "Asam Klorida",
      formula: "HCl",
      image: "assets/materials/hcl.png",
      description: "Larutan asam.",
    ),

    MaterialItem(
      id: "indicator",
      name: "Fenolftalein",
      formula: "PP",
      image: "assets/materials/indicator.png",
      description: "Indikator perubahan warna.",
    ),

  ],

  steps: [

    // STEP 1
    PracticumStep(
      step: 1,
      title: "Menyiapkan Gelas Kimia",
      instruction: "Letakkan Gelas Kimia pada area praktikum.",
      explanation:
          "Gelas kimia digunakan sebagai wadah utama pencampuran larutan.",
      hint: "Buka menu Alat lalu pilih Gelas Kimia.",
      successMessage: "Gelas Kimia berhasil ditempatkan.",
      requiredItemId: "beaker",
      itemType: ItemType.tool,
    ),

    // STEP 2
    PracticumStep(
      step: 2,
      title: "Menambahkan Aquades",
      instruction: "Masukkan Aquades ke dalam Gelas Kimia.",
      explanation:
          "Aquades digunakan sebagai pelarut sebelum penambahan zat.",
      hint: "Buka menu Bahan kemudian pilih Aquades.",
      successMessage: "Aquades berhasil ditambahkan.",
      requiredItemId: "water",
      itemType: ItemType.material,
    ),

    // STEP 3
    PracticumStep(
      step: 3,
      title: "Menimbang NaCl",
      instruction: "Tempatkan NaCl pada Neraca Digital.",
      explanation:
          "Penimbangan memastikan massa zat sesuai prosedur.",
      hint: "Gunakan Neraca Digital terlebih dahulu.",
      successMessage: "NaCl berhasil ditimbang.",
      requiredItemId: "balance",
      itemType: ItemType.tool,
    ),

    // STEP 4
    PracticumStep(
      step: 4,
      title: "Menambahkan NaCl",
      instruction: "Masukkan NaCl ke dalam Gelas Kimia.",
      explanation:
          "NaCl mulai dilarutkan sebelum proses berikutnya.",
      hint: "Pilih NaCl dari menu Bahan.",
      successMessage: "NaCl berhasil ditambahkan.",
      requiredItemId: "nacl",
      itemType: ItemType.material,
    ),

    // STEP 5
    PracticumStep(
      step: 5,
      title: "Mengaduk Larutan",
      instruction: "Gunakan Batang Pengaduk.",
      explanation:
          "Pengadukan mempercepat proses pelarutan.",
      hint: "Pilih Batang Pengaduk dari menu Alat.",
      successMessage: "Larutan berhasil diaduk.",
      requiredItemId: "stirrer",
      itemType: ItemType.tool,
    ),

    // STEP 6
    PracticumStep(
      step: 6,
      title: "Menambahkan NaOH",
      instruction: "Tambahkan NaOH menggunakan Pipet.",
      explanation:
          "NaOH merupakan larutan basa yang digunakan pada percobaan.",
      hint: "Gunakan Pipet Tetes.",
      successMessage: "NaOH berhasil ditambahkan.",
      requiredItemId: "naoh",
      itemType: ItemType.material,
    ),

    // STEP 7
    PracticumStep(
      step: 7,
      title: "Menambahkan HCl",
      instruction: "Tambahkan HCl ke dalam Gelas Kimia.",
      explanation:
          "HCl akan bereaksi dengan larutan sebelumnya.",
      hint: "Gunakan Pipet Tetes kembali.",
      successMessage: "HCl berhasil ditambahkan.",
      requiredItemId: "hcl",
      itemType: ItemType.material,
    ),

    // STEP 8
    PracticumStep(
      step: 8,
      title: "Mengamati Reaksi",
      instruction: "Amati perubahan yang terjadi.",
      explanation:
          "Perhatikan perubahan warna dan kecepatan reaksi.",
      hint: "Perhatikan objek AR pada kamera.",
      successMessage: "Pengamatan selesai.",
      requiredItemId: "indicator",
      itemType: ItemType.material,
    ),

    // STEP 9
    PracticumStep(
      step: 9,
      title: "Mencatat Hasil",
      instruction: "Catat hasil pengamatan.",
      explanation:
          "Semua hasil dicatat sebagai data praktikum.",
      hint: "Buka menu Data.",
      successMessage: "Data berhasil disimpan.",
      requiredItemId: "indicator",
      itemType: ItemType.material,
    ),

    // STEP 10
    PracticumStep(
      step: 10,
      title: "Praktikum Selesai",
      instruction: "Selesaikan praktikum.",
      explanation:
          "Semua langkah telah berhasil dilakukan.",
      hint: "Tekan tombol Finish.",
      successMessage: "Selamat! Praktikum selesai.",
      requiredItemId: "indicator",
      itemType: ItemType.material,
    ),

  ],
);