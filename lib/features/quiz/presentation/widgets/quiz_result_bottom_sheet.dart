import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../data/models/exercise/exercise_model.dart';
import '../../data/repositories/quiz_repository.dart';

/// Bottom Sheet untuk menampilkan skor kuis terakhir dengan 2 tombol aksi:
/// "Kerjakan Ulang" dan "Lihat Kunci Jawaban"
class QuizResultBottomSheet extends StatefulWidget {
  final int idCategory;
  final String categoryName;
  final VoidCallback onRetry;
  final VoidCallback onReview;

  const QuizResultBottomSheet({
    super.key,
    required this.idCategory,
    required this.categoryName,
    required this.onRetry,
    required this.onReview,
  });

  static Future<void> show({
    required BuildContext context,
    required int idCategory,
    required String categoryName,
    required VoidCallback onRetry,
    required VoidCallback onReview,
  }) {
    return showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (_) => QuizResultBottomSheet(
        idCategory: idCategory,
        categoryName: categoryName,
        onRetry: onRetry,
        onReview: onReview,
      ),
    );
  }

  @override
  State<QuizResultBottomSheet> createState() => _QuizResultBottomSheetState();
}

class _QuizResultBottomSheetState extends State<QuizResultBottomSheet> {
  late Future<QuizReportModel> _reportFuture;

