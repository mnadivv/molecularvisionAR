import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../../core/errors/app_exception.dart';
import '../../../../data/models/molecule/molecule_model.dart';
import '../../data/repositories/molecule_repository.dart';
import 'molecule_event.dart';
import 'molecule_state.dart';

/// BLoC pengelola katalog molekul kimia 3D, filter bentuk, dan pencarian
class MoleculeBloc extends Bloc<MoleculeEvent, MoleculeState> {
  final MoleculeRepository _repository;

  MoleculeBloc({required MoleculeRepository repository})
      : _repository = repository,
        super(const MoleculeInitial()) {
    on<MoleculeFetchRequested>(_onFetchRequested);
    on<MoleculeShapeSelected>(_onShapeSelected);
    on<MoleculeSearchSubmitted>(_onSearchSubmitted);
    on<MoleculeRefreshRequested>(_onRefreshRequested);
    on<MoleculeProgressSubmitted>(_onProgressSubmitted);
  }

  Future<void> _onFetchRequested(
    MoleculeFetchRequested event,
    Emitter<MoleculeState> emit,
  ) async {
    emit(const MoleculeLoading());
    try {
      final results = await Future.wait([
        _repository.getMolecules(
          shape: event.shape,
          search: event.search,
        ),
        _repository.getCompletedMoleculeIds(),
      ]);

      final molecules = results[0] as List<MoleculeModel>;
      final completedIds = results[1] as Set<int>;
      final shapes = _extractShapes(molecules);

      emit(MoleculeLoaded(
        molecules: molecules,
        availableShapes: shapes,
        selectedShape: event.shape,
        searchQuery: event.search ?? '',
        completedMoleculeIds: completedIds,
      ));
    } on AppException catch (e) {
      emit(MoleculeFailure(e.message));
    } catch (e) {
      emit(MoleculeFailure(e.toString()));
    }
  }

  Future<void> _onShapeSelected(
    MoleculeShapeSelected event,
    Emitter<MoleculeState> emit,
  ) async {
    if (state is! MoleculeLoaded) return;
    final currentState = state as MoleculeLoaded;

    final targetShape = (event.shape == null || event.shape == 'Semua')
        ? null
        : event.shape;

    if (currentState.selectedShape == targetShape) return;

    emit(currentState.copyWith(
      selectedShape: targetShape,
      clearSelectedShape: targetShape == null,
      isFiltering: true,
    ));

    try {
      final molecules = await _repository.getMolecules(
        shape: targetShape,
        search: currentState.searchQuery,
      );

      emit(currentState.copyWith(
        selectedShape: targetShape,
        clearSelectedShape: targetShape == null,
        molecules: molecules,
        isFiltering: false,
      ));
    } on AppException catch (e) {
      emit(MoleculeFailure(e.message));
    } catch (e) {
      emit(MoleculeFailure(e.toString()));
    }
  }

  Future<void> _onSearchSubmitted(
    MoleculeSearchSubmitted event,
    Emitter<MoleculeState> emit,
  ) async {
    if (state is! MoleculeLoaded) return;
    final currentState = state as MoleculeLoaded;

    emit(currentState.copyWith(
      searchQuery: event.query,
      isFiltering: true,
    ));

    try {
      final molecules = await _repository.getMolecules(
        shape: currentState.selectedShape,
        search: event.query,
      );

      emit(currentState.copyWith(
        searchQuery: event.query,
        molecules: molecules,
        isFiltering: false,
      ));
    } on AppException catch (e) {
      emit(MoleculeFailure(e.message));
    } catch (e) {
      emit(MoleculeFailure(e.toString()));
    }
  }

  Future<void> _onRefreshRequested(
    MoleculeRefreshRequested event,
    Emitter<MoleculeState> emit,
  ) async {
    final currentShape = state is MoleculeLoaded
        ? (state as MoleculeLoaded).selectedShape
        : null;
    final currentQuery = state is MoleculeLoaded
        ? (state as MoleculeLoaded).searchQuery
        : null;

    if (state is MoleculeLoaded) {
      emit((state as MoleculeLoaded).copyWith(isFiltering: true));
    }

    try {
      final results = await Future.wait([
        _repository.getMolecules(
          shape: currentShape,
          search: currentQuery,
        ),
        _repository.getCompletedMoleculeIds(),
      ]);

      final molecules = results[0] as List<MoleculeModel>;
      final completedIds = results[1] as Set<int>;
      final shapes = _extractShapes(molecules);

      emit(MoleculeLoaded(
        molecules: molecules,
        availableShapes: shapes,
        selectedShape: currentShape,
        searchQuery: currentQuery ?? '',
        completedMoleculeIds: completedIds,
        isFiltering: false,
      ));
    } on AppException catch (e) {
      emit(MoleculeFailure(e.message));
    } catch (e) {
      emit(MoleculeFailure(e.toString()));
    }
  }

  Future<void> _onProgressSubmitted(
    MoleculeProgressSubmitted event,
    Emitter<MoleculeState> emit,
  ) async {
    try {
      await _repository.saveMoleculeProgress(event.idMolecule);
      if (state is MoleculeLoaded) {
        final currentSet = (state as MoleculeLoaded).completedMoleculeIds;
        final updatedSet = Set<int>.from(currentSet)..add(event.idMolecule);
        emit((state as MoleculeLoaded).copyWith(
          progressSuccessMoleculeId: event.idMolecule,
          completedMoleculeIds: updatedSet,
        ));
      }
    } catch (_) {
      // Progres belajar tetap dicatat tanpa memblokir tampilan
    }
  }

  List<String> _extractShapes(List<MoleculeModel> molecules) {
    final shapes = <String>{'Semua'};
    for (final m in molecules) {
      if (m.shape != null && m.shape!.trim().isNotEmpty) {
        shapes.add(m.shape!.trim());
      }
    }
    return shapes.toList();
  }
}
