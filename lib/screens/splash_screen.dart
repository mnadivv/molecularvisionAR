import 'dart:async';

import 'package:flutter/material.dart';

import '../utils/app_colors.dart';
import '../widgets/custom_loading_bar.dart';
import 'home_screen.dart';

class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen> {
  double progress = 0;

  @override
  void initState() {
    super.initState();

    Timer.periodic(const Duration(milliseconds: 45), (timer) {
      if (!mounted) return;

      setState(() {
        progress += 0.02;
      });

      if (progress >= 1) {
        timer.cancel();

        Future.delayed(const Duration(milliseconds: 500), () {
          if (!mounted) return;

          Navigator.pushReplacement(
            context,
            PageRouteBuilder(
              transitionDuration: const Duration(milliseconds: 700),
              pageBuilder: (_, animation, __) => FadeTransition(
                opacity: animation,
                child: const HomeScreen(),
              ),
            ),
          );
        });
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,

      body: Stack(
        children: [

          /// Molekul kanan atas
          Positioned(
            top: -40,
            right: -70,
            child: Opacity(
              opacity: .13,
              child: Image.asset(
                "assets/images/bg_molecule.png",
                width: 340,
              ),
            ),
          ),

          /// Molekul kiri bawah
          Positioned(
            bottom: -50,
            left: -80,
            child: Opacity(
              opacity: .10,
              child: Transform.rotate(
                angle: -.55,
                child: Image.asset(
                  "assets/images/bg_molecule.png",
                  width: 320,
                ),
              ),
            ),
          ),

          SafeArea(
            child: Center(
              child: Padding(
                padding:
                    const EdgeInsets.symmetric(horizontal: 30),

                child: Column(
                  mainAxisAlignment:
                      MainAxisAlignment.center,

                  children: [

                    Hero(
                      tag: "logo",

                      child: Container(
                        padding: const EdgeInsets.all(12),

                        decoration: BoxDecoration(
                          shape: BoxShape.circle,
                          color: Colors.white.withValues(
                            alpha: .05,
                          ),
                        ),

                        child: Image.asset(
                          "assets/images/logo_circle.png",
                          width: 120,
                        ),
                      ),
                    ),

                    const SizedBox(height: 28),

                    Image.asset(
                      "assets/images/logo_text.png",
                      width: 250,
                    ),

                    const SizedBox(height: 18),

                    Text(
                      "Bring Chemistry to Life\nwith Augmented Reality",
                      textAlign: TextAlign.center,
                      style: TextStyle(
                        color:
                            Colors.white.withValues(alpha: .75),
                        fontSize: 15,
                        height: 1.5,
                      ),
                    ),

                    const SizedBox(height: 60),

                    CustomLoadingBar(
                      progress: progress,
                    ),

                    const SizedBox(height: 18),

                    Text(
                      "${(progress * 100).toInt()} %",
                      style: const TextStyle(
                        color: Colors.white70,
                        fontSize: 14,
                        fontWeight: FontWeight.w600,
                      ),
                    ),

                    const SizedBox(height: 8),

                    Text(
                      "Loading...",
                      style: TextStyle(
                        color:
                            Colors.white.withValues(alpha: .70),
                        fontSize: 13,
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}