import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../../core/errors/app_exception.dart';
import '../../../../data/models/category/category_model.dart';
import '../../../../data/models/material/material_model.dart';
import '../../data/repositories/materi_repository.dart';
import 'materi_event.dart';
import 'materi_state.dart';

/// BLoC pengelola state materi pembelajaran kimia & kategori filter
class MateriBloc extends Bloc<MateriEvent, MateriState> {
  final MateriRepository _repository;

  MateriBloc({required MateriRepository repository})
      : _repository = repository,
        super(const MateriInitial()) {
    on<MateriFetchRequested>(_onFetchRequested);
    on<MateriCategorySelected>(_onCategorySelected);
    on<MateriSearchSubmitted>(_onSearchSubmitted);
    on<MateriRefreshRequested>(_onRefreshRequested);
    on<MateriProgressSubmitted>(_onProgressSubmitted);
  }

  Future<void> _onFetchRequested(
    MateriFetchRequested event,
    Emitter<MateriState> emit,
  ) async {
    emit(const MateriLoading());
    try {
      final results = await Future.wait([
        _repository.getCategories(),
        _repository.getMaterials(
          idCategory: event.idCategory,
          search: event.search,
        ),
        _repository.getCompletedMaterialIds(),
      ]);

      final categories = results[0] as List<CategoryModel>;
      final materials = results[1] as List<MaterialModel>;
      final completedIds = results[2] as Set<int>;

      emit(MateriLoaded(
        categories: categories,
        materials: materials,
        selectedCategoryId: event.idCategory,
        searchQuery: event.search ?? '',
        completedMaterialIds: completedIds,
      ));
    } on AppException catch (e) {
      emit(MateriFailure(e.message));
    } catch (e) {
      emit(MateriFailure(e.toString()));
    }
  }

  Future<void> _onCategorySelected(
    MateriCategorySelected event,
    Emitter<MateriState> emit,
  ) async {
    if (state is! MateriLoaded) return;
    final currentState = state as MateriLoaded;

    // Hindari fetch ulang jika kategori yang diklik sama persis
    if (currentState.selectedCategoryId == event.idCategory) return;

    emit(currentState.copyWith(
      selectedCategoryId: event.idCategory,
      clearSelectedCategory: event.idCategory == null,
      isFiltering: true,
    ));

    try {
      final materials = await _repository.getMaterials(
        idCategory: event.idCategory,
        search: currentState.searchQuery,
      );
      emit(currentState.copyWith(
        selectedCategoryId: event.idCategory,
        clearSelectedCategory: event.idCategory == null,
        materials: materials,
        isFiltering: false,
      ));
    } on AppException catch (e) {
      emit(MateriFailure(e.message));
    } catch (e) {
      emit(MateriFailure(e.toString()));
    }
  }

  Future<void> _onSearchSubmitted(
    MateriSearchSubmitted event,
    Emitter<MateriState> emit,
  ) async {
    if (state is! MateriLoaded) return;
    final currentState = state as MateriLoaded;

    emit(currentState.copyWith(
      searchQuery: event.query,
      isFiltering: true,
    ));

    try {
      final materials = await _repository.getMaterials(
        idCategory: currentState.selectedCategoryId,
        search: event.query,
      );
      emit(currentState.copyWith(
        searchQuery: event.query,
        materials: materials,
        isFiltering: false,
      ));
    } on AppException catch (e) {
      emit(MateriFailure(e.message));
    } catch (e) {
      emit(MateriFailure(e.toString()));
    }
  }

  Future<void> _onRefreshRequested(
    MateriRefreshRequested event,
    Emitter<MateriState> emit,
  ) async {
    final selectedId = state is MateriLoaded
        ? (state as MateriLoaded).selectedCategoryId
        : null;
    final query = state is MateriLoaded
        ? (state as MateriLoaded).searchQuery
        : null;

    if (state is MateriLoaded) {
      emit((state as MateriLoaded).copyWith(isFiltering: true));
    }

    try {
      final results = await Future.wait([
        _repository.getCategories(),
        _repository.getMaterials(
          idCategory: selectedId,
          search: query,
        ),
        _repository.getCompletedMaterialIds(),
      ]);

      final categories = results[0] as List<CategoryModel>;
      final materials = results[1] as List<MaterialModel>;
      final completedIds = results[2] as Set<int>;

      emit(MateriLoaded(
        categories: categories,
        materials: materials,
        selectedCategoryId: selectedId,
        searchQuery: query ?? '',
        completedMaterialIds: completedIds,
        isFiltering: false,
      ));
    } on AppException catch (e) {
      emit(MateriFailure(e.message));
    } catch (e) {
      emit(MateriFailure(e.toString()));
    }
  }

  Future<void> _onProgressSubmitted(
    MateriProgressSubmitted event,
    Emitter<MateriState> emit,
  ) async {
    try {
      await _repository.saveMaterialProgress(event.idMaterial);
      if (state is MateriLoaded) {
        final currentSet = (state as MateriLoaded).completedMaterialIds;
        final updatedSet = Set<int>.from(currentSet)..add(event.idMaterial);
        emit((state as MateriLoaded).copyWith(
          progressSuccessMaterialId: event.idMaterial,
          completedMaterialIds: updatedSet,
        ));
      }
    } catch (_) {
      // Progres belajar tetap dicatat tanpa memblokir tampilan
    }
  }
}
