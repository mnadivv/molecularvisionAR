import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../data/models/exercise/exercise_model.dart';
import '../../data/repositories/quiz_repository.dart';
import '../bloc/quiz_play/quiz_play_bloc.dart';
import '../bloc/quiz_play/quiz_play_event.dart';
import '../bloc/quiz_play/quiz_play_state.dart';
import 'quiz_review_screen.dart';

/// Halaman Pengerjaan Kuis Interaktif (Mendukung 5 Format Soal & Desain Cosmic Navy)
class QuizScreen extends StatelessWidget {
  final int idCategory;
  final String categoryName;

  const QuizScreen({
    super.key,
    required this.idCategory,
    required this.categoryName,
  });

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) => QuizPlayBloc(
        repository: context.read<QuizRepository>(),
      )..add(QuizPlayStarted(idCategory)),
      child: _QuizPlayView(
        idCategory: idCategory,
        categoryName: categoryName,
      ),
    );
  }
}

class _QuizPlayView extends StatelessWidget {
  final int idCategory;
  final String categoryName;

  const _QuizPlayView({
    required this.idCategory,
    required this.categoryName,
  });

  void _showSubmitConfirmation(
    BuildContext context,
    QuizPlayInProgress state,
  ) {
    final answered = state.answers.length;
    final total = state.questions.length;
    final unanswered = total - answered;

    showDialog(
      context: context,
      builder: (dialogCtx) => AlertDialog(
        backgroundColor: const Color(0xFF141C3D),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(20),
          side: BorderSide(
            color: Colors.white.withValues(alpha: 0.15),
          ),
        ),
        title: const Text(
          'Kirim Jawaban?',
          style: TextStyle(
            color: Colors.white,
            fontWeight: FontWeight.bold,
            fontSize: 18,
          ),
        ),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Kamu telah menjawab $answered dari $total soal.',
              style: const TextStyle(
                color: Colors.white,
                fontSize: 14,
              ),
            ),
            if (unanswered > 0) ...[
              const SizedBox(height: 8),
              Text(
                'Ada $unanswered soal yang belum kamu jawab. Soal yang belum dijawab akan dihitung salah.',
                style: const TextStyle(
                  color: Colors.amberAccent,
                  fontSize: 12.5,
                  height: 1.4,
                ),
              ),
            ],
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(dialogCtx),
            child: const Text(
              'Periksa Lagi',
              style: TextStyle(color: Colors.white70),
            ),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(dialogCtx);
              context.read<QuizPlayBloc>().add(const QuizSubmitRequested());
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: const Color(0xFF10B981),
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
              ),
            ),
            child: const Text('Kirim Sekarang'),
          ),
        ],
      ),
    );
  }

  void _showResultDialog(
    BuildContext context,
    QuizPlaySubmitted state,
  ) {
    final summary = state.response.summary;
    final scoreStr = summary.score % 1 == 0
        ? summary.score.toInt().toString()
        : summary.score.toStringAsFixed(1);

    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (dialogCtx) => PopScope(
        canPop: false,
        child: AlertDialog(
          backgroundColor: const Color(0xFF141C3D),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(24),
            side: const BorderSide(
              color: Color(0x334DD0E1),
              width: 1.5,
            ),
          ),
          title: const Center(
            child: Text(
              'Hasil Kuis',
              style: TextStyle(
                color: Colors.white,
                fontWeight: FontWeight.bold,
                fontSize: 20,
              ),
            ),
          ),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              // Circle Score
              Container(
                width: 104,
                height: 104,
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
                    color: const Color(0xFF10B981),
                    width: 2.5,
                  ),
                  boxShadow: [
                    BoxShadow(
                      color: const Color(0xFF10B981).withValues(alpha: 0.3),
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
                      scoreStr,
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 34,
                        fontWeight: FontWeight.bold,
                        height: 1,
                      ),
                    ),
                    const SizedBox(height: 2),
                    const Text(
                      'NILAI',
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

              // Detail Statistics
              Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 14,
                  vertical: 12,
                ),
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.05),
                  borderRadius: BorderRadius.circular(14),
                ),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceAround,
                  children: [
                    _buildStatCol('Total', '${summary.totalQuestions}', Colors.white),
                    Container(width: 1, height: 24, color: Colors.white24),
                    _buildStatCol('Benar', '${summary.correct}', const Color(0xFF10B981)),
                    Container(width: 1, height: 24, color: Colors.white24),
                    _buildStatCol('Salah', '${summary.wrong}', const Color(0xFFEF4444)),
                  ],
                ),
              ),

              const SizedBox(height: 14),

              Text(
                summary.score >= 70
                    ? 'Luar biasa! Pemahaman kamu sangat baik!'
                    : 'Terus berlatih untuk meningkatkan pemahamanmu.',
                textAlign: TextAlign.center,
                style: TextStyle(
                  color: Colors.white.withValues(alpha: 0.8),
                  fontSize: 12.5,
                ),
              ),
            ],
          ),
          actionsAlignment: MainAxisAlignment.center,
          actions: [
            Column(
              children: [
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton.icon(
                    onPressed: () {
                      Navigator.pop(dialogCtx); // Close dialog
                      Navigator.pushReplacement(
                        context,
                        MaterialPageRoute(
                          builder: (_) => QuizReviewScreen(
                            idCategory: idCategory,
                            categoryName: categoryName,
                          ),
                        ),
                      );
                    },
                    icon: const Icon(Icons.menu_book_rounded, size: 18),
                    label: const Text('Lihat Kunci Jawaban'),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppColors.accent,
                      foregroundColor: const Color(0xFF0F172A),
                      padding: const EdgeInsets.symmetric(vertical: 12),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(14),
                      ),
                    ),
                  ),
                ),
                const SizedBox(height: 8),
                SizedBox(
                  width: double.infinity,
                  child: OutlinedButton(
                    onPressed: () {
                      Navigator.pop(dialogCtx);
                      Navigator.pop(context); // Back to catalog
                    },
                    style: OutlinedButton.styleFrom(
                      side: BorderSide(
                        color: Colors.white.withValues(alpha: 0.3),
                      ),
                      padding: const EdgeInsets.symmetric(vertical: 12),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(14),
                      ),
                    ),
                    child: const Text(
                      'Kembali ke Katalog',
                      style: TextStyle(color: Colors.white70),
                    ),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildStatCol(String label, String value, Color color) {
    return Column(
      children: [
        Text(
          value,
          style: TextStyle(
            color: color,
            fontSize: 16,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 2),
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

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: Stack(
        children: [
          // Background Cosmic decoration
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
            child: BlocConsumer<QuizPlayBloc, QuizPlayState>(
              listener: (context, state) {
                if (state is QuizPlayFailure) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(
                      content: Text(state.message),
                      backgroundColor: Colors.redAccent.shade700,
                      behavior: SnackBarBehavior.floating,
                    ),
                  );
                } else if (state is QuizPlaySubmitted) {
                  _showResultDialog(context, state);
                }
              },
              builder: (context, state) {
                if (state is QuizPlayLoading) {
                  return const Center(
                    child: CircularProgressIndicator(
                      color: AppColors.accent,
                    ),
                  );
                }

                if (state is QuizPlayFailure && state is! QuizPlayInProgress) {
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
                                .read<QuizPlayBloc>()
                                .add(QuizPlayStarted(idCategory)),
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

                if (state is QuizPlayInProgress) {
                  if (state.questions.isEmpty) {
                    return _buildEmptyQuestions(context);
                  }
                  return _buildPlayContent(context, state);
                }

                return const SizedBox.shrink();
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildEmptyQuestions(BuildContext context) {
    return Column(
      children: [
        _buildAppBar(context, '0 Soal'),
        Expanded(
          child: Center(
            child: Padding(
              padding: const EdgeInsets.all(30),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  const Icon(
                    Icons.menu_book_rounded,
                    size: 56,
                    color: AppColors.accent,
                  ),
                  const SizedBox(height: 18),
                  const Text(
                    'Soal Belum Tersedia',
                    textAlign: TextAlign.center,
                    style: TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                      color: Colors.white,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    'Bank soal untuk kategori $categoryName sedang dikembangkan.',
                    textAlign: TextAlign.center,
                    style: TextStyle(
                      color: Colors.white.withValues(alpha: 0.7),
                      fontSize: 13,
                      height: 1.4,
                    ),
                  ),
                  const SizedBox(height: 22),
                  ElevatedButton(
                    onPressed: () => Navigator.pop(context),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppColors.accent,
                      foregroundColor: const Color(0xFF0F172A),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(14),
                      ),
                    ),
                    child: const Text('Kembali ke Katalog'),
                  ),
                ],
              ),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildAppBar(BuildContext context, String subtitle) {
    return Padding(
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
                Text(
                  categoryName,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 18,
                    fontWeight: FontWeight.bold,
                    letterSpacing: 0.2,
                  ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
                Text(
                  subtitle,
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
    );
  }

  Widget _buildPlayContent(BuildContext context, QuizPlayInProgress state) {
    final question = state.currentQuestion;
    final currentIndex = state.currentIndex;
    final total = state.questions.length;
    final progress = (currentIndex + 1) / total;

    return Column(
      children: [
        // App Bar
        _buildAppBar(context, 'Soal ${currentIndex + 1} dari $total'),

        // Progress Indicator
        Container(
          margin: const EdgeInsets.symmetric(horizontal: 20, vertical: 4),
          child: ClipRRect(
            borderRadius: BorderRadius.circular(10),
            child: LinearProgressIndicator(
              value: progress,
              minHeight: 6,
              backgroundColor: Colors.white.withValues(alpha: 0.1),
              valueColor: const AlwaysStoppedAnimation(Color(0xFF10B981)),
            ),
          ),
        ),

        // Scrollable Question Content
        Expanded(
          child: SingleChildScrollView(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 24),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Question Type Badge
                _buildTypeBadge(question.questionType),

                const SizedBox(height: 14),

                // Question Text
                Text(
                  question.questionText,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 17,
                    fontWeight: FontWeight.bold,
                    height: 1.45,
                  ),
                ),

                const SizedBox(height: 24),

                // Dynamic Input based on Question Type
                _buildQuestionInput(context, state, question),
              ],
            ),
          ),
        ),

        // Bottom Navigation Bar
        _buildBottomNavigation(context, state),
      ],
    );
  }

  Widget _buildTypeBadge(String type) {
    String label;
    IconData icon;

    switch (type) {
      case 'multiple_select':
        label = 'Pilihan Jamak (Centang Lebih dari 1)';
        icon = Icons.checklist_rounded;
        break;
      case 'true_false':
        label = 'Benar / Salah (Pernyataan)';
        icon = Icons.rule_rounded;
        break;
      case 'short_answer':
        label = 'Isian Singkat (Angka Saja)';
        icon = Icons.edit_note_rounded;
        break;
      case 'matching':
        label = 'Menjodohkan / Memasangkan';
        icon = Icons.swap_horiz_rounded;
        break;
      case 'multiple_choice':
      default:
        label = 'Pilihan Ganda (1 Jawaban Benar)';
        icon = Icons.radio_button_checked_rounded;
        break;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: AppColors.accent.withValues(alpha: 0.15),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(
          color: AppColors.accent.withValues(alpha: 0.4),
        ),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 16, color: AppColors.accent),
          const SizedBox(width: 6),
          Text(
            label,
            style: const TextStyle(
              color: AppColors.accent,
              fontSize: 11.5,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildQuestionInput(
    BuildContext context,
    QuizPlayInProgress state,
    ExerciseQuestionModel question,
  ) {
    switch (question.questionType) {
      case 'multiple_select':
        return _buildMultipleSelectInput(context, state, question);
      case 'true_false':
        return _buildTrueFalseInput(context, state, question);
      case 'short_answer':
        return _buildShortAnswerInput(context, state, question);
      case 'matching':
        return _buildMatchingInput(context, state, question);
      case 'multiple_choice':
      default:
        return _buildMultipleChoiceInput(context, state, question);
    }
  }

  /// 1. Pilihan Ganda (1 Jawaban Benar)
  Widget _buildMultipleChoiceInput(
    BuildContext context,
    QuizPlayInProgress state,
    ExerciseQuestionModel question,
  ) {
    final currentAnswer = state.answers[question.idQuestion];

    return Column(
      children: question.options.map((opt) {
        final isSelected = currentAnswer == opt.idOption ||
            currentAnswer == opt.idOption.toString() ||
            currentAnswer == opt.optionText;

        return Padding(
          padding: const EdgeInsets.only(bottom: 12),
          child: InkWell(
            onTap: () {
              context.read<QuizPlayBloc>().add(
                    QuizAnswerUpdated(
                      idQuestion: question.idQuestion,
                      answer: opt.idOption,
                    ),
                  );
            },
            borderRadius: BorderRadius.circular(16),
            child: AnimatedContainer(
              duration: const Duration(milliseconds: 180),
              width: double.infinity,
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: isSelected
                    ? const Color(0xFF10B981).withValues(alpha: 0.15)
                    : const Color(0xFF1E2766).withValues(alpha: 0.5),
                borderRadius: BorderRadius.circular(16),
                border: Border.all(
                  color: isSelected
                      ? const Color(0xFF10B981)
                      : Colors.white.withValues(alpha: 0.15),
                  width: isSelected ? 1.8 : 1,
                ),
              ),
              child: Row(
                children: [
                  Container(
                    width: 22,
                    height: 22,
                    decoration: BoxDecoration(
                      shape: BoxShape.circle,
                      border: Border.all(
                        color: isSelected
                            ? const Color(0xFF10B981)
                            : Colors.white54,
                        width: 2,
                      ),
                    ),
                    child: isSelected
                        ? const Center(
                            child: Icon(
                              Icons.circle,
                              size: 11,
                              color: Color(0xFF10B981),
                            ),
                          )
                        : null,
                  ),
                  const SizedBox(width: 14),
                  Expanded(
                    child: Text(
                      opt.optionText,
                      style: TextStyle(
                        color: isSelected ? Colors.white : Colors.white70,
                        fontSize: 14.5,
                        fontWeight:
                            isSelected ? FontWeight.bold : FontWeight.normal,
                        height: 1.4,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        );
      }).toList(),
    );
  }

  /// 2. Pilihan Jamak (Centang Lebih dari 1)
  Widget _buildMultipleSelectInput(
    BuildContext context,
    QuizPlayInProgress state,
    ExerciseQuestionModel question,
  ) {
    final currentList = (state.answers[question.idQuestion] as List?)
            ?.map((e) => int.tryParse(e.toString()) ?? 0)
            .toList() ??
        <int>[];

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Container(
          width: double.infinity,
          padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
          decoration: BoxDecoration(
            color: Colors.amber.withValues(alpha: 0.12),
            borderRadius: BorderRadius.circular(12),
            border: Border.all(color: Colors.amber.withValues(alpha: 0.3)),
          ),
          child: const Row(
            children: [
              Icon(Icons.info_outline_rounded, color: Colors.amber, size: 18),
              SizedBox(width: 8),
              Expanded(
                child: Text(
                  'Pilihlah satu atau lebih jawaban yang menurutmu benar.',
                  style: TextStyle(color: Colors.amber, fontSize: 12),
                ),
              ),
            ],
          ),
        ),
        const SizedBox(height: 14),
        ...question.options.map((opt) {
          final isChecked = currentList.contains(opt.idOption);

          return Padding(
            padding: const EdgeInsets.only(bottom: 12),
            child: InkWell(
              onTap: () {
                final updated = List<int>.from(currentList);
                if (isChecked) {
                  updated.remove(opt.idOption);
                } else {
                  updated.add(opt.idOption);
                }
                context.read<QuizPlayBloc>().add(
                      QuizAnswerUpdated(
                        idQuestion: question.idQuestion,
                        answer: updated.isEmpty ? null : updated,
                      ),
                    );
              },
              borderRadius: BorderRadius.circular(16),
              child: AnimatedContainer(
                duration: const Duration(milliseconds: 180),
                width: double.infinity,
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: isChecked
                      ? const Color(0xFF10B981).withValues(alpha: 0.15)
                      : const Color(0xFF1E2766).withValues(alpha: 0.5),
                  borderRadius: BorderRadius.circular(16),
                  border: Border.all(
                    color: isChecked
                        ? const Color(0xFF10B981)
                        : Colors.white.withValues(alpha: 0.15),
                    width: isChecked ? 1.8 : 1,
                  ),
                ),
                child: Row(
                  children: [
                    Container(
                      width: 22,
                      height: 22,
                      decoration: BoxDecoration(
                        borderRadius: BorderRadius.circular(6),
                        color: isChecked
                            ? const Color(0xFF10B981)
                            : Colors.transparent,
                        border: Border.all(
                          color: isChecked
                              ? const Color(0xFF10B981)
                              : Colors.white54,
                          width: 2,
                        ),
                      ),
                      child: isChecked
                          ? const Icon(
                              Icons.check_rounded,
                              size: 16,
                              color: Colors.white,
                            )
                          : null,
                    ),
                    const SizedBox(width: 14),
                    Expanded(
                      child: Text(
                        opt.optionText,
                        style: TextStyle(
                          color: isChecked ? Colors.white : Colors.white70,
                          fontSize: 14.5,
                          fontWeight:
                              isChecked ? FontWeight.bold : FontWeight.normal,
                          height: 1.4,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          );
        }),
      ],
    );
  }

  /// 3. Benar / Salah (Pernyataan)
  Widget _buildTrueFalseInput(
    BuildContext context,
    QuizPlayInProgress state,
    ExerciseQuestionModel question,
  ) {
    final currentAnswer = state.answers[question.idQuestion]?.toString();
    final isTrueSelected = currentAnswer == 'Benar';
    final isFalseSelected = currentAnswer == 'Salah';

    return Column(
      children: [
        // Tombol BENAR
        InkWell(
          onTap: () {
            context.read<QuizPlayBloc>().add(
                  QuizAnswerUpdated(
                    idQuestion: question.idQuestion,
                    answer: 'Benar',
                  ),
                );
          },
          borderRadius: BorderRadius.circular(16),
          child: AnimatedContainer(
            duration: const Duration(milliseconds: 180),
            width: double.infinity,
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
            decoration: BoxDecoration(
              color: isTrueSelected
                  ? const Color(0xFF10B981).withValues(alpha: 0.2)
                  : const Color(0xFF1E2766).withValues(alpha: 0.5),
              borderRadius: BorderRadius.circular(16),
              border: Border.all(
                color: isTrueSelected
                    ? const Color(0xFF10B981)
                    : Colors.white.withValues(alpha: 0.15),
                width: isTrueSelected ? 2 : 1,
              ),
            ),
            child: Row(
              children: [
                Icon(
                  Icons.check_circle_rounded,
                  color: isTrueSelected
                      ? const Color(0xFF10B981)
                      : Colors.white54,
                  size: 24,
                ),
                const SizedBox(width: 14),
                const Text(
                  'BENAR',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 16,
                    fontWeight: FontWeight.bold,
                    letterSpacing: 0.5,
                  ),
                ),
              ],
            ),
          ),
        ),

        const SizedBox(height: 14),

        // Tombol SALAH
        InkWell(
          onTap: () {
            context.read<QuizPlayBloc>().add(
                  QuizAnswerUpdated(
                    idQuestion: question.idQuestion,
                    answer: 'Salah',
                  ),
                );
          },
          borderRadius: BorderRadius.circular(16),
          child: AnimatedContainer(
            duration: const Duration(milliseconds: 180),
            width: double.infinity,
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
            decoration: BoxDecoration(
              color: isFalseSelected
                  ? const Color(0xFFEF4444).withValues(alpha: 0.2)
                  : const Color(0xFF1E2766).withValues(alpha: 0.5),
              borderRadius: BorderRadius.circular(16),
              border: Border.all(
                color: isFalseSelected
                    ? const Color(0xFFEF4444)
                    : Colors.white.withValues(alpha: 0.15),
                width: isFalseSelected ? 2 : 1,
              ),
            ),
            child: Row(
              children: [
                Icon(
                  Icons.cancel_rounded,
                  color: isFalseSelected
                      ? const Color(0xFFEF4444)
                      : Colors.white54,
                  size: 24,
                ),
                const SizedBox(width: 14),
                const Text(
                  'SALAH',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 16,
                    fontWeight: FontWeight.bold,
                    letterSpacing: 0.5,
                  ),
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }

  /// 4. Isian Singkat (Angka Saja)
  Widget _buildShortAnswerInput(
    BuildContext context,
    QuizPlayInProgress state,
    ExerciseQuestionModel question,
  ) {
    final currentAnswer =
        state.answers[question.idQuestion]?.toString() ?? '';

    return _ShortAnswerFieldWidget(
      initialValue: currentAnswer,
      onChanged: (val) {
        context.read<QuizPlayBloc>().add(
              QuizAnswerUpdated(
                idQuestion: question.idQuestion,
                answer: val.trim().isEmpty ? null : val.trim(),
              ),
            );
      },
    );
  }

  /// 5. Menjodohkan / Memasangkan
  Widget _buildMatchingInput(
    BuildContext context,
    QuizPlayInProgress state,
    ExerciseQuestionModel question,
  ) {
    // Current answer in matching is stored as List<Map<String, dynamic>>:
    // [{'id_option': 1, 'match_text': 'linear'}]
    final rawAnswer = state.answers[question.idQuestion];
    final Map<int, String> pairMap = {};

    if (rawAnswer is List) {
      for (final item in rawAnswer) {
        if (item is Map) {
          final idOpt = int.tryParse(item['id_option']?.toString() ?? '');
          final text = item['match_text']?.toString() ?? '';
          if (idOpt != null) {
            pairMap[idOpt] = text;
          }
        }
      }
    }

    final targets = question.matchingTargets;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Container(
          width: double.infinity,
          padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
          decoration: BoxDecoration(
            color: AppColors.accent.withValues(alpha: 0.12),
            borderRadius: BorderRadius.circular(12),
            border: Border.all(
              color: AppColors.accent.withValues(alpha: 0.3),
            ),
          ),
          child: const Row(
            children: [
              Icon(Icons.swap_horiz_rounded, color: AppColors.accent, size: 18),
              SizedBox(width: 8),
              Expanded(
                child: Text(
                  'Pasangkan setiap item di sebelah kiri dengan target yang sesuai di sebelah kanan.',
                  style: TextStyle(color: AppColors.accent, fontSize: 12),
                ),
              ),
            ],
          ),
        ),

        const SizedBox(height: 16),

        ...question.options.map((opt) {
          final currentMatched = pairMap[opt.idOption];

          return Container(
            margin: const EdgeInsets.only(bottom: 14),
            padding: const EdgeInsets.all(14),
            decoration: BoxDecoration(
              color: const Color(0xFF1E2766).withValues(alpha: 0.5),
              borderRadius: BorderRadius.circular(16),
              border: Border.all(
                color: currentMatched != null
                    ? const Color(0xFF10B981)
                    : Colors.white.withValues(alpha: 0.15),
              ),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  opt.optionText,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 14,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 10),
                InkWell(
                  onTap: () {
                    _showTargetPickerSheet(
                      context,
                      opt.optionText,
                      targets,
                      currentMatched,
                      (selectedTarget) {
                        final updatedPairs = Map<int, String>.from(pairMap);
                        if (selectedTarget == null) {
                          updatedPairs.remove(opt.idOption);
                        } else {
                          updatedPairs[opt.idOption] = selectedTarget;
                        }

                        final payload = updatedPairs.entries
                            .map((e) => {
                                  'id_option': e.key,
                                  'match_text': e.value,
                                })
                            .toList();

                        context.read<QuizPlayBloc>().add(
                              QuizAnswerUpdated(
                                idQuestion: question.idQuestion,
                                answer: payload.isEmpty ? null : payload,
                              ),
                            );
                      },
                    );
                  },
                  borderRadius: BorderRadius.circular(12),
                  child: Container(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 14,
                      vertical: 12,
                    ),
                    decoration: BoxDecoration(
                      color: Colors.white.withValues(alpha: 0.08),
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(
                        color: currentMatched != null
                            ? const Color(0xFF10B981).withValues(alpha: 0.5)
                            : Colors.white.withValues(alpha: 0.2),
                      ),
                    ),
                    child: Row(
                      children: [
                        Expanded(
                          child: Text(
                            currentMatched ?? 'Pilih Pasangan...',
                            style: TextStyle(
                              color: currentMatched != null
                                  ? const Color(0xFF10B981)
                                  : Colors.white60,
                              fontSize: 13,
                              fontWeight: currentMatched != null
                                  ? FontWeight.bold
                                  : FontWeight.normal,
                            ),
                          ),
                        ),
                        const Icon(
                          Icons.arrow_drop_down_rounded,
                          color: Colors.white70,
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          );
        }),
      ],
    );
  }

  void _showTargetPickerSheet(
    BuildContext context,
    String itemTitle,
    List<String> targets,
    String? currentTarget,
    ValueChanged<String?> onSelected,
  ) {
    showModalBottomSheet(
      context: context,
      backgroundColor: const Color(0xFF141C3D),
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      builder: (sheetCtx) => SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(20, 16, 20, 20),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Center(
                child: Container(
                  width: 40,
                  height: 4,
                  decoration: BoxDecoration(
                    color: Colors.white24,
                    borderRadius: BorderRadius.circular(2),
                  ),
                ),
              ),
              const SizedBox(height: 14),
              Text(
                'Pasangan untuk "$itemTitle"',
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 12),
              ...targets.map((target) {
                final isSelected = target == currentTarget;
                return Padding(
                  padding: const EdgeInsets.only(bottom: 8),
                  child: InkWell(
                    onTap: () {
                      Navigator.pop(sheetCtx);
                      onSelected(target);
                    },
                    borderRadius: BorderRadius.circular(12),
                    child: Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 14,
                        vertical: 12,
                      ),
                      decoration: BoxDecoration(
                        color: isSelected
                            ? const Color(0xFF10B981).withValues(alpha: 0.2)
                            : Colors.white.withValues(alpha: 0.05),
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(
                          color: isSelected
                              ? const Color(0xFF10B981)
                              : Colors.white12,
                        ),
                      ),
                      child: Row(
                        children: [
                          Expanded(
                            child: Text(
                              target,
                              style: TextStyle(
                                color: isSelected
                                    ? const Color(0xFF10B981)
                                    : Colors.white,
                                fontSize: 14,
                                fontWeight: isSelected
                                    ? FontWeight.bold
                                    : FontWeight.normal,
                              ),
                            ),
                          ),
                          if (isSelected)
                            const Icon(
                              Icons.check_rounded,
                              color: Color(0xFF10B981),
                              size: 18,
                            ),
                        ],
                      ),
                    ),
                  ),
                );
              }),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildBottomNavigation(
    BuildContext context,
    QuizPlayInProgress state,
  ) {
    final currentIndex = state.currentIndex;
    final total = state.questions.length;
    final isFirst = currentIndex == 0;
    final isLast = currentIndex == total - 1;

    return Container(
      padding: const EdgeInsets.fromLTRB(20, 14, 20, 16),
      decoration: BoxDecoration(
        color: const Color(0xFF141C3D),
        border: Border(
          top: BorderSide(
            color: Colors.white.withValues(alpha: 0.1),
          ),
        ),
      ),
      child: Row(
        children: [
          // Tombol Sebelumnya
          if (!isFirst)
            Expanded(
              child: OutlinedButton.icon(
                onPressed: () {
                  context
                      .read<QuizPlayBloc>()
                      .add(QuizQuestionNavigated(currentIndex - 1));
                },
                icon: const Icon(Icons.arrow_back_rounded, size: 16),
                label: const Text('Sebelumnya'),
                style: OutlinedButton.styleFrom(
                  side: BorderSide(color: Colors.white.withValues(alpha: 0.3)),
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 14),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(14),
                  ),
                ),
              ),
            )
          else
            const Spacer(),

          const SizedBox(width: 12),

          // Tombol Selanjutnya / Kirim
          Expanded(
            child: ElevatedButton.icon(
              onPressed: state.isSubmitting
                  ? null
                  : () {
                      if (isLast) {
                        _showSubmitConfirmation(context, state);
                      } else {
                        context
                            .read<QuizPlayBloc>()
                            .add(QuizQuestionNavigated(currentIndex + 1));
                      }
                    },
              icon: state.isSubmitting
                  ? const SizedBox(
                      width: 16,
                      height: 16,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: Colors.white,
                      ),
                    )
                  : Icon(
                      isLast
                          ? Icons.check_circle_rounded
                          : Icons.arrow_forward_rounded,
                      size: 16,
                    ),
              label: Text(isLast ? 'Kirim Jawaban' : 'Selanjutnya'),
              style: ElevatedButton.styleFrom(
                backgroundColor: isLast
                    ? const Color(0xFF10B981)
                    : AppColors.accent,
                foregroundColor: isLast
                    ? Colors.white
                    : const Color(0xFF0F172A),
                elevation: 4,
                padding: const EdgeInsets.symmetric(vertical: 14),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(14),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

/// Helper stateful widget untuk input Isian Singkat
class _ShortAnswerFieldWidget extends StatefulWidget {
  final String initialValue;
  final ValueChanged<String> onChanged;

  const _ShortAnswerFieldWidget({
    required this.initialValue,
    required this.onChanged,
  });

  @override
  State<_ShortAnswerFieldWidget> createState() =>
      _ShortAnswerFieldWidgetState();
}

class _ShortAnswerFieldWidgetState extends State<_ShortAnswerFieldWidget> {
  late final TextEditingController _controller;

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController(text: widget.initialValue);
  }

  @override
  void didUpdateWidget(covariant _ShortAnswerFieldWidget oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.initialValue != widget.initialValue &&
        widget.initialValue != _controller.text) {
      _controller.text = widget.initialValue;
    }
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Container(
          width: double.infinity,
          padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
          decoration: BoxDecoration(
            color: Colors.cyan.withValues(alpha: 0.12),
            borderRadius: BorderRadius.circular(12),
            border: Border.all(color: Colors.cyan.withValues(alpha: 0.3)),
          ),
          child: const Row(
            children: [
              Icon(Icons.numbers_rounded, color: Colors.cyan, size: 18),
              SizedBox(width: 8),
              Expanded(
                child: Text(
                  'Masukkan jawaban berupa angka desimal atau bulat (contoh: 104.5).',
                  style: TextStyle(color: Colors.cyan, fontSize: 12),
                ),
              ),
            ],
          ),
        ),
        const SizedBox(height: 16),
        Container(
          decoration: BoxDecoration(
            color: const Color(0xFF1E2766).withValues(alpha: 0.5),
            borderRadius: BorderRadius.circular(16),
            border: Border.all(
              color: Colors.white.withValues(alpha: 0.2),
            ),
          ),
          child: TextField(
            controller: _controller,
            keyboardType: const TextInputType.numberWithOptions(decimal: true),
            style: const TextStyle(
              color: Colors.white,
              fontSize: 16,
              fontWeight: FontWeight.bold,
            ),
            onChanged: widget.onChanged,
            decoration: InputDecoration(
              hintText: 'Tuliskan angka jawaban di sini...',
              hintStyle: TextStyle(
                color: Colors.white.withValues(alpha: 0.4),
                fontSize: 14,
              ),
              prefixIcon: const Icon(
                Icons.edit_rounded,
                color: AppColors.accent,
                size: 20,
              ),
              contentPadding: const EdgeInsets.symmetric(
                horizontal: 16,
                vertical: 16,
              ),
              border: InputBorder.none,
            ),
          ),
        ),
      ],
    );
  }
}