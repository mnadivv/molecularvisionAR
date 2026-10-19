import 'package:equatable/equatable.dart';

/// Opsi jawaban kuis
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

/// Butir soal kuis & latihan
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
      questionText: json['question_text'] as String? ?? '',
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

/// Hasil evaluasi skor kuis
class ExerciseResultModel extends Equatable {
  final int totalQuestions;
  final int correctAnswers;
  final double score;
  final bool passed;

  const ExerciseResultModel({
    required this.totalQuestions,
    required this.correctAnswers,
    required this.score,
    required this.passed,
  });

  factory ExerciseResultModel.fromJson(Map<String, dynamic> json) {
    return ExerciseResultModel(
      totalQuestions: json['total_questions'] is int
          ? json['total_questions'] as int
          : int.tryParse(json['total_questions']?.toString() ?? '') ?? 0,
      correctAnswers: json['correct_answers'] is int
          ? json['correct_answers'] as int
          : int.tryParse(json['correct_answers']?.toString() ?? '') ?? 0,
      score: json['score'] is num
          ? (json['score'] as num).toDouble()
          : double.tryParse(json['score']?.toString() ?? '') ?? 0.0,
      passed: json['passed'] is bool
          ? json['passed'] as bool
          : json['passed'].toString().toLowerCase() == 'true',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'total_questions': totalQuestions,
      'correct_answers': correctAnswers,
      'score': score,
      'passed': passed,
    };
  }

  @override
  List<Object?> get props => [totalQuestions, correctAnswers, score, passed];
}
