import 'package:flutter_test/flutter_test.dart';
import 'package:molecular_vision_ar/core/errors/app_exception.dart';
import 'package:molecular_vision_ar/data/models/progress/progress_model.dart';
import 'package:molecular_vision_ar/data/repositories/progress_repository.dart';
import 'package:molecular_vision_ar/features/home/presentation/bloc/progress/progress_bloc.dart';
import 'package:molecular_vision_ar/features/home/presentation/bloc/progress/progress_event.dart';
import 'package:molecular_vision_ar/features/home/presentation/bloc/progress/progress_state.dart';

class MockProgressRepository implements ProgressRepository {
  bool shouldFail = false;

  final ProgressModel mockProgress = const ProgressModel(
    materials: ProgressMetricModel(
      total: 15,
      completed: 11,
      uncompleted: 4,
      percentage: 73.3,
    ),
    molecules: ProgressMetricModel(
      total: 10,
      completed: 8,
      uncompleted: 2,
      percentage: 80.0,
    ),
    quizzes: ProgressMetricModel(
      total: 4,
      completed: 2,
      uncompleted: 2,
      percentage: 50.0,
    ),
    completedMaterialIds: [1, 2, 4],
    completedMoleculeIds: [1, 3],
    completedCategoryIds: [1, 2],
  );

  @override
  Future<ProgressModel> getProgress() async {
    if (shouldFail) {
      throw ServerException('Gagal memuat progres belajar.');
    }
    return mockProgress;
  }
}

void main() {
  group('ProgressBloc Tests', () {
    late MockProgressRepository repository;
    late ProgressBloc bloc;

    setUp(() {
      repository = MockProgressRepository();
      bloc = ProgressBloc(repository: repository);
    });

    tearDown(() {
      bloc.close();
    });

    test('Initial state is ProgressInitial', () {
      expect(bloc.state, const ProgressInitial());
    });

    test('ProgressFetchRequested emits [Loading, Loaded] with complete metrics',
        () async {
      final expectedStates = [
        const ProgressLoading(),
        ProgressLoaded(progress: repository.mockProgress),
      ];

      expectLater(bloc.stream, emitsInOrder(expectedStates));
      bloc.add(const ProgressFetchRequested());
    });

    test('ProgressRefreshRequested updates data without blanking loading state',
        () async {
      bloc.add(const ProgressFetchRequested());
      await bloc.stream.firstWhere((s) => s is ProgressLoaded);

      bloc.add(const ProgressRefreshRequested());
      await expectLater(
        bloc.stream,
        emitsThrough(predicate<ProgressState>((state) {
          return state is ProgressLoaded &&
              state.progress.materials.completed == 11 &&
              state.progress.molecules.completed == 8 &&
              state.progress.quizzes.completed == 2;
        })),
      );
    });

    test('ProgressFetchRequested emits Failure on error', () async {
      repository.shouldFail = true;
      final expectedStates = [
        const ProgressLoading(),
        const ProgressFailure('Gagal memuat progres belajar.'),
      ];

      expectLater(bloc.stream, emitsInOrder(expectedStates));
      bloc.add(const ProgressFetchRequested());
    });
  });
}
