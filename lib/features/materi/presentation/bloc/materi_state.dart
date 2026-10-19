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
  final Set<int> completedMaterialIds;
  final int? progressSuccessMaterialId;

  const MateriLoaded({
    required this.categories,
    required this.materials,
    this.selectedCategoryId,
    this.searchQuery = '',
    this.isFiltering = false,
    this.completedMaterialIds = const {},
    this.progressSuccessMaterialId,
  });

  MateriLoaded copyWith({
    List<CategoryModel>? categories,
    List<MaterialModel>? materials,
    int? selectedCategoryId,
    bool clearSelectedCategory = false,
    String? searchQuery,
    bool? isFiltering,
    Set<int>? completedMaterialIds,
    int? progressSuccessMaterialId,
  }) {
    return MateriLoaded(
      categories: categories ?? this.categories,
      materials: materials ?? this.materials,
      selectedCategoryId: clearSelectedCategory
          ? null
          : (selectedCategoryId ?? this.selectedCategoryId),
      searchQuery: searchQuery ?? this.searchQuery,
      isFiltering: isFiltering ?? this.isFiltering,
      completedMaterialIds: completedMaterialIds ?? this.completedMaterialIds,
      progressSuccessMaterialId: progressSuccessMaterialId,
    );
  }

  @override
  List<Object?> get props => [
        categories,
        materials,
        selectedCategoryId,
        searchQuery,
        isFiltering,
        completedMaterialIds,
        progressSuccessMaterialId,
      ];
}

class MateriFailure extends MateriState {
  final String message;

  const MateriFailure(this.message);

  @override
  List<Object?> get props => [message];
}
