import 'package:flutter/material.dart';

/// Text field bergaya Material White Card sesuai tema search bar materi pembelajaran
class CustomTextField extends StatelessWidget {
  final TextEditingController controller;
  final String label;
  final String hint;
  final IconData? prefixIcon;
  final Widget? suffixIcon;
  final bool obscureText;
  final TextInputType keyboardType;
  final String? errorText;
  final String? Function(String?)? validator;
  final void Function(String)? onChanged;
  final bool enabled;

  const CustomTextField({
    super.key,
    required this.controller,
    required this.label,
    required this.hint,
    this.prefixIcon,
    this.suffixIcon,
    this.obscureText = false,
    this.keyboardType = TextInputType.text,
    this.errorText,
    this.validator,
    this.onChanged,
    this.enabled = true,
  });

  @override
  Widget build(BuildContext context) {
    return FormField<String>(
      initialValue: controller.text,
      validator: validator != null ? (_) => validator!(controller.text) : null,
      enabled: enabled,
      builder: (FormFieldState<String> field) {
        final effectiveError = (errorText != null && errorText!.isNotEmpty)
            ? errorText
            : field.errorText;
        final hasError = effectiveError != null && effectiveError.isNotEmpty;

        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              label,
              style: const TextStyle(
                color: Colors.white,
                fontSize: 13.5,
                fontWeight: FontWeight.w600,
                letterSpacing: 0.2,
              ),
            ),
            const SizedBox(height: 8),
            Container(
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(16),
                border: Border.all(
                  color: hasError ? Colors.redAccent : Colors.transparent,
                  width: hasError ? 1.4 : 0.0,
                ),
                boxShadow: [
                  BoxShadow(
                    color: hasError
                        ? Colors.redAccent.withValues(alpha: 0.15)
                        : Colors.black.withValues(alpha: 0.12),
                    blurRadius: 10,
                    offset: const Offset(0, 4),
                  ),
                ],
              ),
              child: TextField(
                controller: controller,
                obscureText: obscureText,
                keyboardType: keyboardType,
                enabled: enabled,
                style: const TextStyle(
                  color: Color(0xFF1E293B),
                  fontSize: 14.5,
                  fontWeight: FontWeight.w500,
                ),
                cursorColor: const Color(0xFF1E293B),
                onChanged: (val) {
                  field.didChange(val);
                  if (field.hasError) {
                    field.validate();
                  }
                  onChanged?.call(val);
                },
                decoration: InputDecoration(
                  hintText: hint,
                  hintStyle: TextStyle(
                    color: Colors.grey.shade500,
                    fontSize: 14,
                  ),
                  prefixIcon: prefixIcon != null
                      ? Icon(
                          prefixIcon,
                          color: hasError
                              ? Colors.redAccent
                              : Colors.grey.shade600,
                          size: 20,
                        )
                      : null,
                  suffixIcon: suffixIcon,
                  border: InputBorder.none,
                  enabledBorder: InputBorder.none,
                  focusedBorder: InputBorder.none,
                  disabledBorder: InputBorder.none,
                  contentPadding: const EdgeInsets.symmetric(
                    horizontal: 16,
                    vertical: 14,
                  ),
                ),
              ),
            ),
            if (hasError) ...[
              const SizedBox(height: 6),
              Padding(
                padding: const EdgeInsets.only(left: 6),
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.center,
                  children: [
                    const Icon(
                      Icons.error_outline_rounded,
                      color: Colors.redAccent,
                      size: 15,
                    ),
                    const SizedBox(width: 6),
                    Expanded(
                      child: Text(
                        effectiveError,
                        style: const TextStyle(
                          color: Colors.redAccent,
                          fontSize: 12,
                          fontWeight: FontWeight.w500,
                          height: 1.2,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ],
        );
      },
    );
  }
}
