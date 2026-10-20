import 'package:equatable/equatable.dart';

abstract class QuizResultEvent extends Equatable {
  const QuizResultEvent();

  @override
  List<Object?> get props => [];
}

class QuizResultFetchRequested extends QuizResultEvent {
  final int idCategory;

  const QuizResultFetchRequested(this.idCategory);

  @override
  List<Object?> get props => [idCategory];
}
