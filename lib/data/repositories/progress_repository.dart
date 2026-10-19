import '../../core/constants/api_endpoints.dart';
import '../../core/network/dio_client.dart';
import '../models/progress/progress_model.dart';

/// Repository untuk mengambil data rekap progres belajar siswa
class ProgressRepository {
  final DioClient _dioClient;

  ProgressRepository({required DioClient dioClient}) : _dioClient = dioClient;

  /// Mengambil metrik progres materi, molekul, kuis, dan keseluruhan
  Future<ProgressModel> getProgress() async {
    final response = await _dioClient.get(ApiEndpoints.progress);
    final data = response.data['data'] as Map<String, dynamic>? ?? {};
    return ProgressModel.fromJson(data);
  }
}
