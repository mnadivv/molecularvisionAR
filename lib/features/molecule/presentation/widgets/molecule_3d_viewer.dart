import 'dart:io';
import 'dart:math' as math;
import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:model_viewer_plus/model_viewer_plus.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../data/models/molecule/molecule_model.dart';
import '../../data/services/molecule_3d_cache_service.dart';

/// Widget visualisasi 3D molekul kimia berkecepatan tinggi dengan auto-rotate, 360° gesture orbit, dan zoom
class Molecule3DViewer extends StatefulWidget {
  final MoleculeModel molecule;

  const Molecule3DViewer({
    super.key,
    required this.molecule,
  });

  @override
  State<Molecule3DViewer> createState() => _Molecule3DViewerState();
}

class _Molecule3DViewerState extends State<Molecule3DViewer>
    with SingleTickerProviderStateMixin {
  // Mode default: Engine 3D Native instan (0ms load, 60fps fluid)
  bool _useGlb = false;
  bool _autoRotate = true;
  Key _modelViewerKey = UniqueKey();

  // Engine 3D Native Coordinates & Zoom
  double _yaw = -0.35;
  double _pitch = -0.12;
  double _scale = 1.0;

  late final AnimationController _animController;

  // 3D Cache & Download Progress State
  final Molecule3DCacheService _cacheService = Molecule3DCacheService();
  File? _cachedFile;
  String? _cachedHttpUrl;
  bool _isDownloading = false;
  double _downloadProgress = 0.0;
  int _receivedBytes = 0;
  int _totalBytes = 0;
  String? _downloadError;
  CancelToken? _cancelToken;
  bool _isGlbViewReady = false;

  @override
  void initState() {
    super.initState();
    _animController = AnimationController(
      vsync: this,
      duration: const Duration(seconds: 16),
    )..addListener(() {
        if (_autoRotate && !_useGlb) {
          setState(() {
            _yaw += 0.012;
            if (_yaw > math.pi * 2) {
              _yaw -= math.pi * 2;
            }
          });
        }
      });

    if (_autoRotate) {
      _animController.repeat();
    }

    _checkLocalCache();
  }

  Future<void> _checkLocalCache() async {
    final url = widget.molecule.model3dUrl;
    if (url != null && url.isNotEmpty) {
      final file = await _cacheService.getCachedModelFile(url);
      final httpUrl = await _cacheService.getCachedHttpUrl(url);
      if (mounted) {
        setState(() {
          _cachedFile = file;
          _cachedHttpUrl = httpUrl;
        });
      }
    }
  }

  @override
  void didUpdateWidget(covariant Molecule3DViewer oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.molecule.idMolecule != widget.molecule.idMolecule) {
      _cancelToken?.cancel();
      setState(() {
        _useGlb = false; // Selalu mulai dengan native 3D instan
        _modelViewerKey = UniqueKey();
        _yaw = -0.35;
        _pitch = -0.12;
        _scale = 1.0;
        _cachedFile = null;
        _cachedHttpUrl = null;
        _isDownloading = false;
        _downloadProgress = 0.0;
        _receivedBytes = 0;
        _totalBytes = 0;
        _downloadError = null;
      });
      _checkLocalCache();
    }
  }

  @override
  void dispose() {
    _cancelToken?.cancel();
    _animController.dispose();
    super.dispose();
  }

  void _resetView() {
    setState(() {
      _modelViewerKey = UniqueKey();
      _yaw = -0.35;
      _pitch = -0.12;
      _scale = 1.0;
    });
  }

  void _toggleAutoRotate() {
    setState(() {
      _autoRotate = !_autoRotate;
      if (_autoRotate) {
        if (!_animController.isAnimating) {
          _animController.repeat();
        }
      } else {
        _animController.stop();
      }
    });
  }

  void _zoomIn() {
    setState(() {
      _scale = (_scale * 1.2).clamp(0.6, 2.5);
    });
  }

  void _zoomOut() {
    setState(() {
      _scale = (_scale / 1.2).clamp(0.6, 2.5);
    });
  }

  bool get _hasGlbUrl {
    final url = widget.molecule.model3dUrl;
    return url != null &&
        url.trim().isNotEmpty &&
        (url.endsWith('.glb') || url.endsWith('.gltf') || url.contains('.glb'));
  }

  bool get _isDeadHost {
    final url = widget.molecule.model3dUrl ?? '';
    return url.contains('assets.mvar.id');
  }

  Future<void> _startDownloadModel() async {
    final url = widget.molecule.model3dUrl;
    if (url == null || url.trim().isEmpty) return;

    if (_isDeadHost) {
      setState(() {
        _downloadError =
            'Domain assets.mvar.id belum aktif di server DNS.\nGunakan mode 3D Interaktif bawaan.';
      });
      return;
    }

    setState(() {
      _isDownloading = true;
      _downloadProgress = 0.0;
      _receivedBytes = 0;
      _totalBytes = 0;
      _downloadError = null;
      _cancelToken = CancelToken();
    });

    try {
      final file = await _cacheService.downloadAndCacheModel(
        url,
        cancelToken: _cancelToken,
        onProgress: (received, total, progress) {
          if (mounted) {
            setState(() {
              _receivedBytes = received;
              _totalBytes = total;
              _downloadProgress = progress;
            });
          }
        },
      );

      String? httpUrl;
      if (file != null) {
        httpUrl = await _cacheService.getCachedHttpUrl(url);
      }

      if (mounted) {
        setState(() {
          _cachedFile = file;
          _cachedHttpUrl = httpUrl;
          _isDownloading = false;
          _modelViewerKey = UniqueKey();
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _isDownloading = false;
          _downloadError = 'Gagal mengunduh model 3D: $e';
        });
      }
    }
  }

  void _toggleGlbMode() {
    if (_useGlb) {
      setState(() {
        _useGlb = false;
        _isGlbViewReady = false;
      });
    } else {
      setState(() {
        _useGlb = true;
        _isGlbViewReady = false;
      });
      // Berikan jeda frame singkat agar native engine dan platform view binding stabil
      Future.delayed(const Duration(milliseconds: 150), () {
        if (mounted && _useGlb) {
          setState(() {
            _isGlbViewReady = true;
          });
        }
      });
      if (_cachedFile == null && !_isDownloading) {
        _startDownloadModel();
      }
    }
  }

  String _formatFileSize(int bytes) {
    if (bytes <= 0) return '0 B';
    if (bytes < 1024) return '$bytes B';
    if (bytes < 1024 * 1024) return '${(bytes / 1024).toStringAsFixed(1)} KB';
    return '${(bytes / (1024 * 1024)).toStringAsFixed(1)} MB';
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 330,
      decoration: BoxDecoration(
        color: const Color(0xFF071C3F).withValues(alpha: 0.88),
        borderRadius: BorderRadius.circular(28),
        border: Border.all(
          color: Colors.white.withValues(alpha: 0.14),
          width: 1.2,
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.38),
            blurRadius: 22,
            offset: const Offset(0, 10),
          ),
        ],
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(28),
        child: Stack(
          children: [
            // Ambient Radial Glow di belakang model 3D
            Center(
              child: Container(
                width: 240,
                height: 240,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  gradient: RadialGradient(
                    colors: [
                      AppColors.primary.withValues(alpha: 0.35),
                      Colors.transparent,
                    ],
                  ),
                ),
              ),
            ),

            // 3D Canvas (Native Ball-and-Stick atau ModelViewer GLB)
            Positioned.fill(
              child: (_hasGlbUrl && _useGlb)
                  ? _buildGlbOrDownloadView()
                  : _buildNativeInteractive3DViewer(),
            ),

            // Floating Controls Bar (Atas Kanan)
            Positioned(
              top: 14,
              right: 14,
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  if (_hasGlbUrl) ...[
                    _ControlIconButton(
                      icon: _useGlb
                          ? Icons.bubble_chart_rounded
                          : Icons.view_in_ar_rounded,
                      tooltip: _useGlb
                          ? 'Beralih ke 3D Interaktif (Instan)'
                          : (_cachedFile != null
                              ? 'Buka Model 3D (Tersimpan di Cache)'
                              : 'Unduh Model GLB Server'),
                      isActive: _useGlb,
                      onPressed: _toggleGlbMode,
                    ),
                    const SizedBox(width: 8),
                  ],
                  _ControlIconButton(
                    icon: _autoRotate
                        ? Icons.sync_rounded
                        : Icons.sync_disabled_rounded,
                    tooltip: _autoRotate
                        ? 'Hentikan Putaran Otomatis'
                        : 'Nyalakan Putaran Otomatis',
                    isActive: _autoRotate,
                    onPressed: _toggleAutoRotate,
                  ),
                  const SizedBox(width: 8),
                  _ControlIconButton(
                    icon: Icons.refresh_rounded,
                    tooltip: 'Reset Tampilan',
                    onPressed: _resetView,
                  ),
                ],
              ),
            ),

            // Zoom Floating Buttons (Kiri Atas)
            Positioned(
              top: 14,
              left: 14,
              child: Column(
                children: [
                  _ControlIconButton(
                    icon: Icons.add_rounded,
                    tooltip: 'Perbesar (Zoom In)',
                    onPressed: _zoomIn,
                  ),
                  const SizedBox(height: 8),
                  _ControlIconButton(
                    icon: Icons.remove_rounded,
                    tooltip: 'Perkecil (Zoom Out)',
                    onPressed: _zoomOut,
                  ),
                ],
              ),
            ),

            // Gesture Instruction Badge (Bawah Tengah)
            Positioned(
              bottom: 12,
              left: 0,
              right: 0,
              child: Center(
                child: Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 14,
                    vertical: 6,
                  ),
                  decoration: BoxDecoration(
                    color: Colors.black.withValues(alpha: 0.65),
                    borderRadius: BorderRadius.circular(20),
                    border: Border.all(
                      color: Colors.white.withValues(alpha: 0.15),
                      width: 1,
                    ),
                  ),
                  child: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(
                        Icons.touch_app_rounded,
                        size: 13,
                        color: Colors.white.withValues(alpha: 0.8),
                      ),
                      const SizedBox(width: 6),
                      Text(
                        'Geser untuk rotasi 360° • Cubit untuk zoom',
                        style: TextStyle(
                          color: Colors.white.withValues(alpha: 0.85),
                          fontSize: 11,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildGlbOrDownloadView() {
    // 1. Jika sedang mengunduh: Tampilkan Progress Bar Modern
    if (_isDownloading) {
      return Center(
        child: Container(
          margin: const EdgeInsets.symmetric(horizontal: 32),
          padding: const EdgeInsets.symmetric(horizontal: 22, vertical: 20),
          decoration: BoxDecoration(
            color: Colors.black.withValues(alpha: 0.85),
            borderRadius: BorderRadius.circular(20),
            border: Border.all(
              color: AppColors.primary.withValues(alpha: 0.4),
              width: 1.2,
            ),
            boxShadow: [
              BoxShadow(
                color: AppColors.primary.withValues(alpha: 0.2),
                blurRadius: 18,
                spreadRadius: 2,
              ),
            ],
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const SizedBox(
                    width: 18,
                    height: 18,
                    child: CircularProgressIndicator(
                      strokeWidth: 2.2,
                      valueColor: AlwaysStoppedAnimation<Color>(
                        AppColors.primary,
                      ),
                    ),
                  ),
                  const SizedBox(width: 12),
                  Text(
                    'Mengunduh Aset 3D...',
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 14,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 14),
              ClipRRect(
                borderRadius: BorderRadius.circular(8),
                child: LinearProgressIndicator(
                  value: _downloadProgress > 0 ? _downloadProgress : null,
                  minHeight: 8,
                  backgroundColor: Colors.white.withValues(alpha: 0.12),
                  valueColor: const AlwaysStoppedAnimation<Color>(
                    AppColors.primary,
                  ),
                ),
              ),
              const SizedBox(height: 10),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    '${(_downloadProgress * 100).toInt()}%',
                    style: const TextStyle(
                      color: AppColors.primary,
                      fontSize: 12.5,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  Text(
                    _totalBytes > 0
                        ? '${_formatFileSize(_receivedBytes)} / ${_formatFileSize(_totalBytes)}'
                        : _formatFileSize(_receivedBytes),
                    style: TextStyle(
                      color: Colors.white.withValues(alpha: 0.7),
                      fontSize: 12,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 12),
              Text(
                'Aset hanya diunduh 1 kali dan disimpan di memori HP Anda untuk akses instan berikutnya.',
                textAlign: TextAlign.center,
                style: TextStyle(
                  color: Colors.white.withValues(alpha: 0.55),
                  fontSize: 10.5,
                  height: 1.3,
                ),
              ),
            ],
          ),
        ),
      );
    }

    // 2. Jika ada Error Pengunduhan / Host DNS Mati
    if (_downloadError != null) {
      return Center(
        child: Container(
          margin: const EdgeInsets.symmetric(horizontal: 28),
          padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 16),
          decoration: BoxDecoration(
            color: Colors.black.withValues(alpha: 0.88),
            borderRadius: BorderRadius.circular(18),
            border: Border.all(
              color: Colors.amberAccent.withValues(alpha: 0.5),
            ),
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(
                Icons.cloud_off_rounded,
                color: Colors.amberAccent,
                size: 32,
              ),
              const SizedBox(height: 10),
              Text(
                _downloadError!,
                textAlign: TextAlign.center,
                style: TextStyle(
                  color: Colors.white.withValues(alpha: 0.9),
                  fontSize: 12,
                  height: 1.4,
                ),
              ),
              const SizedBox(height: 14),
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  ElevatedButton.icon(
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.white.withValues(alpha: 0.15),
                      foregroundColor: Colors.white,
                      elevation: 0,
                      padding: const EdgeInsets.symmetric(
                        horizontal: 14,
                        vertical: 8,
                      ),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(10),
                      ),
                    ),
                    icon: const Icon(Icons.refresh_rounded, size: 16),
                    label: const Text('Coba Lagi', style: TextStyle(fontSize: 12)),
                    onPressed: _startDownloadModel,
                  ),
                  const SizedBox(width: 10),
                  ElevatedButton.icon(
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppColors.primary,
                      foregroundColor: Colors.white,
                      elevation: 0,
                      padding: const EdgeInsets.symmetric(
                        horizontal: 14,
                        vertical: 8,
                      ),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(10),
                      ),
                    ),
                    icon: const Icon(Icons.bubble_chart_rounded, size: 16),
                    label: const Text(
                      'Mode 3D Interaktif',
                      style: TextStyle(fontSize: 12),
                    ),
                    onPressed: () {
                      setState(() {
                        _useGlb = false;
                      });
                    },
                  ),
                ],
              ),
            ],
          ),
        ),
      );
    }

    // 3. Model Siap Ditampilkan (Load dari Local HTTP Server Cache atau Remote Source)
    if (!_isGlbViewReady) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const SizedBox(
              width: 36,
              height: 36,
              child: CircularProgressIndicator(
                strokeWidth: 2.8,
                valueColor: AlwaysStoppedAnimation<Color>(AppColors.primary),
              ),
            ),
            const SizedBox(height: 12),
            Text(
              'Menyiapkan tampilan 3D...',
              style: TextStyle(
                color: Colors.white.withValues(alpha: 0.8),
                fontSize: 12,
              ),
            ),
          ],
        ),
      );
    }

    final modelSrc = _cachedHttpUrl ??
        (_cachedFile != null
            ? 'file://${_cachedFile!.path}'
            : widget.molecule.model3dUrl!);

    return Stack(
      children: [
        ModelViewer(
          key: _modelViewerKey,
          src: modelSrc,
          alt: 'Model 3D Molekul ${widget.molecule.name}',
          autoRotate: _autoRotate,
          autoRotateDelay: 1000,
          cameraControls: true,
          backgroundColor: Colors.transparent,
          disableZoom: false,
          shadowIntensity: 0.8,
          exposure: 1.1,
          ar: false,
          loading: Loading.eager,
          reveal: Reveal.auto,
          relatedCss: '''
            model-viewer::part(default-progress-bar) {
              display: none !important;
            }
          ''',
        ),
        if (_cachedFile != null)
          Positioned(
            top: 14,
            left: 14,
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
              decoration: BoxDecoration(
                color: const Color(0xFF10B981).withValues(alpha: 0.85),
                borderRadius: BorderRadius.circular(12),
              ),
              child: const Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.offline_pin_rounded, color: Colors.white, size: 13),
                  SizedBox(width: 4),
                  Text(
                    'Tersimpan di Cache',
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 10.5,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ),
            ),
          ),
      ],
    );
  }

  Widget _buildNativeInteractive3DViewer() {
    final atoms = _generateAtoms(
      widget.molecule.formula,
      widget.molecule.shape,
    );
    final bonds = _generateBonds(
      widget.molecule.formula,
      widget.molecule.shape,
      atoms.length,
    );

    return GestureDetector(
      behavior: HitTestBehavior.opaque,
      onScaleStart: (_) {
        if (_autoRotate) {
          _animController.stop();
        }
      },
      onScaleUpdate: (details) {
        setState(() {
          if (details.pointerCount >= 2) {
            _scale = (_scale * details.scale).clamp(0.60, 2.5);
          } else {
            _yaw += details.focalPointDelta.dx * 0.012;
            _pitch = (_pitch - details.focalPointDelta.dy * 0.012)
                .clamp(-1.35, 1.35);
          }
        });
      },
      onScaleEnd: (_) {
        if (_autoRotate && !_animController.isAnimating) {
          _animController.repeat();
        }
      },
      child: Center(
        child: CustomPaint(
          size: const Size(270, 270),
          painter: _BallAndStickPainter(
            atoms: atoms,
            bonds: bonds,
            yaw: _yaw,
            pitch: _pitch,
            scale: _scale,
          ),
        ),
      ),
    );
  }
}

