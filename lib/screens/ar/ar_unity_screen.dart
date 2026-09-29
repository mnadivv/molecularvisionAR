import 'package:flutter/material.dart';
import 'package:flutter_embed_unity/flutter_embed_unity.dart';

import '../../core/models/chemistry_topic.dart';
import '../../utils/app_colors.dart';
import 'practicum_screen.dart';

/// Screen utama untuk menampilkan AR 3D Molekul Kimia via Embedded Unity 6
class ARUnityScreen extends StatefulWidget {
  final ChemistryTopic topic;

  const ARUnityScreen({
    super.key,
    required this.topic,
  });

  @override
  State<ARUnityScreen> createState() => _ARUnityScreenState();
}

class _ARUnityScreenState extends State<ARUnityScreen> {
  bool _isUnityLoaded = false;
  String _lastReceivedMessage = "";
  String _unityStatusMessage = "Menghubungkan ke Unity AR Engine...";

  String get _topicId =>
      widget.topic.title.toLowerCase().trim().replaceAll(' ', '_');

  @override
  void initState() {
    super.initState();
    // Beri jeda singkat agar view Unity terinisialisasi sebelum mengirim pesan
    Future.delayed(const Duration(milliseconds: 1500), () {
      if (mounted) {
        _sendTopicToUnity();
        setState(() {
          _isUnityLoaded = true;
          _unityStatusMessage = "Unity AR Aktif: ${widget.topic.title}";
        });
      }
    });
  }

  void _sendTopicToUnity() {
    try {
      // Kirim topic_id ke GameObject 'ARManager' dengan memanggil method 'ApplyVuforiaTopic'
      sendToUnity('ARManager', 'ApplyVuforiaTopic', _topicId);
    } catch (e) {
      debugPrint("Gagal mengirim topik ke Unity: $e");
    }
  }

  void _onMessageFromUnity(String message) {
    debugPrint("Pesan dari Unity: $message");
    if (!mounted) return;

    setState(() {
      _lastReceivedMessage = message;
      if (message.contains("SceneReady") || message.contains("TargetFound")) {
        _unityStatusMessage = "Marker Terdeteksi: ${widget.topic.title}";
      }
    });
  }

