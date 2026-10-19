import 'package:equatable/equatable.dart';

/// Abstraksi event autentikasi
sealed class AuthEvent extends Equatable {
  const AuthEvent();

  @override
  List<Object?> get props => [];
}

/// Event untuk memeriksa status login saat aplikasi dimulai (di SplashScreen)
class AuthCheckRequested extends AuthEvent {
  const AuthCheckRequested();
}

/// Event saat user submit form login
class AuthLoginSubmitted extends AuthEvent {
  final String email;
  final String password;

  const AuthLoginSubmitted({
    required this.email,
    required this.password,
  });

  @override
  List<Object?> get props => [email, password];
}

/// Event saat user submit form registrasi
class AuthRegisterSubmitted extends AuthEvent {
  final String name;
  final String email;
  final String password;
  final String? school;
  final int? idDegree;

  const AuthRegisterSubmitted({
    required this.name,
    required this.email,
    required this.password,
    this.school,
    this.idDegree,
  });

  @override
  List<Object?> get props => [name, email, password, school, idDegree];
}

/// Event saat user melakukan logout
class AuthLogoutRequested extends AuthEvent {
  const AuthLogoutRequested();
}
