import 'package:flutter/material.dart';
import '../../../../core/theme/app_colors.dart';

/// Horizontal Chip Selector Kategori Bentuk Geometri Molekul persis sesuai tema desain sistem Materi
class MoleculeShapeChips extends StatelessWidget {
  final List<String> shapes;
  final String? selectedShape;
  final ValueChanged<String> onShapeSelected;

  const MoleculeShapeChips({
    super.key,
    required this.shapes,
    required this.selectedShape,
    required this.onShapeSelected,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Kategori Bentuk Molekul',
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
            itemCount: shapes.length,
            separatorBuilder: (_, __) => const SizedBox(width: 10),
            itemBuilder: (context, index) {
              final shape = shapes[index];
              final isSelected = (shape == 'Semua' && selectedShape == null) ||
                  selectedShape == shape;

              return Material(
                color: Colors.transparent,
                child: InkWell(
                  onTap: () => onShapeSelected(shape),
                  borderRadius: BorderRadius.circular(14),
                  child: AnimatedContainer(
                    duration: const Duration(milliseconds: 200),
                    padding: const EdgeInsets.symmetric(
                      horizontal: 16,
                      vertical: 10,
                    ),
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
                      shape,
                      style: TextStyle(
                        color: isSelected
                            ? Colors.white
                            : Colors.white.withValues(alpha: 0.85),
                        fontSize: 13,
                        fontWeight: isSelected
                            ? FontWeight.bold
                            : FontWeight.w500,
                        letterSpacing: 0.2,
                      ),
                    ),
                  ),
                ),
              );
            },
          ),
        ),
      ],
    );
  }
}
