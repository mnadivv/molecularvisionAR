import 'package:flutter/material.dart';
import 'package:flutter_embed_unity/flutter_embed_unity.dart';

import '../../core/models/chemistry_topic.dart';

/// Screen utama AR: Menampilkan Unity AR secara fullscreen tanpa overlay Flutter
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
  String get _topicId =>
      widget.topic.unityScene ??
      widget.topic.title.toLowerCase().trim().replaceAll(' ', '_');

  @override
  void initState() {
    super.initState();
    // Beri jeda singkat agar view Unity terinisialisasi sebelum mengirim pesan
    Future.delayed(const Duration(milliseconds: 1000), () {
      if (mounted) {
        _sendTopicToUnity();
      }
    });
  }

  void _sendTopicToUnity() {
    try {
      // Kirim topic_id / scene ke GameObject 'ARManager' / 'UnityAndroidReceiver'
      sendToUnity('ARManager', 'ApplyVuforiaTopic', _topicId);
      sendToUnity('UnityAndroidReceiver', 'ApplyVuforiaTopic', _topicId);
    } catch (e) {
      debugPrint("Gagal mengirim topik ke Unity: $e");
    }
  }

  void _onMessageFromUnity(String message) {
    debugPrint("Pesan dari Unity: $message");
    if (!mounted) return;

    final trimmed = message.trim().toLowerCase();
    // Jika Unity mengirim sinyal untuk kembali ke Flutter
    if (trimmed == "onback" ||
        trimmed == "back" ||
        trimmed == "exit" ||
        trimmed == "finish" ||
        trimmed.contains("back")) {
      Navigator.of(context).pop();
    }
  }

  @override
  Widget build(BuildContext context) {
    return PopScope(
      canPop: true,
      onPopInvokedWithResult: (didPop, result) {
        if (didPop) {
          // Lifecycle cleanup jika diperlukan saat keluar dari AR
          try {
            sendToUnity('ARManager', 'OnPauseAR', '');
          } catch (_) {}
        }
      },
      child: Scaffold(
        backgroundColor: Colors.black,
        body: EmbedUnity(
          onMessageFromUnity: _onMessageFromUnity,
        ),
      ),
    );
  }
}

