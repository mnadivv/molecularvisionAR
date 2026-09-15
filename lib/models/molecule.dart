import 'package:flutter/material.dart';

class Molecule {
  final String name;
  final String formula;
  final String geometry;
  final String description;
  final IconData icon;
  final Color color;

  const Molecule({
    required this.name,
    required this.formula,
    required this.geometry,
    required this.description,
    required this.icon,
    required this.color,
  });
}

const List<Molecule> molecules = [

  Molecule(
    name: "Air",
    formula: "H₂O",
    geometry: "Bengkok (Bent)",
    description: "Molekul air tersusun atas dua atom hidrogen dan satu atom oksigen.",
    icon: Icons.water_drop,
    color: Colors.blue,
  ),

  Molecule(
    name: "Karbon Dioksida",
    formula: "CO₂",
    geometry: "Linear",
    description: "Gas hasil respirasi dan pembakaran.",
    icon: Icons.cloud,
    color: Colors.green,
  ),

  Molecule(
    name: "Metana",
    formula: "CH₄",
    geometry: "Tetrahedral",
    description: "Komponen utama gas alam.",
    icon: Icons.hexagon,
    color: Colors.orange,
  ),

  Molecule(
    name: "Amonia",
    formula: "NH₃",
    geometry: "Trigonal Piramida",
    description: "Senyawa nitrogen dan hidrogen.",
    icon: Icons.science,
    color: Colors.purple,
  ),

  Molecule(
    name: "Natrium Klorida",
    formula: "NaCl",
    geometry: "Kristal Ionik",
    description: "Garam dapur yang umum digunakan.",
    icon: Icons.grain,
    color: Colors.teal,
  ),

  Molecule(
    name: "Benzena",
    formula: "C₆H₆",
    geometry: "Cincin Heksagonal",
    description: "Senyawa aromatik dasar.",
    icon: Icons.blur_circular,
    color: Colors.red,
  ),
];