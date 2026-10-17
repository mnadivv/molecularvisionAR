import 'package:equatable/equatable.dart';
import '../../../../data/models/category/category_model.dart';
import '../../../../data/models/material/material_model.dart';

abstract class MateriState extends Equatable {
  const MateriState();

  @override
  List<Object?> get props => [];
}

class MateriInitial extends MateriState {
  const MateriInitial();
}

class MateriLoading extends MateriState {
  const MateriLoading();
}

class MateriLoaded extends MateriState {
  final List<CategoryModel> categories;
  final List<MaterialModel> materials;
  final int? selectedCategoryId;
  final String searchQuery;
  final bool isFiltering;

  const MateriLoaded({
    required this.categories,
    required this.materials,
    this.selectedCategoryId,
    this.searchQuery = '',
    this.isFiltering = false,
  });

  MateriLoaded copyWith({
    List<CategoryModel>? categories,
    List<MaterialModel>? materials,
    int? selectedCategoryId,
    bool clearSelectedCategory = false,
    String? searchQuery,
    bool? isFiltering,
  }) {
    return MateriLoaded(
      categories: categories ?? this.categories,
      materials: materials ?? this.materials,
      selectedCategoryId: clearSelectedCategory
          ? null
          : (selectedCategoryId ?? this.selectedCategoryId),
      searchQuery: searchQuery ?? this.searchQuery,
      isFiltering: isFiltering ?? this.isFiltering,
    );
  }

  @override
  List<Object?> get props => [
        categories,
        materials,
        selectedCategoryId,
        searchQuery,
        isFiltering,
      ];
}

class MateriFailure extends MateriState {
  final String message;

  const MateriFailure(this.message);

  @override
  List<Object?> get props => [message];
}
