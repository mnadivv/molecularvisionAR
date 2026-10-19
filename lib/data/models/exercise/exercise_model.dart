import 'package:equatable/equatable.dart';

/// Model Kategori Kuis / Latihan Soal dari GET /api/exercises
class QuizCategoryModel extends Equatable {
  final int idCategory;
  final String name;
  final String? description;
  final int questionsCount;

  const QuizCategoryModel({
    required this.idCategory,
    required this.name,
    this.description,
    this.questionsCount = 0,
  });

  factory QuizCategoryModel.fromJson(Map<String, dynamic> json) {
    return QuizCategoryModel(
      idCategory: json['id_category'] is int
          ? json['id_category'] as int
          : int.tryParse(json['id_category']?.toString() ?? '') ?? 0,
      name: json['name'] as String? ?? 'Kategori Kuis',
      description: json['description'] as String?,
      questionsCount: json['questions_count'] is int
          ? json['questions_count'] as int
          : int.tryParse(json['questions_count']?.toString() ?? '') ?? 0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id_category': idCategory,
      'name': name,
      'description': description,
      'questions_count': questionsCount,
    };
  }

  @override
  List<Object?> get props => [idCategory, name, description, questionsCount];
}

/// Opsi jawaban butir kuis
class ExerciseOptionModel extends Equatable {
  final int idOption;
  final String optionText;

  const ExerciseOptionModel({
    required this.idOption,
    required this.optionText,
  });

  factory ExerciseOptionModel.fromJson(Map<String, dynamic> json) {
    return ExerciseOptionModel(
      idOption: json['id_option'] is int
          ? json['id_option'] as int
          : int.tryParse(json['id_option']?.toString() ?? '') ?? 0,
      optionText: json['option_text'] as String? ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id_option': idOption,
      'option_text': optionText,
    };
  }

  @override
  List<Object?> get props => [idOption, optionText];
}

/// Butir soal kuis & latihan (Mendukung 5 tipe soal dari backend)
class ExerciseQuestionModel extends Equatable {
  final int idQuestion;
  final int? idCategory;
  final String questionText;
  final String questionType;
  final String? questionImage;
  final int points;
  final List<ExerciseOptionModel> options;
  final List<String> matchingTargets;

  const ExerciseQuestionModel({
    required this.idQuestion,
    this.idCategory,
    required this.questionText,
    required this.questionType,
    this.questionImage,
    this.points = 10,
    this.options = const [],
    this.matchingTargets = const [],
  });

  factory ExerciseQuestionModel.fromJson(Map<String, dynamic> json) {
    List<ExerciseOptionModel> parseOptions(dynamic list) {
      if (list is List) {
        return list
            .whereType<Map<String, dynamic>>()
            .map(ExerciseOptionModel.fromJson)
            .toList();
      }
      return [];
    }

    List<String> parseTargets(dynamic list) {
      if (list is List) {
        return list.map((e) => e.toString()).toList();
      }
      return [];
    }

    return ExerciseQuestionModel(
      idQuestion: json['id_question'] is int
          ? json['id_question'] as int
          : int.tryParse(json['id_question']?.toString() ?? '') ?? 0,
      idCategory: json['id_category'] != null
          ? int.tryParse(json['id_category'].toString())
          : null,
      questionText: json['name'] as String? ??
          json['question_text'] as String? ??
          '',
      questionType: json['question_type'] as String? ?? 'multiple_choice',
      questionImage: json['question_image'] as String?,
      points: json['points'] is int
          ? json['points'] as int
          : int.tryParse(json['points']?.toString() ?? '') ?? 10,
      options: parseOptions(json['options']),
      matchingTargets: parseTargets(json['matching_targets']),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id_question': idQuestion,
      'id_category': idCategory,
      'name': questionText,
      'question_text': questionText,
      'question_type': questionType,
      'question_image': questionImage,
      'points': points,
      'options': options.map((e) => e.toJson()).toList(),
      'matching_targets': matchingTargets,
    };
  }

  @override
  List<Object?> get props => [
        idQuestion,
        idCategory,
        questionText,
        questionType,
        questionImage,
        points,
        options,
        matchingTargets,
      ];
}

