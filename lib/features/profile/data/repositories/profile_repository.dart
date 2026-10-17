import '../../../../core/constants/api_endpoints.dart';
import '../../../../core/network/dio_client.dart';
import '../../../../data/models/auth/degree_model.dart';
import '../../../../data/models/auth/user_model.dart';

/// Repository untuk menangani seluruh operasi data profil pengguna dan akun siswa
class ProfileRepository {
  final DioClient _dioClient;

  ProfileRepository({DioClient? dioClient}) : _dioClient = dioClient ?? DioClient();

  /// Mengambil data profil siswa terkini berdasarkan Bearer token
  Future<UserModel> getProfile() async {
    final response = await _dioClient.get(ApiEndpoints.me);
    final data = response.data['data'] as Map<String, dynamic>;
    final userJson = data['user'] as Map<String, dynamic>;
    return UserModel.fromJson(userJson);
  }

  /// Memperbarui informasi profil siswa (nama, email, sekolah, jenjang kelas)
  Future<UserModel> updateProfile({
    required String name,
    required String email,
    String? school,
    int? idDegree,
  }) async {
    final response = await _dioClient.put(
      ApiEndpoints.profile,
      data: {
        'name': name.trim(),
        'email': email.trim(),
        'school': school?.trim(),
        'id_degree': idDegree,
      },
    );

    final data = response.data['data'] as Map<String, dynamic>;
    final userJson = data['user'] as Map<String, dynamic>;
    return UserModel.fromJson(userJson);
  }

  /// Memperbarui kata sandi akun siswa dengan memverifikasi password lama
  Future<String> updatePassword({
    required String currentPassword,
    required String newPassword,
  }) async {
    final response = await _dioClient.put(
      ApiEndpoints.updatePassword,
      data: {
        'current_password': currentPassword,
        'password': newPassword,
      },
    );

    return response.data['message'] as String? ?? 'Kata sandi berhasil diperbarui.';
  }

  /// Mengambil daftar master jenjang kelas untuk pilihan dropdown profil
  Future<List<DegreeModel>> getDegrees() async {
    final response = await _dioClient.get(ApiEndpoints.degrees);
    final data = response.data['data'] as List<dynamic>? ?? [];

    return data
        .whereType<Map<String, dynamic>>()
        .map(DegreeModel.fromJson)
        .toList();
  }
}