class _ControlIconButton extends StatelessWidget {
  final IconData icon;
  final String tooltip;
  final VoidCallback onPressed;
  final bool isActive;

  const _ControlIconButton({
    required this.icon,
    required this.tooltip,
    required this.onPressed,
    this.isActive = false,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: isActive
            ? AppColors.primary.withValues(alpha: 0.6)
            : Colors.black.withValues(alpha: 0.50),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: isActive
              ? AppColors.accent.withValues(alpha: 0.7)
              : Colors.white.withValues(alpha: 0.16),
          width: 1,
        ),
      ),
      child: IconButton(
        icon: Icon(icon, color: Colors.white, size: 17),
        tooltip: tooltip,
        constraints: const BoxConstraints(minWidth: 34, minHeight: 34),
        padding: const EdgeInsets.all(7),
        onPressed: onPressed,
      ),
    );
  }
}

// ==========================================
// 3D Atomic Projection & Geometry Generator
// ==========================================
class _AtomPoint {
  final String element;
  final double x;
  final double y;
  final double z;
  final double radius;

  const _AtomPoint(this.element, this.x, this.y, this.z, this.radius);
}

class _BondPoint {
  final int a;
  final int b;

  const _BondPoint(this.a, this.b);
}

class _ProjectedAtom {
  final double x;
  final double y;
  final double z;