  @override
  void initState() {
    super.initState();
    final repository = context.read<QuizRepository>();
    _reportFuture = repository.getQuizResult(widget.idCategory);
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.fromLTRB(24, 16, 24, 28),
      decoration: const BoxDecoration(
        color: Color(0xFF141C3D),
        borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
        border: Border(
          top: BorderSide(
            color: Color(0x334DD0E1),
            width: 1.5,
          ),
        ),
      ),
      child: SafeArea(
        top: false,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            // Handle bar
            Container(
              width: 44,
              height: 4,
              decoration: BoxDecoration(
                color: Colors.white24,
                borderRadius: BorderRadius.circular(2),
              ),
            ),
            const SizedBox(height: 18),

            // Header info
            Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        widget.categoryName,
                        style: const TextStyle(
                          color: Colors.white,
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      const SizedBox(height: 4),
                      Row(
                        children: [
                          const Icon(
                            Icons.check_circle_rounded,
                            color: Color(0xFF10B981),
                            size: 15,
                          ),
                          const SizedBox(width: 5),
                          Text(
                            'Kuis Telah Diselesaikan',
                            style: TextStyle(
                              color: const Color(0xFF10B981)
                                  .withValues(alpha: 0.9),
                              fontSize: 12,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
                IconButton(
                  onPressed: () => Navigator.pop(context),
                  icon: const Icon(Icons.close_rounded, color: Colors.white70),
                  splashRadius: 20,
                ),
              ],
            ),

            const SizedBox(height: 20),

            // Content FutureBuilder
            FutureBuilder<QuizReportModel>(
              future: _reportFuture,
              builder: (context, snapshot) {
                if (snapshot.connectionState == ConnectionState.waiting) {
                  return const Padding(
                    padding: EdgeInsets.symmetric(vertical: 40),
                    child: Center(
                      child: CircularProgressIndicator(
                        color: AppColors.accent,
                      ),
                    ),
                  );
                }

                if (snapshot.hasError) {
                  return Padding(
                    padding: const EdgeInsets.symmetric(vertical: 24),
                    child: Column(
                      children: [
                        const Icon(
                          Icons.error_outline_rounded,
                          color: Colors.amber,
                          size: 40,
                        ),
                        const SizedBox(height: 10),
                        Text(
                          'Gagal memuat rapor: ${snapshot.error}',
                          textAlign: TextAlign.center,
                          style: const TextStyle(color: Colors.white70, fontSize: 13),
                        ),
                      ],
                    ),
                  );
                }

                final report = snapshot.data!;
                final score = report.score != null
                    ? (report.score! % 1 == 0
                        ? report.score!.toInt().toString()
                        : report.score!.toStringAsFixed(1))
                    : '0';

                return Column(
                  children: [
                    // Lingkaran Skor
                    Container(
                      width: 100,
                      height: 100,
                      decoration: BoxDecoration(
                        shape: BoxShape.circle,
                        gradient: const LinearGradient(
                          colors: [
                            Color(0xFF3C4B9E),
                            Color(0xFF1E2766),
                          ],
                          begin: Alignment.topLeft,
                          end: Alignment.bottomRight,
                        ),
                        border: Border.all(
                          color: const Color(0xFF10B981).withValues(alpha: 0.6),
                          width: 2.5,
                        ),
                        boxShadow: [
                          BoxShadow(
                            color: const Color(0xFF10B981).withValues(alpha: 0.25),
                            blurRadius: 18,
                            spreadRadius: 2,
                          ),
                        ],
                      ),
                      alignment: Alignment.center,
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Text(
                            score,
                            style: const TextStyle(
                              color: Colors.white,
                              fontSize: 32,
                              fontWeight: FontWeight.bold,
                              height: 1,
                            ),
                          ),
                          const SizedBox(height: 2),
                          const Text(
                            'SKOR',
                            style: TextStyle(
                              color: AppColors.accent,
                              fontSize: 10,
                              fontWeight: FontWeight.bold,
                              letterSpacing: 1,
                            ),
                          ),
                        ],
                      ),
                    ),

                    const SizedBox(height: 20),

                    // Ringkasan Soal (Total, Benar, Salah)
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 16,
                        vertical: 14,
                      ),
                      decoration: BoxDecoration(
                        color: Colors.white.withValues(alpha: 0.05),
                        borderRadius: BorderRadius.circular(16),
                        border: Border.all(
                          color: Colors.white.withValues(alpha: 0.1),
                        ),
                      ),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.spaceAround,
                        children: [
                          _buildStatItem(
                            label: 'Total Soal',
                            value: '${report.totalQuestions}',
                            color: Colors.white,
                          ),
                          _buildVerticalDivider(),
                          _buildStatItem(
                            label: 'Benar',
                            value: '${report.correct}',
                            color: const Color(0xFF10B981),
                          ),
                          _buildVerticalDivider(),
                          _buildStatItem(
                            label: 'Salah',
                            value: '${report.wrong}',
                            color: const Color(0xFFEF4444),
                          ),
                        ],
                      ),
                    ),

                    const SizedBox(height: 24),

                    // 2 Action Buttons: Kerjakan Ulang & Lihat Kunci Jawaban
                    Row(
                      children: [
                        // Lihat Kunci Jawaban
                        Expanded(
                          child: OutlinedButton.icon(
                            onPressed: () {
                              Navigator.pop(context);
                              widget.onReview();
                            },
                            icon: const Icon(
                              Icons.menu_book_rounded,
                              size: 18,
                              color: AppColors.accent,
                            ),
                            label: const Text(
                              'Kunci Jawaban',
                              style: TextStyle(
                                fontSize: 13,
                                fontWeight: FontWeight.bold,
                                color: AppColors.accent,
                              ),
                            ),
                            style: OutlinedButton.styleFrom(
                              side: BorderSide(
                                color: AppColors.accent.withValues(alpha: 0.6),
                                width: 1.2,
                              ),
                              padding: const EdgeInsets.symmetric(vertical: 14),
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(14),
                              ),
                            ),
                          ),
                        ),

                        const SizedBox(width: 12),

                        // Kerjakan Ulang
                        Expanded(
                          child: ElevatedButton.icon(
                            onPressed: () {
                              Navigator.pop(context);
                              widget.onRetry();
                            },
                            icon: const Icon(
                              Icons.replay_rounded,
                              size: 18,
                              color: Colors.white,
                            ),
                            label: const Text(
                              'Kerjakan Ulang',
                              style: TextStyle(
                                fontSize: 13,
                                fontWeight: FontWeight.bold,
                                color: Colors.white,
                              ),
                            ),
                            style: ElevatedButton.styleFrom(
                              backgroundColor: const Color(0xFF10B981),
                              foregroundColor: Colors.white,
                              elevation: 4,
                              shadowColor: const Color(0xFF10B981).withValues(alpha: 0.4),
                              padding: const EdgeInsets.symmetric(vertical: 14),
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(14),
                              ),
                            ),
                          ),
                        ),
                      ],
                    ),
                  ],
                );
              },
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildStatItem({
    required String label,
    required String value,
    required Color color,
  }) {
    return Column(
      children: [
        Text(
          value,
          style: TextStyle(
            color: color,
            fontSize: 18,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 3),
        Text(
          label,
          style: TextStyle(
            color: Colors.white.withValues(alpha: 0.7),
            fontSize: 11,
          ),
        ),
      ],
    );
  }

  Widget _buildVerticalDivider() {
    return Container(
      width: 1,
      height: 28,
      color: Colors.white.withValues(alpha: 0.12),
    );
  }
}
