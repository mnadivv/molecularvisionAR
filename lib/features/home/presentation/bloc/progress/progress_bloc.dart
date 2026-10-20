import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:molecular_vision_ar/core/errors/app_exception.dart';
import 'package:molecular_vision_ar/data/repositories/progress_repository.dart';
import 'progress_event.dart';
import 'progress_state.dart';

class ProgressBloc extends Bloc<ProgressEvent, ProgressState> {
  final ProgressRepository _repository;

  ProgressBloc({required ProgressRepository repository})
      : _repository = repository,
        super(const ProgressInitial()) {
    on<ProgressFetchRequested>(_onFetchRequested);
    on<ProgressRefreshRequested>(_onRefreshRequested);
  }

  Future<void> _onFetchRequested(
    ProgressFetchRequested event,
    Emitter<ProgressState> emit,
  ) async {
    if (state is! ProgressLoaded) {
      emit(const ProgressLoading());
    }

    try {
      final progress = await _repository.getProgress();
      emit(ProgressLoaded(progress: progress));
    } on AppException catch (e) {
      emit(ProgressFailure(e.message));
    } catch (e) {
      emit(ProgressFailure(e.toString()));
    }
  }

  Future<void> _onRefreshRequested(
    ProgressRefreshRequested event,
    Emitter<ProgressState> emit,
  ) async {
    if (state is ProgressLoaded) {
      emit((state as ProgressLoaded).copyWith(isRefreshing: true));
    }

    try {
      final progress = await _repository.getProgress();
      emit(ProgressLoaded(progress: progress, isRefreshing: false));
    } on AppException catch (e) {
      emit(ProgressFailure(e.message));
    } catch (e) {
      emit(ProgressFailure(e.toString()));
    }
  }
}
