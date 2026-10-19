import 'package:equatable/equatable.dart';

/// Metrik progres spesifik untuk suatu modul (total, completed, uncompleted, percentage)
class ProgressMetricModel extends Equatable {
  final int total;
  final int completed;
  final int uncompleted;
  final double percentage;

  const ProgressMetricModel({
    required this.total,
    required this.completed,
    required this.uncompleted,
    required this.percentage,
  });

  factory ProgressMetricModel.fromJson(Map<String, dynamic> json) {
    return ProgressMetricModel(
      total: json['total'] is int ? json['total'] as int : int.tryParse(json['total']?.toString() ?? '') ?? 0,
      completed: json['completed'] is int ? json['completed'] as int : int.tryParse(json['completed']?.toString() ?? '') ?? 0,
      uncompleted: json['uncompleted'] is int ? json['uncompleted'] as int : int.tryParse(json['uncompleted']?.toString() ?? '') ?? 0,
      percentage: json['percentage'] is num ? (json['percentage'] as num).toDouble() : double.tryParse(json['percentage']?.toString() ?? '') ?? 0.0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'total': total,
      'completed': completed,
      'uncompleted': uncompleted,
      'percentage': percentage,
    };
  }

  @override
  List<Object?> get props => [total, completed, uncompleted, percentage];
}

/// Model ringkasan dan detail progres belajar siswa
class ProgressModel extends Equatable {
  final ProgressMetricModel materials;
  final ProgressMetricModel molecules;
  final ProgressMetricModel quizzes;
  final List<int> completedMaterialIds;
  final List<int> completedMoleculeIds;
  final List<int> completedCategoryIds;

  const ProgressModel({
    required this.materials,
    required this.molecules,
    required this.quizzes,
    this.completedMaterialIds = const [],
    this.completedMoleculeIds = const [],
    this.completedCategoryIds = const [],
  });

  factory ProgressModel.fromJson(Map<String, dynamic> json) {
    List<int> parseIds(dynamic list) {
      if (list is List) {
        return list.map((e) => int.tryParse(e.toString()) ?? 0).where((id) => id > 0).toList();
      }
      return [];
    }

    return ProgressModel(
      materials: ProgressMetricModel.fromJson(json['materials'] as Map<String, dynamic>? ?? {}),
      molecules: ProgressMetricModel.fromJson(json['molecules'] as Map<String, dynamic>? ?? {}),
      quizzes: ProgressMetricModel.fromJson(json['quizzes'] as Map<String, dynamic>? ?? {}),
      completedMaterialIds: parseIds(json['completed_material_ids']),
      completedMoleculeIds: parseIds(json['completed_molecule_ids']),
      completedCategoryIds: parseIds(json['completed_category_ids']),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'materials': materials.toJson(),
      'molecules': molecules.toJson(),
      'quizzes': quizzes.toJson(),
      'completed_material_ids': completedMaterialIds,
      'completed_molecule_ids': completedMoleculeIds,
      'completed_category_ids': completedCategoryIds,
    };
  }

  @override
  List<Object?> get props => [
        materials,
        molecules,
        quizzes,
        completedMaterialIds,
        completedMoleculeIds,
        completedCategoryIds,
      ];
}
