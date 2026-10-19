import 'package:flutter_test/flutter_test.dart';
import 'package:molecular_vision_ar/core/errors/app_exception.dart';
import 'package:molecular_vision_ar/data/models/category/category_model.dart';
import 'package:molecular_vision_ar/data/models/material/material_model.dart';
import 'package:molecular_vision_ar/features/materi/data/repositories/materi_repository.dart';
import 'package:molecular_vision_ar/features/materi/presentation/bloc/materi_bloc.dart';
import 'package:molecular_vision_ar/features/materi/presentation/bloc/materi_event.dart';
import 'package:molecular_vision_ar/features/materi/presentation/bloc/materi_state.dart';

class MockMateriRepository extends MateriRepository {
  List<CategoryModel> mockCategories = const [
    CategoryModel(idCategory: 1, name: 'Termokimia', order: 1),
    CategoryModel(idCategory: 2, name: 'Ikatan Kimia', order: 2),
  ];

  List<MaterialModel> mockMaterials = const [
    MaterialModel(
      idMaterial: 1,
      idCategory: 1,
      name: 'Energi Reaksi',
      order: 1,
      description: 'Memahami perubahan reaksi',
    ),
    MaterialModel(
      idMaterial: 2,
      idCategory: 1,
      name: 'Eksoterm & Endoterm',
      order: 2,
      description: 'Mempelajari perbedaan reaksi eksoterm & endoterm',
    ),
    MaterialModel(
      idMaterial: 3,
      idCategory: 2,
      name: 'Ikatan Kovalen',
      order: 1,
      description: 'Penggunaan bersama pasangan elektron',
    ),
  ];

  bool shouldFail = false;

  @override
  Future<List<CategoryModel>> getCategories() async {
    if (shouldFail) {
      throw ServerException('Gagal memuat kategori kimia.');
    }
    return mockCategories;
  }

  @override
  Future<List<MaterialModel>> getMaterials({
    int? idCategory,
    String? search,
  }) async {
    if (shouldFail) {
      throw ServerException('Gagal memuat materi kimia.');
    }

    var list = mockMaterials;
    if (idCategory != null) {
      list = list.where((m) => m.idCategory == idCategory).toList();
    }
    if (search != null && search.trim().isNotEmpty) {
      final q = search.trim().toLowerCase();
      list = list
          .where((m) =>
              m.name.toLowerCase().contains(q) ||
              (m.description?.toLowerCase().contains(q) ?? false))
          .toList();
    }
    return list;
  }

  @override
  Future<MaterialModel> getMaterialDetail(int id) async {
    if (shouldFail) {
      throw ServerException('Materi tidak ditemukan.');
    }
    return mockMaterials.firstWhere((m) => m.idMaterial == id);
  }
}

void main() {
  group('MateriBloc Tests', () {
    late MockMateriRepository repository;
    late MateriBloc bloc;

    setUp(() {
      repository = MockMateriRepository();
      bloc = MateriBloc(repository: repository);
    });

    tearDown(() {
      bloc.close();
    });

    test('initial state is MateriInitial', () {
      expect(bloc.state, const MateriInitial());
    });

    test('emits [MateriLoading, MateriLoaded] on successful MateriFetchRequested', () async {
      final expectedStates = [
        const MateriLoading(),
        isA<MateriLoaded>()
            .having((s) => s.categories.length, 'categories count', 2)
            .having((s) => s.materials.length, 'materials count', 3)
            .having((s) => s.selectedCategoryId, 'selected category', isNull),
      ];

      expectLater(bloc.stream, emitsInOrder(expectedStates));
      bloc.add(const MateriFetchRequested());
    });

    test('emits [MateriLoading, MateriFailure] on failed MateriFetchRequested', () async {
      repository.shouldFail = true;

      final expectedStates = [
        const MateriLoading(),
        isA<MateriFailure>().having(
          (s) => s.message,
          'message',
          contains('Gagal memuat'),
        ),
      ];

      expectLater(bloc.stream, emitsInOrder(expectedStates));
      bloc.add(const MateriFetchRequested());
    });

    test('filters materials when MateriCategorySelected is dispatched', () async {
      bloc.add(const MateriFetchRequested());
      await bloc.stream.firstWhere((s) => s is MateriLoaded);

      expectLater(
        bloc.stream,
        emitsInOrder([
          isA<MateriLoaded>()
              .having((s) => s.selectedCategoryId, 'category', 1)
              .having((s) => s.isFiltering, 'isFiltering', true),
          isA<MateriLoaded>()
              .having((s) => s.selectedCategoryId, 'category', 1)
              .having((s) => s.materials.length, 'filtered count', 2)
              .having((s) => s.isFiltering, 'isFiltering', false),
        ]),
      );

      bloc.add(const MateriCategorySelected(1));
    });

    test('searches materials when MateriSearchSubmitted is dispatched', () async {
      bloc.add(const MateriFetchRequested());
      await bloc.stream.firstWhere((s) => s is MateriLoaded);

      expectLater(
        bloc.stream,
        emitsInOrder([
          isA<MateriLoaded>()
              .having((s) => s.searchQuery, 'query', 'Kovalen')
              .having((s) => s.isFiltering, 'isFiltering', true),
          isA<MateriLoaded>()
              .having((s) => s.searchQuery, 'query', 'Kovalen')
              .having((s) => s.materials.length, 'search result count', 1)
              .having((s) => s.materials.first.name, 'first material', 'Ikatan Kovalen')
              .having((s) => s.isFiltering, 'isFiltering', false),
        ]),
      );

      bloc.add(const MateriSearchSubmitted('Kovalen'));
    });

    test('reloads data when MateriRefreshRequested is dispatched', () async {
      bloc.add(const MateriFetchRequested());
      await bloc.stream.firstWhere((s) => s is MateriLoaded);

      expectLater(
        bloc.stream,
        emitsInOrder([
          isA<MateriLoaded>()
              .having((s) => s.isFiltering, 'isFiltering', true),
          isA<MateriLoaded>()
              .having((s) => s.categories.length, 'categories count', 2)
              .having((s) => s.materials.length, 'materials count', 3)
              .having((s) => s.isFiltering, 'isFiltering', false),
        ]),
      );

      bloc.add(const MateriRefreshRequested());
    });
  });
}
