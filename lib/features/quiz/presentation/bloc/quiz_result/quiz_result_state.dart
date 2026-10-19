import 'package:equatable/equatable.dart';
import 'package:molecular_vision_ar/data/models/exercise/exercise_model.dart';

abstract class QuizResultState extends Equatable {
  const QuizResultState();

  @override
  List<Object?> get props => [];
}

class QuizResultInitial extends QuizResultState {
  const QuizResultInitial();
}

class QuizResultLoading extends QuizResultState {
  const QuizResultLoading();
}

class QuizResultLoaded extends QuizResultState {
  final QuizReportModel report;

  const QuizResultLoaded(this.report);

  @override
  List<Object?> get props => [report];
}

class QuizResultFailure extends QuizResultState {
  final String message;

  const QuizResultFailure(this.message);

  @override
  List<Object?> get props => [message];
}
