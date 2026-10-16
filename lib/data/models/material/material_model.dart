import 'package:equatable/equatable.dart';
import '../category/category_model.dart';

/// Model materi pembelajaran kimia
class MaterialModel extends Equatable {
  final int idMaterial;
  final int idCategory;
  final String name;
  final int order;
  final String? description;
  final String? content;
  final CategoryModel? category;

  const MaterialModel({
    required this.idMaterial,
    required this.idCategory,
    required this.name,
    this.order = 0,
    this.description,
    this.content,
    this.category,
  });

  factory MaterialModel.fromJson(Map<String, dynamic> json) {
    return MaterialModel(
      idMaterial: json['id_material'] is int
          ? json['id_material'] as int
          : int.tryParse(json['id_material']?.toString() ?? '') ?? 0,
      idCategory: json['id_category'] is int
          ? json['id_category'] as int
          : int.tryParse(json['id_category']?.toString() ?? '') ?? 0,
      name: json['name'] as String? ?? '',
      order: json['order'] is int
          ? json['order'] as int
          : int.tryParse(json['order']?.toString() ?? '') ?? 0,
      description: json['description'] as String?,
      content: json['content'] as String?,
      category: json['category'] != null && json['category'] is Map<String, dynamic>
          ? CategoryModel.fromJson(json['category'] as Map<String, dynamic>)
          : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id_material': idMaterial,
      'id_category': idCategory,
      'name': name,
      'order': order,
      'description': description,
      'content': content,
      if (category != null) 'category': category!.toJson(),
    };
  }

  @override
  List<Object?> get props => [
        idMaterial,
        idCategory,
        name,
        order,
        description,
        content,
        category,
      ];
}
