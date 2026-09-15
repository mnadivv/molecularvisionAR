import 'package:flutter/material.dart';

class ChemistryTopic {
  final String title;
  final String subtitle;
  final IconData icon;
  final Color color;

  const ChemistryTopic({
    required this.title,
    required this.subtitle,
    required this.icon,
    required this.color,
  });
}

const List<ChemistryTopic> chemistryTopics = [

  ChemistryTopic(
    title: "Ikatan Kimia",
    subtitle: "Konsep dasar ikatan antar atom.",
    icon: Icons.link_rounded,
    color: Colors.blue,
  ),

  ChemistryTopic(
    title: "Termokimia",
    subtitle: "Mempelajari perubahan kalor reaksi.",
    icon: Icons.local_fire_department_rounded,
    color: Colors.red,
  ),

  ChemistryTopic(
    title: "Kalorimetri",
    subtitle: "Perhitungan kalor menggunakan kalorimeter.",
    icon: Icons.science_rounded,
    color: Colors.orange,
  ),

  ChemistryTopic(
    title: "Laju Reaksi",
    subtitle: "Faktor-faktor yang mempengaruhi laju reaksi.",
    icon: Icons.speed_rounded,
    color: Colors.green,
  ),

  ChemistryTopic(
    title: "Asam Basa",
    subtitle: "Teori serta sifat larutan asam dan basa.",
    icon: Icons.opacity_rounded,
    color: Colors.purple,
  ),

  ChemistryTopic(
    title: "Hidrolisis",
    subtitle: "Reaksi ion garam dengan air.",
    icon: Icons.water_drop_rounded,
    color: Colors.cyan,
  ),

  ChemistryTopic(
    title: "Hasil Kali Kelarutan",
    subtitle: "Konsep Ksp dan kelarutan senyawa.",
    icon: Icons.grain_rounded,
    color: Colors.teal,
  ),

  ChemistryTopic(
    title: "Kesetimbangan Kimia",
    subtitle: "Kesetimbangan reaksi dan faktor pergeseran.",
    icon: Icons.balance_rounded,
    color: Colors.indigo,
  ),

  ChemistryTopic(
    title: "Sel Volta",
    subtitle: "Konversi energi kimia menjadi listrik.",
    icon: Icons.battery_charging_full_rounded,
    color: Colors.amber,
  ),

  ChemistryTopic(
    title: "Elektrolisis",
    subtitle: "Konversi energi listrik menjadi energi kimia.",
    icon: Icons.electric_bolt_rounded,
    color: Colors.deepOrange,
  ),

  ChemistryTopic(
    title: "Hidrokarbon",
    subtitle: "Struktur dan reaksi senyawa karbon.",
    icon: Icons.hexagon_rounded,
    color: Colors.brown,
  ),

  ChemistryTopic(
    title: "Daur Ulang Polimer",
    subtitle: "Pemanfaatan kembali limbah polimer.",
    icon: Icons.recycling_rounded,
    color: Colors.lightGreen,
  ),
];