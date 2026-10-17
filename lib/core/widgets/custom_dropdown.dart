import 'package:flutter/material.dart';
import '../theme/app_colors.dart';

/// Dropdown selector bergaya Glassmorphism untuk jenjang/kelas
class CustomDropdown<T> extends StatelessWidget {
  final String label;
  final String hint;
  final T? value;
  final List<DropdownMenuItem<T>> items;
  final ValueChanged<T?>? onChanged;
  final String? errorText;
  final IconData? prefixIcon;
  final bool isLoading;

  const CustomDropdown({
    super.key,
    required this.label,
    required this.hint,
    required this.value,
    required this.items,
    required this.onChanged,
    this.errorText,
    this.prefixIcon,
    this.isLoading = false,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          label,
          style: const TextStyle(
            color: Colors.white,
            fontSize: 14,
            fontWeight: FontWeight.w600,
          ),
        ),
        const SizedBox(height: 8),
        Container(
          decoration: BoxDecoration(
            color: Colors.white.withValues(alpha: 0.08),
            borderRadius: BorderRadius.circular(16),
            border: Border.all(
              color: errorText != null
                  ? Colors.redAccent.withValues(alpha: 0.8)
                  : Colors.white.withValues(alpha: 0.15),
              width: 1.2,
            ),
          ),
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
            child: Row(
              children: [
                if (prefixIcon != null) ...[
                  Icon(
                    prefixIcon,
                    color: AppColors.accent.withValues(alpha: 0.8),
                    size: 22,
                  ),
                  const SizedBox(width: 12),
                ],
                Expanded(
                  child: isLoading
                      ? Row(
                          children: [
                            SizedBox(
                              width: 16,
                              height: 16,
                              child: CircularProgressIndicator(
                                strokeWidth: 2,
                                valueColor: AlwaysStoppedAnimation<Color>(
                                  AppColors.accent,
                                ),
                              ),
                            ),
                            const SizedBox(width: 12),
                            Text(
                              'Memuat data...',
                              style: TextStyle(
                                color: Colors.white.withValues(alpha: 0.5),
                                fontSize: 14,
                              ),
                            ),
                          ],
                        )
                      : DropdownButtonHideUnderline(
                          child: DropdownButton<T>(
                            value: value,
                            isExpanded: true,
                            hint: Text(
                              hint,
                              style: TextStyle(
                                color: Colors.white.withValues(alpha: 0.35),
                                fontSize: 14,
                              ),
                            ),
                            dropdownColor: const Color(0xFF0C2454),
                            icon: const Icon(
                              Icons.keyboard_arrow_down_rounded,
                              color: Colors.white70,
                            ),
                            style: const TextStyle(
                              color: Colors.white,
                              fontSize: 15,
                            ),
                            items: items,
                            onChanged: onChanged,
                          ),
                        ),
                ),
              ],
            ),
          ),
        ),
        if (errorText != null) ...[
          const SizedBox(height: 6),
          Padding(
            padding: const EdgeInsets.only(left: 6),
            child: Text(
              errorText!,
              style: const TextStyle(
                color: Colors.redAccent,
                fontSize: 12,
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
        ],
      ],
    );
  }
}
