import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../../core/errors/app_exception.dart';
import '../../../../data/models/auth/degree_model.dart';
import '../../../../data/models/auth/user_model.dart';
import '../../data/repositories/profile_repository.dart';
import 'profile_event.dart';
import 'profile_state.dart';

/// BLoC pengelola status data profil, formulir edit profil, dan ganti kata sandi siswa
class ProfileBloc extends Bloc<ProfileEvent, ProfileState> {
  final ProfileRepository _repository;

  ProfileBloc({required ProfileRepository repository})
      : _repository = repository,
        super(const ProfileInitial()) {
    on<ProfileLoadRequested>(_onProfileLoadRequested);
    on<ProfileUpdateSubmitted>(_onProfileUpdateSubmitted);
    on<PasswordUpdateSubmitted>(_onPasswordUpdateSubmitted);
  }

  Future<void> _onProfileLoadRequested(
    ProfileLoadRequested event,
    Emitter<ProfileState> emit,
  ) async {
    emit(const ProfileLoading());
    try {
      final results = await Future.wait([
        _repository.getProfile(),
        _repository.getDegrees(),
      ]);

      final user = results[0] as UserModel;
      final degrees = results[1] as List<DegreeModel>;

      emit(ProfileLoaded(user: user, degrees: degrees));
    } on AppException catch (e) {
      emit(ProfileFailure(message: e.message));
    } catch (e) {
      emit(ProfileFailure(message: e.toString()));
    }
  }

  Future<void> _onProfileUpdateSubmitted(
    ProfileUpdateSubmitted event,
    Emitter<ProfileState> emit,
  ) async {
    final currentUser = _getCurrentUser();
    final currentDegrees = _getCurrentDegrees();

    if (currentUser != null) {
      emit(ProfileUpdating(user: currentUser, degrees: currentDegrees));
    }

    try {
      final updatedUser = await _repository.updateProfile(
        name: event.name,
        email: event.email,
        school: event.school,
        idDegree: event.idDegree,
      );

      emit(
        ProfileUpdateSuccess(
          user: updatedUser,
          degrees: currentDegrees,
          message: 'Profil berhasil diperbarui.',
        ),
      );
    } on AppException catch (e) {
      if (e is ValidationException) {
        emit(
          ProfileFailure(
            message: e.message,
            validationErrors: e.errors,
            user: currentUser,
            degrees: currentDegrees,
          ),
        );
      } else {
        emit(
          ProfileFailure(
            message: e.message,
            user: currentUser,
            degrees: currentDegrees,
          ),
        );
      }
    } catch (e) {
      emit(
        ProfileFailure(
          message: e.toString(),
          user: currentUser,
          degrees: currentDegrees,
        ),
      );
    }
  }

  Future<void> _onPasswordUpdateSubmitted(
    PasswordUpdateSubmitted event,
    Emitter<ProfileState> emit,
  ) async {
    final currentUser = _getCurrentUser();
    final currentDegrees = _getCurrentDegrees();

    if (currentUser != null) {
      emit(PasswordUpdating(user: currentUser, degrees: currentDegrees));
    }

    try {
      final message = await _repository.updatePassword(
        currentPassword: event.currentPassword,
        newPassword: event.newPassword,
      );

      if (currentUser != null) {
        emit(
          PasswordUpdateSuccess(
            user: currentUser,
            degrees: currentDegrees,
            message: message,
          ),
        );
      }
    } on AppException catch (e) {
      if (e is ValidationException) {
        emit(
          ProfileFailure(
            message: e.message,
            validationErrors: e.errors,
            user: currentUser,
            degrees: currentDegrees,
          ),
        );
      } else {
        emit(
          ProfileFailure(
            message: e.message,
            user: currentUser,
            degrees: currentDegrees,
          ),
        );
      }
    } catch (e) {
      emit(
        ProfileFailure(
          message: e.toString(),
          user: currentUser,
          degrees: currentDegrees,
        ),
      );
    }
  }

  UserModel? _getCurrentUser() {
    final s = state;
    if (s is ProfileLoaded) return s.user;
    if (s is ProfileUpdating) return s.user;
    if (s is ProfileUpdateSuccess) return s.user;
    if (s is PasswordUpdating) return s.user;
    if (s is PasswordUpdateSuccess) return s.user;
    if (s is ProfileFailure) return s.user;
    return null;
  }

  List<DegreeModel> _getCurrentDegrees() {
    final s = state;
    if (s is ProfileLoaded) return s.degrees;
    if (s is ProfileUpdating) return s.degrees;
    if (s is ProfileUpdateSuccess) return s.degrees;
    if (s is PasswordUpdating) return s.degrees;
    if (s is PasswordUpdateSuccess) return s.degrees;
    if (s is ProfileFailure) return s.degrees ?? const [];
    return const [];
  }
}
