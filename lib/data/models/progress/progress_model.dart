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

  const ProgressMetricModel.empty()
      : total = 0,
        completed = 0,
        uncompleted = 0,
        percentage = 0.0;

  factory ProgressMetricModel.fromJson(Map<String, dynamic> json) {
    return ProgressMetricModel(
      total: json['total'] is int
          ? json['total'] as int
          : int.tryParse(json['total']?.toString() ?? '') ?? 0,
      completed: json['completed'] is int
          ? json['completed'] as int
          : int.tryParse(json['completed']?.toString() ?? '') ?? 0,
      uncompleted: json['uncompleted'] is int
          ? json['uncompleted'] as int
          : int.tryParse(json['uncompleted']?.toString() ?? '') ?? 0,
      percentage: json['percentage'] is num
          ? (json['percentage'] as num).toDouble()
          : double.tryParse(json['percentage']?.toString() ?? '') ?? 0.0,
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

  /// Rasio penyelesaian dalam bentuk string, e.g. "11/15"
  String get ratioText => '$completed/$total';

  /// Nilai progres dalam rentang 0.0 sampai 1.0 untuk LinearProgressIndicator
  double get progressFraction {
    if (total <= 0) return 0.0;
    final val = completed / total;
    return val.clamp(0.0, 1.0);
  }

  @override
  List<Object?> get props => [total, completed, uncompleted, percentage];
}

/// Model ringkasan dan detail progres belajar siswa
class ProgressModel extends Equatable {
  final ProgressMetricModel materials;
  final ProgressMetricModel molecules;
  final ProgressMetricModel quizzes;
  final ProgressMetricModel? overall;
  final List<int> completedMaterialIds;
  final List<int> completedMoleculeIds;
  final List<int> completedCategoryIds;

  const ProgressModel({
    required this.materials,
    required this.molecules,
    required this.quizzes,
    this.overall,
    this.completedMaterialIds = const [],
    this.completedMoleculeIds = const [],
    this.completedCategoryIds = const [],
  });

  const ProgressModel.empty()
      : materials = const ProgressMetricModel.empty(),
        molecules = const ProgressMetricModel.empty(),
        quizzes = const ProgressMetricModel.empty(),
        overall = null,
        completedMaterialIds = const [],
        completedMoleculeIds = const [],
        completedCategoryIds = const [];

  factory ProgressModel.fromJson(Map<String, dynamic> json) {
    List<int> parseIds(dynamic list) {
      if (list is List) {
        return list
            .map((e) => int.tryParse(e.toString()) ?? 0)
            .where((id) => id > 0)
            .toList();
      }
      return [];
    }

    final completedMap =
        json['completed_ids'] as Map<String, dynamic>? ?? {};

    final matIds = json['completed_material_ids'] != null
        ? parseIds(json['completed_material_ids'])
        : parseIds(completedMap['materials']);

    final molIds = json['completed_molecule_ids'] != null
        ? parseIds(json['completed_molecule_ids'])
        : parseIds(completedMap['molecules']);

    final catIds = json['completed_category_ids'] != null
        ? parseIds(json['completed_category_ids'])
        : (completedMap['quiz'] != null
            ? parseIds(completedMap['quiz'])
            : parseIds(completedMap['categories']));

    final quizJson = (json['quiz'] ?? json['quizzes']) as Map<String, dynamic>? ?? {};
    final overallJson = json['overall'] as Map<String, dynamic>?;

    return ProgressModel(
      materials: ProgressMetricModel.fromJson(
        json['materials'] as Map<String, dynamic>? ?? {},
      ),
      molecules: ProgressMetricModel.fromJson(
        json['molecules'] as Map<String, dynamic>? ?? {},
      ),
      quizzes: ProgressMetricModel.fromJson(quizJson),
      overall: overallJson != null ? ProgressMetricModel.fromJson(overallJson) : null,
      completedMaterialIds: matIds,
      completedMoleculeIds: molIds,
      completedCategoryIds: catIds,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'materials': materials.toJson(),
      'molecules': molecules.toJson(),
      'quizzes': quizzes.toJson(),
      'quiz': quizzes.toJson(),
      if (overall != null) 'overall': overall!.toJson(),
      'completed_material_ids': completedMaterialIds,
      'completed_molecule_ids': completedMoleculeIds,
      'completed_category_ids': completedCategoryIds,
      'completed_ids': {
        'materials': completedMaterialIds,
        'molecules': completedMoleculeIds,
        'quiz': completedCategoryIds,
      },
    };
  }

  @override
  List<Object?> get props => [
        materials,
        molecules,
        quizzes,
        overall,
        completedMaterialIds,
        completedMoleculeIds,
        completedCategoryIds,
      ];
}
