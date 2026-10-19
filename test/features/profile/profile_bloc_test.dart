import 'package:flutter_test/flutter_test.dart';
import 'package:molecular_vision_ar/core/errors/app_exception.dart';
import 'package:molecular_vision_ar/data/models/auth/degree_model.dart';
import 'package:molecular_vision_ar/data/models/auth/user_model.dart';
import 'package:molecular_vision_ar/features/profile/data/repositories/profile_repository.dart';
import 'package:molecular_vision_ar/features/profile/presentation/bloc/profile_bloc.dart';
import 'package:molecular_vision_ar/features/profile/presentation/bloc/profile_event.dart';
import 'package:molecular_vision_ar/features/profile/presentation/bloc/profile_state.dart';

class MockProfileRepository extends ProfileRepository {
  UserModel mockUser = const UserModel(
    idUser: 1,
    name: 'Siswa Test',
    email: 'siswa@mvar.id',
    school: 'SMAN 1 Kimia',
    idDegree: 1,
  );

  List<DegreeModel> mockDegrees = const [
    DegreeModel(idDegree: 1, name: 'Kelas 10'),
    DegreeModel(idDegree: 2, name: 'Kelas 11'),
  ];

  bool shouldFailLoad = false;
  bool shouldFailUpdate = false;
  bool shouldFailPassword = false;

  @override
  Future<UserModel> getProfile() async {
    if (shouldFailLoad) {
      throw ServerException('Gagal memuat profil.');
    }
    return mockUser;
  }

  @override
  Future<List<DegreeModel>> getDegrees() async {
    if (shouldFailLoad) {
      throw ServerException('Gagal memuat jenjang.');
    }
    return mockDegrees;
  }

  @override
  Future<UserModel> updateProfile({
    required String name,
    required String email,
    String? school,
    int? idDegree,
  }) async {
    if (shouldFailUpdate) {
      throw ValidationException('Validasi gagal.', errors: {
        'email': ['Email sudah digunakan.'],
      });
    }
    mockUser = mockUser.copyWith(
      name: name,
      email: email,
      school: school,
      idDegree: idDegree,
    );
    return mockUser;
  }

  @override
  Future<String> updatePassword({
    required String currentPassword,
    required String newPassword,
  }) async {
    if (shouldFailPassword) {
      throw ValidationException('Kata sandi lama salah.', errors: {
        'current_password': ['Kata sandi lama tidak sesuai.'],
      });
    }
    return 'Kata sandi berhasil diperbarui.';
  }
}

void main() {
  group('ProfileBloc Tests', () {
    late MockProfileRepository repository;
    late ProfileBloc bloc;

    setUp(() {
      repository = MockProfileRepository();
      bloc = ProfileBloc(repository: repository);
    });

    tearDown(() {
      bloc.close();
    });

    test('initial state is ProfileInitial', () {
      expect(bloc.state, const ProfileInitial());
    });

    test('emits [ProfileLoading, ProfileLoaded] on successful ProfileLoadRequested', () async {
      final expectedStates = [
        const ProfileLoading(),
        ProfileLoaded(
          user: repository.mockUser,
          degrees: repository.mockDegrees,
        ),
      ];

      expectLater(bloc.stream, emitsInOrder(expectedStates));
      bloc.add(const ProfileLoadRequested());
    });

    test('emits [ProfileLoading, ProfileFailure] on failed ProfileLoadRequested', () async {
      repository.shouldFailLoad = true;

      final expectedStates = [
        const ProfileLoading(),
        const ProfileFailure(message: 'Gagal memuat profil.'),
      ];

      expectLater(bloc.stream, emitsInOrder(expectedStates));
      bloc.add(const ProfileLoadRequested());
    });

    test('emits [ProfileUpdating, ProfileUpdateSuccess] on successful ProfileUpdateSubmitted', () async {
      // Load initial state first
      bloc.emit(ProfileLoaded(user: repository.mockUser, degrees: repository.mockDegrees));

      final updatedUser = repository.mockUser.copyWith(name: 'Nama Baru');

      final expectedStates = [
        ProfileUpdating(user: repository.mockUser, degrees: repository.mockDegrees),
        ProfileUpdateSuccess(
          user: updatedUser,
          degrees: repository.mockDegrees,
          message: 'Profil berhasil diperbarui.',
        ),
      ];

      expectLater(bloc.stream, emitsInOrder(expectedStates));

      bloc.add(
        ProfileUpdateSubmitted(
          name: 'Nama Baru',
          email: repository.mockUser.email,
          school: repository.mockUser.school,
          idDegree: repository.mockUser.idDegree,
        ),
      );
    });

    test('emits [ProfileUpdating, ProfileFailure] on validation error in ProfileUpdateSubmitted', () async {
      bloc.emit(ProfileLoaded(user: repository.mockUser, degrees: repository.mockDegrees));
      repository.shouldFailUpdate = true;

      final expectedStates = [
        ProfileUpdating(user: repository.mockUser, degrees: repository.mockDegrees),
        ProfileFailure(
          message: 'Validasi gagal.',
          validationErrors: const {
            'email': ['Email sudah digunakan.'],
          },
          user: repository.mockUser,
          degrees: repository.mockDegrees,
        ),
      ];

      expectLater(bloc.stream, emitsInOrder(expectedStates));

      bloc.add(
        ProfileUpdateSubmitted(
          name: repository.mockUser.name,
          email: 'duplicate@mvar.id',
        ),
      );
    });

    test('emits [PasswordUpdating, PasswordUpdateSuccess] on successful PasswordUpdateSubmitted', () async {
      bloc.emit(ProfileLoaded(user: repository.mockUser, degrees: repository.mockDegrees));

      final expectedStates = [
        PasswordUpdating(user: repository.mockUser, degrees: repository.mockDegrees),
        PasswordUpdateSuccess(
          user: repository.mockUser,
          degrees: repository.mockDegrees,
          message: 'Kata sandi berhasil diperbarui.',
        ),
      ];

      expectLater(bloc.stream, emitsInOrder(expectedStates));

      bloc.add(
        const PasswordUpdateSubmitted(
          currentPassword: 'OldPassword123!',
          newPassword: 'NewPassword123!',
        ),
      );
    });

    test('emits [PasswordUpdating, ProfileFailure] on wrong current password', () async {
      bloc.emit(ProfileLoaded(user: repository.mockUser, degrees: repository.mockDegrees));
      repository.shouldFailPassword = true;

      final expectedStates = [
        PasswordUpdating(user: repository.mockUser, degrees: repository.mockDegrees),
        ProfileFailure(
          message: 'Kata sandi lama salah.',
          validationErrors: const {
            'current_password': ['Kata sandi lama tidak sesuai.'],
          },
          user: repository.mockUser,
          degrees: repository.mockDegrees,
        ),
      ];

      expectLater(bloc.stream, emitsInOrder(expectedStates));

      bloc.add(
        const PasswordUpdateSubmitted(
          currentPassword: 'WrongPassword!',
          newPassword: 'NewPassword123!',
        ),
      );
    });
  });
}
