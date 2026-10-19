import '../../../../core/constants/api_endpoints.dart';
import '../../../../core/network/dio_client.dart';
import '../../../../data/models/exercise/exercise_model.dart';

/// Repository untuk komunikasi modul Kuis & Latihan Soal dengan Backend API
class QuizRepository {
  final DioClient _dioClient;

  QuizRepository({required DioClient dioClient}) : _dioClient = dioClient;

  /// Mengambil seluruh kategori kuis/latihan yang tersedia dari backend
  Future<List<QuizCategoryModel>> getQuizCategories() async {
    final response = await _dioClient.get(ApiEndpoints.exercises);
    final data = response.data['data'] as List? ?? [];
    return data
        .whereType<Map<String, dynamic>>()
        .map(QuizCategoryModel.fromJson)
        .toList();
  }

  /// Mengambil Set ID kategori kuis yang telah selesai dikerjakan oleh user
  Future<Set<int>> getCompletedQuizCategoryIds() async {
    try {
      final response = await _dioClient.get(ApiEndpoints.progress);
      final completedMap =
          response.data['data']?['completed_ids'] as Map<String, dynamic>? ?? {};
      final list = completedMap['quiz'] as List? ?? [];
      return list
          .map((id) => int.tryParse(id.toString()) ?? 0)
          .where((id) => id > 0)
          .toSet();
    } catch (_) {
      return {};
    }
  }

  /// Mengambil butir soal kuis untuk kategori tertentu (Anti-cheat)
  Future<List<ExerciseQuestionModel>> getQuizQuestions(int idCategory) async {
    final response =
        await _dioClient.get(ApiEndpoints.exerciseDetail(idCategory));
    final data = response.data['data']?['questions'] as List? ?? [];
    return data
        .whereType<Map<String, dynamic>>()
        .map(ExerciseQuestionModel.fromJson)
        .toList();
  }

  /// Mengirimkan seluruh jawaban siswa untuk dikoreksi (auto-grading)
  Future<QuizSubmissionResponseModel> submitQuizAnswers(
    int idCategory,
    List<Map<String, dynamic>> answers,
  ) async {
    final response = await _dioClient.post(
      ApiEndpoints.exerciseDetail(idCategory),
      data: {
        'answers': answers,
      },
    );
    return QuizSubmissionResponseModel.fromJson(
      response.data['data'] as Map<String, dynamic>? ?? {},
    );
  }

  /// Mengambil rapor hasil pengerjaan kuis siswa untuk kategori tertentu
  Future<QuizReportModel> getQuizResult(int idCategory) async {
    final response =
        await _dioClient.get(ApiEndpoints.exerciseResult(idCategory));
    return QuizReportModel.fromJson(
      response.data['data'] as Map<String, dynamic>? ?? {},
    );
  }
}
