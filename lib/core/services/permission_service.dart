import 'package:flutter/foundation.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// Service terpusat untuk manajemen perizinan (khususnya Kamera untuk AR/Virtual Lab)
class PermissionService {
  static const String _keyInitialCameraPrompt =
      'initial_camera_permission_prompted';

  /// Memeriksa apakah izin kamera saat ini sudah diberikan (granted)
  static Future<bool> hasCameraPermission() async {
    try {
      final status = await Permission.camera.status;
      return status.isGranted;
    } catch (e) {
      debugPrint('Error checking camera permission: $e');
      return false;
    }
  }

  /// Mendapatkan status detail izin kamera (granted, denied, permanentlyDenied, restricted)
  static Future<PermissionStatus> getCameraStatus() async {
    try {
      return await Permission.camera.status;
    } catch (e) {
      debugPrint('Error getting camera status: $e');
      return PermissionStatus.denied;
    }
  }

  /// Meminta izin kamera secara langsung ke sistem operasi
  static Future<PermissionStatus> requestCameraPermission() async {
    try {
      final status = await Permission.camera.request();
      debugPrint('Requested camera permission result: $status');
      return status;
    } catch (e) {
      debugPrint('Error requesting camera permission: $e');
      return PermissionStatus.denied;
    }
  }

  /// Meminta izin kamera saat startup awal aplikasi (hanya dipanggil satu kali saat pertama buka)
  static Future<void> requestInitialCameraPermission() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final alreadyPrompted = prefs.getBool(_keyInitialCameraPrompt) ?? false;

      // Jika belum pernah diprompt dan belum di-grant, minta izin sistem
      if (!alreadyPrompted) {
        final isGranted = await Permission.camera.isGranted;
        if (!isGranted) {
          final result = await Permission.camera.request();
          debugPrint('Initial startup camera permission requested: $result');
        }
        await prefs.setBool(_keyInitialCameraPrompt, true);
      }
    } catch (e) {
      debugPrint('Error in requestInitialCameraPermission: $e');
    }
  }

  /// Membuka halaman pengaturan aplikasi di Android/iOS jika izin ditolak permanen
  static Future<bool> openSettings() async {
    try {
      return await openAppSettings();
    } catch (e) {
      debugPrint('Error opening app settings: $e');
      return false;
    }
  }
}
