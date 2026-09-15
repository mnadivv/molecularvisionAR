import 'package:flutter/material.dart';
import 'app_colors.dart';

class AppTextStyle {
  static const TextStyle title = TextStyle(
    color: AppColors.white,
    fontSize: 30,
    fontWeight: FontWeight.bold,
    letterSpacing: .5,
  );

  static TextStyle subtitle = TextStyle(
    color: AppColors.white.withValues(alpha: .75),
    fontSize: 15,
    height: 1.5,
  );

  static const TextStyle cardTitle = TextStyle(
    color: AppColors.white,
    fontSize: 18,
    fontWeight: FontWeight.w600,
  );

  static const TextStyle button = TextStyle(
    color: AppColors.white,
    fontSize: 16,
    fontWeight: FontWeight.bold,
  );

  static TextStyle small = TextStyle(
    color: AppColors.white70,
    fontSize: 13,
  );
}