import 'package:flutter/material.dart';

class ProgressHeader extends StatelessWidget {
  final String title;
  final int currentStep;
  final int totalStep;
  final VoidCallback onBack;

  const ProgressHeader({
    super.key,
    required this.title,
    required this.currentStep,
    required this.totalStep,
    required this.onBack,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.all(16),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.black.withOpacity(.45),
        borderRadius: BorderRadius.circular(20),
      ),
      child: Row(
        children: [
          IconButton(
            onPressed: onBack,
            icon: const Icon(
              Icons.arrow_back,
              color: Colors.white,
            ),
          ),

          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: const TextStyle(
                    color: Colors.white,
                    fontWeight: FontWeight.bold,
                    fontSize: 18,
                  ),
                ),

                const SizedBox(height: 6),

                LinearProgressIndicator(
                  value: currentStep / totalStep,
                  backgroundColor: Colors.white24,
                ),
              ],
            ),
          ),

          const SizedBox(width: 12),

          Text(
            "$currentStep/$totalStep",
            style: const TextStyle(
              color: Colors.white,
              fontWeight: FontWeight.bold,
            ),
          ),
        ],
      ),
    );
  }
}