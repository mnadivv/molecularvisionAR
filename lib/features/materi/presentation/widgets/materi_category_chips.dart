import 'package:flutter/material.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../data/models/category/category_model.dart';

/// Horizontal Chip Selector Kategori Pembelajaran Kimia sesuai Figma 65:135-141
class MateriCategoryChips extends StatelessWidget {
  final List<CategoryModel> categories;
  final int? selectedCategoryId;
  final ValueChanged<int?> onCategorySelected;

  const MateriCategoryChips({
    super.key,
    required this.categories,
    required this.selectedCategoryId,
    required this.onCategorySelected,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Kategori Materi',
          style: TextStyle(
            color: Colors.white,
            fontSize: 16,
            fontWeight: FontWeight.bold,
            letterSpacing: 0.3,
          ),
        ),
        const SizedBox(height: 12),
        SizedBox(
          height: 42,
          child: ListView.separated(
            scrollDirection: Axis.horizontal,
            itemCount: categories.length + 1, // +1 untuk chip "Semua"
            separatorBuilder: (_, __) => const SizedBox(width: 10),
            itemBuilder: (context, index) {
              if (index == 0) {
                final isSelected = selectedCategoryId == null;
                return _buildChip(
                  label: 'Semua',
                  isSelected: isSelected,
                  onTap: () => onCategorySelected(null),
                );
              }

              final category = categories[index - 1];
              final isSelected = selectedCategoryId == category.idCategory;

              return _buildChip(
                label: category.name,
                isSelected: isSelected,
                onTap: () => onCategorySelected(category.idCategory),
              );
            },
          ),
        ),
      ],
    );
  }

  Widget _buildChip({
    required String label,
    required bool isSelected,
    required VoidCallback onTap,
  }) {
    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(14),
        child: AnimatedContainer(
          duration: const Duration(milliseconds: 200),
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(14),
            color: isSelected
                ? const Color(0xFF38438A)
                : Colors.white.withValues(alpha: 0.08),
            border: Border.all(
              color: isSelected
                  ? AppColors.accent
                  : Colors.white.withValues(alpha: 0.12),
              width: isSelected ? 1.4 : 1.0,
            ),
            boxShadow: isSelected
                ? [
                    BoxShadow(
                      color: AppColors.accent.withValues(alpha: 0.25),
                      blurRadius: 10,
                      offset: const Offset(0, 3),
                    ),
                  ]
                : null,
          ),
          alignment: Alignment.center,
          child: Text(
            label,
            style: TextStyle(
              color: isSelected ? Colors.white : Colors.white70,
              fontSize: 13,
              fontWeight: isSelected ? FontWeight.bold : FontWeight.w500,
            ),
          ),
        ),
      ),
    );
  }
}
