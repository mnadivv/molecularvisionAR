import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';
import 'package:youtube_player_iframe/youtube_player_iframe.dart';

import '../../../../core/theme/app_colors.dart';

/// Ekstraksi ID video YouTube (11 karakter) dari segala format URL YouTube
String? extractYoutubeVideoId(String url) {
  if (url.trim().isEmpty) return null;
  final cleanUrl = url.trim();

  // Pola regex mencakup format watch?v=, embed/, shorts/, youtu.be, dan youtube-nocookie
  final regExp = RegExp(
    r'(?:https?:)?\/\/(?:www\.|m\.)?(?:youtube\.com\/(?:watch\?v=|embed\/|v\/|shorts\/)|youtu\.be\/|youtube-nocookie\.com\/embed\/)([a-zA-Z0-9_-]{11})',
    caseSensitive: false,
  );
  final match = regExp.firstMatch(cleanUrl);
  if (match != null && match.groupCount >= 1) {
    return match.group(1);
  }

  // Fallback melalui query parameter Uri
  try {
    final uri = Uri.parse(cleanUrl.startsWith('//') ? 'https:$cleanUrl' : cleanUrl);
    if (uri.queryParameters.containsKey('v')) {
      final v = uri.queryParameters['v'];
      if (v != null && v.length == 11) return v;
    }
    if (uri.pathSegments.isNotEmpty) {
      final last = uri.pathSegments.last;
      if (last.length == 11) return last;
    }
  } catch (_) {}

  return null;
}

/// Widget pemutar video YouTube interaktif untuk konten materi pembelajaran
class MateriYoutubePlayer extends StatefulWidget {
  final String videoId;
  final String originalUrl;

  const MateriYoutubePlayer({
    super.key,
    required this.videoId,
    required this.originalUrl,
  });

  @override
  State<MateriYoutubePlayer> createState() => _MateriYoutubePlayerState();
}

class _MateriYoutubePlayerState extends State<MateriYoutubePlayer> {
  late final YoutubePlayerController _controller;

  @override
  void initState() {
    super.initState();
    _controller = YoutubePlayerController.fromVideoId(
      videoId: widget.videoId,
      params: const YoutubePlayerParams(
        showControls: true,
        showFullscreenButton: true,
        mute: false,
        strictRelatedVideos: true,
      ),
    );
  }

  @override
  void dispose() {
    _controller.close();
    super.dispose();
  }

  Future<void> _openInYouTubeApp() async {
    final uri = Uri.parse('https://www.youtube.com/watch?v=${widget.videoId}');
    try {
      await launchUrl(uri, mode: LaunchMode.externalApplication);
    } catch (_) {}
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.symmetric(vertical: 16),
      decoration: BoxDecoration(
        color: const Color(0xFF1E2766),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: Colors.white.withValues(alpha: 0.18),
          width: 1.2,
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.35),
            blurRadius: 14,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      clipBehavior: Clip.antiAlias,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          // 16:9 Video Player View
          AspectRatio(
            aspectRatio: 16 / 9,
            child: YoutubePlayer(
              controller: _controller,
              aspectRatio: 16 / 9,
            ),
          ),

          // Bottom Bar Info & Tombol Buka di YouTube
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
            color: Colors.black.withValues(alpha: 0.30),
            child: Row(
              children: [
                const Icon(
                  Icons.play_circle_fill_rounded,
                  color: Colors.redAccent,
                  size: 20,
                ),
                const SizedBox(width: 8),
                const Expanded(
                  child: Text(
                    'Video Pembelajaran YouTube',
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 13,
                      fontWeight: FontWeight.w600,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                ),
                InkWell(
                  onTap: _openInYouTubeApp,
                  borderRadius: BorderRadius.circular(8),
                  child: Padding(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 8,
                      vertical: 4,
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Text(
                          'Buka YouTube',
                          style: TextStyle(
                            color: AppColors.accent,
                            fontSize: 11.5,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(width: 4),
                        Icon(
                          Icons.open_in_new_rounded,
                          color: AppColors.accent,
                          size: 14,
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
