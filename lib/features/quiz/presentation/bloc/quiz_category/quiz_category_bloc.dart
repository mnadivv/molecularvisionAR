import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:molecular_vision_ar/core/errors/app_exception.dart';
import 'package:molecular_vision_ar/data/models/exercise/exercise_model.dart';
import 'package:molecular_vision_ar/features/quiz/data/repositories/quiz_repository.dart';
import 'quiz_category_event.dart';
import 'quiz_category_state.dart';

class QuizCategoryBloc extends Bloc<QuizCategoryEvent, QuizCategoryState> {
  final QuizRepository _repository;

  QuizCategoryBloc({required QuizRepository repository})
      : _repository = repository,
        super(const QuizCategoryInitial()) {
    on<QuizCategoryFetchRequested>(_onFetchRequested);
    on<QuizCategoryRefreshRequested>(_onRefreshRequested);
  }

  Future<void> _onFetchRequested(
    QuizCategoryFetchRequested event,
    Emitter<QuizCategoryState> emit,
  ) async {
    if (state is! QuizCategoryLoaded) {
      emit(const QuizCategoryLoading());
    }

    try {
      final results = await Future.wait([
        _repository.getQuizCategories(),
        _repository.getCompletedQuizCategoryIds(),
      ]);

      final categories = results[0] as List<QuizCategoryModel>;
      final completedIds = results[1] as Set<int>;

      emit(QuizCategoryLoaded(
        categories: categories,
        completedCategoryIds: completedIds,
        searchQuery: event.search ?? '',
      ));
    } on AppException catch (e) {
      emit(QuizCategoryFailure(e.message));
    } catch (e) {
      emit(QuizCategoryFailure(e.toString()));
    }
  }

  Future<void> _onRefreshRequested(
    QuizCategoryRefreshRequested event,
    Emitter<QuizCategoryState> emit,
  ) async {
    final currentSearch = state is QuizCategoryLoaded
        ? (state as QuizCategoryLoaded).searchQuery
        : '';

    if (state is QuizCategoryLoaded) {
      emit((state as QuizCategoryLoaded).copyWith(isRefreshing: true));
    }

    try {
      final results = await Future.wait([
        _repository.getQuizCategories(),
        _repository.getCompletedQuizCategoryIds(),
      ]);

      final categories = results[0] as List<QuizCategoryModel>;
      final completedIds = results[1] as Set<int>;

      emit(QuizCategoryLoaded(
        categories: categories,
        completedCategoryIds: completedIds,
        searchQuery: currentSearch,
        isRefreshing: false,
      ));
    } on AppException catch (e) {
      emit(QuizCategoryFailure(e.message));
    } catch (e) {
      emit(QuizCategoryFailure(e.toString()));
    }
  }
}
