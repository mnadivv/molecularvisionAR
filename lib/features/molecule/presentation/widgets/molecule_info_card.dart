import 'package:flutter/material.dart';
import 'package:flutter_widget_from_html/flutter_widget_from_html.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../data/models/molecule/molecule_model.dart';

/// Card informasi detail spesifikasi molekul dan penjelasan teori kimia
class MoleculeInfoCard extends StatelessWidget {
  final MoleculeModel molecule;
  final bool isCompleted;
  final VoidCallback onMarkCompleted;

  const MoleculeInfoCard({
    super.key,
    required this.molecule,
    this.isCompleted = false,
    required this.onMarkCompleted,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: const Color(0xFF0C2454).withValues(alpha: 0.90),
        borderRadius: const BorderRadius.vertical(top: Radius.circular(32)),
        border: Border.all(
          color: Colors.white.withValues(alpha: 0.15),
          width: 1.2,
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.4),
            blurRadius: 24,
            offset: const Offset(0, -8),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Top Handle Bar
          Center(
            child: Container(
              margin: const EdgeInsets.only(top: 12, bottom: 16),
              width: 44,
              height: 4,
              decoration: BoxDecoration(
                color: Colors.white.withValues(alpha: 0.25),
                borderRadius: BorderRadius.circular(2),
              ),
            ),
          ),

          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 22),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Header Formula Badge & Name
                Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 16,
                        vertical: 8,
                      ),
                      decoration: BoxDecoration(
                        gradient: const LinearGradient(
                          colors: [Color(0xFF38438A), Color(0xFF252F6B)],
                        ),
                        borderRadius: BorderRadius.circular(14),
                        border: Border.all(
                          color: AppColors.accent.withValues(alpha: 0.5),
                          width: 1.2,
                        ),
                        boxShadow: [
                          BoxShadow(
                            color: AppColors.primary.withValues(alpha: 0.3),
                            blurRadius: 10,
                            offset: const Offset(0, 3),
                          ),
                        ],
                      ),
                      child: Text(
                        molecule.formula,
                        style: const TextStyle(
                          color: Colors.white,
                          fontSize: 20,
                          fontWeight: FontWeight.w800,
                          letterSpacing: 0.5,
                        ),
                      ),
                    ),
                    const SizedBox(width: 14),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            molecule.name,
                            style: const TextStyle(
                              color: Colors.white,
                              fontSize: 18,
                              fontWeight: FontWeight.bold,
                            ),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                          const SizedBox(height: 2),
                          Text(
                            'Katalog Molekul 3D Kimia',
                            style: TextStyle(
                              color: Colors.white.withValues(alpha: 0.60),
                              fontSize: 12,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),

                const SizedBox(height: 20),

                // Grid 4 Atribut Karakteristik Molekul
                Row(
                  children: [
                    Expanded(
                      child: _AttributeTile(
                        icon: Icons.category_outlined,
                        label: 'Bentuk Geometri',
                        value: molecule.shape ?? '-',
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: _AttributeTile(
                        icon: Icons.architecture_rounded,
                        label: 'Sudut Ikatan',
                        value: molecule.bent ?? '-',
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 12),
                Row(
                  children: [
                    Expanded(
                      child: _AttributeTile(
                        icon: Icons.hub_outlined,
                        label: 'Jenis Ikatan',
                        value: molecule.bondType ?? '-',
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: _AttributeTile(
                        icon: Icons.science_outlined,
                        label: 'Rumus Kimia',
                        value: molecule.formula,
                      ),
                    ),
                  ],
                ),

                const SizedBox(height: 22),

                // Penjelasan & Teori Kimia
                const Row(
                  children: [
                    Icon(
                      Icons.menu_book_rounded,
                      color: AppColors.accent,
                      size: 20,
                    ),
                    SizedBox(width: 8),
                    Text(
                      'Penjelasan Teori Kimia',
                      style: TextStyle(
                        color: Colors.white,
                        fontSize: 15,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 10),

                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: Colors.white.withValues(alpha: 0.05),
                    borderRadius: BorderRadius.circular(16),
                    border: Border.all(
                      color: Colors.white.withValues(alpha: 0.08),
                    ),
                  ),
                  child: (molecule.description != null &&
                          molecule.description!.trim().isNotEmpty)
                      ? HtmlWidget(
                          molecule.description!,
                          textStyle: TextStyle(
                            color: Colors.white.withValues(alpha: 0.85),
                            fontSize: 13.5,
                            height: 1.5,
                          ),
                        )
                      : Text(
                          'Molekul ${molecule.name} (${molecule.formula}) merupakan senyawa kimia dengan karakteristik bentuk geometri ${molecule.shape ?? 'khas'} dan ikatan ${molecule.bondType ?? 'kimia'}.',
                          style: TextStyle(
                            color: Colors.white.withValues(alpha: 0.85),
                            fontSize: 13.5,
                            height: 1.5,
                          ),
                        ),
                ),

                const SizedBox(height: 24),

                // Tombol Progres Belajar
                if (isCompleted)
                  Container(
                    width: double.infinity,
                    height: 52,
                    decoration: BoxDecoration(
                      color: const Color(0xFF10B981).withValues(alpha: 0.15),
                      borderRadius: BorderRadius.circular(16),
                      border: Border.all(
                        color: const Color(0xFF10B981).withValues(alpha: 0.45),
                        width: 1.5,
                      ),
                    ),
                    child: const Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(
                          Icons.check_circle_rounded,
                          color: Color(0xFF10B981),
                          size: 20,
                        ),
                        SizedBox(width: 8),
                        Text(
                          'SUDAH DIPELAJARI',
                          style: TextStyle(
                            color: Color(0xFF10B981),
                            fontSize: 14,
                            fontWeight: FontWeight.bold,
                            letterSpacing: 0.5,
                          ),
                        ),
                      ],
                    ),
                  )
                else
                  SizedBox(
                    width: double.infinity,
                    height: 52,
                    child: ElevatedButton.icon(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: const Color(0xFF10B981),
                        foregroundColor: Colors.white,
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(16),
                        ),
                        elevation: 2,
                      ),
                      onPressed: onMarkCompleted,
                      icon: const Icon(
                        Icons.check_circle_outline_rounded,
                        size: 20,
                      ),
                      label: const Text(
                        'TANDAI SELESAI DIPELAJARI',
                        style: TextStyle(
                          fontSize: 14,
                          fontWeight: FontWeight.bold,
                          letterSpacing: 0.5,
                        ),
                      ),
                    ),
                  ),

                const SizedBox(height: 24),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _AttributeTile extends StatelessWidget {
  final IconData icon;
  final String label;
  final String value;

  const _AttributeTile({
    required this.icon,
    required this.label,
    required this.value,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
      decoration: BoxDecoration(
        color: Colors.white.withValues(alpha: 0.06),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: Colors.white.withValues(alpha: 0.08),
        ),
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              color: AppColors.accent.withValues(alpha: 0.14),
            ),
            child: Icon(
              icon,
              size: 16,
              color: AppColors.accent,
            ),
          ),
          const SizedBox(width: 10),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  label,
                  style: TextStyle(
                    color: Colors.white.withValues(alpha: 0.55),
                    fontSize: 11,
                    fontWeight: FontWeight.w500,
                  ),
                ),
                const SizedBox(height: 2),
                Text(
                  value,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 13,
                    fontWeight: FontWeight.w700,
                  ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
