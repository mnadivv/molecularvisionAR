import 'package:flutter/material.dart';

import '../../data/quiz_data.dart';
import '../../models/quiz_question.dart';
import '../../core/constants/app_colors.dart';

class QuizScreen extends StatefulWidget {
  final String materialId;
  final String materialName;

  const QuizScreen({
    super.key,
    required this.materialId,
    required this.materialName,
  });

  @override
  State<QuizScreen> createState() =>
      _QuizScreenState();
}

class _QuizScreenState extends State<QuizScreen> {
  late List<QuizQuestion> questions;

  int currentIndex = 0;

  final Map<String, dynamic> answers = {};

  @override
  void initState() {
    super.initState();

    questions =
        QuizData.getQuestionsForMaterial(
      widget.materialId,
    );
  }

  QuizQuestion get currentQuestion =>
      questions[currentIndex];

  dynamic get currentAnswer =>
      answers[currentQuestion.id];

  @override
  Widget build(BuildContext context) {
    if (questions.isEmpty) {
      return buildEmptyQuiz();
    }

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        elevation: 0,
        title: Text(
          widget.materialName,
          style: const TextStyle(
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
      body: Column(
        children: [
          buildProgress(),

          Expanded(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(20),
              child: buildQuestion(),
            ),
          ),

          buildNavigation(),
        ],
      ),
    );
  }

