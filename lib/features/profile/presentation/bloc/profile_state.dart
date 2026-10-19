import 'package:equatable/equatable.dart';
import '../../../../data/models/auth/degree_model.dart';
import '../../../../data/models/auth/user_model.dart';

/// State dasar untuk ProfileBloc
abstract class ProfileState extends Equatable {
  const ProfileState();

  @override
  List<Object?> get props => [];
}

/// Keadaan inisial sebelum ada aksi
class ProfileInitial extends ProfileState {
  const ProfileInitial();
}

/// Keadaan memuat data awal profil dan daftar master jenjang
class ProfileLoading extends ProfileState {
  const ProfileLoading();
}

/// Data profil dan daftar jenjang berhasil dimuat
class ProfileLoaded extends ProfileState {
  final UserModel user;
  final List<DegreeModel> degrees;

  const ProfileLoaded({
    required this.user,
    required this.degrees,
  });

  @override
  List<Object?> get props => [user, degrees];
}

/// Sedang memproses update data profil
class ProfileUpdating extends ProfileState {
  final UserModel user;
  final List<DegreeModel> degrees;

  const ProfileUpdating({
    required this.user,
    required this.degrees,
  });

  @override
  List<Object?> get props => [user, degrees];
}

/// Pembaruan profil berhasil
class ProfileUpdateSuccess extends ProfileState {
  final UserModel user;
  final List<DegreeModel> degrees;
  final String message;

  const ProfileUpdateSuccess({
    required this.user,
    required this.degrees,
    required this.message,
  });

  @override
  List<Object?> get props => [user, degrees, message];
}

/// Sedang memproses pembaruan kata sandi
class PasswordUpdating extends ProfileState {
  final UserModel user;
  final List<DegreeModel> degrees;

  const PasswordUpdating({
    required this.user,
    required this.degrees,
  });

  @override
  List<Object?> get props => [user, degrees];
}

/// Pembaruan kata sandi berhasil
class PasswordUpdateSuccess extends ProfileState {
  final UserModel user;
  final List<DegreeModel> degrees;
  final String message;

  const PasswordUpdateSuccess({
    required this.user,
    required this.degrees,
    required this.message,
  });

  @override
  List<Object?> get props => [user, degrees, message];
}

/// Terjadi kesalahan saat memuat atau memperbarui data
class ProfileFailure extends ProfileState {
  final String message;
  final Map<String, List<String>>? validationErrors;
  final UserModel? user;
  final List<DegreeModel>? degrees;

  const ProfileFailure({
    required this.message,
    this.validationErrors,
    this.user,
    this.degrees,
  });

  @override
  List<Object?> get props => [message, validationErrors, user, degrees];
}
