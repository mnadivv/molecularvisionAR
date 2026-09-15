import 'dart:math' as math;

import 'package:flutter/material.dart';

import '../../models/molecule.dart';
import '../../utils/app_colors.dart';

class MoleculeScreen extends StatefulWidget {
  const MoleculeScreen({super.key});

  @override
  State<MoleculeScreen> createState() => _MoleculeScreenState();
}

class _MoleculeScreenState extends State<MoleculeScreen> {
  int selectedIndex = 0;

  @override
  Widget build(BuildContext context) {
    final molecule = molecules[selectedIndex];

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        foregroundColor: Colors.white,
        title: const Text(
          'Molecular Vision',
          style: TextStyle(fontWeight: FontWeight.w700),
        ),
      ),
      body: SafeArea(
        child: Column(
          children: [
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 8, 20, 14),
              child: _IntroCard(molecule: molecule),
            ),
            SizedBox(
              height: 86,
              child: ListView.separated(
                padding: const EdgeInsets.symmetric(horizontal: 20),
                scrollDirection: Axis.horizontal,
                itemCount: molecules.length,
                separatorBuilder: (_, __) => const SizedBox(width: 10),
                itemBuilder: (context, index) {
                  final item = molecules[index];
                  final selected = index == selectedIndex;
                  return GestureDetector(
                    onTap: () => setState(() => selectedIndex = index),
                    child: AnimatedContainer(
                      duration: const Duration(milliseconds: 220),
                      width: 112,
                      padding: const EdgeInsets.all(10),
                      decoration: BoxDecoration(
                        color: selected
                            ? item.color.withValues(alpha: .22)
                            : Colors.white.withValues(alpha: .06),
                        borderRadius: BorderRadius.circular(18),
                        border: Border.all(
                          color: selected
                              ? item.color.withValues(alpha: .75)
                              : Colors.white.withValues(alpha: .08),
                        ),
                      ),
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(item.icon, color: item.color, size: 25),
                          const SizedBox(height: 5),
                          Text(
                            item.formula,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                            style: const TextStyle(
                              color: Colors.white,
                              fontWeight: FontWeight.w700,
                            ),
                          ),
                        ],
                      ),
                    ),
                  );
                },
              ),
            ),
            const SizedBox(height: 12),
            Expanded(
              child: MolecularDetailView(molecule: molecule),
            ),
          ],
        ),
      ),
    );
  }
}

class _IntroCard extends StatelessWidget {
  final Molecule molecule;

  const _IntroCard({required this.molecule});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: [
            molecule.color.withValues(alpha: .20),
            Colors.white.withValues(alpha: .05),
          ],
        ),
        borderRadius: BorderRadius.circular(22),
        border: Border.all(color: Colors.white.withValues(alpha: .08)),
      ),
      child: Row(
        children: [
          Container(
            width: 48,
            height: 48,
            decoration: BoxDecoration(
              color: molecule.color.withValues(alpha: .16),
              shape: BoxShape.circle,
            ),
            child: Icon(molecule.icon, color: molecule.color),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  molecule.name,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 18,
                    fontWeight: FontWeight.w800,
                  ),
                ),
                const SizedBox(height: 2),
                Text(
                  molecule.formula,
                  style: TextStyle(
                    color: molecule.color,
                    fontSize: 14,
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ],
            ),
          ),
          const Icon(Icons.threed_rotation, color: Colors.white54),
        ],
      ),
    );
  }
}

class MolecularDetailView extends StatefulWidget {
  final Molecule molecule;

  const MolecularDetailView({super.key, required this.molecule});

  @override
  State<MolecularDetailView> createState() => _MolecularDetailViewState();
}

class _MolecularDetailViewState extends State<MolecularDetailView> {
  double yaw = -.35;
  double pitch = -.12;
  double scale = 1.0;

  void _reset() {
    setState(() {
      yaw = -.35;
      pitch = -.12;
      scale = 1.0;
    });
  }

