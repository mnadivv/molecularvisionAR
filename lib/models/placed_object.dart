import 'package:flutter/material.dart';

class PlacedObject {

  final String id;
  final String name;
  final String image;

  Offset position;

  final bool isTool;

  bool selected;
  bool reacted;

  double rotation;
  double scale;

  PlacedObject({
    required this.id,
    required this.name,
    required this.image,
    required this.position,
    required this.isTool,
    this.selected = false,
    this.reacted = false,
    this.rotation = 0,
    this.scale = 1,
  });

}