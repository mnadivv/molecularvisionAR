import 'package:flutter/material.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../auth/presentation/screens/login_screen.dart';
import '../../models/onboarding_item.dart';
import '../../services/onboarding_service.dart';
import '../widgets/onboarding_indicator.dart';
import '../widgets/onboarding_page_content.dart';

/// Halaman Onboarding Pengenalan Fitur Utama MVAR (Hanya tampil 1x saat awal install)
class OnboardingScreen extends StatefulWidget {
  const OnboardingScreen({super.key});

  @override
  State<OnboardingScreen> createState() => _OnboardingScreenState();
}

class _OnboardingScreenState extends State<OnboardingScreen> {
  final PageController _pageController = PageController();
  final List<OnboardingItem> _items = OnboardingItem.defaultItems;
  int _currentIndex = 0;

  @override
  void dispose() {
    _pageController.dispose();
    super.dispose();
  }

  Future<void> _finishOnboarding() async {
    // Tandai status onboarding selesai di SharedPreferences
    await OnboardingService.setOnboardingCompleted();

    if (!mounted) return;

    // Transisi halus ke halaman login
    Navigator.pushReplacement(
      context,
      PageRouteBuilder(
        transitionDuration: const Duration(milliseconds: 650),
        pageBuilder: (_, animation, __) => FadeTransition(
          opacity: animation,
          child: const LoginScreen(),
        ),
      ),
    );
  }

  void _nextPage() {
    if (_currentIndex < _items.length - 1) {
      _pageController.nextPage(
        duration: const Duration(milliseconds: 400),
        curve: Curves.easeInOutCubic,
      );
    } else {
      _finishOnboarding();
    }
  }

