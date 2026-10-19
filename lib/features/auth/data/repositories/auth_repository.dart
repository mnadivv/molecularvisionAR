import '../../../../core/constants/api_endpoints.dart';
import '../../../../core/network/dio_client.dart';
import '../../../../core/storage/token_storage_service.dart';
import '../../../../data/models/auth/auth_response_model.dart';
import '../../../../data/models/auth/degree_model.dart';
import '../../../../data/models/auth/user_model.dart';

/// Repository untuk menangani seluruh operasi autentikasi dan manajemen sesi siswa
class AuthRepository {
  final DioClient _dioClient;
  final TokenStorageService _tokenStorage;

  AuthRepository({
    DioClient? dioClient,
    TokenStorageService? tokenStorage,
  })  : _dioClient = dioClient ?? DioClient(),
        _tokenStorage = tokenStorage ?? TokenStorageService();

  DioClient get dioClient => _dioClient;
  TokenStorageService get tokenStorage => _tokenStorage;

  /// Melakukan login siswa
  Future<AuthResponseModel> login({
    required String email,
    required String password,
  }) async {
    final response = await _dioClient.post(
      ApiEndpoints.login,
      data: {
        'email': email.trim(),
        'password': password,
      },
    );

    final data = response.data['data'] as Map<String, dynamic>;
    final authResponse = AuthResponseModel.fromJson(data);

    // Simpan token ke secure storage
    await _tokenStorage.saveToken(authResponse.token);

    return authResponse;
  }

  /// Melakukan registrasi siswa baru
  Future<AuthResponseModel> register({
    required String name,
    required String email,
    required String password,
    String? school,
    int? idDegree,
  }) async {
    final response = await _dioClient.post(
      ApiEndpoints.register,
      data: {
        'name': name.trim(),
        'email': email.trim(),
        'password': password,
        if (school != null && school.trim().isNotEmpty) 'school': school.trim(),
        if (idDegree != null) 'id_degree': idDegree,
      },
    );

    final data = response.data['data'] as Map<String, dynamic>;
    final authResponse = AuthResponseModel.fromJson(data);

    // Simpan token ke secure storage
    await _tokenStorage.saveToken(authResponse.token);

    return authResponse;
  }

  /// Mengambil daftar master jenjang kelas (untuk dropdown registrasi)
  Future<List<DegreeModel>> getDegrees() async {
    final response = await _dioClient.get(ApiEndpoints.degrees);
    final data = response.data['data'] as List<dynamic>? ?? [];

    return data
        .whereType<Map<String, dynamic>>()
        .map(DegreeModel.fromJson)
        .toList();
  }

  /// Mengambil data profil siswa saat ini (jika token masih aktif)
  Future<UserModel?> getCurrentUser() async {
    final hasToken = await _tokenStorage.hasToken();
    if (!hasToken) return null;

    try {
      final response = await _dioClient.get(ApiEndpoints.me);
      final data = response.data['data'] as Map<String, dynamic>? ?? {};
      final userData = data.containsKey('user') && data['user'] is Map<String, dynamic>
          ? data['user'] as Map<String, dynamic>
          : data;
      return UserModel.fromJson(userData);
    } catch (_) {
      // Jika token invalid/expired, hapus token lokal
      await _tokenStorage.deleteToken();
      return null;
    }
  }

  /// Melakukan logout dan mencabut token Sanctum di server
  Future<void> logout() async {
    try {
      await _dioClient.delete(ApiEndpoints.logout);
    } catch (_) {
      // Abaikan error server saat logout (misal offline), token lokal tetap wajib dihapus
    } finally {
      await _tokenStorage.deleteToken();
    }
  }
}
