import 'package:flutter_test/flutter_test.dart';
import 'package:molecular_vision_ar/features/onboarding/models/onboarding_item.dart';
import 'package:molecular_vision_ar/features/onboarding/services/onboarding_service.dart';
import 'package:shared_preferences/shared_preferences.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  group('OnboardingService & OnboardingItem Tests', () {
    setUp(() {
      SharedPreferences.setMockInitialValues({});
    });

    test('isOnboardingCompleted returns false initially', () async {
      final isCompleted = await OnboardingService.isOnboardingCompleted();
      expect(isCompleted, isFalse);
    });

    test('setOnboardingCompleted persists true flag', () async {
      await OnboardingService.setOnboardingCompleted();
      final isCompleted = await OnboardingService.isOnboardingCompleted();
      expect(isCompleted, isTrue);
    });

    test('resetOnboarding clears completed flag', () async {
      await OnboardingService.setOnboardingCompleted();
      expect(await OnboardingService.isOnboardingCompleted(), isTrue);

      await OnboardingService.resetOnboarding();
      expect(await OnboardingService.isOnboardingCompleted(), isFalse);
    });

    test('defaultItems contains 4 complete overview slides', () {
      final items = OnboardingItem.defaultItems;
      expect(items.length, 4);

      for (final item in items) {
        expect(item.title.isNotEmpty, isTrue);
        expect(item.subtitle.isNotEmpty, isTrue);
        expect(item.badge.isNotEmpty, isTrue);
        expect(item.tags.isNotEmpty, isTrue);
      }
    });
  });
}