  @override
  Widget build(BuildContext context) {
    final activeItem = _items[_currentIndex];
    final isLastPage = _currentIndex == _items.length - 1;

    return Scaffold(
      backgroundColor: AppColors.background,
      body: Stack(
        children: [
          // Background ambient molecule top right
          Positioned(
            top: -40,
            right: -60,
            child: Opacity(
              opacity: 0.12,
              child: Image.asset(
                'assets/images/bg_molecule.png',
                width: 320,
              ),
            ),
          ),

          // Background ambient molecule bottom left
          Positioned(
            bottom: -50,
            left: -70,
            child: Opacity(
              opacity: 0.09,
              child: Transform.rotate(
                angle: -0.6,
                child: Image.asset(
                  'assets/images/bg_molecule.png',
                  width: 300,
                ),
              ),
            ),
          ),

          SafeArea(
            child: Column(
              children: [
                // Top Header (Brand Logo & Skip Button)
                Padding(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 20,
                    vertical: 12,
                  ),
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      // Logo & Title
                      Row(
                        children: [
                          Container(
                            width: 32,
                            height: 32,
                            decoration: BoxDecoration(
                              shape: BoxShape.circle,
                              border: Border.all(
                                color: AppColors.accent.withValues(alpha: 0.6),
                                width: 1.2,
                              ),
                            ),
                            child: ClipOval(
                              child: Image.asset(
                                'assets/images/logo_circle.png',
                                fit: BoxFit.cover,
                              ),
                            ),
                          ),
                          const SizedBox(width: 10),
                          const Text(
                            'MVAR',
                            style: TextStyle(
                              color: Colors.white,
                              fontSize: 16,
                              fontWeight: FontWeight.w800,
                              letterSpacing: 1.2,
                            ),
                          ),
                        ],
                      ),

                      // Skip Button (Hanya tampil di slide 0-2)
                      if (!isLastPage)
                        TextButton(
                          onPressed: _finishOnboarding,
                          style: TextButton.styleFrom(
                            foregroundColor: Colors.white60,
                            padding: const EdgeInsets.symmetric(
                              horizontal: 14,
                              vertical: 6,
                            ),
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(16),
                            ),
                          ),
                          child: const Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              Text(
                                'Lewati',
                                style: TextStyle(
                                  fontSize: 13.5,
                                  fontWeight: FontWeight.w600,
                                ),
                              ),
                              SizedBox(width: 4),
                              Icon(Icons.chevron_right_rounded, size: 18),
                            ],
                          ),
                        )
                      else
                        const SizedBox(height: 36),
                    ],
                  ),
                ),

                // Center Sliding Content
                Expanded(
                  child: PageView.builder(
                    controller: _pageController,
                    itemCount: _items.length,
                    onPageChanged: (index) {
                      setState(() {
                        _currentIndex = index;
                      });
                    },
                    itemBuilder: (context, index) {
                      return OnboardingPageContent(item: _items[index]);
                    },
                  ),
                ),

                // Bottom Navigation & Controls
                Padding(
                  padding: const EdgeInsets.fromLTRB(24, 16, 24, 28),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      // Dots / Capsule Indicator
                      OnboardingIndicator(
                        count: _items.length,
                        currentIndex: _currentIndex,
                        activeColor: activeItem.accentColor,
                      ),

                      const SizedBox(height: 28),

                      // Bottom Button
                      AnimatedSwitcher(
                        duration: const Duration(milliseconds: 300),
                        child: isLastPage
                            ? SizedBox(
                                key: const ValueKey('start_button'),
                                width: double.infinity,
                                height: 54,
                                child: ElevatedButton(
                                  style: ElevatedButton.styleFrom(
                                    backgroundColor: activeItem.accentColor,
                                    foregroundColor: Colors.white,
                                    elevation: 6,
                                    shadowColor: activeItem.accentColor
                                        .withValues(alpha: 0.5),
                                    shape: RoundedRectangleBorder(
                                      borderRadius: BorderRadius.circular(16),
                                    ),
                                  ),
                                  onPressed: _finishOnboarding,
                                  child: const Row(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    children: [
                                      Text(
                                        'Mulai Sekarang',
                                        style: TextStyle(
                                          fontSize: 15.5,
                                          fontWeight: FontWeight.bold,
                                          letterSpacing: 0.3,
                                        ),
                                      ),
                                      SizedBox(width: 8),
                                      Icon(
                                        Icons.rocket_launch_rounded,
                                        size: 19,
                                      ),
                                    ],
                                  ),
                                ),
                              )
                            : Row(
                                key: const ValueKey('nav_row'),
                                mainAxisAlignment:
                                    MainAxisAlignment.spaceBetween,
                                children: [
                                  // Halaman ke-X dari Y
                                  Text(
                                    '${_currentIndex + 1} dari ${_items.length}',
                                    style: TextStyle(
                                      color: Colors.white.withValues(alpha: 0.5),
                                      fontSize: 13,
                                      fontWeight: FontWeight.w500,
                                    ),
                                  ),

                                  // Tombol Lanjut (Next)
                                  ElevatedButton(
                                    style: ElevatedButton.styleFrom(
                                      backgroundColor: activeItem.accentColor,
                                      foregroundColor: Colors.white,
                                      elevation: 4,
                                      padding: const EdgeInsets.symmetric(
                                        horizontal: 22,
                                        vertical: 12,
                                      ),
                                      shape: RoundedRectangleBorder(
                                        borderRadius: BorderRadius.circular(14),
                                      ),
                                    ),
                                    onPressed: _nextPage,
                                    child: const Row(
                                      mainAxisSize: MainAxisSize.min,
                                      children: [
                                        Text(
                                          'Lanjut',
                                          style: TextStyle(
                                            fontSize: 14,
                                            fontWeight: FontWeight.bold,
                                          ),
                                        ),
                                        SizedBox(width: 6),
                                        Icon(
                                          Icons.arrow_forward_rounded,
                                          size: 17,
                                        ),
                                      ],
                                    ),
                                  ),
                                ],
                              ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