  void _showTopicInfoModal() {
    showModalBottomSheet(
      context: context,
      backgroundColor: AppColors.background,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      builder: (_) {
        return Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Center(
                child: Container(
                  width: 40,
                  height: 4,
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
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Icon(
                      widget.topic.icon,
                      color: widget.topic.color,
                      size: 26,
                    ),
                  ),
                  const SizedBox(width: 14),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          widget.topic.title,
                          style: const TextStyle(
                            color: Colors.white,
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        Text(
                          widget.topic.subtitle,
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
              Container(
                padding: const EdgeInsets.all(14),
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.05),
                  borderRadius: BorderRadius.circular(14),
                  border: Border.all(color: Colors.white12),
                ),
                child: Row(
                  children: [
                    const Icon(
                      Icons.touch_app_rounded,
                      color: Colors.white70,
                      size: 22,
                    ),
                    const SizedBox(width: 12),
                    const Expanded(
                      child: Text(
                        "Arahkan kamera ke Kartu Marker Vuforia untuk memunculkan model molekul 3D.",
                        style: TextStyle(
                          color: Colors.white,
                          fontSize: 13,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 16),
            ],
          ),
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.black,
      body: Stack(
        children: [
          /// Unity AR View yang menyatu di dalam Flutter (Single APK)
          Positioned.fill(
            child: EmbedUnity(
              onMessageFromUnity: _onMessageFromUnity,
            ),
          ),

          /// Top Bar HUD
          Positioned(
            top: 0,
            left: 0,
            right: 0,
            child: SafeArea(
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                child: Row(
                  children: [
                    // Tombol Kembali ke Flutter
                    Container(
                      decoration: BoxDecoration(
                        color: Colors.black.withValues(alpha: 0.5),
                        shape: BoxShape.circle,
                        border: Border.all(
                          color: Colors.white.withValues(alpha: 0.2),
                        ),
                      ),
                      child: IconButton(
                        icon: const Icon(Icons.arrow_back, color: Colors.white),
                        onPressed: () => Navigator.pop(context),
                      ),
                    ),

                    const SizedBox(width: 12),

                    // Topic Badge & Status
                    Expanded(
                      child: Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 14,
                          vertical: 10,
                        ),
                        decoration: BoxDecoration(
                          color: Colors.black.withValues(alpha: 0.55),
                          borderRadius: BorderRadius.circular(20),
                          border: Border.all(
                            color: widget.topic.color.withValues(alpha: 0.5),
                          ),
                        ),
                        child: Row(
                          children: [
                            Icon(
                              widget.topic.icon,
                              color: widget.topic.color,
                              size: 20,
                            ),
                            const SizedBox(width: 8),
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                mainAxisSize: MainAxisSize.min,
                                children: [
                                  Text(
                                    widget.topic.title,
                                    style: const TextStyle(
                                      color: Colors.white,
                                      fontWeight: FontWeight.bold,
                                      fontSize: 14,
                                    ),
                                    maxLines: 1,
                                    overflow: TextOverflow.ellipsis,
                                  ),
                                  Text(
                                    _unityStatusMessage,
                                    style: TextStyle(
                                      color: _isUnityLoaded
                                          ? Colors.greenAccent
                                          : Colors.amberAccent,
                                      fontSize: 11,
                                    ),
                                    maxLines: 1,
                                    overflow: TextOverflow.ellipsis,
                                  ),
                                ],
                              ),
                            ),
                            Container(
                              width: 8,
                              height: 8,
                              decoration: BoxDecoration(
                                shape: BoxShape.circle,
                                color: _isUnityLoaded
                                    ? Colors.greenAccent
                                    : Colors.amberAccent,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),

                    const SizedBox(width: 8),

                    // Tombol Kirim Ulang Topik / Refresh
                    Container(
                      decoration: BoxDecoration(
                        color: Colors.black.withValues(alpha: 0.5),
                        shape: BoxShape.circle,
                        border: Border.all(
                          color: Colors.white.withValues(alpha: 0.2),
                        ),
                      ),
                      child: IconButton(
                        icon: const Icon(Icons.sync_rounded, color: Colors.white),
                        tooltip: "Kirim Ulang Topik ke Unity",
                        onPressed: () {
                          _sendTopicToUnity();
                          ScaffoldMessenger.of(context).showSnackBar(
                            SnackBar(
                              content: Text("Topik '${widget.topic.title}' dimuat ke Unity"),
                              duration: const Duration(seconds: 1),
                              behavior: SnackBarBehavior.floating,
                            ),
                          );
                        },
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),

          /// Bottom Floating Toolbar HUD
          Positioned(
            bottom: 24,
            left: 20,
            right: 20,
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                // Tombol Buka Info
                _buildCircleActionButton(
                  icon: Icons.info_outline_rounded,
                  tooltip: "Info Materi",
                  onTap: _showTopicInfoModal,
                ),

                // Tombol Praktikum 2D
                Container(
                  decoration: BoxDecoration(
                    color: Colors.black.withValues(alpha: 0.65),
                    borderRadius: BorderRadius.circular(24),
                    border: Border.all(
                      color: widget.topic.color.withValues(alpha: 0.4),
                    ),
                  ),
                  child: TextButton.icon(
                    icon: Icon(
                      Icons.science_outlined,
                      color: widget.topic.color,
                      size: 20,
                    ),
                    label: const Text(
                      "Simulasi 2D",
                      style: TextStyle(
                        color: Colors.white,
                        fontWeight: FontWeight.w600,
                        fontSize: 13,
                      ),
                    ),
                    onPressed: () {
                      Navigator.push(
                        context,
                        MaterialPageRoute(
                          builder: (_) => const PracticumScreen(),
                        ),
                      );
                    },
                  ),
                ),

                // Tombol Reload Topik
                _buildCircleActionButton(
                  icon: Icons.refresh_rounded,
                  tooltip: "Reload Model 3D",
                  onTap: _sendTopicToUnity,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildCircleActionButton({
    required IconData icon,
    required String tooltip,
    required VoidCallback onTap,
  }) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.black.withValues(alpha: 0.6),
        shape: BoxShape.circle,
        border: Border.all(
          color: Colors.white.withValues(alpha: 0.25),
        ),
      ),
      child: IconButton(
        icon: Icon(icon, color: Colors.white, size: 22),
        tooltip: tooltip,
        onPressed: onTap,
      ),
    );
  }
}
