import 'package:flutter_test/flutter_test.dart';
import 'package:molecular_vision_ar/core/errors/app_exception.dart';
import 'package:molecular_vision_ar/data/models/molecule/molecule_model.dart';
import 'package:molecular_vision_ar/features/molecule/data/repositories/molecule_repository.dart';
import 'package:molecular_vision_ar/features/molecule/presentation/bloc/molecule_bloc.dart';
import 'package:molecular_vision_ar/features/molecule/presentation/bloc/molecule_event.dart';
import 'package:molecular_vision_ar/features/molecule/presentation/bloc/molecule_state.dart';

class MockMoleculeRepository extends MoleculeRepository {
  List<MoleculeModel> mockMolecules = const [
    MoleculeModel(
      idMolecule: 1,
      name: 'Air',
      order: 1,
      formula: 'H2O',
      shape: 'Bengkok (Bent)',
      bent: '104.5°',
      bondType: 'Kovalen Polar',
      model3dUrl: 'https://assets.mvar.id/models/h2o.glb',
      description: 'Molekul air tersusun dari dua atom H dan satu atom O.',
    ),
    MoleculeModel(
      idMolecule: 2,
      name: 'Karbon Dioksida',
      order: 2,
      formula: 'CO2',
      shape: 'Linear',
      bent: '180°',
      bondType: 'Kovalen Nonpolar',
      model3dUrl: 'https://assets.mvar.id/models/co2.glb',
      description: 'Molekul linear dengan dua ikatan rangkap.',
    ),
    MoleculeModel(
      idMolecule: 3,
      name: 'Metana',
      order: 3,
      formula: 'CH4',
      shape: 'Tetrahedral',
      bent: '109.5°',
      bondType: 'Kovalen Nonpolar',
      model3dUrl: 'https://assets.mvar.id/models/ch4.glb',
      description: 'Molekul gas alam tetrahedral.',
    ),
  ];

  bool shouldFail = false;
  int? lastRecordedMoleculeId;
  Set<int> mockCompletedMoleculeIds = {1};

  @override
  Future<Set<int>> getCompletedMoleculeIds() async {
    return mockCompletedMoleculeIds;
  }

  @override
  Future<List<MoleculeModel>> getMolecules({
    String? search,
    String? shape,
  }) async {
    if (shouldFail) {
      throw ServerException('Gagal memuat katalog molekul 3D.');
    }

    var list = mockMolecules;
    if (shape != null && shape.isNotEmpty && shape != 'Semua') {
      list = list.where((m) => m.shape == shape).toList();
    }
    if (search != null && search.isNotEmpty) {
      final q = search.toLowerCase();
      list = list.where((m) {
        return m.name.toLowerCase().contains(q) ||
            m.formula.toLowerCase().contains(q);
      }).toList();
    }

    return list;
  }

  @override
  Future<MoleculeModel> getMoleculeDetail(int id) async {
    if (shouldFail) {
      throw ServerException('Gagal memuat detail molekul.');
    }
    return mockMolecules.firstWhere(
      (m) => m.idMolecule == id,
      orElse: () => mockMolecules.first,
    );
  }

  @override
  Future<bool> saveMoleculeProgress(
    int idMolecule, {
    bool isCompleted = true,
  }) async {
    lastRecordedMoleculeId = idMolecule;
    mockCompletedMoleculeIds.add(idMolecule);
    return true;
  }
}

