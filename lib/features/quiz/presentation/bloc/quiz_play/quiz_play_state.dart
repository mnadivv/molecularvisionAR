import 'package:equatable/equatable.dart';
import 'package:molecular_vision_ar/data/models/exercise/exercise_model.dart';

abstract class QuizPlayState extends Equatable {
  const QuizPlayState();

  @override
  List<Object?> get props => [];
}

class QuizPlayInitial extends QuizPlayState {
  const QuizPlayInitial();
}

class QuizPlayLoading extends QuizPlayState {
  const QuizPlayLoading();
}

class QuizPlayInProgress extends QuizPlayState {
  final int idCategory;
  final List<ExerciseQuestionModel> questions;
  final int currentIndex;
  final Map<int, dynamic> answers; // idQuestion -> answer value
  final bool isSubmitting;

  const QuizPlayInProgress({
    required this.idCategory,
    required this.questions,
    this.currentIndex = 0,
    this.answers = const {},
    this.isSubmitting = false,
  });

  ExerciseQuestionModel get currentQuestion => questions[currentIndex];
  dynamic get currentAnswer => answers[currentQuestion.idQuestion];
  int get answeredCount => answers.length;
  int get totalQuestions => questions.length;
  bool get isLastQuestion => currentIndex == questions.length - 1;

  QuizPlayInProgress copyWith({
    int? idCategory,
    List<ExerciseQuestionModel>? questions,
    int? currentIndex,
    Map<int, dynamic>? answers,
    bool? isSubmitting,
  }) {
    return QuizPlayInProgress(
      idCategory: idCategory ?? this.idCategory,
      questions: questions ?? this.questions,
      currentIndex: currentIndex ?? this.currentIndex,
      answers: answers ?? this.answers,
      isSubmitting: isSubmitting ?? this.isSubmitting,
    );
  }

  @override
  List<Object?> get props => [
        idCategory,
        questions,
        currentIndex,
        answers,
        isSubmitting,
      ];
}

class QuizPlaySubmitted extends QuizPlayState {
  final int idCategory;
  final QuizSubmissionResponseModel response;

  const QuizPlaySubmitted({
    required this.idCategory,
    required this.response,
  });

  @override
  List<Object?> get props => [idCategory, response];
}

class QuizPlayFailure extends QuizPlayState {
  final String message;

  const QuizPlayFailure(this.message);

  @override
  List<Object?> get props => [message];
}
