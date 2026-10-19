import 'package:equatable/equatable.dart';
import 'package:molecular_vision_ar/data/models/exercise/exercise_model.dart';

abstract class QuizCategoryState extends Equatable {
  const QuizCategoryState();

  @override
  List<Object?> get props => [];
}

class QuizCategoryInitial extends QuizCategoryState {
  const QuizCategoryInitial();
}

class QuizCategoryLoading extends QuizCategoryState {
  const QuizCategoryLoading();
}

class QuizCategoryLoaded extends QuizCategoryState {
  final List<QuizCategoryModel> categories;
  final Set<int> completedCategoryIds;
  final String searchQuery;
  final bool isRefreshing;

  const QuizCategoryLoaded({
    required this.categories,
    required this.completedCategoryIds,
    this.searchQuery = '',
    this.isRefreshing = false,
  });

  int get totalCategoriesCount => categories.length;
  int get completedCategoriesCount => completedCategoryIds.length;

  List<QuizCategoryModel> get filteredCategories {
    if (searchQuery.trim().isEmpty) return categories;
    final query = searchQuery.toLowerCase();
    return categories
        .where((cat) =>
            cat.name.toLowerCase().contains(query) ||
            (cat.description?.toLowerCase().contains(query) ?? false))
        .toList();
  }

  QuizCategoryLoaded copyWith({
    List<QuizCategoryModel>? categories,
    Set<int>? completedCategoryIds,
    String? searchQuery,
    bool? isRefreshing,
  }) {
    return QuizCategoryLoaded(
      categories: categories ?? this.categories,
      completedCategoryIds: completedCategoryIds ?? this.completedCategoryIds,
      searchQuery: searchQuery ?? this.searchQuery,
      isRefreshing: isRefreshing ?? this.isRefreshing,
    );
  }

  @override
  List<Object?> get props => [
        categories,
        completedCategoryIds,
        searchQuery,
        isRefreshing,
      ];
}

class QuizCategoryFailure extends QuizCategoryState {
  final String message;

  const QuizCategoryFailure(this.message);

  @override
  List<Object?> get props => [message];
}
