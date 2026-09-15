import 'package:camera/camera.dart';

class CameraService {
  static CameraController? controller;

  static Future<void> initialize() async {
    final cameras = await availableCameras();

    final backCamera = cameras.firstWhere(
      (camera) => camera.lensDirection == CameraLensDirection.back,
    );

    controller = CameraController(
      backCamera,
      ResolutionPreset.high,
      enableAudio: false,
    );

    await controller!.initialize();
  }

  static void dispose() {
    controller?.dispose();
  }
}