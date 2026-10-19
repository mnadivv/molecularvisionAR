import 'package:dio/dio.dart';

/// Abstraksi dasar error aplikasi
sealed class AppException implements Exception {
  final String message;
  final int? statusCode;

  const AppException(this.message, {this.statusCode});

  @override
  String toString() => message;

  /// Factory untuk mengonversi [DioException] menjadi [AppException] yang user-friendly
  factory AppException.fromDioException(DioException dioException) {
    switch (dioException.type) {
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.sendTimeout:
      case DioExceptionType.receiveTimeout:
      case DioExceptionType.transformTimeout:
        return const NetworkException(
          'Koneksi ke server timeout. Silakan periksa jaringan Anda.',
        );

      case DioExceptionType.connectionError:
        return const NetworkException(
          'Gagal terhubung ke server. Pastikan server backend aktif dan perangkat terhubung jaringan.',
        );

      case DioExceptionType.badResponse:
        final response = dioException.response;
        final statusCode = response?.statusCode;
        final responseData = response?.data;

        String serverMessage = 'Terjadi kesalahan pada server.';
        if (responseData is Map<String, dynamic>) {
          if (responseData['message'] is String && (responseData['message'] as String).isNotEmpty) {
            serverMessage = responseData['message'] as String;
          }
        }

        switch (statusCode) {
          case 401:
            return UnauthorizedException(serverMessage, statusCode);
          case 403:
            return ForbiddenException(serverMessage, statusCode);
          case 404:
            return NotFoundException(serverMessage, statusCode);
          case 422:
            Map<String, List<String>> validationErrors = {};
            if (responseData is Map<String, dynamic> && responseData['errors'] is Map) {
              final rawErrors = responseData['errors'] as Map<String, dynamic>;
              rawErrors.forEach((key, value) {
                if (value is List) {
                  validationErrors[key] = value.map((e) => e.toString()).toList();
                }
              });
            }
            return ValidationException(
              serverMessage,
              statusCode: statusCode,
              errors: validationErrors,
            );
          case 500:
          case 502:
          case 503:
            return ServerException(serverMessage, statusCode);
          default:
            return UnexpectedException(serverMessage, statusCode);
        }

      case DioExceptionType.cancel:
        return const UnexpectedException('Permintaan ke server dibatalkan.');

      case DioExceptionType.badCertificate:
        return const NetworkException('Sertifikat keamanan server tidak valid.');

      case DioExceptionType.unknown:
        final innerError = dioException.error;
        if (innerError != null) {
          final errorStr = innerError.toString();
          if (errorStr.contains('SocketException') || errorStr.contains('Failed host lookup') || errorStr.contains('OS Error')) {
            return const NetworkException('Gagal terhubung ke server. Periksa koneksi internet perangkat Anda.');
          }
          if (errorStr.contains('HandshakeException') || errorStr.contains('CERTIFICATE_VERIFY_FAILED')) {
            return const NetworkException('Gagal memverifikasi sertifikat SSL server.');
          }
          return NetworkException('Kendala jaringan: $innerError');
        }
        return const NetworkException('Terjadi kendala jaringan yang tidak diketahui.');
    }
  }
}

class NetworkException extends AppException {
  const NetworkException([super.message = 'Koneksi jaringan bermasalah.']);
}

class UnauthorizedException extends AppException {
  const UnauthorizedException([
    super.message = 'Sesi telah berakhir atau kredensial salah.',
    int? statusCode = 401,
  ]) : super(statusCode: statusCode);
}

class ForbiddenException extends AppException {
  const ForbiddenException([
    super.message = 'Akses ditolak. Akun tidak memiliki hak akses.',
    int? statusCode = 403,
  ]) : super(statusCode: statusCode);
}

class NotFoundException extends AppException {
  const NotFoundException([
    super.message = 'Data yang diminta tidak ditemukan.',
    int? statusCode = 404,
  ]) : super(statusCode: statusCode);
}

class ValidationException extends AppException {
  final Map<String, List<String>> errors;

  const ValidationException(
    super.message, {
    super.statusCode = 422,
    this.errors = const {},
  });

  /// Mengambil error pertama yang bisa langsung ditampilkan di UI
  String? get firstError {
    if (errors.isEmpty) return message;
    final firstKey = errors.keys.first;
    return errors[firstKey]?.firstOrNull ?? message;
  }
}

class ServerException extends AppException {
  const ServerException([
    super.message = 'Terjadi kesalahan internal pada server.',
    int? statusCode = 500,
  ]) : super(statusCode: statusCode);
}

class UnexpectedException extends AppException {
  const UnexpectedException([
    super.message = 'Terjadi kesalahan yang tidak terduga.',
    int? statusCode,
  ]) : super(statusCode: statusCode);
}
