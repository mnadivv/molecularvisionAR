import 'package:flutter/material.dart';

import '../core/constants/app_colors.dart';

class BottomToolbar extends StatelessWidget {

  final VoidCallback onTool;

  final VoidCallback onMaterial;

  final VoidCallback onData;

  final VoidCallback onHint;

  final VoidCallback onNext;

  final bool canNext;

  const BottomToolbar({
    super.key,
    required this.onTool,
    required this.onMaterial,
    required this.onData,
    required this.onHint,
    required this.onNext,
    required this.canNext,
  });

  @override
  Widget build(BuildContext context) {

    return Container(

      margin: const EdgeInsets.all(20),

      padding: const EdgeInsets.symmetric(
        horizontal: 14,
        vertical: 10,
      ),

      decoration: BoxDecoration(

        color: Colors.black.withOpacity(.45),

        borderRadius: BorderRadius.circular(22),

      ),

      child: Row(

        mainAxisAlignment: MainAxisAlignment.spaceEvenly,

        children: [

          _ToolbarButton(
            icon: Icons.handyman,
            label: "Alat",
            color: AppColors.primary,
            onTap: onTool,
          ),

          _ToolbarButton(
            icon: Icons.biotech,
            label: "Bahan",
            color: Colors.orange,
            onTap: onMaterial,
          ),

          _ToolbarButton(
            icon: Icons.description,
            label: "Data",
            color: Colors.green,
            onTap: onData,
          ),

          _ToolbarButton(
            icon: Icons.lightbulb,
            label: "Hint",
            color: Colors.amber,
            onTap: onHint,
          ),

          Opacity(
            opacity: canNext ? 1 : 0.4,
            child: IgnorePointer(
              ignoring: !canNext,
              child: _ToolbarButton(
                icon: Icons.arrow_forward,
                label: "Next",
                color: Colors.blue,
                onTap: onNext,
              ),
            ),
          ),

        ],
      ),
    );

  }

}

class _ToolbarButton extends StatelessWidget {

  final IconData icon;

  final String label;

  final Color color;

  final VoidCallback onTap;

  const _ToolbarButton({

    required this.icon,

    required this.label,

    required this.color,

    required this.onTap,

  });

  @override
  Widget build(BuildContext context) {

    return InkWell(

      borderRadius: BorderRadius.circular(18),

      onTap: onTap,

      child: AnimatedContainer(

        duration: const Duration(milliseconds: 250),

        padding: const EdgeInsets.symmetric(
          horizontal: 16,
          vertical: 12,
        ),

        decoration: BoxDecoration(

          color: Colors.white,

          borderRadius: BorderRadius.circular(18),

        ),

        child: Row(

          children: [

            Icon(
              icon,
              color: color,
            ),

            const SizedBox(width: 8),

            Text(

              label,

              style: const TextStyle(

                fontWeight: FontWeight.bold,

              ),
            )

          ],
        ),
      ),
    );
  }
}