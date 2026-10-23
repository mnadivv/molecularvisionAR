import 'package:flutter_test/flutter_test.dart';
import 'package:molecular_vision_ar/core/constants/app_links.dart';

void main() {
  group('AppLinks Tests', () {
    test('practicumModuleDriveUrl is a valid HTTPS URI', () {
      expect(AppLinks.practicumModuleDriveUrl.isNotEmpty, isTrue);
      expect(AppLinks.practicumModuleDriveUrl.startsWith('https://'), isTrue);

      final uri = Uri.tryParse(AppLinks.practicumModuleDriveUrl);
      expect(uri, isNotNull);
      expect(uri!.hasScheme, isTrue);
      expect(uri.host.contains('drive.google.com'), isTrue);
    });
  });
}