  Widget buildEmptyQuiz() {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        title: Text(widget.materialName),
      ),
      body: Center(
        child: Padding(
          padding: const EdgeInsets.all(30),
          child: Container(
            padding: const EdgeInsets.all(25),
            decoration: BoxDecoration(
              color: AppColors.surface,
              borderRadius:
                  BorderRadius.circular(22),
              border: Border.all(
                color: AppColors.border,
              ),
            ),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Icon(
                  Icons.menu_book_rounded,
                  size: 55,
                  color: AppColors.primary,
                ),
                const SizedBox(height: 18),
                Text(
                  'Soal belum tersedia',
                  textAlign: TextAlign.center,
                  style: const TextStyle(
                    fontSize: 20,
                    fontWeight: FontWeight.bold,
                    color: AppColors.textPrimary,
                  ),
                ),
                const SizedBox(height: 8),
                Text(
                  'Bank soal untuk materi ${widget.materialName} sedang dikembangkan.',
                  textAlign: TextAlign.center,
                  style: const TextStyle(
                    color: AppColors.textSecondary,
                    height: 1.4,
                  ),
                ),
                const SizedBox(height: 22),
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: () {
                      Navigator.pop(context);
                    },
                    style: ElevatedButton.styleFrom(
                      backgroundColor:
                          AppColors.primary,
                      foregroundColor:
                          Colors.white,
                      padding:
                          const EdgeInsets.symmetric(
                        vertical: 14,
                      ),
                      shape:
                          RoundedRectangleBorder(
                        borderRadius:
                            BorderRadius.circular(14),
                      ),
                    ),
                    child: const Text(
                      'Kembali Pilih Materi',
                    ),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget buildProgress() {
    final progress =
        (currentIndex + 1) / questions.length;

    return Container(
      color: AppColors.surface,
      padding: const EdgeInsets.fromLTRB(
        20,
        14,
        20,
        16,
      ),
      child: Column(
        children: [
          Row(
            mainAxisAlignment:
                MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'Soal ${currentIndex + 1}',
                style: const TextStyle(
                  color: AppColors.textPrimary,
                  fontWeight: FontWeight.bold,
                ),
              ),
              Text(
                '${questions.length} soal',
                style: const TextStyle(
                  color: AppColors.textSecondary,
                ),
              ),
            ],
          ),
          const SizedBox(height: 10),
          ClipRRect(
            borderRadius:
                BorderRadius.circular(20),
            child: LinearProgressIndicator(
              value: progress,
              minHeight: 8,
              backgroundColor:
                  AppColors.border,
              valueColor:
                  const AlwaysStoppedAnimation(
                AppColors.primary,
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget buildQuestion() {
    final question = currentQuestion;

    return Column(
      crossAxisAlignment:
          CrossAxisAlignment.start,
      children: [
        buildStemBadge(question.stemAspect),

        const SizedBox(height: 16),

        Text(
          question.question,
          style: const TextStyle(
            color: AppColors.textPrimary,
            fontSize: 20,
            fontWeight: FontWeight.bold,
            height: 1.45,
          ),
        ),

        const SizedBox(height: 25),

        buildAnswerWidget(question),

        if (question.hint.isNotEmpty) ...[
          const SizedBox(height: 24),
          buildHint(question),
        ],
      ],
    );
  }

  Widget buildStemBadge(
    StemAspect aspect,
  ) {
    late String label;
    late IconData icon;

    switch (aspect) {
      case StemAspect.science:
        label = 'SCIENCE';
        icon = Icons.science_rounded;
        break;

      case StemAspect.technology:
        label = 'TECHNOLOGY';
        icon = Icons.computer_rounded;
        break;

      case StemAspect.engineering:
        label = 'ENGINEERING';
        icon = Icons.engineering_rounded;
        break;

      case StemAspect.mathematics:
        label = 'MATHEMATICS';
        icon = Icons.calculate_rounded;
        break;

      case StemAspect.integrated:
        label = 'STEM INTEGRATED';
        icon = Icons.auto_awesome_rounded;
        break;
    }

    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: 12,
        vertical: 8,
      ),
      decoration: BoxDecoration(
        color:
            AppColors.secondary.withOpacity(0.1),
        borderRadius:
            BorderRadius.circular(30),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            icon,
            size: 18,
            color: AppColors.secondary,
          ),
          const SizedBox(width: 7),
          Text(
            label,
            style: const TextStyle(
              color: AppColors.secondary,
              fontSize: 12,
              fontWeight: FontWeight.bold,
              letterSpacing: 0.5,
            ),
          ),
        ],
      ),
    );
  }

  Widget buildAnswerWidget(
    QuizQuestion question,
  ) {
    switch (question.type) {
      case QuestionType.multipleChoice:
        return buildMultipleChoice(question);

      case QuestionType.multipleAnswer:
        return buildMultipleAnswer(question);

      case QuestionType.trueFalse:
        return buildTrueFalse(question);

      case QuestionType.matching:
        return buildMatching(question);

      case QuestionType.numericEssay:
        return buildNumericEssay(question);
    }
  }

  Widget buildMultipleChoice(
    QuizQuestion question,
  ) {
    return Column(
      children: question.options.map(
        (option) {
          final selected =
              currentAnswer == option;

          return Padding(
            padding:
                const EdgeInsets.only(bottom: 12),
            child: InkWell(
              onTap: () {
                setState(() {
                  answers[question.id] =
                      option;
                });
              },
              borderRadius:
                  BorderRadius.circular(17),
              child: AnimatedContainer(
                duration:
                    const Duration(milliseconds: 180),
                width: double.infinity,
                padding:
                    const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: selected
                      ? AppColors.primary
                          .withOpacity(0.08)
                      : AppColors.surface,
                  borderRadius:
                      BorderRadius.circular(17),
                  border: Border.all(
                    color: selected
                        ? AppColors.primary
                        : AppColors.border,
                    width: selected ? 2 : 1,
                  ),
                ),
                child: Row(
                  children: [
                    Container(
                      width: 24,
                      height: 24,
                      decoration: BoxDecoration(
                        shape: BoxShape.circle,
                        border: Border.all(
                          color: selected
                              ? AppColors.primary
                              : AppColors
                                  .textSecondary,
                          width: 2,
                        ),
                      ),
                      child: selected
                          ? const Center(
                              child: Icon(
                                Icons.circle,
                                size: 12,
                                color:
                                    AppColors.primary,
                              ),
                            )
                          : null,
                    ),
                    const SizedBox(width: 13),
                    Expanded(
                      child: Text(
                        option,
                        style:
                            const TextStyle(
                          color:
                              AppColors.textPrimary,
                          fontSize: 15,
                          height: 1.4,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          );
        },
      ).toList(),
    );
  }

  Widget buildMultipleAnswer(
    QuizQuestion question,
  ) {
    final selected =
        List<String>.from(
      currentAnswer ?? [],
    );

    return Column(
      children: [
        Container(
          width: double.infinity,
          padding: const EdgeInsets.all(14),
          decoration: BoxDecoration(
            color:
                AppColors.warning.withOpacity(0.1),
            borderRadius:
                BorderRadius.circular(14),
            border: Border.all(
              color:
                  AppColors.warning.withOpacity(0.3),
            ),
          ),
          child: const Row(
            crossAxisAlignment:
                CrossAxisAlignment.start,
            children: [
              Icon(
                Icons.info_outline_rounded,
                color: AppColors.warning,
              ),
              SizedBox(width: 10),
              Expanded(
                child: Text(
                  'Pilih semua jawaban yang benar.',
                  style: TextStyle(
                    color: AppColors.textPrimary,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
            ],
          ),
        ),

        const SizedBox(height: 15),

        ...question.options.map(
          (option) {
            final checked =
                selected.contains(option);

            return Padding(
              padding:
                  const EdgeInsets.only(bottom: 10),
              child: CheckboxListTile(
                value: checked,
                onChanged: (_) {
                  final updated =
                      List<String>.from(selected);

                  if (updated.contains(option)) {
                    updated.remove(option);
                  } else {
                    updated.add(option);
                  }

                  setState(() {
                    answers[question.id] =
                        updated;
                  });
                },
                activeColor:
                    AppColors.primary,
                title: Text(
                  option,
                  style: const TextStyle(
                    color: AppColors.textPrimary,
                  ),
                ),
                shape:
                    RoundedRectangleBorder(
                  borderRadius:
                      BorderRadius.circular(14),
                ),
                tileColor: AppColors.surface,
                controlAffinity:
                    ListTileControlAffinity.leading,
              ),
            );
          },
        ),
      ],
    );
  }

  Widget buildTrueFalse(
    QuizQuestion question,
  ) {
    return Column(
      children: question.options.map(
        (option) {
          final selected =
              currentAnswer == option;

          return Padding(
            padding:
                const EdgeInsets.only(bottom: 12),
            child: SizedBox(
              width: double.infinity,
              child: OutlinedButton(
                onPressed: () {
                  setState(() {
                    answers[question.id] =
                        option;
                  });
                },
                style: OutlinedButton.styleFrom(
                  backgroundColor: selected
                      ? AppColors.primary
                          .withOpacity(0.08)
                      : AppColors.surface,
                  foregroundColor:
                      AppColors.textPrimary,
                  side: BorderSide(
                    color: selected
                        ? AppColors.primary
                        : AppColors.border,
                    width: selected ? 2 : 1,
                  ),
                  padding:
                      const EdgeInsets.symmetric(
                    vertical: 17,
                  ),
                  shape:
                      RoundedRectangleBorder(
                    borderRadius:
                        BorderRadius.circular(15),
                  ),
                ),
                child: Text(
                  option,
                  style: const TextStyle(
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
            ),
          );
        },
      ).toList(),
    );
  }

  Widget buildMatching(
    QuizQuestion question,
  ) {
    final selected =
        Map<String, String>.from(
      currentAnswer ?? {},
    );

    final rightOptions =
        question.matchingPairs
            .map((pair) => pair.right)
            .toList();

    return Column(
      children: question.matchingPairs.map(
        (pair) {
          return Padding(
            padding:
                const EdgeInsets.only(bottom: 18),
            child: Column(
              crossAxisAlignment:
                  CrossAxisAlignment.start,
              children: [
                Text(
                  pair.left,
                  style: const TextStyle(
                    color: AppColors.textPrimary,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 8),
                DropdownButtonFormField<String>(
                  value: selected[pair.left],
                  isExpanded: true,
                  decoration:
                      InputDecoration(
                    filled: true,
                    fillColor:
                        AppColors.surface,
                    hintText:
                        'Pilih pasangan',
                    border:
                        OutlineInputBorder(
                      borderRadius:
                          BorderRadius.circular(
                        14,
                      ),
                      borderSide:
                          const BorderSide(
                        color:
                            AppColors.border,
                      ),
                    ),
                  ),
                  items: rightOptions
                      .map(
                        (right) =>
                            DropdownMenuItem(
                          value: right,
                          child: Text(right),
                        ),
                      )
                      .toList(),
                  onChanged: (value) {
                    if (value == null) return;

                    final updated =
                        Map<String, String>.from(
                      selected,
                    );

                    updated[pair.left] =
                        value;

                    setState(() {
                      answers[question.id] =
                          updated;
                    });
                  },
                ),
              ],
            ),
          );
        },
      ).toList(),
    );
  }

  Widget buildNumericEssay(
    QuizQuestion question,
  ) {
    final controller =
        TextEditingController(
      text:
          currentAnswer?.toString() ?? '',
    );

    return Column(
      crossAxisAlignment:
          CrossAxisAlignment.start,
      children: [
        Container(
          width: double.infinity,
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color:
                AppColors.secondary.withOpacity(0.08),
            borderRadius:
                BorderRadius.circular(15),
            border: Border.all(
              color:
                  AppColors.secondary.withOpacity(0.2),
            ),
          ),
          child: const Column(
            crossAxisAlignment:
                CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Icon(
                    Icons.calculate_rounded,
                    color: AppColors.secondary,
                  ),
                  SizedBox(width: 8),
                  Text(
                    'Jawaban Angka',
                    style: TextStyle(
                      color:
                          AppColors.textPrimary,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
              SizedBox(height: 8),
              Text(
                'Masukkan angka saja. Tidak perlu mengetik satuan.',
                style: TextStyle(
                  color:
                      AppColors.textSecondary,
                ),
              ),
            ],
          ),
        ),

        const SizedBox(height: 16),

        TextField(
          controller: controller,
          keyboardType:
              const TextInputType.numberWithOptions(
            decimal: true,
          ),
          onChanged: (value) {
            answers[question.id] = value;
          },
          decoration:
              InputDecoration(
            filled: true,
            fillColor: AppColors.surface,
            labelText: 'Jawaban',
            hintText: 'Contoh: 5 atau 0.02',
            prefixIcon: const Icon(
              Icons.edit_rounded,
              color: AppColors.primary,
            ),
            border:
                OutlineInputBorder(
              borderRadius:
                  BorderRadius.circular(15),
              borderSide:
                  const BorderSide(
                color: AppColors.border,
              ),
            ),
            focusedBorder:
                OutlineInputBorder(
              borderRadius:
                  BorderRadius.circular(15),
              borderSide:
                  const BorderSide(
                color: AppColors.primary,
                width: 2,
              ),
            ),
          ),
        ),
      ],
    );
  }

  Widget buildHint(
    QuizQuestion question,
  ) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color:
            AppColors.warning.withOpacity(0.08),
        borderRadius:
            BorderRadius.circular(15),
      ),
      child: Row(
        crossAxisAlignment:
            CrossAxisAlignment.start,
        children: [
          const Icon(
            Icons.lightbulb_outline_rounded,
            color: AppColors.warning,
          ),
          const SizedBox(width: 10),
          Expanded(
            child: Text(
              question.hint,
              style: const TextStyle(
                color: AppColors.textPrimary,
                height: 1.4,
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget buildNavigation() {
    final isFirst = currentIndex == 0;
    final isLast =
        currentIndex == questions.length - 1;

    final canContinue =
        hasCurrentAnswer();

    return Container(
      padding: const EdgeInsets.fromLTRB(
        20,
        12,
        20,
        20,
      ),
      decoration: BoxDecoration(
        color: AppColors.surface,
        boxShadow: [
          BoxShadow(
            color:
                Colors.black.withOpacity(0.05),
            blurRadius: 12,
            offset: const Offset(0, -3),
          ),
        ],
      ),
      child: Row(
        children: [
          if (!isFirst) ...[
            Expanded(
              child: OutlinedButton(
                onPressed: previousQuestion,
                style:
                    OutlinedButton.styleFrom(
                  foregroundColor:
                      AppColors.primary,
                  side: const BorderSide(
                    color: AppColors.primary,
                  ),
                  padding:
                      const EdgeInsets.symmetric(
                    vertical: 14,
                  ),
                  shape:
                      RoundedRectangleBorder(
                    borderRadius:
                        BorderRadius.circular(14),
                  ),
                ),
                child:
                    const Text('Sebelumnya'),
              ),
            ),
            const SizedBox(width: 12),
          ],

          Expanded(
            flex: 2,
            child: ElevatedButton(
              onPressed: canContinue
                  ? () {
                      if (isLast) {
                        finishQuiz();
                      } else {
                        nextQuestion();
                      }
                    }
                  : null,
              style:
                  ElevatedButton.styleFrom(
                backgroundColor:
                    AppColors.primary,
                foregroundColor:
                    Colors.white,
                disabledBackgroundColor:
                    AppColors.border,
                padding:
                    const EdgeInsets.symmetric(
                  vertical: 14,
                ),
                shape:
                    RoundedRectangleBorder(
                  borderRadius:
                      BorderRadius.circular(14),
                ),
              ),
              child: Text(
                isLast
                    ? 'Selesai'
                    : 'Berikutnya',
              ),
            ),
          ),
        ],
      ),
    );
  }

  bool hasCurrentAnswer() {
    final answer = currentAnswer;

    if (answer == null) {
      return false;
    }

    if (answer is String) {
      return answer.trim().isNotEmpty;
    }

    if (answer is List) {
      return answer.isNotEmpty;
    }

    if (answer is Map) {
      return answer.isNotEmpty;
    }

    return true;
  }

  void nextQuestion() {
    if (currentIndex <
        questions.length - 1) {
      setState(() {
        currentIndex++;
      });
    }
  }

  void previousQuestion() {
    if (currentIndex > 0) {
      setState(() {
        currentIndex--;
      });
    }
  }

  bool isAnswerCorrect(
    QuizQuestion question,
    dynamic answer,
  ) {
    if (answer == null) {
      return false;
    }

    switch (question.type) {
      case QuestionType.multipleChoice:
      case QuestionType.trueFalse:
        return answer ==
            question.correctAnswer;

      case QuestionType.multipleAnswer:
        final student =
            List<String>.from(answer)
              ..sort();

        final correct =
            List<String>.from(
              question.correctAnswers,
            )..sort();

        if (student.length != correct.length) {
          return false;
        }

        for (int i = 0;
            i < student.length;
            i++) {
          if (student[i] != correct[i]) {
            return false;
          }
        }

        return true;

      case QuestionType.matching:
        final student =
            Map<String, String>.from(answer);

        for (final pair
            in question.matchingPairs) {
          if (student[pair.left] !=
              pair.right) {
            return false;
          }
        }

        return true;

      case QuestionType.numericEssay:
        final value =
            double.tryParse(
          answer
              .toString()
              .replaceAll(',', '.')
              .trim(),
        );

        if (value == null ||
            question.numericAnswer == null) {
          return false;
        }

        return (value -
                    question.numericAnswer!)
                .abs() <=
            question.numericTolerance;
    }
  }

  void finishQuiz() {
    int correct = 0;

    for (final question in questions) {
      if (isAnswerCorrect(
        question,
        answers[question.id],
      )) {
        correct++;
      }
    }

    final score =
        ((correct / questions.length) * 100)
            .round();

    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) {
        return AlertDialog(
          shape:
              RoundedRectangleBorder(
            borderRadius:
                BorderRadius.circular(22),
          ),
          title: const Text(
            'Kuis Selesai',
            style: TextStyle(
              fontWeight: FontWeight.bold,
            ),
          ),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Container(
                width: 100,
                height: 100,
                decoration:
                    BoxDecoration(
                  color:
                      AppColors.primary
                          .withOpacity(0.1),
                  shape: BoxShape.circle,
                ),
                alignment:
                    Alignment.center,
                child: Text(
                  '$score',
                  style: const TextStyle(
                    color:
                        AppColors.primary,
                    fontSize: 32,
                    fontWeight:
                        FontWeight.bold,
                  ),
                ),
              ),
              const SizedBox(height: 18),
              Text(
                '$correct dari ${questions.length} soal benar',
                textAlign:
                    TextAlign.center,
                style: const TextStyle(
                  color:
                      AppColors.textPrimary,
                  fontWeight:
                      FontWeight.w600,
                ),
              ),
              const SizedBox(height: 8),
              const Text(
                'Pembahasan lengkap akan tersedia pada halaman hasil.',
                textAlign:
                    TextAlign.center,
                style: TextStyle(
                  color:
                      AppColors.textSecondary,
                ),
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () {
                Navigator.pop(context);
                Navigator.pop(context);
              },
              child:
                  const Text('Selesai'),
            ),
          ],
        );
      },
    );
  }
}