  @override
  Widget build(BuildContext context) {
    final atoms = _atomsFor(widget.molecule.formula);
    final bonds = _bondsFor(widget.molecule.formula);

    return LayoutBuilder(
      builder: (context, constraints) {
        final size = math.min(constraints.maxWidth, constraints.maxHeight - 112);

        return Column(
          children: [
            Expanded(
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 18),
                child: GestureDetector(
                  onScaleStart: (_) {},
                  onScaleUpdate: (details) {
                    setState(() {
                      if (details.pointerCount >= 2) {
                        scale = (scale * details.scale).clamp(.65, 1.8);
                      } else {
                        yaw += details.focalPointDelta.dx * .012;
                        pitch = (pitch - details.focalPointDelta.dy * .012)
                            .clamp(-1.15, 1.15);
                      }
                    });
                  },
                  child: Container(
                    decoration: BoxDecoration(
                      borderRadius: BorderRadius.circular(28),
                      color: Colors.black.withValues(alpha: .12),
                      border: Border.all(
                        color: Colors.white.withValues(alpha: .08),
                      ),
                    ),
                    child: Stack(
                      children: [
                        Center(
                          child: SizedBox(
                            width: size,
                            height: size,
                            child: CustomPaint(
                              painter: MoleculePainter(
                                atoms: atoms,
                                bonds: bonds,
                                yaw: yaw,
                                pitch: pitch,
                                scale: scale,
                              ),
                            ),
                          ),
                        ),
                        Positioned(
                          top: 14,
                          right: 14,
                          child: IconButton.filledTonal(
                            onPressed: _reset,
                            tooltip: 'Reset tampilan',
                            icon: const Icon(Icons.refresh_rounded),
                          ),
                        ),
                        const Positioned(
                          left: 18,
                          bottom: 14,
                          child: Row(
                            children: [
                              Icon(Icons.touch_app, size: 17, color: Colors.white54),
                              SizedBox(width: 6),
                              Text(
                                'Geser untuk rotasi • Cubit untuk zoom',
                                style: TextStyle(color: Colors.white54, fontSize: 11),
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(18, 12, 18, 16),
              child: _MoleculeInfo(molecule: widget.molecule),
            ),
          ],
        );
      },
    );
  }
}

class _MoleculeInfo extends StatelessWidget {
  final Molecule molecule;

  const _MoleculeInfo({required this.molecule});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white.withValues(alpha: .055),
        borderRadius: BorderRadius.circular(20),
      ),
      child: Row(
        children: [
          Expanded(child: _Info(label: 'Geometri', value: molecule.geometry)),
          Container(width: 1, height: 35, color: Colors.white12),
          const SizedBox(width: 14),
          Expanded(
            flex: 2,
            child: _Info(label: 'Tentang', value: molecule.description),
          ),
        ],
      ),
    );
  }
}

class _Info extends StatelessWidget {
  final String label;
  final String value;

  const _Info({required this.label, required this.value});

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(label, style: const TextStyle(color: Colors.white54, fontSize: 11)),
        const SizedBox(height: 4),
        Text(
          value,
          maxLines: 3,
          overflow: TextOverflow.ellipsis,
          style: const TextStyle(
            color: Colors.white,
            fontSize: 12,
            height: 1.3,
            fontWeight: FontWeight.w600,
          ),
        ),
      ],
    );
  }
}

class AtomPoint {
  final String element;
  final double x;
  final double y;
  final double z;
  final double radius;

  const AtomPoint(this.element, this.x, this.y, this.z, this.radius);
}

class BondPoint {
  final int a;
  final int b;

  const BondPoint(this.a, this.b);
}

class MoleculePainter extends CustomPainter {
  final List<AtomPoint> atoms;
  final List<BondPoint> bonds;
  final double yaw;
  final double pitch;
  final double scale;

  MoleculePainter({
    required this.atoms,
    required this.bonds,
    required this.yaw,
    required this.pitch,
    required this.scale,
  });

  @override
  void paint(Canvas canvas, Size size) {
    final center = Offset(size.width / 2, size.height / 2);
    final base = math.min(size.width, size.height) * .28 * scale;

    final projected = <_Projected>[];
    for (final atom in atoms) {
      final p = _project(atom, base);
      projected.add(p);
    }

    final sortedIndices = List<int>.generate(atoms.length, (i) => i)
      ..sort((a, b) => projected[a].z.compareTo(projected[b].z));

    final bondPaint = Paint()
      ..strokeWidth = 8
      ..strokeCap = StrokeCap.round
      ..color = Colors.white.withValues(alpha: .42);

    for (final bond in bonds) {
      final a = projected[bond.a];
      final b = projected[bond.b];
      canvas.drawLine(
        center + Offset(a.x, a.y),
        center + Offset(b.x, b.y),
        bondPaint,
      );
    }

    for (final index in sortedIndices) {
      final atom = atoms[index];
      final p = projected[index];
      final depth = ((p.z + 1) / 2).clamp(.0, 1.0);
      final radius = base * atom.radius * (.78 + depth * .22);
      final color = _elementColor(atom.element);

      final shadow = Paint()
        ..color = Colors.black.withValues(alpha: .35)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 8);
      canvas.drawCircle(center + Offset(p.x + 4, p.y + 7), radius, shadow);

      final gradient = RadialGradient(
        center: const Alignment(-.35, -.4),
        radius: 1.0,
        colors: [
          Colors.white.withValues(alpha: .88),
          color,
          Color.lerp(color, Colors.black, .25)!,
        ],
        stops: const [.05, .42, 1],
      );

      canvas.drawCircle(
        center + Offset(p.x, p.y),
        radius,
        Paint()..shader = gradient.createShader(
          Rect.fromCircle(center: center + Offset(p.x, p.y), radius: radius),
        ),
      );

      final text = TextPainter(
        text: TextSpan(
          text: atom.element,
          style: TextStyle(
            color: Colors.white,
            fontSize: math.max(9, radius * .38),
            fontWeight: FontWeight.w800,
          ),
        ),
        textDirection: TextDirection.ltr,
      )..layout();
      text.paint(
        canvas,
        center + Offset(p.x - text.width / 2, p.y - text.height / 2),
      );
    }
  }

