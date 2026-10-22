import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:molecular_vision_ar/core/services/permission_service.dart';
import 'package:shared_preferences/shared_preferences.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  group('PermissionService Tests', () {
    setUp(() {
      SharedPreferences.setMockInitialValues({});
    });

    test('requestInitialCameraPermission sets preference flag', () async {
      // Mock platform channel for permission_handler to return granted
      const MethodChannel channel =
          MethodChannel('flutter.baseflow.com/permissions/methods');
      TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger
          .setMockMethodCallHandler(channel, (MethodCall methodCall) async {
        if (methodCall.method == 'checkPermissionStatus') {
          return 1; // PermissionStatus.granted
        }
        if (methodCall.method == 'requestPermissions') {
          return {1: 1};
        }
        return null;
      });

      await PermissionService.requestInitialCameraPermission();

      final prefs = await SharedPreferences.getInstance();
      expect(
        prefs.getBool('initial_camera_permission_prompted'),
        isTrue,
      );
    });

    test('hasCameraPermission returns true when permission is granted',
        () async {
      const MethodChannel channel =
          MethodChannel('flutter.baseflow.com/permissions/methods');
      TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger
          .setMockMethodCallHandler(channel, (MethodCall methodCall) async {
        if (methodCall.method == 'checkPermissionStatus') {
          return 1; // PermissionStatus.granted
        }
        return null;
      });

      final result = await PermissionService.hasCameraPermission();
      expect(result, isTrue);
    });

    test('hasCameraPermission returns false when permission is denied',
        () async {
      const MethodChannel channel =
          MethodChannel('flutter.baseflow.com/permissions/methods');
      TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger
          .setMockMethodCallHandler(channel, (MethodCall methodCall) async {
        if (methodCall.method == 'checkPermissionStatus') {
          return 0; // PermissionStatus.denied
        }
        return null;
      });

      final result = await PermissionService.hasCameraPermission();
      expect(result, isFalse);
    });
  });
}
