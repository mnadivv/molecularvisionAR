import 'package:equatable/equatable.dart';

abstract class QuizPlayEvent extends Equatable {
  const QuizPlayEvent();

  @override
  List<Object?> get props => [];
}

class QuizPlayStarted extends QuizPlayEvent {
  final int idCategory;

  const QuizPlayStarted(this.idCategory);

  @override
  List<Object?> get props => [idCategory];
}

class QuizQuestionNavigated extends QuizPlayEvent {
  final int index;

  const QuizQuestionNavigated(this.index);

  @override
  List<Object?> get props => [index];
}

class QuizAnswerUpdated extends QuizPlayEvent {
  final int idQuestion;
  final dynamic answer;

  const QuizAnswerUpdated({
    required this.idQuestion,
    required this.answer,
  });

  @override
  List<Object?> get props => [idQuestion, answer];
}

class QuizSubmitRequested extends QuizPlayEvent {
  const QuizSubmitRequested();
}

class QuizPlayResetRequested extends QuizPlayEvent {
  const QuizPlayResetRequested();
}
