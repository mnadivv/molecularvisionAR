import 'package:equatable/equatable.dart';

abstract class QuizCategoryEvent extends Equatable {
  const QuizCategoryEvent();

  @override
  List<Object?> get props => [];
}

class QuizCategoryFetchRequested extends QuizCategoryEvent {
  final String? search;

  const QuizCategoryFetchRequested({this.search});

  @override
  List<Object?> get props => [search];
}

class QuizCategoryRefreshRequested extends QuizCategoryEvent {
  const QuizCategoryRefreshRequested();
}
