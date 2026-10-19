import 'package:flutter/material.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../data/models/auth/user_model.dart';

/// Kartu visual profil siswa bergaya Glassmorphism dengan avatar gradien dan badge informasi
class ProfileInfoCard extends StatelessWidget {
  final UserModel user;

  const ProfileInfoCard({super.key, required this.user});

  @override
  Widget build(BuildContext context) {
    final degreeName = user.degree?.name ?? 'Belum ditentukan';
    final roleName = user.role?.name ?? 'Siswa';

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(22),
      decoration: BoxDecoration(
        color: Colors.white.withValues(alpha: 0.06),
        borderRadius: BorderRadius.circular(24),
        border: Border.all(
          color: Colors.white.withValues(alpha: 0.12),
          width: 1.2,
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.25),
            blurRadius: 20,
            offset: const Offset(0, 10),
          ),
        ],
      ),
      child: Column(
        children: [
          Container(
            width: 86,
            height: 86,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              color: AppColors.primary,
            ),
            child: Center(
              child: const Icon(
                Icons.person_rounded,
                color: AppColors.accent,
                size: 40,
              ),
            ),
          ),
          const SizedBox(height: 16),

          // Nama Siswa
          Text(
            user.name,
            textAlign: TextAlign.center,
            style: const TextStyle(
              color: Colors.white,
              fontSize: 20,
              fontWeight: FontWeight.bold,
              letterSpacing: 0.5,
            ),
          ),
          const SizedBox(height: 6),

          // Email Siswa
          Text(
            user.email,
            textAlign: TextAlign.center,
            style: TextStyle(
              color: Colors.white.withValues(alpha: 0.7),
              fontSize: 14,
            ),
          ),
          const SizedBox(height: 16),

          // Badges: Role & Jenjang
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              _buildBadge(
                label: roleName.toUpperCase(),
                icon: Icons.verified_user_rounded,
                color: AppColors.accent,
              ),
              const SizedBox(width: 10),
              _buildBadge(
                label: degreeName,
                icon: Icons.school_rounded,
                color: Colors.amber,
              ),
            ],
          ),
          const SizedBox(height: 24),

          Divider(color: Colors.white.withValues(alpha: 0.1)),
          const SizedBox(height: 16),

          // Data Fields
          _buildInfoRow(
            icon: Icons.person_outline_rounded,
            title: 'Nama Lengkap',
            value: user.name,
          ),
          const SizedBox(height: 14),
          _buildInfoRow(
            icon: Icons.email_outlined,
            title: 'Email Terdaftar',
            value: user.email,
          ),
          const SizedBox(height: 14),
          _buildInfoRow(
            icon: Icons.location_city_rounded,
            title: 'Asal Sekolah',
            value: user.school?.isNotEmpty == true
                ? user.school!
                : 'Belum diisi',
          ),
          const SizedBox(height: 14),
          _buildInfoRow(
            icon: Icons.class_outlined,
            title: 'Jenjang / Kelas',
            value: degreeName,
          ),
        ],
      ),
    );
  }

  Widget _buildBadge({
    required String label,
    required IconData icon,
    required Color color,
  }) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.14),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: color.withValues(alpha: 0.35), width: 1),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, color: color, size: 14),
          const SizedBox(width: 6),
          Text(
            label,
            style: TextStyle(
              color: color,
              fontSize: 12,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildInfoRow({
    required IconData icon,
    required String title,
    required String value,
  }) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Container(
          padding: const EdgeInsets.all(8),
          decoration: BoxDecoration(
            color: Colors.white.withValues(alpha: 0.05),
            borderRadius: BorderRadius.circular(10),
          ),
          child: Icon(icon, color: AppColors.accent, size: 18),
        ),
        const SizedBox(width: 14),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                title,
                style: TextStyle(
                  color: Colors.white.withValues(alpha: 0.5),
                  fontSize: 12,
                ),
              ),
              const SizedBox(height: 2),
              Text(
                value,
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 14,
                  fontWeight: FontWeight.w500,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
