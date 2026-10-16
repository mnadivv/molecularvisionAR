import 'package:flutter/services.dart';

class UnityLauncherService {
  static const MethodChannel _channel =
      MethodChannel('com.mvar.molecularvisionAR/unity_launcher');

  /// Nama package default untuk APK Unity AR
  /// Sesuaikan dengan Player Settings > Identification > Package Name di Unity
  static String defaultPackageName = 'com.mvar.unityar';

  /// Daftar kemungkinan package name Unity jika menggunakan default Unity
  static const List<String> knownPackageNames = [
    'com.mvar.unityar',
    'com.mvar.molecularvisionar',
    'com.DefaultCompany.MolecularVisionAR',
  ];

  /// Cek apakah APK Unity terinstal di HP Android
  static Future<bool> isUnityInstalled({String? packageName}) async {
    try {
      final targetPackage = packageName ?? defaultPackageName;
      final bool isInstalled = await _channel.invokeMethod('isAppInstalled', {
        'packageName': targetPackage,
      });

      if (isInstalled) return true;

      // Jika package default tidak ada, periksa kemungkinan nama package lain
      if (packageName == null) {
        for (final pkg in knownPackageNames) {
          final bool found = await _channel.invokeMethod('isAppInstalled', {
            'packageName': pkg,
          });
          if (found) {
            defaultPackageName = pkg;
            return true;
          }
        }
      }

      return false;
    } on PlatformException {
      return false;
    }
  }

  /// Menjalankan APK Unity dengan mengirim data topik via Android Intent extras
  static Future<bool> launchUnityApp({
    required String topicId,
    required String topicTitle,
    String? packageName,
  }) async {
    try {
      final targetPackage = packageName ?? defaultPackageName;
      final bool success = await _channel.invokeMethod('launchUnityApp', {
        'packageName': targetPackage,
        'topicId': topicId,
        'topicTitle': topicTitle,
      });
      return success;
    } on PlatformException {
      return false;
    }
  }

  /// Menjalankan Unity via Deep Link URI (misal: mvar://ar?topic=ikatan_kimia)
  static Future<bool> launchDeepLink({required String topicId}) async {
    try {
      final uri = "mvar://ar?topic=$topicId";
      final bool success = await _channel.invokeMethod('launchDeepLink', {
        'uri': uri,
      });
      return success;
    } on PlatformException {
      return false;
    }
  }
}
