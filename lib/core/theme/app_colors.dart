import 'package:flutter/material.dart';

class AppColors {
  AppColors._();

  // Background & Surface
  static const Color background = Color(0xFF061A40);
  static const Color surface = Color(0xFF0C2454);

  // Primary & Accent / Secondary
  static const Color primary = Color(0xFF1976D2);
  static const Color accent = Color(0xFF4DD0E1);
  static const Color secondary = Color(0xFF4DD0E1);

  // Text
  static const Color white = Colors.white;
  static const Color white70 = Colors.white70;
  static const Color textPrimary = Colors.white;
  static const Color textSecondary = Colors.white70;

  // Status Indicators
  static const Color success = Color(0xFF22C55E);
  static const Color warning = Color(0xFFF59E0B);
  static const Color error = Color(0xFFEF4444);

  // Border & Divider
  static const Color border = Color(0x334DD0E1);

  // Card
  static Color card = Colors.white.withValues(alpha: 0.12);
  static Color cardPressed = primary.withValues(alpha: 0.35);
  static Color cardCircle = Colors.white.withValues(alpha: 0.12);
  static Color cardCirclePressed = Colors.white.withValues(alpha: 0.22);

  // Shadow
  static Color shadow = Colors.black.withValues(alpha: 0.20);
  static Color shadowPressed = accent.withValues(alpha: 0.35);
}