import 'package:flutter_test/flutter_test.dart';
import 'package:molecular_vision_ar/core/errors/app_exception.dart';
import 'package:molecular_vision_ar/data/models/auth/auth_response_model.dart';
import 'package:molecular_vision_ar/data/models/auth/user_model.dart';
import 'package:molecular_vision_ar/data/models/auth/degree_model.dart';
import 'package:molecular_vision_ar/features/auth/data/repositories/auth_repository.dart';
import 'package:molecular_vision_ar/features/auth/presentation/bloc/auth_bloc.dart';
import 'package:molecular_vision_ar/features/auth/presentation/bloc/auth_event.dart';
import 'package:molecular_vision_ar/features/auth/presentation/bloc/auth_state.dart';

class MockAuthRepository extends AuthRepository {
  UserModel? mockUser;
  bool shouldFailLogin = false;
  String? failMessage;

  @override
  Future<UserModel?> getCurrentUser() async => mockUser;

  @override
  Future<AuthResponseModel> login({
    required String email,
    required String password,
  }) async {
    if (shouldFailLogin) {
      throw UnauthorizedException(failMessage ?? 'Kredensial salah.');
    }
    final user = mockUser ??
        const UserModel(
          idUser: 1,
          name: 'Test Student',
          email: 'student@mvar.id',
        );
    return AuthResponseModel(
      user: user,
      token: 'fake-token-123',
      tokenType: 'Bearer',
    );
  }

  @override
  Future<AuthResponseModel> register({
    required String name,
    required String email,
    required String password,
    String? school,
    int? idDegree,
  }) async {
    final user = UserModel(
      idUser: 2,
      name: name,
      email: email,
      school: school,
      idDegree: idDegree,
    );
    return AuthResponseModel(
      user: user,
      token: 'fake-register-token',
      tokenType: 'Bearer',
    );
  }

  @override
  Future<List<DegreeModel>> getDegrees() async {
    return [
      const DegreeModel(idDegree: 1, name: 'Kelas 10'),
      const DegreeModel(idDegree: 2, name: 'Kelas 11'),
    ];
  }

  @override
  Future<void> logout() async {
    mockUser = null;
  }
}

void main() {
  group('AuthBloc', () {
    late MockAuthRepository repository;

    setUp(() {
      repository = MockAuthRepository();
    });

    test('initial state is AuthInitial', () {
      final bloc = AuthBloc(authRepository: repository);
      expect(bloc.state, equals(const AuthInitial()));
    });

    test('emits [AuthLoading, Unauthenticated] on AuthCheckRequested when no user is logged in', () async {
      final bloc = AuthBloc(authRepository: repository);

      expectLater(
        bloc.stream,
        emitsInOrder([
          const AuthLoading(),
          const Unauthenticated(),
        ]),
      );

      bloc.add(const AuthCheckRequested());
    });

    test('emits [AuthLoading, Authenticated] on AuthCheckRequested when user session is active', () async {
      repository.mockUser = const UserModel(
        idUser: 10,
        name: 'Active Student',
        email: 'active@mvar.id',
      );

      final bloc = AuthBloc(authRepository: repository);

      expectLater(
        bloc.stream,
        emitsInOrder([
          const AuthLoading(),
          Authenticated(repository.mockUser!),
        ]),
      );

      bloc.add(const AuthCheckRequested());
    });

    test('emits [AuthLoading, Authenticated] on successful AuthLoginSubmitted', () async {
      final bloc = AuthBloc(authRepository: repository);

      expectLater(
        bloc.stream,
        emitsInOrder([
          const AuthLoading(),
          isA<Authenticated>(),
        ]),
      );

      bloc.add(const AuthLoginSubmitted(
        email: 'student@mvar.id',
        password: 'password123',
      ));
    });

    test('emits [AuthLoading, AuthFailure] on failed AuthLoginSubmitted', () async {
      repository.shouldFailLogin = true;
      repository.failMessage = 'Email atau kata sandi tidak valid.';

      final bloc = AuthBloc(authRepository: repository);

      expectLater(
        bloc.stream,
        emitsInOrder([
          const AuthLoading(),
          const AuthFailure('Email atau kata sandi tidak valid.'),
        ]),
      );

      bloc.add(const AuthLoginSubmitted(
        email: 'wrong@mvar.id',
        password: 'wrongpassword',
      ));
    });

    test('emits [AuthLoading, Unauthenticated] on AuthLogoutRequested', () async {
      final bloc = AuthBloc(authRepository: repository);

      expectLater(
        bloc.stream,
        emitsInOrder([
          const AuthLoading(),
          const Unauthenticated(),
        ]),
      );

      bloc.add(const AuthLogoutRequested());
    });
  });
}
