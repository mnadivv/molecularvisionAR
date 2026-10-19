import 'package:equatable/equatable.dart';

/// Model kategori pembelajaran kimia
class CategoryModel extends Equatable {
  final int idCategory;
  final String name;
  final int order;
  final String? description;
  final int materialsCount;
  final int questionsCount;

  const CategoryModel({
    required this.idCategory,
    required this.name,
    this.order = 0,
    this.description,
    this.materialsCount = 0,
    this.questionsCount = 0,
  });

  factory CategoryModel.fromJson(Map<String, dynamic> json) {
    return CategoryModel(
      idCategory: json['id_category'] is int
          ? json['id_category'] as int
          : int.tryParse(json['id_category']?.toString() ?? '') ?? 0,
      name: json['name'] as String? ?? '',
      order: json['order'] is int
          ? json['order'] as int
          : int.tryParse(json['order']?.toString() ?? '') ?? 0,
      description: json['description'] as String?,
      materialsCount: json['materials_count'] is int
          ? json['materials_count'] as int
          : int.tryParse(json['materials_count']?.toString() ?? '') ?? 0,
      questionsCount: json['questions_count'] is int
          ? json['questions_count'] as int
          : int.tryParse(json['questions_count']?.toString() ?? '') ?? 0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id_category': idCategory,
      'name': name,
      'order': order,
      'description': description,
      'materials_count': materialsCount,
      'questions_count': questionsCount,
    };
  }

  @override
  List<Object?> get props => [
        idCategory,
        name,
        order,
        description,
        materialsCount,
        questionsCount,
      ];
}
