import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:molecular_vision_ar/features/ar/presentation/widgets/ar_module_banner_card.dart';

void main() {
  group('ARModuleBannerCard Tests', () {
    testWidgets('renders all texts and icons correctly', (tester) async {
      await tester.pumpWidget(
        const MaterialApp(
          home: Scaffold(
            body: ARModuleBannerCard(),
          ),
        ),
      );

      expect(find.text('MODUL PRAKTIKUM'), findsOneWidget);
      expect(find.text('Modul Praktikum AR'), findsOneWidget);
      expect(
        find.textContaining('panduan praktikum keseluruhan'),
        findsOneWidget,
      );
      expect(find.text('Buka Modul di Google Drive'), findsOneWidget);
      expect(find.byIcon(Icons.menu_book_rounded), findsOneWidget);
      expect(find.byIcon(Icons.folder_shared_rounded), findsOneWidget);
      expect(find.byIcon(Icons.open_in_new_rounded), findsOneWidget);
    });
  });
}
