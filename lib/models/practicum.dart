import 'material_item.dart';
import 'practicum_step.dart';
import 'tool_item.dart';

class Practicum {
  final String id;

  final String title;

  final String description;

  final String objective;

  final int estimatedMinutes;

  final int difficulty;

  final List<ToolItem> tools;

  final List<MaterialItem> materials;

  final List<PracticumStep> steps;

  const Practicum({
    required this.id,
    required this.title,
    required this.description,
    required this.objective,
    required this.estimatedMinutes,
    required this.difficulty,
    required this.tools,
    required this.materials,
    required this.steps,
  });
}