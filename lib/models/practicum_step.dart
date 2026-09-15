enum ItemType {
  tool,
  material,
}

class PracticumStep {
  final int step;

  final String title;

  final String instruction;

  final String explanation;

  final String hint;

  final String successMessage;

  final String requiredItemId;

  final ItemType itemType;

  const PracticumStep({
    required this.step,
    required this.title,
    required this.instruction,
    required this.explanation,
    required this.hint,
    required this.successMessage,
    required this.requiredItemId,
    required this.itemType,
  });
}