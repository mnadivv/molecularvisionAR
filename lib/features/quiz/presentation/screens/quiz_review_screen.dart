import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../data/models/exercise/exercise_model.dart';
import '../../data/repositories/quiz_repository.dart';
import '../bloc/quiz_result/quiz_result_bloc.dart';
import '../bloc/quiz_result/quiz_result_event.dart';
import '../bloc/quiz_result/quiz_result_state.dart';

/// Halaman Kunci Jawaban & Pembahasan Kuis
class QuizReviewScreen extends StatelessWidget {
  final int idCategory;
  final String categoryName;

  const QuizReviewScreen({
    super.key,
    required this.idCategory,
    required this.categoryName,
  });

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) => QuizResultBloc(
        repository: context.read<QuizRepository>(),
      )..add(QuizResultFetchRequested(idCategory)),
      child: Scaffold(
        backgroundColor: AppColors.background,
        body: Stack(
          children: [
            // Background cosmic decoration
            Positioned(
              top: -60,
              left: -60,
              child: Opacity(
                opacity: 0.08,
                child: Image.asset(
                  'assets/images/bg_molecule.png',
                  width: 220,
                ),
              ),
            ),
            Positioned(
              bottom: -60,
              right: -60,
              child: Opacity(
                opacity: 0.08,
                child: Transform.rotate(
                  angle: 0.5,
                  child: Image.asset(
                    'assets/images/bg_molecule.png',
                    width: 240,
                  ),
                ),
              ),
            ),

            SafeArea(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Custom AppBar
                  Padding(
                    padding: const EdgeInsets.fromLTRB(14, 10, 20, 10),
                    child: Row(
                      children: [
                        IconButton(
                          onPressed: () => Navigator.of(context).pop(),
                          icon: const Icon(
                            Icons.arrow_back_ios_new_rounded,
                            color: Colors.white,
                            size: 20,
                          ),
                          splashRadius: 22,
                        ),
                        const SizedBox(width: 8),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              const Text(
                                'Kunci Jawaban & Hasil',
                                style: TextStyle(
                                  color: Colors.white,
                                  fontSize: 18,
                                  fontWeight: FontWeight.bold,
                                  letterSpacing: 0.2,
                                ),
                              ),
                              Text(
                                categoryName,
                                style: TextStyle(
                                  color: Colors.white.withValues(alpha: 0.7),
                                  fontSize: 12,
                                ),
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis,
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),

                  // Review content
                  Expanded(
                    child: BlocBuilder<QuizResultBloc, QuizResultState>(
                      builder: (context, state) {
                        if (state is QuizResultLoading) {
                          return const Center(
                            child: CircularProgressIndicator(
                              color: AppColors.accent,
                            ),
                          );
                        }

                        if (state is QuizResultFailure) {
                          return Center(
                            child: Padding(
                              padding: const EdgeInsets.all(24),
                              child: Column(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: [
                                  const Icon(
                                    Icons.error_outline_rounded,
                                    color: Colors.amber,
                                    size: 54,
                                  ),
                                  const SizedBox(height: 14),
                                  Text(
                                    state.message,
                                    textAlign: TextAlign.center,
                                    style: const TextStyle(
                                      color: Colors.white70,
                                      fontSize: 14,
                                    ),
                                  ),
                                  const SizedBox(height: 18),
                                  ElevatedButton(
                                    onPressed: () => context
                                        .read<QuizResultBloc>()
                                        .add(QuizResultFetchRequested(idCategory)),
                                    style: ElevatedButton.styleFrom(
                                      backgroundColor: AppColors.accent,
                                      foregroundColor: const Color(0xFF0F172A),
                                    ),
                                    child: const Text('Coba Lagi'),
                                  ),
                                ],
                              ),
                            ),
                          );
                        }

                        if (state is QuizResultLoaded) {
                          final report = state.report;
                          return _buildReviewList(context, report);
                        }

                        return const SizedBox.shrink();
                      },
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildReviewList(BuildContext context, QuizReportModel report) {
    final scoreStr = report.score != null
        ? (report.score! % 1 == 0
            ? report.score!.toInt().toString()
            : report.score!.toStringAsFixed(1))
        : '0';

    return ListView(
      padding: const EdgeInsets.fromLTRB(20, 10, 20, 24),
      children: [
        // Summary Header Card
        Container(
          padding: const EdgeInsets.all(18),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(22),
            gradient: const LinearGradient(
              begin: Alignment.topLeft,
              end: Alignment.bottomRight,
              colors: [
                Color(0xFF3C4B9E),
                Color(0xFF1E2766),
              ],
            ),
            border: Border.all(
              color: Colors.white.withValues(alpha: 0.18),
              width: 1.2,
            ),
          ),
          child: Row(
            children: [
              Container(
                width: 72,
                height: 72,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: Colors.white.withValues(alpha: 0.1),
                  border: Border.all(
                    color: const Color(0xFF10B981),
                    width: 2,
                  ),
                ),
                alignment: Alignment.center,
                child: Text(
                  scoreStr,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 26,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'Rapor Hasil Latihan',
                      style: TextStyle(
                        color: Colors.white,
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 6),
                    Text(
                      '${report.correct} dari ${report.totalQuestions} soal benar',
                      style: const TextStyle(
                        color: Color(0xFF10B981),
                        fontSize: 13,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    const SizedBox(height: 2),
                    Text(
                      '${report.wrong} soal salah atau belum dijawab',
                      style: TextStyle(
                        color: Colors.white.withValues(alpha: 0.7),
                        fontSize: 12,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),

        const SizedBox(height: 22),

        const Text(
          'Pembahasan Butir Soal',
          style: TextStyle(
            color: Colors.white,
            fontSize: 17,
            fontWeight: FontWeight.bold,
            letterSpacing: 0.2,
          ),
        ),

        const SizedBox(height: 12),

        ...report.reviews.asMap().entries.map((entry) {
          final index = entry.key;
          final item = entry.value;
          return _buildReviewCard(index + 1, item);
        }),
      ],
    );
  }

  Widget _buildReviewCard(int number, QuizReviewItemModel item) {
    final isCorrect = item.isCorrect;
    final isAnswered = item.isAnswered;

    String typeLabel = 'Pilihan Ganda';
    switch (item.questionType) {
      case 'multiple_select':
        typeLabel = 'Pilihan Jamak';
        break;
      case 'true_false':
        typeLabel = 'Benar / Salah';
        break;
      case 'short_answer':
        typeLabel = 'Isian Singkat';
        break;
      case 'matching':
        typeLabel = 'Menjodohkan';
        break;
      default:
        typeLabel = 'Pilihan Ganda';
    }

    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: const Color(0xFF1E2766).withValues(alpha: 0.7),
        borderRadius: BorderRadius.circular(18),
        border: Border.all(
          color: isCorrect
              ? const Color(0xFF10B981).withValues(alpha: 0.4)
              : const Color(0xFFEF4444).withValues(alpha: 0.4),
          width: 1.2,
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Header: Soal #, Type, Status Pill
          Row(
            children: [
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Text(
                  'Soal $number',
                  style: const TextStyle(
                    color: AppColors.accent,
                    fontSize: 12,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
              const SizedBox(width: 8),
              Text(
                typeLabel,
                style: TextStyle(
                  color: Colors.white.withValues(alpha: 0.6),
                  fontSize: 11.5,
                ),
              ),
              const Spacer(),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(
                  color: isCorrect
                      ? const Color(0xFF10B981).withValues(alpha: 0.2)
                      : const Color(0xFFEF4444).withValues(alpha: 0.2),
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(
                    color: isCorrect
                        ? const Color(0xFF10B981).withValues(alpha: 0.6)
                        : const Color(0xFFEF4444).withValues(alpha: 0.6),
                  ),
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(
                      isCorrect
                          ? Icons.check_circle_rounded
                          : Icons.cancel_rounded,
                      size: 14,
                      color: isCorrect
                          ? const Color(0xFF10B981)
                          : const Color(0xFFEF4444),
                    ),
                    const SizedBox(width: 4),
                    Text(
                      isCorrect ? 'Benar' : 'Salah',
                      style: TextStyle(
                        color: isCorrect
                            ? const Color(0xFF10B981)
                            : const Color(0xFFEF4444),
                        fontSize: 11,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),

          const SizedBox(height: 12),

          // Question Text
          Text(
            item.questionName,
            style: const TextStyle(
              color: Colors.white,
              fontSize: 14.5,
              fontWeight: FontWeight.w600,
              height: 1.4,
            ),
          ),

          const SizedBox(height: 14),

          // User Answer Box
          Container(
            width: double.infinity,
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
            decoration: BoxDecoration(
              color: isCorrect
                  ? const Color(0xFF10B981).withValues(alpha: 0.1)
                  : const Color(0xFFEF4444).withValues(alpha: 0.1),
              borderRadius: BorderRadius.circular(10),
              border: Border.all(
                color: isCorrect
                    ? const Color(0xFF10B981).withValues(alpha: 0.3)
                    : const Color(0xFFEF4444).withValues(alpha: 0.3),
              ),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Jawaban Kamu:',
                  style: TextStyle(
                    color: Colors.white.withValues(alpha: 0.6),
                    fontSize: 11,
                  ),
                ),
                const SizedBox(height: 2),
                Text(
                  isAnswered && item.userAnswer != null && item.userAnswer!.isNotEmpty
                      ? item.userAnswer!
                      : '(Tidak dijawab)',
                  style: TextStyle(
                    color: isCorrect
                        ? const Color(0xFF10B981)
                        : const Color(0xFFEF4444),
                    fontSize: 13,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ],
            ),
          ),

          // Correct Answer Box (if user was wrong or always show)
          if (!isCorrect && item.correctAnswer != null) ...[
            const SizedBox(height: 8),
            Container(
              width: double.infinity,
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
              decoration: BoxDecoration(
                color: const Color(0xFF10B981).withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(10),
                border: Border.all(
                  color: const Color(0xFF10B981).withValues(alpha: 0.4),
                ),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Kunci Jawaban Benar:',
                    style: TextStyle(
                      color: Colors.white.withValues(alpha: 0.6),
                      fontSize: 11,
                    ),
                  ),
                  const SizedBox(height: 2),
                  Text(
                    item.correctAnswer!,
                    style: const TextStyle(
                      color: Color(0xFF10B981),
                      fontSize: 13,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ],
      ),
    );
  }
}
