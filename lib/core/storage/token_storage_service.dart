import 'package:flutter/foundation.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// Service untuk menyimpan dan mengelola token autentikasi secara terenkripsi.
/// Menggunakan Android Keystore & iOS Keychain via `flutter_secure_storage`,
/// dengan auto-fallback ke `SharedPreferences` jika secure storage platform channel tidak tersedia.
class TokenStorageService {
  final FlutterSecureStorage _storage;
  static const String _keyToken = 'mvar_auth_token';

  TokenStorageService({FlutterSecureStorage? storage})
      : _storage = storage ??
            const FlutterSecureStorage(
              aOptions: AndroidOptions(
                resetOnError: true,
              ),
              iOptions: IOSOptions(
                accessibility: KeychainAccessibility.first_unlock,
              ),
            );

  /// Menyimpan personal access token Sanctum
  Future<void> saveToken(String token) async {
    try {
      await _storage.write(key: _keyToken, value: token);
    } catch (e) {
      debugPrint('[TokenStorageService] Secure storage write failed: $e, falling back to SharedPreferences');
      try {
        final prefs = await SharedPreferences.getInstance();
        await prefs.setString(_keyToken, token);
      } catch (e2) {
        debugPrint('[TokenStorageService] SharedPreferences write fallback failed: $e2');
      }
    }
  }

  /// Mengambil token yang tersimpan
  Future<String?> getToken() async {
    try {
      final token = await _storage.read(key: _keyToken);
      if (token != null && token.isNotEmpty) return token;
    } catch (e) {
      debugPrint('[TokenStorageService] Secure storage read failed: $e, falling back to SharedPreferences');
    }

    try {
      final prefs = await SharedPreferences.getInstance();
      return prefs.getString(_keyToken);
    } catch (e2) {
      debugPrint('[TokenStorageService] SharedPreferences read fallback failed: $e2');
      return null;
    }
  }

  /// Menghapus token saat user logout atau session expired
  Future<void> deleteToken() async {
    try {
      await _storage.delete(key: _keyToken);
    } catch (e) {
      debugPrint('[TokenStorageService] Secure storage delete failed: $e');
    }

    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.remove(_keyToken);
    } catch (e2) {
      debugPrint('[TokenStorageService] SharedPreferences remove failed: $e2');
    }
  }

  /// Mengecek apakah user memiliki token tersimpan
  Future<bool> hasToken() async {
    try {
      final token = await getToken();
      return token != null && token.isNotEmpty;
    } catch (_) {
      return false;
    }
  }
}
