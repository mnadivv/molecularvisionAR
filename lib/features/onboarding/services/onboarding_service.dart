import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// Service terpusat untuk mengelola status penyelesaian Onboarding
class OnboardingService {
  static const String _keyOnboardingCompleted = 'onboarding_completed';

  /// Memeriksa apakah pengguna sudah menyelesaikan onboarding (hanya diakses 1x saat awal install)
  static Future<bool> isOnboardingCompleted() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      return prefs.getBool(_keyOnboardingCompleted) ?? false;
    } catch (e) {
      debugPrint('[OnboardingService] Gagal membaca status onboarding: $e');
      return false;
    }
  }

  /// Menandai bahwa onboarding telah diselesaikan sehingga tidak muncul lagi di masa depan
  static Future<void> setOnboardingCompleted() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.setBool(_keyOnboardingCompleted, true);
      debugPrint('[OnboardingService] Onboarding ditandai telah selesai');
    } catch (e) {
      debugPrint('[OnboardingService] Gagal menyimpan status onboarding: $e');
    }
  }

  /// Me-reset status onboarding (berguna untuk pengujian atau pengaturan reset aplikasi)
  static Future<void> resetOnboarding() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.remove(_keyOnboardingCompleted);
      debugPrint('[OnboardingService] Status onboarding di-reset');
    } catch (e) {
      debugPrint('[OnboardingService] Gagal reset status onboarding: $e');
    }
  }
}
