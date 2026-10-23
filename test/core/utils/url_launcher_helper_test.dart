import 'package:flutter_test/flutter_test.dart';
import 'package:molecular_vision_ar/core/utils/url_launcher_helper.dart';

void main() {
  group('UrlLauncherHelper Tests', () {
    test('launchExternalUrl returns false for empty or invalid scheme URLs', () async {
      expect(await UrlLauncherHelper.launchExternalUrl(''), isFalse);
      expect(await UrlLauncherHelper.launchExternalUrl('   '), isFalse);
      expect(await UrlLauncherHelper.launchExternalUrl('not-a-valid-url'), isFalse);
    });
  });
}
