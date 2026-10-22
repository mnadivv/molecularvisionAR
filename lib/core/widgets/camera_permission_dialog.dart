import 'package:flutter/material.dart';
import 'package:permission_handler/permission_handler.dart';

import '../services/permission_service.dart';
import '../theme/app_colors.dart';

/// Dialog kustom bertema Molecular Vision AR untuk meminta izin kamera
class CameraPermissionDialog extends StatefulWidget {
  final VoidCallback? onGranted;

  const CameraPermissionDialog({
    super.key,
    this.onGranted,
  });

  /// Menampilkan dialog izin kamera secara statis
  static Future<bool> show(
    BuildContext context, {
    VoidCallback? onGranted,
  }) async {
    final result = await showDialog<bool>(
      context: context,
      barrierDismissible: false,
      builder: (ctx) => CameraPermissionDialog(
        onGranted: onGranted,
      ),
    );
    return result ?? false;
  }

  @override
  State<CameraPermissionDialog> createState() => _CameraPermissionDialogState();
}

class _CameraPermissionDialogState extends State<CameraPermissionDialog> {
  bool _isLoading = false;
  bool _isPermanentlyDenied = false;

  @override
  void initState() {
    super.initState();
    _checkInitialStatus();
  }

  Future<void> _checkInitialStatus() async {
    final status = await PermissionService.getCameraStatus();
    if (mounted) {
      setState(() {
        _isPermanentlyDenied = status.isPermanentlyDenied;
      });
    }
  }

  Future<void> _handlePermissionAction() async {
    if (_isPermanentlyDenied) {
      // Jika pengguna menolak permanen, arahkan ke halaman App Settings di HP
      await PermissionService.openSettings();
      if (mounted) {
        Navigator.of(context).pop(false);
      }
      return;
    }

    setState(() => _isLoading = true);
    final status = await PermissionService.requestCameraPermission();
    if (!mounted) return;
    setState(() => _isLoading = false);

    if (status.isGranted) {
      Navigator.of(context).pop(true);
      widget.onGranted?.call();
    } else if (status.isPermanentlyDenied) {
      setState(() {
        _isPermanentlyDenied = true;
      });
    } else {
      // Pengguna menolak sekali lagi
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text(
            'Izin kamera diperlukan untuk membuka Augmented Reality.',
          ),
          backgroundColor: AppColors.error,
          duration: Duration(seconds: 3),
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Dialog(
      backgroundColor: Colors.transparent,
      insetPadding: const EdgeInsets.symmetric(horizontal: 24),
      child: Container(
        padding: const EdgeInsets.all(24),
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(24),
          border: Border.all(
            color: AppColors.accent.withValues(alpha: 0.35),
            width: 1.4,
          ),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.5),
              blurRadius: 28,
              offset: const Offset(0, 10),
            ),
          ],
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            // Glowing Camera Icon
            Container(
              width: 68,
              height: 68,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                color: AppColors.accent.withValues(alpha: 0.15),
                border: Border.all(
                  color: AppColors.accent.withValues(alpha: 0.5),
                  width: 1.5,
                ),
              ),
              child: const Center(
                child: Icon(
                  Icons.camera_alt_rounded,
                  color: AppColors.accent,
                  size: 34,
                ),
              ),
            ),
            const SizedBox(height: 18),

            // Dialog Title
            const Text(
              'Akses Kamera Diperlukan',
              textAlign: TextAlign.center,
              style: TextStyle(
                color: Colors.white,
                fontSize: 19,
                fontWeight: FontWeight.bold,
                letterSpacing: 0.2,
              ),
            ),
            const SizedBox(height: 12),

            // Dialog Description
            Text(
              _isPermanentlyDenied
                  ? 'Izin kamera sebelumnya telah dinonaktifkan secara permanen. Silakan aktifkan izin kamera melalui Pengaturan Aplikasi di perangkat Anda agar dapat menggunakan fitur Augmented Reality.'
                  : 'Virtual Laboratory AR membutuhkan akses kamera untuk mendeteksi kartu marker kimia dan memunculkan model molekul 3D secara interaktif.',
              textAlign: TextAlign.center,
              style: TextStyle(
                color: Colors.white.withValues(alpha: 0.8),
                fontSize: 13.5,
                height: 1.45,
              ),
            ),
            const SizedBox(height: 24),

            // Action Buttons
            Row(
              children: [
                // Cancel Button
                Expanded(
                  child: OutlinedButton(
                    style: OutlinedButton.styleFrom(
                      padding: const EdgeInsets.symmetric(vertical: 13),
                      side: BorderSide(
                        color: Colors.white.withValues(alpha: 0.25),
                      ),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(14),
                      ),
                    ),
                    onPressed: () => Navigator.of(context).pop(false),
                    child: Text(
                      'Batal',
                      style: TextStyle(
                        color: Colors.white.withValues(alpha: 0.7),
                        fontWeight: FontWeight.w600,
                        fontSize: 14,
                      ),
                    ),
                  ),
                ),
                const SizedBox(width: 12),

                // Grant / Settings Button
                Expanded(
                  flex: 1,
                  child: ElevatedButton(
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppColors.primary,
                      foregroundColor: Colors.white,
                      elevation: 0,
                      padding: const EdgeInsets.symmetric(vertical: 13),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(14),
                      ),
                    ),
                    onPressed: _isLoading ? null : _handlePermissionAction,
                    child: _isLoading
                        ? const SizedBox(
                            width: 18,
                            height: 18,
                            child: CircularProgressIndicator(
                              strokeWidth: 2,
                              color: Colors.white,
                            ),
                          )
                        : Text(
                            _isPermanentlyDenied
                                ? 'Pengaturan'
                                : 'Izinkan Kamera',
                            style: const TextStyle(
                              fontWeight: FontWeight.bold,
                              fontSize: 14,
                            ),
                          ),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
