import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:molecular_vision_ar/core/constants/api_endpoints.dart';
import 'package:molecular_vision_ar/core/network/dio_client.dart';
import 'package:molecular_vision_ar/core/network/interceptors/auth_interceptor.dart';
import 'package:molecular_vision_ar/core/network/interceptors/logging_interceptor.dart';
import 'package:molecular_vision_ar/core/storage/token_storage_service.dart';

class FakeTokenStorageService extends TokenStorageService {
  String? fakeToken;

  @override
  Future<String?> getToken() async => fakeToken;

  @override
  Future<void> saveToken(String token) async {
    fakeToken = token;
  }

  @override
  Future<void> deleteToken() async {
    fakeToken = null;
  }

  @override
  Future<bool> hasToken() async => fakeToken != null && fakeToken!.isNotEmpty;
}

class TestRequestInterceptorHandler extends RequestInterceptorHandler {
  RequestOptions? optionsPassed;

  @override
  void next(RequestOptions options) {
    optionsPassed = options;
  }
}

class TestErrorInterceptorHandler extends ErrorInterceptorHandler {
  DioException? errorPassed;

  @override
  void next(DioException err) {
    errorPassed = err;
  }
}

void main() {
  group('DioClient & Network Config', () {
    test('ApiEndpoints provides correct base configuration and endpoints', () {
      expect(ApiEndpoints.login, equals('/login'));
      expect(ApiEndpoints.register, equals('/register'));
      expect(ApiEndpoints.me, equals('/me'));
      expect(ApiEndpoints.molecules, equals('/molecules'));
      expect(ApiEndpoints.materials, equals('/materials'));
      expect(ApiEndpoints.connectTimeout, equals(const Duration(seconds: 15)));
    });

    test('DioClient initializes with expected BaseOptions and Interceptors', () {
      final fakeStorage = FakeTokenStorageService();
      final client = DioClient(tokenStorage: fakeStorage);

      expect(client.dio.options.connectTimeout, equals(ApiEndpoints.connectTimeout));
      expect(client.dio.options.receiveTimeout, equals(ApiEndpoints.receiveTimeout));
      expect(client.dio.options.headers['Accept'], equals('application/json'));
      expect(client.dio.options.headers['Content-Type'], equals('application/json'));

      final interceptorTypes = client.dio.interceptors.map((i) => i.runtimeType).toList();
      expect(interceptorTypes, contains(AuthInterceptor));
      expect(interceptorTypes, contains(LoggingInterceptor));
    });

    test('AuthInterceptor injects Bearer token into request headers', () async {
      final fakeStorage = FakeTokenStorageService();
      fakeStorage.fakeToken = 'test-token-123';

      final interceptor = AuthInterceptor(tokenStorage: fakeStorage);
      final options = RequestOptions(path: '/materials');
      final handler = TestRequestInterceptorHandler();

      await interceptor.onRequest(options, handler);

      expect(options.headers['Authorization'], equals('Bearer test-token-123'));
      expect(options.headers['Accept'], equals('application/json'));
      expect(handler.optionsPassed, isNotNull);
    });

    test('AuthInterceptor invokes onSessionExpired on 401 error', () async {
      final fakeStorage = FakeTokenStorageService();
      fakeStorage.fakeToken = 'expired-token';

      bool sessionExpiredCalled = false;
      final interceptor = AuthInterceptor(
        tokenStorage: fakeStorage,
        onSessionExpired: () {
          sessionExpiredCalled = true;
        },
      );

      final dioException = DioException(
        requestOptions: RequestOptions(path: '/materials'),
        type: DioExceptionType.badResponse,
        response: Response(
          requestOptions: RequestOptions(path: '/materials'),
          statusCode: 401,
        ),
      );

      final handler = TestErrorInterceptorHandler();
      await interceptor.onError(dioException, handler);

      expect(sessionExpiredCalled, isTrue);
      expect(fakeStorage.fakeToken, isNull);
      expect(handler.errorPassed, equals(dioException));
    });

    test('AuthInterceptor does NOT invoke onSessionExpired on 401 from login endpoint', () async {
      final fakeStorage = FakeTokenStorageService();
      fakeStorage.fakeToken = 'existing-token';

      bool sessionExpiredCalled = false;
      final interceptor = AuthInterceptor(
        tokenStorage: fakeStorage,
        onSessionExpired: () {
          sessionExpiredCalled = true;
        },
      );

      final dioException = DioException(
        requestOptions: RequestOptions(path: '/login'),
        type: DioExceptionType.badResponse,
        response: Response(
          requestOptions: RequestOptions(path: '/login'),
          statusCode: 401,
        ),
      );

      final handler = TestErrorInterceptorHandler();
      await interceptor.onError(dioException, handler);

      expect(sessionExpiredCalled, isFalse);
      expect(fakeStorage.fakeToken, equals('existing-token'));
      expect(handler.errorPassed, equals(dioException));
    });
  });
}
