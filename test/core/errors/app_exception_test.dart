import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:molecular_vision_ar/core/errors/app_exception.dart';

void main() {
  group('AppException', () {
    test('maps connection timeout to NetworkException', () {
      final dioException = DioException(
        requestOptions: RequestOptions(path: '/test'),
        type: DioExceptionType.connectionTimeout,
      );

      final exception = AppException.fromDioException(dioException);
      expect(exception, isA<NetworkException>());
    });

    test('maps 401 response to UnauthorizedException', () {
      final dioException = DioException(
        requestOptions: RequestOptions(path: '/me'),
        type: DioExceptionType.badResponse,
        response: Response(
          requestOptions: RequestOptions(path: '/me'),
          statusCode: 401,
          data: {'message': 'Unauthenticated.'},
        ),
      );

      final exception = AppException.fromDioException(dioException);
      expect(exception, isA<UnauthorizedException>());
      expect(exception.message, equals('Unauthenticated.'));
    });

    test('maps 403 response to ForbiddenException', () {
      final dioException = DioException(
        requestOptions: RequestOptions(path: '/login'),
        type: DioExceptionType.badResponse,
        response: Response(
          requestOptions: RequestOptions(path: '/login'),
          statusCode: 403,
          data: {'message': 'Akses ditolak.'},
        ),
      );

      final exception = AppException.fromDioException(dioException);
      expect(exception, isA<ForbiddenException>());
      expect(exception.message, equals('Akses ditolak.'));
    });

    test('maps 422 response to ValidationException with parsed field errors', () {
      final dioException = DioException(
        requestOptions: RequestOptions(path: '/register'),
        type: DioExceptionType.badResponse,
        response: Response(
          requestOptions: RequestOptions(path: '/register'),
          statusCode: 422,
          data: {
            'message': 'The given data was invalid.',
            'errors': {
              'email': ['Email sudah terdaftar.'],
              'password': ['Kata sandi minimal 8 karakter.'],
            },
          },
        ),
      );

      final exception = AppException.fromDioException(dioException);
      expect(exception, isA<ValidationException>());
      final validationEx = exception as ValidationException;
      expect(validationEx.errors['email']?.first, equals('Email sudah terdaftar.'));
      expect(validationEx.firstError, equals('Email sudah terdaftar.'));
    });

    test('maps 500 response to ServerException', () {
      final dioException = DioException(
        requestOptions: RequestOptions(path: '/materials'),
        type: DioExceptionType.badResponse,
        response: Response(
          requestOptions: RequestOptions(path: '/materials'),
          statusCode: 500,
          data: {'message': 'Server error'},
        ),
      );

      final exception = AppException.fromDioException(dioException);
      expect(exception, isA<ServerException>());
    });
  });
}
