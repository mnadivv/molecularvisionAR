import 'package:flutter/material.dart';

import '../../../../core/theme/app_colors.dart';

/// Indikator halaman geser Onboarding dengan animasi kapsul ekspansif dinamis
class OnboardingIndicator extends StatelessWidget {
  final int count;
  final int currentIndex;
  final Color? activeColor;

  const OnboardingIndicator({
    super.key,
    required this.count,
    required this.currentIndex,
    this.activeColor,
  });

  @override
  Widget build(BuildContext context) {
    final effectiveActiveColor = activeColor ?? AppColors.accent;

    return Row(
      mainAxisSize: MainAxisSize.min,
      children: List.generate(count, (index) {
        final isActive = index == currentIndex;

        return AnimatedContainer(
          duration: const Duration(milliseconds: 320),
          curve: Curves.easeInOutCubic,
          margin: const EdgeInsets.symmetric(horizontal: 4),
          height: 8,
          width: isActive ? 28 : 8,
          decoration: BoxDecoration(
            color: isActive
                ? effectiveActiveColor
                : Colors.white.withValues(alpha: 0.22),
            borderRadius: BorderRadius.circular(10),
            boxShadow: isActive
                ? [
                    BoxShadow(
                      color: effectiveActiveColor.withValues(alpha: 0.55),
                      blurRadius: 8,
                      offset: const Offset(0, 1),
                    ),
                  ]
                : null,
          ),
        );
      }),
    );
  }
}
