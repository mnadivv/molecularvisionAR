import 'package:flutter_test/flutter_test.dart';
import 'package:molecular_vision_ar/core/errors/app_exception.dart';
import 'package:molecular_vision_ar/data/models/exercise/exercise_model.dart';
import 'package:molecular_vision_ar/features/quiz/data/repositories/quiz_repository.dart';
import 'package:molecular_vision_ar/features/quiz/presentation/bloc/quiz_category/quiz_category_bloc.dart';
import 'package:molecular_vision_ar/features/quiz/presentation/bloc/quiz_category/quiz_category_event.dart';
import 'package:molecular_vision_ar/features/quiz/presentation/bloc/quiz_category/quiz_category_state.dart';
import 'package:molecular_vision_ar/features/quiz/presentation/bloc/quiz_play/quiz_play_bloc.dart';
import 'package:molecular_vision_ar/features/quiz/presentation/bloc/quiz_play/quiz_play_event.dart';
import 'package:molecular_vision_ar/features/quiz/presentation/bloc/quiz_play/quiz_play_state.dart';

class MockQuizRepository implements QuizRepository {
  bool shouldFail = false;

  final List<QuizCategoryModel> mockCategories = const [
    QuizCategoryModel(
      idCategory: 1,
      name: 'Geometri Molekul',
      description: 'Latihan bentuk geometri molekul',
      questionsCount: 5,
    ),
    QuizCategoryModel(
      idCategory: 2,
      name: 'Ikatan Kimia',
      description: 'Latihan ikatan kovalen & ion',
      questionsCount: 3,
    ),
  ];

  final Set<int> mockCompletedIds = {1};

  final List<ExerciseQuestionModel> mockQuestions = const [
    ExerciseQuestionModel(
      idQuestion: 101,
      idCategory: 1,
      questionText: 'Berapakah sudut ikatan pada molekul air (H2O)?',
      questionType: 'multiple_choice',
      options: [
        ExerciseOptionModel(idOption: 1, optionText: '104.5°'),
        ExerciseOptionModel(idOption: 2, optionText: '109.5°'),
        ExerciseOptionModel(idOption: 3, optionText: '120°'),
      ],
    ),
    ExerciseQuestionModel(
      idQuestion: 102,
      idCategory: 1,
      questionText: 'Pilihlah molekul yang memiliki ikatan polar.',
      questionType: 'multiple_select',
      options: [
        ExerciseOptionModel(idOption: 4, optionText: 'H2O'),
        ExerciseOptionModel(idOption: 5, optionText: 'NH3'),
        ExerciseOptionModel(idOption: 6, optionText: 'CH4'),
      ],
    ),
    ExerciseQuestionModel(
      idQuestion: 103,
      idCategory: 1,
      questionText: 'Molekul CO2 memiliki geometri linier.',
      questionType: 'true_false',
      options: [
        ExerciseOptionModel(idOption: 7, optionText: 'Benar'),
        ExerciseOptionModel(idOption: 8, optionText: 'Salah'),
      ],
    ),
    ExerciseQuestionModel(
      idQuestion: 104,
      idCategory: 1,
      questionText: 'Berapakah sudut ikatan tetrahedral dalam derajat?',
      questionType: 'short_answer',
    ),
    ExerciseQuestionModel(
      idQuestion: 105,
      idCategory: 1,
      questionText: 'Jodohkan molekul berikut dengan bentuk geometrinya.',
      questionType: 'matching',
      options: [
        ExerciseOptionModel(idOption: 9, optionText: 'H2O'),
        ExerciseOptionModel(idOption: 10, optionText: 'CH4'),
      ],
      matchingTargets: ['Bengkok', 'Tetrahedral'],
    ),
  ];

  @override
  Future<List<QuizCategoryModel>> getQuizCategories() async {
    if (shouldFail) {
      throw ServerException('Gagal memuat kategori kuis');
    }
    return mockCategories;
  }

  @override
  Future<Set<int>> getCompletedQuizCategoryIds() async {
    if (shouldFail) {
      throw ServerException('Gagal memuat progress kuis');
    }
    return mockCompletedIds;
  }

  @override
  Future<List<ExerciseQuestionModel>> getQuizQuestions(int idCategory) async {
    if (shouldFail) {
      throw ServerException('Gagal memuat butir soal');
    }
    return mockQuestions;
  }

  @override
  Future<QuizSubmissionResponseModel> submitQuizAnswers(
    int idCategory,
    List<Map<String, dynamic>> answers,
  ) async {
    if (shouldFail) {
      throw ServerException('Gagal mengirimkan jawaban kuis');
    }
    return const QuizSubmissionResponseModel(
      idCategory: 1,
      summary: QuizSubmissionSummaryModel(
        totalQuestions: 5,
        answered: 5,
        correct: 4,
        wrong: 1,
        score: 80.0,
        isCompleted: true,
      ),
      results: [
        QuizSubmissionItemResult(
          idQuestion: 101,
          isCorrect: true,
          userAnswer: '104.5°',
          correctAnswer: '104.5°',
        ),
      ],
    );
  }

