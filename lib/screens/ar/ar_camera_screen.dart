import 'package:flutter/material.dart';

import '../../core/models/chemistry_topic.dart';
import '../../services/unity_launcher_service.dart';
import '../../utils/app_colors.dart';
import 'ar_unity_screen.dart';
import 'practicum_screen.dart';

class ARCameraScreen extends StatefulWidget {
  final ChemistryTopic topic;

  const ARCameraScreen({
    super.key,
    required this.topic,
  });

  @override
  State<ARCameraScreen> createState() => _ARCameraScreenState();
}

class _ARCameraScreenState extends State<ARCameraScreen> {
  bool _isUnityInstalled = false;
  bool _isCheckingUnity = true;

  @override
  void initState() {
    super.initState();
    _checkUnityApp();
  }

  Future<void> _checkUnityApp() async {
    final installed = await UnityLauncherService.isUnityInstalled();
    if (mounted) {
      setState(() {
        _isUnityInstalled = installed;
        _isCheckingUnity = false;
      });
    }
  }

  String get _topicId =>
      widget.topic.title.toLowerCase().trim().replaceAll(' ', '_');

  void _handleStartAR() {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) => ARUnityScreen(topic: widget.topic),
      ),
    );
  }

  void _showUnityConnectionModal() {
    final packageController = TextEditingController(
      text: UnityLauncherService.defaultPackageName,
    );

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: AppColors.background,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
      ),
      builder: (ctx) {
        return Padding(
          padding: EdgeInsets.only(
            left: 24,
            right: 24,
            top: 24,
            bottom: MediaQuery.of(ctx).viewInsets.bottom + 24,
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Center(
                child: Container(
                  width: 44,
                  height: 5,
                  decoration: BoxDecoration(
                    color: Colors.white24,
                    borderRadius: BorderRadius.circular(10),
                  ),
                ),
              ),
              const SizedBox(height: 20),
              Row(
                children: [
                  Container(
                    padding: const EdgeInsets.all(10),
                    decoration: BoxDecoration(
                      color: widget.topic.color.withValues(alpha: 0.15),
                      borderRadius: BorderRadius.circular(14),
                    ),
                    child: Icon(
                      Icons.hub_rounded,
                      color: widget.topic.color,
                      size: 26,
                    ),
                  ),
                  const SizedBox(width: 14),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text(
                          "Koneksi Unity AR",
                          style: TextStyle(
                            color: Colors.white,
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        Text(
                          "Pilih metode tampilan AR untuk ${widget.topic.title}",
                          style: const TextStyle(
                            color: Colors.white70,
                            fontSize: 13,
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 20),

              /// Opsi 1: Unity Widget Embedded
              _buildModalOption(
                icon: Icons.view_in_ar_rounded,
                title: "Buka AR Terintegrasi (Embedded)",
                subtitle:
                    "Menampilkan Unity AR di dalam tampilan aplikasi (unityLibrary).",
                color: widget.topic.color,
                onTap: () {
                  Navigator.pop(ctx);
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (_) => ARUnityScreen(topic: widget.topic),
                    ),
                  );
                },
              ),

              const SizedBox(height: 12),

              /// Opsi 2: Buka Standalone APK Unity
              _buildModalOption(
                icon: Icons.android_rounded,
                title: "Buka Aplikasi APK Unity",
                subtitle:
                    "Jalankan APK Unity terpisah yang terpasang di HP dengan parameter materi.",
                color: Colors.cyanAccent,
                onTap: () async {
                  Navigator.pop(ctx);
                  final pkg = packageController.text.trim();
                  final success = await UnityLauncherService.launchUnityApp(
                    topicId: _topicId,
                    topicTitle: widget.topic.title,
                    packageName: pkg.isNotEmpty ? pkg : null,
                  );

                  if (!success && mounted) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(
                        content: Text(
                          "APK Unity ($pkg) belum terpasang di perangkat ini.",
                        ),
                        backgroundColor: Colors.redAccent,
                        behavior: SnackBarBehavior.floating,
                      ),
                    );
                  }
                },
              ),

              const SizedBox(height: 12),

              /// Opsi 3: Simulasi Praktikum 2D
              _buildModalOption(
                icon: Icons.science_outlined,
                title: "Mode Simulasi 2D",
                subtitle: "Gunakan kamera & simulasi 2D bawaan Flutter.",
                color: Colors.orangeAccent,
                onTap: () {
                  Navigator.pop(ctx);
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (_) => const PracticumScreen(),
                    ),
                  );
                },
              ),

              const SizedBox(height: 18),

              /// Detail Parameter untuk Unity Developer
              Container(
                padding: const EdgeInsets.all(14),
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.05),
                  borderRadius: BorderRadius.circular(14),
                  border: Border.all(
                    color: Colors.white.withValues(alpha: 0.1),
                  ),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      "Data Intent yang Dikirim ke Unity:",
                      style: TextStyle(
                        color: Colors.white70,
                        fontSize: 12,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 6),
                    Text(
                      "• topic_id: $_topicId\n• topic_title: ${widget.topic.title}\n• Target Package: ${UnityLauncherService.defaultPackageName}",
                      style: const TextStyle(
                        color: Colors.cyanAccent,
                        fontSize: 11,
                        fontFamily: "monospace",
                        height: 1.5,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildModalOption({
    required IconData icon,
    required String title,
    required String subtitle,
    required Color color,
    required VoidCallback onTap,
  }) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(16),
      child: Container(
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(
          color: Colors.white.withValues(alpha: 0.07),
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: Colors.white.withValues(alpha: 0.12),
          ),
        ),
        child: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(10),
              decoration: BoxDecoration(
                color: color.withValues(alpha: 0.15),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Icon(icon, color: color, size: 22),
            ),
            const SizedBox(width: 14),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    title,
                    style: const TextStyle(
                      color: Colors.white,
                      fontWeight: FontWeight.w600,
                      fontSize: 14,
                    ),
                  ),
                  const SizedBox(height: 3),
                  Text(
                    subtitle,
                    style: const TextStyle(
                      color: Colors.white60,
                      fontSize: 12,
                    ),
                  ),
                ],
              ),
            ),
            const Icon(
              Icons.chevron_right_rounded,
              color: Colors.white38,
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        iconTheme: const IconThemeData(
          color: Colors.white,
        ),
        title: Text(
          widget.topic.title,
          style: const TextStyle(
            color: Colors.white,
          ),
        ),
      ),
      body: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          children: [
            /// Preview AR
            Container(
              height: 240,
              width: double.infinity,
              decoration: BoxDecoration(
                color: Colors.white.withValues(alpha: .06),
                borderRadius: BorderRadius.circular(24),
                border: Border.all(
                  color: Colors.white.withValues(alpha: .12),
                ),
              ),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(
                    Icons.view_in_ar_rounded,
                    size: 80,
                    color: widget.topic.color,
                  ),
                  const SizedBox(height: 16),
                  Text(
                    widget.topic.title,
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 22,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    "Augmented Reality 3D Molekul",
                    style: TextStyle(
                      color: Colors.white.withValues(alpha: .7),
                      fontSize: 13,
                    ),
                  ),
                  const SizedBox(height: 12),

                  /// Status deteksi Unity
                  Container(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 12,
                      vertical: 6,
                    ),
                    decoration: BoxDecoration(
                      color: _isUnityInstalled
                          ? Colors.green.withValues(alpha: 0.15)
                          : Colors.amber.withValues(alpha: 0.15),
                      borderRadius: BorderRadius.circular(20),
                      border: Border.all(
                        color: _isUnityInstalled
                            ? Colors.greenAccent.withValues(alpha: 0.5)
                            : Colors.amberAccent.withValues(alpha: 0.5),
                      ),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(
                          _isUnityInstalled
                              ? Icons.check_circle_rounded
                              : Icons.sync_rounded,
                          size: 14,
                          color: _isUnityInstalled
                              ? Colors.greenAccent
                              : Colors.amberAccent,
                        ),
                        const SizedBox(width: 6),
                        Text(
                          _isCheckingUnity
                              ? "Memeriksa Unity..."
                              : (_isUnityInstalled
                                  ? "APK Unity Terpasang"
                                  : "Siap Terhubung ke Unity"),
                          style: TextStyle(
                            fontSize: 12,
                            color: _isUnityInstalled
                                ? Colors.greenAccent
                                : Colors.amberAccent,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),

            const SizedBox(height: 24),

            /// Petunjuk
            Container(
              padding: const EdgeInsets.all(18),
              decoration: BoxDecoration(
                color: Colors.white.withValues(alpha: .06),
                borderRadius: BorderRadius.circular(20),
              ),
              child: const Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    "Petunjuk Penggunaan AR",
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 17,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  SizedBox(height: 12),
                  Text(
                    "1. Tekan tombol Mulai Kamera AR di bawah.\n"
                    "2. Sistem akan membuka Unity AR sesuai topik materi.\n"
                    "3. Arahkan kamera ke permukaan datar atau marker.\n"
                    "4. Interaksi 3D molekul (rotasi, zoom, label) akan aktif.\n"
                    "5. Tekan tombol kembali di Unity untuk balik ke Flutter.",
                    style: TextStyle(
                      color: Colors.white70,
                      height: 1.6,
                      fontSize: 13,
                    ),
                  ),
                ],
              ),
            ),

            const Spacer(),

            /// Tombol Mulai AR
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                icon: const Icon(Icons.camera_alt),
                label: const Text(
                  "Mulai Kamera AR",
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                style: ElevatedButton.styleFrom(
                  minimumSize: const Size(double.infinity, 56),
                  backgroundColor: widget.topic.color,
                  foregroundColor: Colors.white,
                  elevation: 4,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(18),
                  ),
                ),
                onPressed: _handleStartAR,
              ),
            ),

            const SizedBox(height: 12),

            /// Tombol Opsi Alternatif
            SizedBox(
              width: double.infinity,
              child: OutlinedButton.icon(
                icon: const Icon(Icons.settings_outlined),
                label: const Text("Pilih Metode AR / Simulasi"),
                style: OutlinedButton.styleFrom(
                  minimumSize: const Size(double.infinity, 46),
                  foregroundColor: Colors.white70,
                  side: BorderSide(
                    color: Colors.white.withValues(alpha: .2),
                  ),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(16),
                  ),
                ),
                onPressed: _showUnityConnectionModal,
              ),
            ),

            const SizedBox(height: 14),
          ],
        ),
      ),
    );
  }
}