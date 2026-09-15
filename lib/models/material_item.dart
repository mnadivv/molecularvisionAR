class MaterialItem {
  final String id;

  final String name;

  final String formula;

  final String image;

  final String description;

  final bool draggable;

  const MaterialItem({
    required this.id,
    required this.name,
    required this.formula,
    required this.image,
    required this.description,
    this.draggable = true,
  });
}