void main() {
  group('MoleculeBloc Tests', () {
    late MockMoleculeRepository mockRepository;
    late MoleculeBloc bloc;

    setUp(() {
      mockRepository = MockMoleculeRepository();
      bloc = MoleculeBloc(repository: mockRepository);
    });

    tearDown(() {
      bloc.close();
    });

    test('initial state is MoleculeInitial', () {
      expect(bloc.state, equals(const MoleculeInitial()));
    });

    test('emits [MoleculeLoading, MoleculeLoaded] on successful MoleculeFetchRequested', () async {
      final expected = [
        const MoleculeLoading(),
        isA<MoleculeLoaded>()
            .having((s) => s.molecules.length, 'molecules count', 3)
            .having((s) => s.availableShapes, 'available shapes',
                containsAll(['Semua', 'Bengkok (Bent)', 'Linear', 'Tetrahedral']))
            .having((s) => s.selectedShape, 'selected shape', isNull)
            .having((s) => s.searchQuery, 'search query', ''),
      ];

      expectLater(bloc.stream, emitsInOrder(expected));
      bloc.add(const MoleculeFetchRequested());
    });

    test('emits [MoleculeLoading, MoleculeFailure] on failed MoleculeFetchRequested', () async {
      mockRepository.shouldFail = true;

      final expected = [
        const MoleculeLoading(),
        isA<MoleculeFailure>().having(
          (s) => s.message,
          'message',
          'Gagal memuat katalog molekul 3D.',
        ),
      ];

      expectLater(bloc.stream, emitsInOrder(expected));
      bloc.add(const MoleculeFetchRequested());
    });

    test('filters molecules when MoleculeShapeSelected is dispatched', () async {
      bloc.add(const MoleculeFetchRequested());
      await bloc.stream.firstWhere((s) => s is MoleculeLoaded);

      final expected = [
        isA<MoleculeLoaded>()
            .having((s) => s.isFiltering, 'isFiltering true', true)
            .having((s) => s.selectedShape, 'selected shape', 'Linear'),
        isA<MoleculeLoaded>()
            .having((s) => s.isFiltering, 'isFiltering false', false)
            .having((s) => s.selectedShape, 'selected shape', 'Linear')
            .having((s) => s.molecules.length, 'filtered molecules count', 1)
            .having((s) => s.molecules.first.formula, 'formula', 'CO2'),
      ];

      expectLater(bloc.stream, emitsInOrder(expected));
      bloc.add(const MoleculeShapeSelected('Linear'));
    });

    test('searches molecules when MoleculeSearchSubmitted is dispatched', () async {
      bloc.add(const MoleculeFetchRequested());
      await bloc.stream.firstWhere((s) => s is MoleculeLoaded);

      final expected = [
        isA<MoleculeLoaded>()
            .having((s) => s.isFiltering, 'isFiltering true', true)
            .having((s) => s.searchQuery, 'search query', 'CH4'),
        isA<MoleculeLoaded>()
            .having((s) => s.isFiltering, 'isFiltering false', false)
            .having((s) => s.molecules.length, 'search result count', 1)
            .having((s) => s.molecules.first.name, 'name', 'Metana'),
      ];

      expectLater(bloc.stream, emitsInOrder(expected));
      bloc.add(const MoleculeSearchSubmitted('CH4'));
    });

    test('reloads data when MoleculeRefreshRequested is dispatched', () async {
      bloc.add(const MoleculeFetchRequested());
      await bloc.stream.firstWhere((s) => s is MoleculeLoaded);

      final expected = [
        isA<MoleculeLoaded>().having((s) => s.isFiltering, 'isFiltering', true),
        isA<MoleculeLoaded>()
            .having((s) => s.isFiltering, 'isFiltering', false)
            .having((s) => s.molecules.length, 'molecules count', 3),
      ];

      expectLater(bloc.stream, emitsInOrder(expected));
      bloc.add(const MoleculeRefreshRequested());
    });

    test('records progress and updates completedMoleculeIds when MoleculeProgressSubmitted is dispatched', () async {
      bloc.add(const MoleculeFetchRequested());
      await bloc.stream.firstWhere((s) => s is MoleculeLoaded);

      final expected = [
        isA<MoleculeLoaded>()
            .having(
              (s) => s.progressSuccessMoleculeId,
              'progressSuccessMoleculeId',
              2,
            )
            .having(
              (s) => s.completedMoleculeIds.contains(2),
              'completedMoleculeIds contains 2',
              true,
            ),
      ];

      expectLater(bloc.stream, emitsInOrder(expected));
      bloc.add(const MoleculeProgressSubmitted(2));

      await Future.delayed(const Duration(milliseconds: 50));
      expect(mockRepository.lastRecordedMoleculeId, equals(2));
      expect(mockRepository.mockCompletedMoleculeIds, contains(2));
    });
  });
}
