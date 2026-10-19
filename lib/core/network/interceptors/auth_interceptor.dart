import 'package:dio/dio.dart';
import '../../constants/api_endpoints.dart';
import '../../storage/token_storage_service.dart';

/// Interceptor untuk otomatis menginjeksi header Bearer token Sanctum
/// dan menangani sesi kedaluwarsa (HTTP 401).
class AuthInterceptor extends Interceptor {
  final TokenStorageService _tokenStorage;
  final void Function()? _onSessionExpired;

  AuthInterceptor({
    required TokenStorageService tokenStorage,
    void Function()? onSessionExpired,
  })  : _tokenStorage = tokenStorage,
        _onSessionExpired = onSessionExpired;

  @override
  Future<void> onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    // Pastikan server selalu mengembalikan response format JSON
    options.headers['Accept'] = 'application/json';

    final uriPath = options.uri.path.replaceAll(RegExp(r'/+$'), '');
    final isPublicEndpoint = uriPath.endsWith(ApiEndpoints.login) ||
        uriPath.endsWith(ApiEndpoints.register) ||
        uriPath.endsWith(ApiEndpoints.degrees);

    if (!isPublicEndpoint) {
      try {
        // Ambil token dari storage untuk endpoint terproteksi
        final token = await _tokenStorage.getToken();

        if (token != null && token.isNotEmpty) {
          // Sisipkan Bearer token jika belum diset secara manual
          options.headers.putIfAbsent('Authorization', () => 'Bearer $token');
        }
      } catch (_) {
        // Abaikan jika token gagal dibaca, biarkan request berlanjut tanpa header Bearer
      }
    }

    return handler.next(options);
  }

  @override
  Future<void> onError(
    DioException err,
    ErrorInterceptorHandler handler,
  ) async {
    final statusCode = err.response?.statusCode;
    final path = err.requestOptions.path;

    // Jika terjadi 401 Unauthorized dan BUKAN dari endpoint login
    // (pada login, 401 berarti email/password salah, bukan session expired)
    if (statusCode == 401 && !path.endsWith(ApiEndpoints.login)) {
      await _tokenStorage.deleteToken();
      _onSessionExpired?.call();
    }

    return handler.next(err);
  }
}
