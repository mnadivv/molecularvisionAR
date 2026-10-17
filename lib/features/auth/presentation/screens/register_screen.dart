import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../core/widgets/custom_button.dart';
import '../../../../core/widgets/custom_dropdown.dart';
import '../../../../core/widgets/custom_text_field.dart';
import '../../../../data/models/auth/degree_model.dart';
import '../../../home/presentation/screens/home_screen.dart';
import '../../data/repositories/auth_repository.dart';
import '../bloc/auth_bloc.dart';
import '../bloc/auth_event.dart';
import '../bloc/auth_state.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _formKey = GlobalKey<FormState>();

  final _nameController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _schoolController = TextEditingController();

  int? _selectedDegreeId;
  List<DegreeModel> _degrees = [];
  bool _isLoadingDegrees = true;
  bool _obscurePassword = true;

  @override
  void initState() {
    super.initState();
    _fetchDegrees();
  }

  Future<void> _fetchDegrees() async {
    try {
      final repository = RepositoryProvider.of<AuthRepository>(
        context,
        listen: false,
      );
      final degrees = await repository.getDegrees();
      if (mounted) {
        setState(() {
          _degrees = degrees;
          _isLoadingDegrees = false;
        });
      }
    } catch (_) {
      if (mounted) {
        setState(() {
          _isLoadingDegrees = false;
        });
      }
    }
  }

  @override
  void dispose() {
    _nameController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    _schoolController.dispose();
    super.dispose();
  }

  void _submitRegister() {
    if (_formKey.currentState?.validate() ?? false) {
      context.read<AuthBloc>().add(
        AuthRegisterSubmitted(
          name: _nameController.text,
          email: _emailController.text,
          password: _passwordController.text,
          school: _schoolController.text,
          idDegree: _selectedDegreeId,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        foregroundColor: Colors.white,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back_ios_new_rounded),
          onPressed: () => Navigator.of(context).pop(),
        ),
      ),
      body: BlocConsumer<AuthBloc, AuthState>(
        listener: (context, state) {
          if (state is Authenticated) {
            Navigator.of(context).pushAndRemoveUntil(
              MaterialPageRoute(builder: (_) => const HomeScreen()),
              (route) => false,
            );
          } else if (state is AuthFailure) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Row(
                  children: [
                    const Icon(
                      Icons.error_outline_rounded,
                      color: Colors.white,
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Text(
                        state.message,
                        style: const TextStyle(fontWeight: FontWeight.w500),
                      ),
                    ),
                  ],
                ),
                backgroundColor: Colors.redAccent.shade700,
                behavior: SnackBarBehavior.floating,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
                margin: const EdgeInsets.all(16),
              ),
            );
          }
        },
        builder: (context, state) {
          final isLoading = state is AuthLoading;
          final validationErrors = state is AuthFailure
              ? state.validationErrors
              : null;

          return Stack(
            children: [
              // Ambient Decorative Glow
              Positioned(
                top: -80,
                left: -80,
                child: Container(
                  width: 250,
                  height: 250,
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    color: AppColors.accent.withValues(alpha: 0.10),
                  ),
                ),
              ),

              // Content
              SafeArea(
                child: SingleChildScrollView(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 24,
                    vertical: 10,
                  ),
                  physics: const BouncingScrollPhysics(),
                  child: Form(
                    key: _formKey,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // Title
                        const Text(
                          'Daftar Akun Siswa',
                          style: TextStyle(
                            color: Colors.white,
                            fontSize: 26,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: 6),
                        Text(
                          'Mulai eksplorasi materi dan praktikum kimia interaktif 3D AR.',
                          style: TextStyle(
                            color: Colors.white.withValues(alpha: 0.65),
                            fontSize: 13,
                          ),
                        ),
                        const SizedBox(height: 28),

                        // Form Card
                        Container(
                          padding: const EdgeInsets.all(22),
                          decoration: BoxDecoration(
                            color: Colors.white.withValues(alpha: 0.06),
                            borderRadius: BorderRadius.circular(24),
                            border: Border.all(
                              color: Colors.white.withValues(alpha: 0.12),
                              width: 1.2,
                            ),
                          ),
                          child: Column(
                            children: [
                              CustomTextField(
                                controller: _nameController,
                                label: 'Nama Lengkap',
                                hint: 'Masukkan Nama Lengkap',
                                prefixIcon: Icons.person_outline_rounded,
                                errorText:
                                    validationErrors?['name']?.firstOrNull,
                                validator: (val) {
                                  if (val == null || val.trim().isEmpty) {
                                    return 'Nama lengkap wajib diisi.';
                                  }
                                  return null;
                                },
                              ),
                              const SizedBox(height: 18),
                              CustomTextField(
                                controller: _emailController,
                                label: 'Email',
                                hint: 'Masukkan Email',
                                prefixIcon: Icons.email_outlined,
                                keyboardType: TextInputType.emailAddress,
                                errorText:
                                    validationErrors?['email']?.firstOrNull,
                                validator: (val) {
                                  if (val == null || val.trim().isEmpty) {
                                    return 'Email wajib diisi.';
                                  }
                                  if (!val.contains('@')) {
                                    return 'Format email tidak valid.';
                                  }
                                  return null;
                                },
                              ),
                              const SizedBox(height: 18),
                              CustomTextField(
                                controller: _passwordController,
                                label: 'Kata Sandi',
                                hint: 'Masukkan Kata Sandi',
                                prefixIcon: Icons.lock_outline_rounded,
                                obscureText: _obscurePassword,
                                errorText:
                                    validationErrors?['password']?.firstOrNull,
                                suffixIcon: IconButton(
                                  icon: Icon(
                                    _obscurePassword
                                        ? Icons.visibility_off_outlined
                                        : Icons.visibility_outlined,
                                    color: Colors.white70,
                                    size: 20,
                                  ),
                                  onPressed: () {
                                    setState(() {
                                      _obscurePassword = !_obscurePassword;
                                    });
                                  },
                                ),
                                validator: (val) {
                                  if (val == null || val.length < 8) {
                                    return 'Kata sandi minimal 8 karakter.';
                                  }
                                  return null;
                                },
                              ),
                              const SizedBox(height: 18),
                              CustomTextField(
                                controller: _schoolController,
                                label: 'Asal Sekolah (Opsional)',
                                hint: 'Masukkan Asal Sekolah',
                                prefixIcon: Icons.school_outlined,
                                errorText:
                                    validationErrors?['school']?.firstOrNull,
                              ),
                              const SizedBox(height: 18),
                              CustomDropdown<int>(
                                label: 'Jenjang / Kelas',
                                hint: 'Pilih Kelas',
                                value: _selectedDegreeId,
                                prefixIcon: Icons.class_outlined,
                                isLoading: _isLoadingDegrees,
                                errorText:
                                    validationErrors?['id_degree']?.firstOrNull,
                                items: _degrees.map((d) {
                                  return DropdownMenuItem<int>(
                                    value: d.idDegree,
                                    child: Text(d.name),
                                  );
                                }).toList(),
                                onChanged: (val) {
                                  setState(() {
                                    _selectedDegreeId = val;
                                  });
                                },
                              ),
                              const SizedBox(height: 28),
                              CustomButton(
                                text: 'BUAT AKUN ',
                                isLoading: isLoading,
                                icon: Icons.person_add_alt_1_rounded,
                                onPressed: _submitRegister,
                              ),
                            ],
                          ),
                        ),

                        const SizedBox(height: 24),

                        // Back to Login
                        Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Text(
                              'Sudah memiliki akun? ',
                              style: TextStyle(
                                color: Colors.white.withValues(alpha: 0.7),
                                fontSize: 14,
                              ),
                            ),
                            GestureDetector(
                              onTap: () => Navigator.of(context).pop(),
                              child: const Text(
                                'Masuk di Sini',
                                style: TextStyle(
                                  color: AppColors.accent,
                                  fontSize: 14,
                                  fontWeight: FontWeight.bold,
                                  decoration: TextDecoration.underline,
                                  decorationColor: AppColors.accent,
                                ),
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 30),
                      ],
                    ),
                  ),
                ),
              ),
            ],
          );
        },
      ),
    );
  }
}
