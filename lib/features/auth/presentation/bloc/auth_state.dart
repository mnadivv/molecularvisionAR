import 'package:equatable/equatable.dart';
import '../../../../data/models/auth/user_model.dart';

/// Abstraksi state autentikasi
sealed class AuthState extends Equatable {
  const AuthState();

  @override
  List<Object?> get props => [];
}

/// State awal
class AuthInitial extends AuthState {
  const AuthInitial();
}

/// State saat proses autentikasi sedang berjalan (loading spinner)
class AuthLoading extends AuthState {
  const AuthLoading();
}

/// State saat siswa berhasil terotentikasi
class Authenticated extends AuthState {
  final UserModel user;

  const Authenticated(this.user);

  @override
  List<Object?> get props => [user];
}

/// State saat siswa belum login atau sesi berakhir
class Unauthenticated extends AuthState {
  const Unauthenticated();
}

/// State saat terjadi error (kredensial salah, validasi gagal, atau role dilarang)
class AuthFailure extends AuthState {
  final String message;
  final Map<String, List<String>> validationErrors;

  const AuthFailure(
    this.message, {
    this.validationErrors = const {},
  });

  @override
  List<Object?> get props => [message, validationErrors];
}
