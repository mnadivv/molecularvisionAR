import 'package:flutter/material.dart';

class ChemistryTopic {
  final String title;
  final String subtitle;
  final IconData icon;
  final Color color;
  final String? practicumTitle;
  final String? unityScene;

  const ChemistryTopic({
    required this.title,
    required this.subtitle,
    required this.icon,
    required this.color,
    this.practicumTitle,
    this.unityScene,
  });
}

const List<ChemistryTopic> chemistryTopics = [

  ChemistryTopic(
    title: "Ikatan Kimia",
    subtitle: "Konsep dasar ikatan antar atom & visualisasi AR interaktif.",
    icon: Icons.link_rounded,
    color: Colors.blue,
    practicumTitle: "Praktikum 1 - Ikatan Kimia",
    unityScene: "praktikum1",
  ),

  ChemistryTopic(
    title: "Termokimia",
    subtitle: "Mempelajari perubahan kalor reaksi.",
    icon: Icons.local_fire_department_rounded,
    color: Colors.red,
    practicumTitle: "Praktikum 2 - Termokimia",
    unityScene: "praktikum2",
  ),

  ChemistryTopic(
    title: "Laju Reaksi",
    subtitle: "Faktor-faktor yang mempengaruhi laju reaksi.",
    icon: Icons.speed_rounded,
    color: Colors.green,
    practicumTitle: "Praktikum 3 - Laju Reaksi",
    unityScene: "praktikum3",
  ),

  ChemistryTopic(
    title: "Stokiometri",
    subtitle: "Perhitungan jumlah zat dalam reaksi kimia.",
    icon: Icons.calculate_rounded,
    color: Colors.orange,
    practicumTitle: "Praktikum 4 - Stokiometri",
    unityScene: "praktikum4",
  ),

  ChemistryTopic(
    title: "Elektrolisis dan Asam Basa",
    subtitle: "Reaksi elektrolisis serta konsep asam dan basa dalam larutan.",
    icon: Icons.science_rounded,
    color: Colors.purple,
    practicumTitle: "Praktikum 5 - Asam Basa",
    unityScene: "praktikum5",
  ),

  ChemistryTopic(
    title: "Hidrolisis Garam",
    subtitle: "Reaksi ion garam dengan air.",
    icon: Icons.water_drop_rounded,
    color: Colors.cyan,
    practicumTitle: "Praktikum 6 - Hidrolisis",
    unityScene: "praktikum6",
  ),

  ChemistryTopic(
    title: "Hasil Kali Kelarutan",
    subtitle: "Konsep Ksp dan kelarutan senyawa.",
    icon: Icons.grain_rounded,
    color: Colors.teal,
    practicumTitle: "Praktikum 7 - Hasil Kali Kelarutan",
    unityScene: "praktikum7",
  ),

  ChemistryTopic(
    title: "Kesetimbangan Kimia",
    subtitle: "Kesetimbangan reaksi dan faktor pergeseran.",
    icon: Icons.balance_rounded,
    color: Colors.indigo,
    practicumTitle: "Praktikum 8 - Kesetimbangan Kimia",
    unityScene: "praktikum8",
  ),

  ChemistryTopic(
    title: "Sel Volta",
    subtitle: "Konversi energi kimia menjadi listrik.",
    icon: Icons.battery_charging_full_rounded,
    color: Colors.amber,
    practicumTitle: "Praktikum 9 - Sel Volta",
    unityScene: "praktikum9",
  ),

  ChemistryTopic(
    title: "Hidrokarbon",
    subtitle: "Struktur dan reaksi senyawa karbon.",
    icon: Icons.hexagon_rounded,
    color: Colors.brown,
    practicumTitle: "Praktikum 10 - Hidrokarbon",
    unityScene: "praktikum10",
  ),

  ChemistryTopic(
    title: "Daur Ulang Polimer",
    subtitle: "Pemanfaatan kembali limbah polimer.",
    icon: Icons.recycling_rounded,
    color: Colors.lightGreen,
    practicumTitle: "Praktikum 11 - Daur Ulang Polimer",
    unityScene: "praktikum12",
  ),

];