/// Hasil evaluasi skor kuis setelah submit (POST /api/exercises/{id_category})
class QuizSubmissionSummaryModel extends Equatable {
  final int totalQuestions;
  final int answered;
  final int correct;
  final int wrong;
  final double score;
  final bool isCompleted;

  const QuizSubmissionSummaryModel({
    required this.totalQuestions,
    required this.answered,
    required this.correct,
    required this.wrong,
    required this.score,
    required this.isCompleted,
  });

  bool get passed => isCompleted || score >= 70;

  factory QuizSubmissionSummaryModel.fromJson(Map<String, dynamic> json) {
    return QuizSubmissionSummaryModel(
      totalQuestions: json['total_questions'] is int
          ? json['total_questions'] as int
          : int.tryParse(json['total_questions']?.toString() ?? '') ?? 0,
      answered: json['answered'] is int
          ? json['answered'] as int
          : int.tryParse(json['answered']?.toString() ?? '') ?? 0,
      correct: json['correct'] is int
          ? json['correct'] as int
          : (json['correct_answers'] is int
              ? json['correct_answers'] as int
              : int.tryParse(json['correct']?.toString() ??
                      json['correct_answers']?.toString() ??
                      '') ??
                  0),
      wrong: json['wrong'] is int
          ? json['wrong'] as int
          : int.tryParse(json['wrong']?.toString() ?? '') ?? 0,
      score: json['score'] is num
          ? (json['score'] as num).toDouble()
          : double.tryParse(json['score']?.toString() ?? '') ?? 0.0,
      isCompleted: json['is_completed'] is bool
          ? json['is_completed'] as bool
          : (json['passed'] is bool
              ? json['passed'] as bool
              : (json['is_completed']?.toString().toLowerCase() == 'true' ||
                  json['passed']?.toString().toLowerCase() == 'true')),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'total_questions': totalQuestions,
      'answered': answered,
      'correct': correct,
      'wrong': wrong,
      'score': score,
      'is_completed': isCompleted,
    };
  }

  @override
  List<Object?> get props => [
        totalQuestions,
        answered,
        correct,
        wrong,
        score,
        isCompleted,
      ];
}

/// Detail butir jawaban per soal pada hasil submit
class QuizSubmissionItemResult extends Equatable {
  final int idQuestion;
  final bool isCorrect;
  final String userAnswer;
  final String correctAnswer;

  const QuizSubmissionItemResult({
    required this.idQuestion,
    required this.isCorrect,
    required this.userAnswer,
    required this.correctAnswer,
  });

  factory QuizSubmissionItemResult.fromJson(Map<String, dynamic> json) {
    return QuizSubmissionItemResult(
      idQuestion: json['id_question'] is int
          ? json['id_question'] as int
          : int.tryParse(json['id_question']?.toString() ?? '') ?? 0,
      isCorrect: json['is_correct'] is bool
          ? json['is_correct'] as bool
          : json['is_correct'].toString().toLowerCase() == 'true',
      userAnswer: json['user_answer']?.toString() ?? '',
      correctAnswer: json['correct_answer']?.toString() ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id_question': idQuestion,
      'is_correct': isCorrect,
      'user_answer': userAnswer,
      'correct_answer': correctAnswer,
    };
  }

  @override
  List<Object?> get props => [idQuestion, isCorrect, userAnswer, correctAnswer];
}

/// Response lengkap submit kuis
class QuizSubmissionResponseModel extends Equatable {
  final int idCategory;
  final QuizSubmissionSummaryModel summary;
  final List<QuizSubmissionItemResult> results;

  const QuizSubmissionResponseModel({
    required this.idCategory,
    required this.summary,
    required this.results,
  });

  factory QuizSubmissionResponseModel.fromJson(Map<String, dynamic> json) {
    final list = json['results'] as List? ?? [];
    return QuizSubmissionResponseModel(
      idCategory: json['id_category'] is int
          ? json['id_category'] as int
          : int.tryParse(json['id_category']?.toString() ?? '') ?? 0,
      summary: QuizSubmissionSummaryModel.fromJson(
        json['summary'] as Map<String, dynamic>? ?? {},
      ),
      results: list
          .whereType<Map<String, dynamic>>()
          .map(QuizSubmissionItemResult.fromJson)
          .toList(),
    );
  }

  @override
  List<Object?> get props => [idCategory, summary, results];
}