  _Projected _project(AtomPoint atom, double base) {
    final cy = math.cos(yaw);
    final sy = math.sin(yaw);
    final cp = math.cos(pitch);
    final sp = math.sin(pitch);

    final x1 = atom.x * cy - atom.z * sy;
    final z1 = atom.x * sy + atom.z * cy;
    final y1 = atom.y * cp - z1 * sp;
    final z2 = atom.y * sp + z1 * cp;

    return _Projected(x1 * base, y1 * base, z2);
  }

  @override
  bool shouldRepaint(covariant MoleculePainter oldDelegate) =>
      oldDelegate.yaw != yaw ||
      oldDelegate.pitch != pitch ||
      oldDelegate.scale != scale ||
      oldDelegate.atoms != atoms;
}

class _Projected {
  final double x;
  final double y;
  final double z;

  const _Projected(this.x, this.y, this.z);
}

Color _elementColor(String element) {
  switch (element) {
    case 'H':
      return const Color(0xFF90CAF9);
    case 'O':
      return const Color(0xFFE57373);
    case 'C':
      return const Color(0xFF616161);
    case 'N':
      return const Color(0xFF7986CB);
    case 'Na':
      return const Color(0xFFFFB74D);
    case 'Cl':
      return const Color(0xFF81C784);
    default:
      return Colors.blueGrey;
  }
}

List<AtomPoint> _atomsFor(String formula) {
  switch (formula) {
    case 'H₂O':
      return const [
        AtomPoint('O', 0, 0, 0, .95),
        AtomPoint('H', -.92, .35, .05, .62),
        AtomPoint('H', .92, .35, .05, .62),
      ];
    case 'CO₂':
      return const [
        AtomPoint('O', -1.25, 0, 0, .68),
        AtomPoint('C', 0, 0, 0, .82),
        AtomPoint('O', 1.25, 0, 0, .68),
      ];
    case 'CH₄':
      return const [
        AtomPoint('C', 0, 0, 0, .85),
        AtomPoint('H', 0, -1.0, 0, .52),
        AtomPoint('H', .95, .35, .25, .52),
        AtomPoint('H', -.82, .35, .45, .52),
        AtomPoint('H', 0, .45, -1.0, .52),
      ];
    case 'NH₃':
      return const [
        AtomPoint('N', 0, -.05, 0, .86),
        AtomPoint('H', 0, .95, .15, .52),
        AtomPoint('H', -.82, .35, .35, .52),
        AtomPoint('H', .82, .35, .35, .52),
      ];
    case 'NaCl':
      return const [
        AtomPoint('Na', -.85, 0, 0, .85),
        AtomPoint('Cl', .85, 0, 0, 1.0),
      ];
    case 'C₆H₆':
      final atoms = <AtomPoint>[];
      for (var i = 0; i < 6; i++) {
        final a = i * math.pi / 3;
        atoms.add(AtomPoint('C', math.cos(a) * 1.0, math.sin(a) * 1.0, 0, .72));
      }
      for (var i = 0; i < 6; i++) {
        final a = i * math.pi / 3;
        atoms.add(AtomPoint('H', math.cos(a) * 1.65, math.sin(a) * 1.65, 0, .46));
      }
      return atoms;
    default:
      return const [AtomPoint('C', 0, 0, 0, .9)];
  }
}

List<BondPoint> _bondsFor(String formula) {
  switch (formula) {
    case 'H₂O':
      return const [BondPoint(0, 1), BondPoint(0, 2)];
    case 'CO₂':
      return const [BondPoint(0, 1), BondPoint(1, 2)];
    case 'CH₄':
      return const [
        BondPoint(0, 1), BondPoint(0, 2), BondPoint(0, 3), BondPoint(0, 4),
      ];
    case 'NH₃':
      return const [BondPoint(0, 1), BondPoint(0, 2), BondPoint(0, 3)];
    case 'NaCl':
      return const [BondPoint(0, 1)];
    case 'C₆H₆':
      final bonds = <BondPoint>[];
      for (var i = 0; i < 6; i++) {
        bonds.add(BondPoint(i, (i + 1) % 6));
        bonds.add(BondPoint(i, 6 + i));
      }
      return bonds;
    default:
      return const [];
  }
}
