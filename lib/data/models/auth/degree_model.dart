import 'package:equatable/equatable.dart';

/// Model untuk master data jenjang kelas (Degree)
class DegreeModel extends Equatable {
  final int idDegree;
  final String name;

  const DegreeModel({
    required this.idDegree,
    required this.name,
  });

  factory DegreeModel.fromJson(Map<String, dynamic> json) {
    return DegreeModel(
      idDegree: json['id_degree'] is int
          ? json['id_degree'] as int
          : int.tryParse(json['id_degree'].toString()) ?? 0,
      name: json['name'] as String? ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id_degree': idDegree,
      'name': name,
    };
  }

  @override
  List<Object?> get props => [idDegree, name];
}
