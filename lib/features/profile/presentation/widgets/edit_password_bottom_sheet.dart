import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../core/widgets/custom_button.dart';
import '../../../../core/widgets/custom_text_field.dart';
import '../bloc/profile_bloc.dart';
import '../bloc/profile_event.dart';
import '../bloc/profile_state.dart';

/// Bottom Sheet untuk formulir pembaruan kata sandi siswa
class EditPasswordBottomSheet extends StatefulWidget {
  const EditPasswordBottomSheet({super.key});

  static Future<void> show(BuildContext context) {
    return showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (_) => BlocProvider.value(
        value: context.read<ProfileBloc>(),
        child: const EditPasswordBottomSheet(),
      ),
    );
  }

  @override
  State<EditPasswordBottomSheet> createState() => _EditPasswordBottomSheetState();
}

class _EditPasswordBottomSheetState extends State<EditPasswordBottomSheet> {
  final _formKey = GlobalKey<FormState>();
  final _currentPasswordController = TextEditingController();
  final _newPasswordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();

  bool _obscureCurrent = true;
  bool _obscureNew = true;
  bool _obscureConfirm = true;

  @override
  void dispose() {
    _currentPasswordController.dispose();
    _newPasswordController.dispose();
    _confirmPasswordController.dispose();
    super.dispose();
  }

  void _submitPassword() {
    if (_formKey.currentState?.validate() ?? false) {
      context.read<ProfileBloc>().add(
            PasswordUpdateSubmitted(
              currentPassword: _currentPasswordController.text,
              newPassword: _newPasswordController.text,
            ),
          );
    }
  }

  @override
  Widget build(BuildContext context) {
    final bottomInset = MediaQuery.of(context).viewInsets.bottom;

    return BlocConsumer<ProfileBloc, ProfileState>(
      listener: (context, state) {
        if (state is PasswordUpdateSuccess) {
          Navigator.of(context).pop();
        }
      },
      builder: (context, state) {
        final isLoading = state is PasswordUpdating;
        final validationErrors = state is ProfileFailure ? state.validationErrors : null;

        return Container(
          padding: EdgeInsets.fromLTRB(24, 20, 24, 24 + bottomInset),
          decoration: BoxDecoration(
            color: const Color(0xFF071B42),
            borderRadius: const BorderRadius.vertical(top: Radius.circular(28)),
            border: Border.all(
              color: Colors.white.withValues(alpha: 0.15),
              width: 1.2,
            ),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withValues(alpha: 0.5),
                blurRadius: 30,
                offset: const Offset(0, -10),
              ),
            ],
          ),
          child: SingleChildScrollView(
            child: Form(
              key: _formKey,
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Handle Bar
                  Center(
                    child: Container(
                      width: 44,
                      height: 4,
                      decoration: BoxDecoration(
                        color: Colors.white.withValues(alpha: 0.25),
                        borderRadius: BorderRadius.circular(2),
                      ),
                    ),
                  ),
                  const SizedBox(height: 18),

                  // Header Title & Close Button
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Row(
                        children: [
                          Icon(Icons.lock_reset_rounded, color: AppColors.accent, size: 26),
                          SizedBox(width: 10),
                          Text(
                            'Ubah Kata Sandi',
                            style: TextStyle(
                              color: Colors.white,
                              fontSize: 18,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ],
                      ),
                      IconButton(
                        icon: const Icon(Icons.close_rounded, color: Colors.white70),
                        onPressed: () => Navigator.of(context).pop(),
                      ),
                    ],
                  ),
                  const SizedBox(height: 6),
                  Text(
                    'Pastikan kata sandi baru Anda kuat dan mudah diingat.',
                    style: TextStyle(
                      color: Colors.white.withValues(alpha: 0.65),
                      fontSize: 13,
                    ),
                  ),
                  const SizedBox(height: 22),

                  // Field: Kata Sandi Saat Ini
                  CustomTextField(
                    controller: _currentPasswordController,
                    label: 'Kata Sandi Saat Ini',
                    hint: '••••••••',
                    prefixIcon: Icons.lock_outline_rounded,
                    obscureText: _obscureCurrent,
                    errorText: validationErrors?['current_password']?.firstOrNull,
                    suffixIcon: IconButton(
                      icon: Icon(
                        _obscureCurrent ? Icons.visibility_off_outlined : Icons.visibility_outlined,
                        color: Colors.grey.shade600,
                        size: 20,
                      ),
                      onPressed: () {
                        setState(() {
                          _obscureCurrent = !_obscureCurrent;
                        });
                      },
                    ),
                    validator: (val) {
                      if (val == null || val.isEmpty) {
                        return 'Kata sandi saat ini wajib diisi.';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 16),

                  // Field: Kata Sandi Baru
                  CustomTextField(
                    controller: _newPasswordController,
                    label: 'Kata Sandi Baru',
                    hint: 'Minimal 8 karakter',
                    prefixIcon: Icons.key_rounded,
                    obscureText: _obscureNew,
                    errorText: validationErrors?['password']?.firstOrNull,
                    suffixIcon: IconButton(
                      icon: Icon(
                        _obscureNew ? Icons.visibility_off_outlined : Icons.visibility_outlined,
                        color: Colors.grey.shade600,
                        size: 20,
                      ),
                      onPressed: () {
                        setState(() {
                          _obscureNew = !_obscureNew;
                        });
                      },
                    ),
                    validator: (val) {
                      if (val == null || val.isEmpty) {
                        return 'Kata sandi baru wajib diisi.';
                      }
                      if (val.length < 8) {
                        return 'Kata sandi minimal 8 karakter.';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 16),

                  // Field: Konfirmasi Kata Sandi Baru
                  CustomTextField(
                    controller: _confirmPasswordController,
                    label: 'Konfirmasi Kata Sandi Baru',
                    hint: 'Ketik ulang kata sandi baru',
                    prefixIcon: Icons.check_circle_outline_rounded,
                    obscureText: _obscureConfirm,
                    suffixIcon: IconButton(
                      icon: Icon(
                        _obscureConfirm ? Icons.visibility_off_outlined : Icons.visibility_outlined,
                        color: Colors.grey.shade600,
                        size: 20,
                      ),
                      onPressed: () {
                        setState(() {
                          _obscureConfirm = !_obscureConfirm;
                        });
                      },
                    ),
                    validator: (val) {
                      if (val == null || val.isEmpty) {
                        return 'Konfirmasi kata sandi wajib diisi.';
                      }
                      if (val != _newPasswordController.text) {
                        return 'Konfirmasi kata sandi tidak cocok.';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 26),

                  // Tombol Simpan Password
                  CustomButton(
                    text: 'PERBARUI KATA SANDI',
                    isLoading: isLoading,
                    icon: Icons.lock_open_rounded,
                    onPressed: _submitPassword,
                  ),
                ],
              ),
            ),
          ),
        );
      },
    );
  }
}
