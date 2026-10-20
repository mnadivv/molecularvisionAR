import 'package:flutter/material.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../data/models/progress/progress_model.dart';

/// Card ringkasan progres belajar siswa di Beranda (Materi, Molekul, dan Latihan Soal)
class HomeProgressCard extends StatelessWidget {
  final ProgressModel? progress;
  final bool isLoading;
  final VoidCallback? onRetry;

  const HomeProgressCard({
    super.key,
    required this.progress,
    this.isLoading = false,
    this.onRetry,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(22),
        gradient: const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFF38438A), Color(0xFF252F6B)],
        ),
        border: Border.all(
          color: Colors.white.withValues(alpha: 0.18),
          width: 1.2,
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.25),
            blurRadius: 16,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Header Row
          Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'Progres Belajar Kamu',
                      style: TextStyle(
                        color: Colors.white,
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                        letterSpacing: 0.2,
                      ),
                    ),
                    const SizedBox(height: 2),
                    Text(
                      'Pantau penyelesaian materi, molekul, dan kuis',
                      style: TextStyle(
                        color: Colors.white.withValues(alpha: 0.7),
                        fontSize: 11.5,
                      ),
                    ),
                  ],
                ),
              ),
              if (isLoading)
                const SizedBox(
                  width: 16,
                  height: 16,
                  child: CircularProgressIndicator(
                    strokeWidth: 2,
                    color: AppColors.accent,
                  ),
                ),
            ],
          ),

          const SizedBox(height: 20),

          // Divider halus
          Container(height: 1, color: Colors.white.withValues(alpha: 0.1)),

          const SizedBox(height: 18),

          // 1. Progres Materi
          _buildProgressItem(
            icon: Icons.menu_book_rounded,
            iconColor: const Color(0xFF60A5FA), // Light Blue
            title: 'Materi Pembelajaran',
            detailText: progress != null
                ? '${progress!.materials.completed}/${progress!.materials.total} materi'
                : '0/0 materi',
            percentage: progress?.materials.percentage ?? 0.0,
            progressFraction: progress?.materials.progressFraction ?? 0.0,
            barColor: const Color(0xFF10B981), // Emerald Green
          ),

          const SizedBox(height: 16),

          // 2. Progres Molekul
          _buildProgressItem(
            icon: Icons.science_rounded,
            iconColor: const Color(0xFFA78BFA), // Light Purple
            title: 'Daftar Molekul 3D',
            detailText: progress != null
                ? '${progress!.molecules.completed}/${progress!.molecules.total} molekul'
                : '0/0 molekul',
            percentage: progress?.molecules.percentage ?? 0.0,
            progressFraction: progress?.molecules.progressFraction ?? 0.0,
            barColor: AppColors.accent, // Cyan
          ),

          const SizedBox(height: 16),

          // 3. Progres Latihan Soal / Kuis
          _buildProgressItem(
            icon: Icons.quiz_rounded,
            iconColor: const Color(0xFFFBBF24), // Amber
            title: 'Latihan Soal (Kuis)',
            detailText: progress != null
                ? '${progress!.quizzes.completed}/${progress!.quizzes.total} kuis'
                : '0/0 kuis',
            percentage: progress?.quizzes.percentage ?? 0.0,
            progressFraction: progress?.quizzes.progressFraction ?? 0.0,
            barColor: const Color(0xFF34D399), // Mint Green
          ),
        ],
      ),
    );
  }

  Widget _buildProgressItem({
    required IconData icon,
    required Color iconColor,
    required String title,
    required String detailText,
    required double percentage,
    required double progressFraction,
    required Color barColor,
  }) {
    final percentStr = percentage % 1 == 0
        ? percentage.toInt().toString()
        : percentage.toStringAsFixed(1);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Icon(icon, size: 16, color: iconColor),
            const SizedBox(width: 8),
            Text(
              title,
              style: const TextStyle(
                color: Colors.white,
                fontSize: 13,
                fontWeight: FontWeight.w600,
              ),
            ),
            const Spacer(),
            // Detail count: contoh "11/15 materi"
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
              decoration: BoxDecoration(
                color: Colors.white.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Text(
                detailText,
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 11.5,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
            const SizedBox(width: 8),
            // Persentase
            SizedBox(
              width: 38,
              child: Text(
                '$percentStr%',
                textAlign: TextAlign.end,
                style: TextStyle(
                  color: barColor,
                  fontSize: 12,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
          ],
        ),

        const SizedBox(height: 8),

        // Linear Progress Bar
        ClipRRect(
          borderRadius: BorderRadius.circular(10),
          child: LinearProgressIndicator(
            value: progressFraction,
            minHeight: 7,
            backgroundColor: Colors.white.withValues(alpha: 0.12),
            valueColor: AlwaysStoppedAnimation(barColor),
          ),
        ),
      ],
    );
  }
}
