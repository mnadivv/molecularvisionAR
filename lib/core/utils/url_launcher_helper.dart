import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:url_launcher/url_launcher.dart';

/// Helper utility for safely launching external URLs with native fallback and user feedback.
class UrlLauncherHelper {
  UrlLauncherHelper._();

  static const MethodChannel _nativeChannel =
      MethodChannel('com.mvar.molecularvisionAR/unity_launcher');

  static DateTime? _lastLaunchTime;

  /// Returns true if at least 600ms has elapsed since the last tap.
  static bool get _canLaunchNow {
    final now = DateTime.now();
    if (_lastLaunchTime == null ||
        now.difference(_lastLaunchTime!) > const Duration(milliseconds: 600)) {
      _lastLaunchTime = now;
      return true;
    }
    return false;
  }

  /// Launches an external URL.
  ///
  /// Strategy:
  /// 1. Tries `url_launcher` package with `externalApplication`.
  /// 2. If channel error or failure occurs, falls back to native Android `launchDeepLink` intent channel.
  static Future<bool> launchExternalUrl(String urlString) async {
    final uri = Uri.tryParse(urlString.trim());
    if (uri == null || !uri.hasScheme) {
      return false;
    }

    // 1. Try standard url_launcher
    try {
      await launchUrl(
        uri,
        mode: LaunchMode.externalApplication,
      );
      return true;
    } catch (e) {
      debugPrint('[UrlLauncherHelper] url_launcher failed, trying native channel: $e');
    }

    // 2. Fallback to native MainActivity intent launcher
    try {
      final bool? nativeResult = await _nativeChannel.invokeMethod<bool>(
        'launchDeepLink',
        {'uri': urlString.trim()},
      );
      if (nativeResult == true) {
        return true;
      }
    } catch (e) {
      debugPrint('[UrlLauncherHelper] Native launchDeepLink error: $e');
    }

    // 3. Fallback to platformDefault / inAppBrowserView
    try {
      await launchUrl(
        uri,
        mode: LaunchMode.platformDefault,
      );
      return true;
    } catch (_) {}

    return false;
  }

  /// Opens an external URL and displays a SnackBar notification if launching fails.
  static Future<bool> openUrl(
    BuildContext context,
    String urlString, {
    String? errorMessage,
  }) async {
    if (!_canLaunchNow) return false;

    final uri = Uri.tryParse(urlString.trim());
    if (uri == null || !uri.hasScheme) {
      if (context.mounted) {
        _showErrorSnackBar(
          context,
          'Format tautan tidak valid: $urlString',
        );
      }
      return false;
    }

    final success = await launchExternalUrl(urlString);

    if (!success && context.mounted) {
      _showErrorSnackBar(
        context,
        errorMessage ??
            'Tidak dapat membuka tautan. Pastikan browser atau aplikasi Google Drive terpasang.',
      );
      return false;
    }

    return success;
  }

  static void _showErrorSnackBar(BuildContext context, String message) {
    if (!context.mounted) return;
    ScaffoldMessenger.of(context).hideCurrentSnackBar();
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Row(
          children: [
            const Icon(Icons.error_outline, color: Colors.white, size: 20),
            const SizedBox(width: 10),
            Expanded(
              child: Text(
                message,
                style: const TextStyle(color: Colors.white, fontSize: 13),
              ),
            ),
          ],
        ),
        backgroundColor: const Color(0xFFEF4444),
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
        ),
        duration: const Duration(seconds: 4),
      ),
    );
  }
}
