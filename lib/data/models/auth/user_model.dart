import 'package:equatable/equatable.dart';
import 'degree_model.dart';
import 'role_model.dart';

/// Model data profil pengguna (User)
class UserModel extends Equatable {
  final int idUser;
  final String name;
  final String email;
  final String? school;
  final int? idDegree;
  final int? idRole;
  final RoleModel? role;
  final DegreeModel? degree;

  const UserModel({
    required this.idUser,
    required this.name,
    required this.email,
    this.school,
    this.idDegree,
    this.idRole,
    this.role,
    this.degree,
  });

  factory UserModel.fromJson(Map<String, dynamic> json) {
    return UserModel(
      idUser: json['id_user'] is int
          ? json['id_user'] as int
          : int.tryParse(json['id_user']?.toString() ?? '') ?? 0,
      name: json['name'] as String? ?? '',
      email: json['email'] as String? ?? '',
      school: json['school'] as String?,
      idDegree: json['id_degree'] != null
          ? int.tryParse(json['id_degree'].toString())
          : null,
      idRole: json['id_role'] != null
          ? int.tryParse(json['id_role'].toString())
          : null,
      role: json['role'] != null && json['role'] is Map<String, dynamic>
          ? RoleModel.fromJson(json['role'] as Map<String, dynamic>)
          : null,
      degree: json['degree'] != null && json['degree'] is Map<String, dynamic>
          ? DegreeModel.fromJson(json['degree'] as Map<String, dynamic>)
          : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id_user': idUser,
      'name': name,
      'email': email,
      'school': school,
      'id_degree': idDegree,
      'id_role': idRole,
      if (role != null) 'role': role!.toJson(),
      if (degree != null) 'degree': degree!.toJson(),
    };
  }

  UserModel copyWith({
    int? idUser,
    String? name,
    String? email,
    String? school,
    int? idDegree,
    int? idRole,
    RoleModel? role,
    DegreeModel? degree,
  }) {
    return UserModel(
      idUser: idUser ?? this.idUser,
      name: name ?? this.name,
      email: email ?? this.email,
      school: school ?? this.school,
      idDegree: idDegree ?? this.idDegree,
      idRole: idRole ?? this.idRole,
      role: role ?? this.role,
      degree: degree ?? this.degree,
    );
  }

  @override
  List<Object?> get props => [
        idUser,
        name,
        email,
        school,
        idDegree,
        idRole,
        role,
        degree,
      ];
}
