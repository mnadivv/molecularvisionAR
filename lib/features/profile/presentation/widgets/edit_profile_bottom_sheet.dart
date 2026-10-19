import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../core/widgets/custom_button.dart';
import '../../../../core/widgets/custom_dropdown.dart';
import '../../../../core/widgets/custom_text_field.dart';
import '../../../../data/models/auth/degree_model.dart';
import '../../../../data/models/auth/user_model.dart';
import '../bloc/profile_bloc.dart';
import '../bloc/profile_event.dart';
import '../bloc/profile_state.dart';

/// Bottom Sheet untuk formulir pembaruan data profil siswa
class EditProfileBottomSheet extends StatefulWidget {
  final UserModel user;
  final List<DegreeModel> degrees;

  const EditProfileBottomSheet({
    super.key,
    required this.user,
    required this.degrees,
  });

  static Future<void> show(
    BuildContext context, {
    required UserModel user,
    required List<DegreeModel> degrees,
  }) {
    return showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (_) => BlocProvider.value(
        value: context.read<ProfileBloc>(),
        child: EditProfileBottomSheet(user: user, degrees: degrees),
      ),
    );
  }

  @override
  State<EditProfileBottomSheet> createState() => _EditProfileBottomSheetState();
}

class _EditProfileBottomSheetState extends State<EditProfileBottomSheet> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _nameController;
  late final TextEditingController _emailController;
  late final TextEditingController _schoolController;
  int? _selectedDegreeId;

  @override
  void initState() {
    super.initState();
    _nameController = TextEditingController(text: widget.user.name);
    _emailController = TextEditingController(text: widget.user.email);
    _schoolController = TextEditingController(text: widget.user.school ?? '');
    _selectedDegreeId = widget.user.idDegree;
  }

  @override
  void dispose() {
    _nameController.dispose();
    _emailController.dispose();
    _schoolController.dispose();
    super.dispose();
  }

  void _submitUpdate() {
    if (_formKey.currentState?.validate() ?? false) {
      context.read<ProfileBloc>().add(
            ProfileUpdateSubmitted(
              name: _nameController.text,
              email: _emailController.text,
              school: _schoolController.text,
              idDegree: _selectedDegreeId,
            ),
          );
    }
  }

  @override
  Widget build(BuildContext context) {
    final bottomInset = MediaQuery.of(context).viewInsets.bottom;

    return BlocConsumer<ProfileBloc, ProfileState>(
      listener: (context, state) {
        if (state is ProfileUpdateSuccess) {
          Navigator.of(context).pop();
        }
      },
      builder: (context, state) {
        final isLoading = state is ProfileUpdating;
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
                          Icon(Icons.edit_note_rounded, color: AppColors.accent, size: 26),
                          SizedBox(width: 10),
                          Text(
                            'Edit Profil Siswa',
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
                    'Perbarui data diri untuk kelengkapan akun belajar kimia.',
                    style: TextStyle(
                      color: Colors.white.withValues(alpha: 0.65),
                      fontSize: 13,
                    ),
                  ),
                  const SizedBox(height: 22),

                  // Field: Nama Lengkap
                  CustomTextField(
                    controller: _nameController,
                    label: 'Nama Lengkap',
                    hint: 'Nama lengkap Anda',
                    prefixIcon: Icons.person_outline_rounded,
                    errorText: validationErrors?['name']?.firstOrNull,
                    validator: (val) {
                      if (val == null || val.trim().isEmpty) {
                        return 'Nama wajib diisi.';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 16),

                  // Field: Email
                  CustomTextField(
                    controller: _emailController,
                    label: 'Email Siswa',
                    hint: 'nama@sekolah.sch.id',
                    prefixIcon: Icons.email_outlined,
                    keyboardType: TextInputType.emailAddress,
                    errorText: validationErrors?['email']?.firstOrNull,
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
                  const SizedBox(height: 16),

                  // Field: Asal Sekolah
                  CustomTextField(
                    controller: _schoolController,
                    label: 'Asal Sekolah',
                    hint: 'Contoh: SMAN 1 Jakarta',
                    prefixIcon: Icons.school_outlined,
                    errorText: validationErrors?['school']?.firstOrNull,
                  ),
                  const SizedBox(height: 16),

                  // Field: Dropdown Jenjang
                  CustomDropdown<int>(
                    label: 'Jenjang / Kelas',
                    hint: 'Pilih jenjang kelas',
                    value: _selectedDegreeId,
                    prefixIcon: Icons.class_outlined,
                    items: widget.degrees
                        .map(
                          (d) => DropdownMenuItem<int>(
                            value: d.idDegree,
                            child: Text(d.name),
                          ),
                        )
                        .toList(),
                    onChanged: (val) {
                      setState(() {
                        _selectedDegreeId = val;
                      });
                    },
                  ),
                  const SizedBox(height: 26),

                  // Tombol Simpan
                  CustomButton(
                    text: 'SIMPAN PERUBAHAN',
                    isLoading: isLoading,
                    icon: Icons.check_circle_outline_rounded,
                    onPressed: _submitUpdate,
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
