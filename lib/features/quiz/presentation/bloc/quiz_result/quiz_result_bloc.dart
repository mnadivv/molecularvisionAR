import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:molecular_vision_ar/core/errors/app_exception.dart';
import 'package:molecular_vision_ar/features/quiz/data/repositories/quiz_repository.dart';
import 'quiz_result_event.dart';
import 'quiz_result_state.dart';

class QuizResultBloc extends Bloc<QuizResultEvent, QuizResultState> {
  final QuizRepository _repository;

  QuizResultBloc({required QuizRepository repository})
      : _repository = repository,
        super(const QuizResultInitial()) {
    on<QuizResultFetchRequested>(_onFetchRequested);
  }

  Future<void> _onFetchRequested(
    QuizResultFetchRequested event,
    Emitter<QuizResultState> emit,
  ) async {
    emit(const QuizResultLoading());
    try {
      final report = await _repository.getQuizResult(event.idCategory);
      emit(QuizResultLoaded(report));
    } on AppException catch (e) {
      emit(QuizResultFailure(e.message));
    } catch (e) {
      emit(QuizResultFailure(e.toString()));
    }
  }
}
