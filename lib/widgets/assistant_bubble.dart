import 'package:flutter/material.dart';

import '../core/constants/app_colors.dart';

class AssistantBubble extends StatelessWidget {
  final String title;
  final String instruction;
  final String hint;

  const AssistantBubble({
    super.key,
    required this.title,
    required this.instruction,
    required this.hint,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 330,

      padding: const EdgeInsets.all(18),

      decoration: BoxDecoration(
        color: Colors.white,

        borderRadius: BorderRadius.circular(24),

        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(.12),
            blurRadius: 18,
            offset: const Offset(0, 6),
          )
        ],
      ),

      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,

        children: [

          Row(

            children: [

              CircleAvatar(

                radius: 24,

                backgroundColor: AppColors.primary,

                child: const Icon(
                  Icons.psychology,
                  color: Colors.white,
                ),
              ),

              const SizedBox(width: 12),

              const Expanded(
                child: Text(
                  "MVAR Assistant",
                  style: TextStyle(
                    fontWeight: FontWeight.bold,
                    fontSize: 17,
                  ),
                ),
              )

            ],
          ),

          const SizedBox(height: 18),

          Text(
            title,
            style: const TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 19,
            ),
          ),

          const SizedBox(height: 10),

          Text(
            instruction,
            style: const TextStyle(
              height: 1.6,
            ),
          ),

          const SizedBox(height: 18),

          Container(

            padding: const EdgeInsets.all(12),

            decoration: BoxDecoration(
              color: Colors.orange.shade50,

              borderRadius: BorderRadius.circular(14),
            ),

            child: Row(

              crossAxisAlignment: CrossAxisAlignment.start,

              children: [

                const Icon(
                  Icons.lightbulb,
                  color: Colors.orange,
                ),

                const SizedBox(width: 10),

                Expanded(
                  child: Text(hint),
                )

              ],
            ),
          )
        ],
      ),
    );
  }
}