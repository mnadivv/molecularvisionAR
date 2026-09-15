class ToolItem {
  final String id;

  final String name;

  final String image;

  final String description;

  final String category;

  final bool draggable;

  const ToolItem({
    required this.id,
    required this.name,
    required this.image,
    required this.description,
    required this.category,
    this.draggable = true,
  });
}