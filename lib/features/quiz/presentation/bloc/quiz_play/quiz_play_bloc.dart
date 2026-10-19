import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:molecular_vision_ar/core/errors/app_exception.dart';
import 'package:molecular_vision_ar/features/quiz/data/repositories/quiz_repository.dart';
import 'quiz_play_event.dart';
import 'quiz_play_state.dart';

class QuizPlayBloc extends Bloc<QuizPlayEvent, QuizPlayState> {
  final QuizRepository _repository;

  QuizPlayBloc({required QuizRepository repository})
      : _repository = repository,
        super(const QuizPlayInitial()) {
    on<QuizPlayStarted>(_onPlayStarted);
    on<QuizQuestionNavigated>(_onQuestionNavigated);
    on<QuizAnswerUpdated>(_onAnswerUpdated);
    on<QuizSubmitRequested>(_onSubmitRequested);
    on<QuizPlayResetRequested>(_onResetRequested);
  }

  Future<void> _onPlayStarted(
    QuizPlayStarted event,
    Emitter<QuizPlayState> emit,
  ) async {
    emit(const QuizPlayLoading());
    try {
      final questions = await _repository.getQuizQuestions(event.idCategory);
      emit(QuizPlayInProgress(
        idCategory: event.idCategory,
        questions: questions,
        currentIndex: 0,
        answers: const {},
      ));
    } on AppException catch (e) {
      emit(QuizPlayFailure(e.message));
    } catch (e) {
      emit(QuizPlayFailure(e.toString()));
    }
  }

  void _onQuestionNavigated(
    QuizQuestionNavigated event,
    Emitter<QuizPlayState> emit,
  ) {
    if (state is! QuizPlayInProgress) return;
    final current = state as QuizPlayInProgress;
    if (event.index >= 0 && event.index < current.questions.length) {
      emit(current.copyWith(currentIndex: event.index));
    }
  }

  void _onAnswerUpdated(
    QuizAnswerUpdated event,
    Emitter<QuizPlayState> emit,
  ) {
    if (state is! QuizPlayInProgress) return;
    final current = state as QuizPlayInProgress;

    final updatedAnswers = Map<int, dynamic>.from(current.answers);
    if (event.answer == null) {
      updatedAnswers.remove(event.idQuestion);
    } else {
      updatedAnswers[event.idQuestion] = event.answer;
    }

    emit(current.copyWith(answers: updatedAnswers));
  }

  Future<void> _onSubmitRequested(
    QuizSubmitRequested event,
    Emitter<QuizPlayState> emit,
  ) async {
    if (state is! QuizPlayInProgress) return;
    final current = state as QuizPlayInProgress;

    emit(current.copyWith(isSubmitting: true));

    try {
      final answersList = <Map<String, dynamic>>[];
      for (final q in current.questions) {
        if (current.answers.containsKey(q.idQuestion)) {
          answersList.add({
            'id_question': q.idQuestion,
            'answer': current.answers[q.idQuestion],
          });
        }
      }

      final response = await _repository.submitQuizAnswers(
        current.idCategory,
        answersList,
      );

      emit(QuizPlaySubmitted(
        idCategory: current.idCategory,
        response: response,
      ));
    } on AppException catch (e) {
      emit(current.copyWith(isSubmitting: false));
      emit(QuizPlayFailure(e.message));
    } catch (e) {
      emit(current.copyWith(isSubmitting: false));
      emit(QuizPlayFailure(e.toString()));
    }
  }

  void _onResetRequested(
    QuizPlayResetRequested event,
    Emitter<QuizPlayState> emit,
  ) {
    if (state is QuizPlayInProgress) {
      final current = state as QuizPlayInProgress;
      emit(QuizPlayInProgress(
        idCategory: current.idCategory,
        questions: current.questions,
        currentIndex: 0,
        answers: const {},
      ));
    } else if (state is QuizPlaySubmitted) {
      final current = state as QuizPlaySubmitted;
      add(QuizPlayStarted(current.idCategory));
    }
  }
}
