import 'package:flutter/material.dart';

import '../../../../core/constants/app_links.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../core/utils/url_launcher_helper.dart';

/// Featured banner card on the AR Menu screen to open the complete practicum module in Google Drive.
class ARModuleBannerCard extends StatelessWidget {
  final String? driveUrl;
  final VoidCallback? onOpened;

  const ARModuleBannerCard({
    super.key,
    this.driveUrl,
    this.onOpened,
  });

  String get _effectiveDriveUrl => driveUrl ?? AppLinks.practicumModuleDriveUrl;

  Future<void> _handleOpenDrive(BuildContext context) async {
    final success = await UrlLauncherHelper.openUrl(
      context,
      _effectiveDriveUrl,
      errorMessage:
          'Gagal membuka modul praktikum di Google Drive. Silakan periksa koneksi atau browser Anda.',
    );

    if (success && onOpened != null) {
      onOpened!();
    }
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(20),
        gradient: const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            Color(0xFF1F3D7A),
            Color(0xFF112652),
          ],
        ),
        border: Border.all(
          color: AppColors.accent.withValues(alpha: 0.38),
          width: 1.4,
        ),
        boxShadow: [
          BoxShadow(
            color: AppColors.accent.withValues(alpha: 0.12),
            blurRadius: 16,
            offset: const Offset(0, 4),
          ),
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.35),
            blurRadius: 14,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: () => _handleOpenDrive(context),
          borderRadius: BorderRadius.circular(20),
          splashColor: AppColors.accent.withValues(alpha: 0.2),
          highlightColor: Colors.white.withValues(alpha: 0.05),
          child: Padding(
            padding: const EdgeInsets.all(18),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    /// Icon Container with neon glow
                    Container(
                      width: 50,
                      height: 50,
                      decoration: BoxDecoration(
                        borderRadius: BorderRadius.circular(16),
                        color: AppColors.accent.withValues(alpha: 0.18),
                        border: Border.all(
                          color: AppColors.accent.withValues(alpha: 0.5),
                          width: 1.2,
                        ),
                      ),
                      child: const Center(
                        child: Icon(
                          Icons.menu_book_rounded,
                          size: 26,
                          color: AppColors.accent,
                        ),
                      ),
                    ),
                    const SizedBox(width: 14),

                    /// Header & Tag
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: 8,
                              vertical: 3,
                            ),
                            decoration: BoxDecoration(
                              borderRadius: BorderRadius.circular(8),
                              color: AppColors.accent.withValues(alpha: 0.15),
                              border: Border.all(
                                color: AppColors.accent.withValues(alpha: 0.3),
                                width: 0.8,
                              ),
                            ),
                            child: const Text(
                              "MODUL PRAKTIKUM",
                              style: TextStyle(
                                color: AppColors.accent,
                                fontSize: 10,
                                fontWeight: FontWeight.bold,
                                letterSpacing: 0.8,
                              ),
                            ),
                          ),
                          const SizedBox(height: 6),
                          const Text(
                            "Modul Praktikum AR",
                            style: TextStyle(
                              color: Colors.white,
                              fontSize: 16,
                              fontWeight: FontWeight.bold,
                              letterSpacing: 0.2,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 12),
                Text(
                  "Buka dan unduh buku panduan praktikum keseluruhan melalui Google Drive.",
                  style: TextStyle(
                    color: Colors.white.withValues(alpha: 0.82),
                    fontSize: 12.5,
                    height: 1.4,
                  ),
                ),
                const SizedBox(height: 14),

                /// Button Row
                Row(
                  children: [
                    Expanded(
                      child: Container(
                        padding: const EdgeInsets.symmetric(vertical: 10),
                        decoration: BoxDecoration(
                          borderRadius: BorderRadius.circular(12),
                          color: AppColors.accent.withValues(alpha: 0.16),
                          border: Border.all(
                            color: AppColors.accent.withValues(alpha: 0.45),
                            width: 1.0,
                          ),
                        ),
                        child: const Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Icon(
                              Icons.folder_shared_rounded,
                              size: 17,
                              color: AppColors.accent,
                            ),
                            SizedBox(width: 8),
                            Text(
                              "Buka Modul di Google Drive",
                              style: TextStyle(
                                color: AppColors.accent,
                                fontSize: 13,
                                fontWeight: FontWeight.w600,
                                letterSpacing: 0.3,
                              ),
                            ),
                            SizedBox(width: 6),
                            Icon(
                              Icons.open_in_new_rounded,
                              size: 15,
                              color: AppColors.accent,
                            ),
                          ],
                        ),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