/// Butir review soal beserta kunci jawaban (GET /api/exercises/{id_category}/result)
class QuizReviewItemModel extends Equatable {
  final int idQuestion;
  final String questionName;
  final String questionType;
  final bool isAnswered;
  final bool isCorrect;
  final String? userAnswer;
  final String? correctAnswer;

  const QuizReviewItemModel({
    required this.idQuestion,
    required this.questionName,
    required this.questionType,
    required this.isAnswered,
    required this.isCorrect,
    this.userAnswer,
    this.correctAnswer,
  });

  factory QuizReviewItemModel.fromJson(Map<String, dynamic> json) {
    return QuizReviewItemModel(
      idQuestion: json['id_question'] is int
          ? json['id_question'] as int
          : int.tryParse(json['id_question']?.toString() ?? '') ?? 0,
      questionName: json['question_name'] as String? ?? '',
      questionType: json['question_type'] as String? ?? 'multiple_choice',
      isAnswered: json['is_answered'] is bool
          ? json['is_answered'] as bool
          : json['is_answered'].toString().toLowerCase() == 'true',
      isCorrect: json['is_correct'] is bool
          ? json['is_correct'] as bool
          : json['is_correct'].toString().toLowerCase() == 'true',
      userAnswer: json['user_answer']?.toString(),
      correctAnswer: json['correct_answer']?.toString(),
    );
  }

  @override
  List<Object?> get props => [
        idQuestion,
        questionName,
        questionType,
        isAnswered,
        isCorrect,
        userAnswer,
        correctAnswer,
      ];
}

/// Rapor hasil kuis kategori (GET /api/exercises/{id_category}/result)
class QuizReportModel extends Equatable {
  final int idCategory;
  final String categoryName;
  final int totalQuestions;
  final int answered;
  final int correct;
  final int wrong;
  final double? score;
  final bool isCompleted;
  final String? lastAttemptAt;
  final List<QuizReviewItemModel> reviews;

  const QuizReportModel({
    required this.idCategory,
    required this.categoryName,
    required this.totalQuestions,
    required this.answered,
    required this.correct,
    required this.wrong,
    this.score,
    required this.isCompleted,
    this.lastAttemptAt,
    this.reviews = const [],
  });

  factory QuizReportModel.fromJson(Map<String, dynamic> json) {
    final cat = json['category'] as Map<String, dynamic>? ?? {};
    final summary = json['summary'] as Map<String, dynamic>? ?? {};
    final reviewList = json['review'] as List? ?? [];

    return QuizReportModel(
      idCategory: cat['id_category'] is int
          ? cat['id_category'] as int
          : int.tryParse(cat['id_category']?.toString() ?? '') ?? 0,
      categoryName: cat['name'] as String? ?? 'Kategori Kuis',
      totalQuestions: summary['total_questions'] is int
          ? summary['total_questions'] as int
          : int.tryParse(summary['total_questions']?.toString() ?? '') ?? 0,
      answered: summary['answered'] is int
          ? summary['answered'] as int
          : int.tryParse(summary['answered']?.toString() ?? '') ?? 0,
      correct: summary['correct'] is int
          ? summary['correct'] as int
          : int.tryParse(summary['correct']?.toString() ?? '') ?? 0,
      wrong: summary['wrong'] is int
          ? summary['wrong'] as int
          : int.tryParse(summary['wrong']?.toString() ?? '') ?? 0,
      score: summary['score'] != null
          ? (summary['score'] is num
              ? (summary['score'] as num).toDouble()
              : double.tryParse(summary['score'].toString()))
          : null,
      isCompleted: summary['is_completed'] is bool
          ? summary['is_completed'] as bool
          : summary['is_completed'].toString().toLowerCase() == 'true',
      lastAttemptAt: summary['last_attempt_at']?.toString(),
      reviews: reviewList
          .whereType<Map<String, dynamic>>()
          .map(QuizReviewItemModel.fromJson)
          .toList(),
    );
  }

  @override
  List<Object?> get props => [
        idCategory,
        categoryName,
        totalQuestions,
        answered,
        correct,
        wrong,
        score,
        isCompleted,
        lastAttemptAt,
        reviews,
      ];
}

// Backward compatibility alias
typedef ExerciseResultModel = QuizSubmissionSummaryModel;
