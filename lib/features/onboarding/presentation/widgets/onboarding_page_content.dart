import 'package:flutter/material.dart';

import '../../models/onboarding_item.dart';

/// Konten kartu visual untuk satu slide pada halaman Onboarding
class OnboardingPageContent extends StatelessWidget {
  final OnboardingItem item;

  const OnboardingPageContent({super.key, required this.item});

  @override
  Widget build(BuildContext context) {
    final size = MediaQuery.of(context).size;
    final isShortScreen = size.height < 700;

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 24),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          // Hero Glowing Centerpiece Icon
          Stack(
            alignment: Alignment.center,
            children: [
              // Radial Ambient Glow
              Container(
                width: isShortScreen ? 140 : 180,
                height: isShortScreen ? 140 : 180,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  gradient: RadialGradient(
                    colors: [
                      item.accentColor.withValues(alpha: 0.28),
                      Colors.transparent,
                    ],
                  ),
                ),
              ),

              // Outer Ring
              Container(
                width: isShortScreen ? 110 : 136,
                height: isShortScreen ? 110 : 136,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: const Color(0xFF0C2454).withValues(alpha: 0.85),
                  border: Border.all(
                    color: item.accentColor.withValues(alpha: 0.5),
                    width: 1.8,
                  ),
                ),
                child: Center(
                  child: Icon(
                    item.icon,
                    size: isShortScreen ? 50 : 64,
                    color: item.accentColor,
                  ),
                ),
              ),
            ],
          ),

          SizedBox(height: isShortScreen ? 20 : 36),

          // Badge Kategori Fitur
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 6),
            decoration: BoxDecoration(
              color: item.accentColor.withValues(alpha: 0.16),
              borderRadius: BorderRadius.circular(20),
              border: Border.all(
                color: item.accentColor.withValues(alpha: 0.4),
                width: 1,
              ),
            ),
            child: Text(
              item.badge.toUpperCase(),
              style: TextStyle(
                color: item.accentColor,
                fontSize: 11.5,
                fontWeight: FontWeight.bold,
                letterSpacing: 1.2,
              ),
            ),
          ),

          const SizedBox(height: 16),

          // Judul Fitur
          Text(
            item.title,
            textAlign: TextAlign.center,
            style: TextStyle(
              color: Colors.white,
              fontSize: isShortScreen ? 20 : 23,
              fontWeight: FontWeight.bold,
              letterSpacing: 0.2,
              height: 1.3,
            ),
          ),

          const SizedBox(height: 12),

          // Subtitle / Deskripsi Fitur
          Text(
            item.subtitle,
            textAlign: TextAlign.center,
            style: TextStyle(
              color: Colors.white.withValues(alpha: 0.72),
              fontSize: isShortScreen ? 13 : 14.5,
              height: 1.5,
            ),
          ),

          const SizedBox(height: 20),

          // Tag Chips Fitur
          Wrap(
            spacing: 8,
            runSpacing: 8,
            alignment: WrapAlignment.center,
            children: item.tags.map((tag) {
              return Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 10,
                  vertical: 4,
                ),
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.06),
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(
                    color: Colors.white.withValues(alpha: 0.12),
                    width: 0.8,
                  ),
                ),
                child: Text(
                  tag,
                  style: TextStyle(
                    color: Colors.white.withValues(alpha: 0.65),
                    fontSize: 11.5,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              );
            }).toList(),
          ),
        ],
      ),
    );
  }
}
