import 'package:equatable/equatable.dart';
import 'user_model.dart';

/// Model untuk wrapper response autentikasi (user, token, token_type)
class AuthResponseModel extends Equatable {
  final UserModel user;
  final String token;
  final String tokenType;

  const AuthResponseModel({
    required this.user,
    required this.token,
    required this.tokenType,
  });

  factory AuthResponseModel.fromJson(Map<String, dynamic> json) {
    return AuthResponseModel(
      user: UserModel.fromJson(json['user'] as Map<String, dynamic>? ?? {}),
      token: json['token'] as String? ?? '',
      tokenType: json['token_type'] as String? ?? 'Bearer',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'user': user.toJson(),
      'token': token,
      'token_type': tokenType,
    };
  }

  @override
  List<Object?> get props => [user, token, tokenType];
}
