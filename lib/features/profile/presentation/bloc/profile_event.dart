import 'package:equatable/equatable.dart';

/// Event dasar untuk ProfileBloc
abstract class ProfileEvent extends Equatable {
  const ProfileEvent();

  @override
  List<Object?> get props => [];
}

/// Permintaan untuk memuat data profil siswa dan daftar jenjang
class ProfileLoadRequested extends ProfileEvent {
  const ProfileLoadRequested();
}

/// Permintaan pembaruan data profil siswa
class ProfileUpdateSubmitted extends ProfileEvent {
  final String name;
  final String email;
  final String? school;
  final int? idDegree;

  const ProfileUpdateSubmitted({
    required this.name,
    required this.email,
    this.school,
    this.idDegree,
  });

  @override
  List<Object?> get props => [name, email, school, idDegree];
}

/// Permintaan pembaruan kata sandi akun siswa
class PasswordUpdateSubmitted extends ProfileEvent {
  final String currentPassword;
  final String newPassword;

  const PasswordUpdateSubmitted({
    required this.currentPassword,
    required this.newPassword,
  });

  @override
  List<Object?> get props => [currentPassword, newPassword];
}
