import 'package:flutter/material.dart';

import '../utils/app_colors.dart';

class CustomLoadingBar extends StatelessWidget {
  final double progress;

  const CustomLoadingBar({
    super.key,
    required this.progress,
  });

  @override
  Widget build(BuildContext context) {
    return ClipRRect(
      borderRadius: BorderRadius.circular(20),
      child: Container(
        width: 250,
        height: 10,
        color: Colors.white.withValues(alpha: .15),
        child: Align(
          alignment: Alignment.centerLeft,
          child: AnimatedContainer(
            duration: const Duration(milliseconds: 300),
            width: 250 * progress,
            decoration: BoxDecoration(
              color: AppColors.accent,
              borderRadius: BorderRadius.circular(20),
            ),
          ),
        ),
      ),
    );
  }
}