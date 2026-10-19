import 'package:dio/dio.dart';
import '../constants/api_endpoints.dart';
import '../errors/app_exception.dart';
import '../storage/token_storage_service.dart';
import 'interceptors/auth_interceptor.dart';
import 'interceptors/logging_interceptor.dart';

/// Entrypoint utama HTTP Client untuk aplikasi MVAR Mobile.
/// Mengonfigurasi timeout, interceptor autentikasi, logging,
/// dan menerjemahkan DioException menjadi AppException yang siap dikonsumsi UI/BLoC.
class DioClient {
  final Dio _dio;
  final TokenStorageService _tokenStorage;

  DioClient({
    TokenStorageService? tokenStorage,
    Dio? dio,
    String? baseUrl,
    void Function()? onSessionExpired,
  })  : _tokenStorage = tokenStorage ?? TokenStorageService(),
        _dio = dio ??
            Dio(
              BaseOptions(
                baseUrl: baseUrl ?? ApiEndpoints.baseUrl,
                connectTimeout: ApiEndpoints.connectTimeout,
                receiveTimeout: ApiEndpoints.receiveTimeout,
                sendTimeout: ApiEndpoints.sendTimeout,
                headers: const {
                  'Content-Type': 'application/json',
                  'Accept': 'application/json',
                },
                responseType: ResponseType.json,
              ),
            ) {
    // Tambahkan interceptor jika belum terdaftar
    _dio.interceptors.addAll([
      AuthInterceptor(
        tokenStorage: _tokenStorage,
        onSessionExpired: onSessionExpired,
      ),
      LoggingInterceptor(),
    ]);
  }

  /// Akses langsung ke raw instance Dio jika dibutuhkan (misal upload progress)
  Dio get dio => _dio;

  /// Akses ke token storage service
  TokenStorageService get tokenStorage => _tokenStorage;

  /// GET Request
  Future<Response<T>> get<T>(
    String path, {
    Map<String, dynamic>? queryParameters,
    Options? options,
    CancelToken? cancelToken,
  }) async {
    try {
      return await _dio.get<T>(
        path,
        queryParameters: queryParameters,
        options: options,
        cancelToken: cancelToken,
      );
    } on DioException catch (e) {
      throw AppException.fromDioException(e);
    }
  }

  /// POST Request
  Future<Response<T>> post<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
    CancelToken? cancelToken,
  }) async {
    try {
      return await _dio.post<T>(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
        cancelToken: cancelToken,
      );
    } on DioException catch (e) {
      throw AppException.fromDioException(e);
    }
  }

  /// PUT Request
  Future<Response<T>> put<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
    CancelToken? cancelToken,
  }) async {
    try {
      return await _dio.put<T>(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
        cancelToken: cancelToken,
      );
    } on DioException catch (e) {
      throw AppException.fromDioException(e);
    }
  }

  /// PATCH Request
  Future<Response<T>> patch<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
    CancelToken? cancelToken,
  }) async {
    try {
      return await _dio.patch<T>(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
        cancelToken: cancelToken,
      );
    } on DioException catch (e) {
      throw AppException.fromDioException(e);
    }
  }

  /// DELETE Request
  Future<Response<T>> delete<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    Options? options,
    CancelToken? cancelToken,
  }) async {
    try {
      return await _dio.delete<T>(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
        cancelToken: cancelToken,
      );
    } on DioException catch (e) {
      throw AppException.fromDioException(e);
    }
  }
}