  @override
  Future<QuizReportModel> getQuizResult(int idCategory) async {
    if (shouldFail) {
      throw ServerException('Gagal memuat hasil kuis');
    }
    return const QuizReportModel(
      idCategory: 1,
      categoryName: 'Geometri Molekul',
      totalQuestions: 5,
      answered: 5,
      correct: 4,
      wrong: 1,
      score: 80.0,
      isCompleted: true,
      reviews: [
        QuizReviewItemModel(
          idQuestion: 101,
          questionName: 'Berapakah sudut ikatan pada molekul air (H2O)?',
          questionType: 'multiple_choice',
          isAnswered: true,
          isCorrect: true,
          userAnswer: '104.5°',
          correctAnswer: '104.5°',
        ),
      ],
    );
  }
}

void main() {
  group('QuizCategoryBloc Tests', () {
    late MockQuizRepository repository;
    late QuizCategoryBloc bloc;

    setUp(() {
      repository = MockQuizRepository();
      bloc = QuizCategoryBloc(repository: repository);
    });

    tearDown(() {
      bloc.close();
    });

    test('Initial state is QuizCategoryInitial', () {
      expect(bloc.state, const QuizCategoryInitial());
    });

    test('QuizCategoryFetchRequested emits [Loading, Loaded] with categories & completedIds',
        () async {
      final expectedStates = [
        const QuizCategoryLoading(),
        QuizCategoryLoaded(
          categories: repository.mockCategories,
          completedCategoryIds: const {1},
          searchQuery: '',
        ),
      ];

      expectLater(bloc.stream, emitsInOrder(expectedStates));
      bloc.add(const QuizCategoryFetchRequested());
    });

    test('QuizCategoryFetchRequested with search filters properly', () async {
      bloc.add(const QuizCategoryFetchRequested(search: 'Geometri'));

      await expectLater(
        bloc.stream,
        emitsThrough(predicate<QuizCategoryState>((state) {
          if (state is QuizCategoryLoaded) {
            return state.filteredCategories.length == 1 &&
                state.filteredCategories.first.name == 'Geometri Molekul';
          }
          return false;
        })),
      );
    });

    test('QuizCategoryFetchRequested emits Failure on error', () async {
      repository.shouldFail = true;
      final expectedStates = [
        const QuizCategoryLoading(),
        const QuizCategoryFailure('Gagal memuat kategori kuis'),
      ];

      expectLater(bloc.stream, emitsInOrder(expectedStates));
      bloc.add(const QuizCategoryFetchRequested());
    });
  });

  group('QuizPlayBloc Tests', () {
    late MockQuizRepository repository;
    late QuizPlayBloc bloc;

    setUp(() {
      repository = MockQuizRepository();
      bloc = QuizPlayBloc(repository: repository);
    });

    tearDown(() {
      bloc.close();
    });

    test('QuizPlayStarted fetches questions and sets currentIndex = 0', () async {
      bloc.add(const QuizPlayStarted(1));

      await expectLater(
        bloc.stream,
        emitsThrough(predicate<QuizPlayState>((state) {
          if (state is QuizPlayInProgress) {
            return state.questions.length == 5 &&
                state.currentIndex == 0 &&
                state.answers.isEmpty;
          }
          return false;
        })),
      );
    });

    test('QuizQuestionNavigated navigates between questions', () async {
      bloc.add(const QuizPlayStarted(1));
      await bloc.stream.firstWhere((s) => s is QuizPlayInProgress);

      bloc.add(const QuizQuestionNavigated(2));
      await expectLater(
        bloc.stream,
        emitsThrough(predicate<QuizPlayState>((state) {
          return state is QuizPlayInProgress && state.currentIndex == 2;
        })),
      );
    });

    test('QuizAnswerUpdated records answer for a question', () async {
      bloc.add(const QuizPlayStarted(1));
      await bloc.stream.firstWhere((s) => s is QuizPlayInProgress);

      // Select option 1 for question 101
      bloc.add(const QuizAnswerUpdated(idQuestion: 101, answer: 1));

      await expectLater(
        bloc.stream,
        emitsThrough(predicate<QuizPlayState>((state) {
          return state is QuizPlayInProgress && state.answers[101] == 1;
        })),
      );
    });

    test('QuizSubmitRequested submits answers and emits QuizPlaySubmitted', () async {
      bloc.add(const QuizPlayStarted(1));
      await bloc.stream.firstWhere((s) => s is QuizPlayInProgress);

      bloc.add(const QuizAnswerUpdated(idQuestion: 101, answer: 1));
      bloc.add(const QuizSubmitRequested());

      await expectLater(
        bloc.stream,
        emitsThrough(predicate<QuizPlayState>((state) {
          if (state is QuizPlaySubmitted) {
            return state.response.summary.score == 80.0 &&
                state.response.summary.correct == 4;
          }
          return false;
        })),
      );
    });
  });
}
