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
    final map = json.containsKey('user') && json['user'] is Map<String, dynamic>
        ? json['user'] as Map<String, dynamic>
        : json;

    return UserModel(
      idUser: map['id_user'] is int
          ? map['id_user'] as int
          : int.tryParse(map['id_user']?.toString() ?? '') ?? 0,
      name: map['name'] as String? ?? '',
      email: map['email'] as String? ?? '',
      school: map['school'] as String?,
      idDegree: map['id_degree'] != null
          ? int.tryParse(map['id_degree'].toString())
          : null,
      idRole: map['id_role'] != null
          ? int.tryParse(map['id_role'].toString())
          : null,
      role: map['role'] != null && map['role'] is Map<String, dynamic>
          ? RoleModel.fromJson(map['role'] as Map<String, dynamic>)
          : null,
      degree: map['degree'] != null && map['degree'] is Map<String, dynamic>
          ? DegreeModel.fromJson(map['degree'] as Map<String, dynamic>)
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
