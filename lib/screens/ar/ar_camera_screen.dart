import 'package:flutter/material.dart';

import '../../core/models/chemistry_topic.dart';
import '../../utils/app_colors.dart';
import '../ar/practicum_screen.dart';

class ARCameraScreen extends StatelessWidget {
  final ChemistryTopic topic;

  const ARCameraScreen({
    super.key,
    required this.topic,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,

      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        iconTheme: const IconThemeData(
          color: Colors.white,
        ),
        title: Text(
          topic.title,
          style: const TextStyle(
            color: Colors.white,
          ),
        ),
      ),

      body: Padding(
        padding: const EdgeInsets.all(20),

        child: Column(
          children: [

            /// Preview AR
            Container(
              height: 260,
              width: double.infinity,

              decoration: BoxDecoration(
                color: Colors.white.withValues(alpha: .06),
                borderRadius: BorderRadius.circular(24),
                border: Border.all(
                  color: Colors.white.withValues(alpha: .12),
                ),
              ),

              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [

                  Icon(
                    Icons.view_in_ar_rounded,
                    size: 90,
                    color: topic.color,
                  ),

                  const SizedBox(height: 20),

                  Text(
                    topic.title,
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 24,
                      fontWeight: FontWeight.bold,
                    ),
                  ),

                  const SizedBox(height: 12),

                  Text(
                    "Praktikum AR siap digunakan",
                    style: TextStyle(
                      color: Colors.white.withValues(alpha: .7),
                    ),
                  ),
                ],
              ),
            ),

            const SizedBox(height: 30),

            /// Petunjuk
            Container(
              padding: const EdgeInsets.all(18),

              decoration: BoxDecoration(
                color: Colors.white.withValues(alpha: .06),
                borderRadius: BorderRadius.circular(20),
              ),

              child: const Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [

                  Text(
                    "Petunjuk Praktikum",
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                    ),
                  ),

                  SizedBox(height: 15),

                  Text(
                    "1. Tekan tombol Mulai Kamera AR.\n"
                    "2. Izinkan akses kamera.\n"
                    "3. Kamera akan aktif sebagai latar belakang.\n"
                    "4. Pilih alat atau bahan dari menu bawah.\n"
                    "5. Drag & Drop alat ke area praktikum.\n"
                    "6. Ikuti instruksi Assistant hingga praktikum selesai.\n"
                    "7. Tekan Next untuk melanjutkan setiap langkah.",
                    style: TextStyle(
                      color: Colors.white70,
                      height: 1.6,
                    ),
                  ),
                ],
              ),
            ),

            const Spacer(),

            SizedBox(
              width: double.infinity,

              child: ElevatedButton.icon(

                icon: const Icon(Icons.camera_alt),

                label: const Text(
                  "Mulai Kamera AR",
                ),

                style: ElevatedButton.styleFrom(
                  minimumSize: const Size(double.infinity, 58),

                  backgroundColor: topic.color,

                  foregroundColor: Colors.white,

                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(18),
                  ),
                ),

                onPressed: () {

                  Navigator.push(

                    context,

                    MaterialPageRoute(

                      builder: (_) => const PracticumScreen(),

                    ),

                  );

                },

              ),
            ),

            const SizedBox(height: 20),
          ],
        ),
      ),
    );
  }
}