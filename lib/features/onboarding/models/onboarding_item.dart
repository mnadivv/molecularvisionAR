import 'package:flutter/material.dart';

/// Model data item untuk setiap slide pada halaman Onboarding
class OnboardingItem {
  final String title;
  final String subtitle;
  final IconData icon;
  final Color accentColor;
  final List<String> tags;
  final String badge;

  const OnboardingItem({
    required this.title,
    required this.subtitle,
    required this.icon,
    required this.accentColor,
    required this.tags,
    required this.badge,
  });

  /// Daftar slide resmi overview fitur unggulan Molecular Vision AR
  static const List<OnboardingItem> defaultItems = [
    OnboardingItem(
      badge: 'Visualisasi 3D',
      title: 'Eksplorasi Molekul Kimia 3D Interaktif',
      subtitle:
          'Pelajari geometri molekul VSEPR, sudut ikatan, dan visualisasi ball-and-stick 360° secara mendalam langsung di genggaman Anda.',
      icon: Icons.bubble_chart_rounded,
      accentColor: Color(0xFF4DD0E1), // Cyan Neon
      tags: ['#Molekul3D', '#GeometriVSEPR', '#Interaktif'],
    ),
    OnboardingItem(
      badge: 'Augmented Reality',
      title: 'Laboratorium Virtual Berbasis AR',
      subtitle:
          'Rasakan pengalaman praktikum kimia masa depan. Arahkan kamera ponsel ke kartu marker untuk memunculkan model molekul nyata di dunia nyata.',
      icon: Icons.view_in_ar_rounded,
      accentColor: Color(0xFF29B6F6), // Vivid Blue
      tags: ['#VuforiaAR', '#KameraRealTime', '#PraktikumVirtual'],
    ),
    OnboardingItem(
      badge: 'Materi Lengkap',
      title: 'Materi Pembelajaran Kimia Terstruktur',
      subtitle:
          'Akses rangkuman konsep teori kimia lengkap dari ikatan kimia hingga termokimia, dilengkapi video pembelajaran animasi terintegrasi.',
      icon: Icons.auto_stories_rounded,
      accentColor: Color(0xFFFFB300), // Amber Warm
      tags: ['#MateriKimia', '#VideoEdukasi', '#KurikulumMerdeka'],
    ),
    OnboardingItem(
      badge: 'Evaluasi & Progres',
      title: 'Latihan Soal & Pelacakan Capaian Belajar',
      subtitle:
          'Uji pemahaman Anda dengan beragam variasi soal kuis interaktif dan pantau grafik kenaikan progres belajar secara real-time setiap hari.',
      icon: Icons.emoji_events_rounded,
      accentColor: Color(0xFF10B981), // Emerald Green
      tags: ['#LatihanSoal', '#SmartProgress', '#EvaluasiHasil'],
    ),
  ];
}
