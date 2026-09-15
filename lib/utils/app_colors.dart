import 'package:flutter/material.dart';

class AppColors {
  // Background
  static const Color background = Color(0xFF061A40);

  // Primary
  static const Color primary = Color(0xFF1976D2);

  // Accent
  static const Color accent = Color(0xFF4DD0E1);

  // Text
  static const Color white = Colors.white;
  static const Color white70 = Colors.white70;

  // Card
  static Color card =
      Colors.white.withValues(alpha: 0.12);

  static Color cardPressed =
      primary.withValues(alpha: 0.35);

  static Color cardCircle =
      Colors.white.withValues(alpha: 0.12);

  static Color cardCirclePressed =
      Colors.white.withValues(alpha: 0.22);

  // Shadow
  static Color shadow =
      Colors.black.withValues(alpha: 0.20);

  static Color shadowPressed =
      accent.withValues(alpha: 0.35);
}