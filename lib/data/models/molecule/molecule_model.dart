import 'package:equatable/equatable.dart';

/// Model katalog molekul kimia 3D
class MoleculeModel extends Equatable {
  final int idMolecule;
  final String name;
  final int order;
  final String formula;
  final String? shape;
  final String? bent;
  final String? bondType;
  final String? description;
  final String? model3dUrl;

  const MoleculeModel({
    required this.idMolecule,
    required this.name,
    this.order = 0,
    required this.formula,
    this.shape,
    this.bent,
    this.bondType,
    this.description,
    this.model3dUrl,
  });

  factory MoleculeModel.fromJson(Map<String, dynamic> json) {
    return MoleculeModel(
      idMolecule: json['id_molecule'] is int
          ? json['id_molecule'] as int
          : int.tryParse(json['id_molecule']?.toString() ?? '') ?? 0,
      name: json['name'] as String? ?? '',
      order: json['order'] is int
          ? json['order'] as int
          : int.tryParse(json['order']?.toString() ?? '') ?? 0,
      formula: json['formula'] as String? ?? '',
      shape: json['shape'] as String?,
      bent: json['bent'] as String?,
      bondType: json['bond_type'] as String?,
      description: json['description'] as String?,
      model3dUrl: json['model_3d_url'] as String?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id_molecule': idMolecule,
      'name': name,
      'order': order,
      'formula': formula,
      'shape': shape,
      'bent': bent,
      'bond_type': bondType,
      'description': description,
      'model_3d_url': model3dUrl,
    };
  }

  @override
  List<Object?> get props => [
        idMolecule,
        name,
        order,
        formula,
        shape,
        bent,
        bondType,
        description,
        model3dUrl,
      ];
}