  const _ProjectedAtom(this.x, this.y, this.z);
}

class _BallAndStickPainter extends CustomPainter {
  final List<_AtomPoint> atoms;
  final List<_BondPoint> bonds;
  final double yaw;
  final double pitch;
  final double scale;

  _BallAndStickPainter({
    required this.atoms,
    required this.bonds,
    required this.yaw,
    required this.pitch,
    required this.scale,
  });

  @override
  void paint(Canvas canvas, Size size) {
    final center = Offset(size.width / 2, size.height / 2);
    final base = math.min(size.width, size.height) * 0.28 * scale;

    final projected = <_ProjectedAtom>[];
    for (final atom in atoms) {
      projected.add(_project(atom, base));
    }

    // Depth Sorting
    final sortedIndices = List<int>.generate(atoms.length, (i) => i)
      ..sort((a, b) => projected[a].z.compareTo(projected[b].z));

    // Ikatan Kovalen / Kimia (Bonds)
    final bondPaint = Paint()
      ..strokeWidth = 7.5
      ..strokeCap = StrokeCap.round
      ..color = Colors.white.withValues(alpha: 0.45);

    for (final bond in bonds) {
      if (bond.a < projected.length && bond.b < projected.length) {
        final a = projected[bond.a];
        final b = projected[bond.b];
        canvas.drawLine(
          center + Offset(a.x, a.y),
          center + Offset(b.x, b.y),
          bondPaint,
        );
      }
    }

    // Bola Atom (Atoms Spheres) dengan Shading 3D & Highlight
    for (final index in sortedIndices) {
      final atom = atoms[index];
      final p = projected[index];
      final depth = ((p.z + 1) / 2).clamp(0.0, 1.0);
      final radius = base * atom.radius * (0.80 + depth * 0.20);
      final color = _elementColor(atom.element);

      // Drop shadow lembut
      final shadow = Paint()
        ..color = Colors.black.withValues(alpha: 0.45)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 8);
      canvas.drawCircle(center + Offset(p.x + 4, p.y + 6), radius, shadow);

      // Gradien Radial Shading 3D
      final gradient = RadialGradient(
        center: const Alignment(-0.35, -0.40),
        radius: 0.95,
        colors: [
          Colors.white.withValues(alpha: 0.92),
          color,
          Color.lerp(color, Colors.black, 0.40)!,
        ],
        stops: const [0.05, 0.45, 1.0],
      );

      canvas.drawCircle(
        center + Offset(p.x, p.y),
        radius,
        Paint()
          ..shader = gradient.createShader(
            Rect.fromCircle(center: center + Offset(p.x, p.y), radius: radius),
          ),
      );

      // Label Simbol Kimia Elemen
      final text = TextPainter(
        text: TextSpan(
          text: atom.element,
          style: TextStyle(
            color: Colors.white,
            fontSize: math.max(10, radius * 0.40),
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

  _ProjectedAtom _project(_AtomPoint atom, double base) {
    final cy = math.cos(yaw);
    final sy = math.sin(yaw);
    final cp = math.cos(pitch);
    final sp = math.sin(pitch);

    final x1 = atom.x * cy - atom.z * sy;
    final z1 = atom.x * sy + atom.z * cy;
    final y1 = atom.y * cp - z1 * sp;
    final z2 = atom.y * sp + z1 * cp;

    return _ProjectedAtom(x1 * base, y1 * base, z2);
  }

  @override
  bool shouldRepaint(covariant _BallAndStickPainter oldDelegate) =>
      oldDelegate.yaw != yaw ||
      oldDelegate.pitch != pitch ||
      oldDelegate.scale != scale ||
      oldDelegate.atoms != atoms;
}

Color _elementColor(String element) {
  switch (element.toUpperCase()) {
    case 'H':
      return const Color(0xFF64B5F6); // Biru muda
    case 'O':
      return const Color(0xFFE53935); // Merah oksigen
    case 'C':
      return const Color(0xFF424242); // Abu-abu gelap karbon
    case 'N':
      return const Color(0xFF5C6BC0); // Indigo nitrogen
    case 'NA':
      return const Color(0xFFFFA726); // Oranye natrium
    case 'CL':
      return const Color(0xFF66BB6A); // Hijau klorin
    case 'S':
      return const Color(0xFFFDD835); // Kuning sulfur
    case 'F':
      return const Color(0xFF26A69A); // Teal fluorin
    case 'P':
      return const Color(0xFFFF7043); // Oranye-merah fosfor
    case 'BR':
      return const Color(0xFF8D6E63); // Coklat bromin
    case 'I':
      return const Color(0xFFAB47BC); // Ungu iodin
    default:
      return const Color(0xFF7E57C2);
  }
}

/// Menghasilkan titik koordinat 3D atom berdasarkan rumus dan bentuk geometri
List<_AtomPoint> _generateAtoms(String formula, String? shape) {
  final clean = formula
      .replaceAll('₂', '2')
      .replaceAll('₃', '3')
      .replaceAll('₄', '4')
      .replaceAll('₆', '6')
      .toUpperCase();
  final shapeLower = (shape ?? '').toLowerCase();

  // 1. Bent / Bengkok (Air / H2O, SO2, dll.)
  if (clean.contains('H2O') || shapeLower.contains('bent') || shapeLower.contains('bengkok')) {
    final central = clean.contains('H2O') ? 'O' : (clean.isNotEmpty ? clean[0] : 'O');
    final ligand = clean.contains('H2O') ? 'H' : 'X';
    return [
      _AtomPoint(central, 0, 0, 0, 0.95),
      _AtomPoint(ligand, -0.92, 0.38, 0.05, 0.60),
      _AtomPoint(ligand, 0.92, 0.38, 0.05, 0.60),
    ];
  }

  // 2. Linear (CO2, BeCl2, dll.)
  if (clean.contains('CO2') || shapeLower.contains('linear')) {
    final central = clean.contains('CO2') ? 'C' : 'A';
    final ligand = clean.contains('CO2') ? 'O' : 'B';
    return [
      _AtomPoint(ligand, -1.25, 0, 0, 0.70),
      _AtomPoint(central, 0, 0, 0, 0.85),
      _AtomPoint(ligand, 1.25, 0, 0, 0.70),
    ];
  }

  // 3. Tetrahedral (Metana / CH4, CCl4, SiH4, dll.)
  if (clean.contains('CH4') || shapeLower.contains('tetrahedral')) {
    final central = clean.contains('CH4') ? 'C' : 'C';
    final ligand = clean.contains('CH4') ? 'H' : 'X';
    return [
      _AtomPoint(central, 0, 0, 0, 0.85),
      _AtomPoint(ligand, 0, -1.05, 0, 0.52),
      _AtomPoint(ligand, 0.95, 0.35, 0.25, 0.52),
      _AtomPoint(ligand, -0.85, 0.35, 0.45, 0.52),
      _AtomPoint(ligand, 0, 0.45, -1.02, 0.52),
    ];
  }

  // 4. Trigonal Piramidal (Amonia / NH3, PCl3, dll.)
  if (clean.contains('NH3') || shapeLower.contains('piramid') || shapeLower.contains('pyramid')) {
    final central = clean.contains('NH3') ? 'N' : 'N';
    final ligand = clean.contains('NH3') ? 'H' : 'X';
    return [
      _AtomPoint(central, 0, -0.15, 0, 0.86),
      _AtomPoint(ligand, 0, 0.90, 0.15, 0.52),
      _AtomPoint(ligand, -0.82, 0.40, 0.35, 0.52),
      _AtomPoint(ligand, 0.82, 0.40, 0.35, 0.52),
    ];
  }

  // 5. Trigonal Planar (BF3, SO3, dll.)
  if (shapeLower.contains('planar')) {
    return const [
      _AtomPoint('B', 0, 0, 0, 0.85),
      _AtomPoint('F', 0, -1.15, 0, 0.55),
      _AtomPoint('F', 1.0, 0.58, 0, 0.55),
      _AtomPoint('F', -1.0, 0.58, 0, 0.55),
    ];
  }

  // 6. Oktahedral (SF6, dll.)
  if (clean.contains('SF6') || shapeLower.contains('oktahedral') || shapeLower.contains('octahedral')) {
    return const [
      _AtomPoint('S', 0, 0, 0, 0.90),
      _AtomPoint('F', 1.15, 0, 0, 0.55),
      _AtomPoint('F', -1.15, 0, 0, 0.55),
      _AtomPoint('F', 0, 1.15, 0, 0.55),
      _AtomPoint('F', 0, -1.15, 0, 0.55),
      _AtomPoint('F', 0, 0, 1.15, 0.55),
      _AtomPoint('F', 0, 0, -1.15, 0.55),
    ];
  }

  // 7. Senyawa Diatomik (NaCl, HCl, CO, dll.)
  if (clean.contains('NACL')) {
    return const [
      _AtomPoint('Na', -0.85, 0, 0, 0.85),
      _AtomPoint('Cl', 0.85, 0, 0, 1.0),
    ];
  }
  if (clean.contains('HCL')) {
    return const [
      _AtomPoint('H', -0.80, 0, 0, 0.60),
      _AtomPoint('Cl', 0.80, 0, 0, 1.0),
    ];
  }

  // Fallback Poliatomik Simetris
  final centralSymbol = clean.isNotEmpty ? clean.substring(0, 1) : 'C';
  return [
    _AtomPoint(centralSymbol, 0, 0, 0, 0.88),
    const _AtomPoint('X', -0.85, 0.35, 0, 0.55),
    const _AtomPoint('Y', 0.85, 0.35, 0, 0.55),
  ];
}

/// Menghubungkan ikatan kovalen antar atom secara otomatis
List<_BondPoint> _generateBonds(String formula, String? shape, int atomCount) {
  final clean = formula.replaceAll('₂', '2').replaceAll('₃', '3').replaceAll('₄', '4').replaceAll('₆', '6').toUpperCase();

  if (clean.contains('CO2')) {
    return const [_BondPoint(0, 1), _BondPoint(1, 2)];
  }

  if (atomCount <= 1) return const [];

  if (atomCount == 2) {
    return const [_BondPoint(0, 1)];
  }

  // Pusat berada di index 0 atau 1
  final bonds = <_BondPoint>[];
  final centralIndex = clean.contains('CO2') ? 1 : 0;

  for (var i = 0; i < atomCount; i++) {
    if (i != centralIndex) {
      bonds.add(_BondPoint(centralIndex, i));
    }
  }

  return bonds;
}
