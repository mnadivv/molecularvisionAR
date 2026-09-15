import 'package:flutter/material.dart';

import '../core/constants/app_colors.dart';
import '../models/tool_item.dart';
import '../models/placed_object.dart';

class ToolDrawer extends StatelessWidget {
  final bool visible;

  final List<ToolItem> tools;

  final Function(ToolItem) onSelected;

  const ToolDrawer({
    super.key,
    required this.visible,
    required this.tools,
    required this.onSelected,
  });

  @override
  Widget build(BuildContext context) {
    return AnimatedPositioned(
      duration: const Duration(milliseconds: 350),

      curve: Curves.easeInOut,

      left: visible ? 20 : -330,

      top: 110,

      bottom: 90,

      width: 300,

      child: Material(
        color: Colors.transparent,

        child: Container(
          decoration: BoxDecoration(
            color: Colors.white,

            borderRadius: BorderRadius.circular(26),

            boxShadow: [
              BoxShadow(
                blurRadius: 20,
                color: Colors.black.withOpacity(.18),
              ),
            ],
          ),

          child: Column(
            children: [
              const SizedBox(height: 20),

              Row(
                mainAxisAlignment: MainAxisAlignment.center,

                children: const [
                  Icon(
                    Icons.science,
                    color: AppColors.primary,
                  ),

                  SizedBox(width: 10),

                  Text(
                    "TOOLS",
                    style: TextStyle(
                      fontSize: 22,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),

              const SizedBox(height: 20),

              Expanded(
                child: ListView.builder(
                  padding: const EdgeInsets.all(16),

                  itemCount: tools.length,

                  itemBuilder: (context, index) {
                    final tool = tools[index];

                    return _ToolCard(
                      tool: tool,
                      onTap: () => onSelected(tool),
                    );
                  },
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _ToolCard extends StatelessWidget {
  final ToolItem tool;

  final VoidCallback onTap;

  const _ToolCard({
    required this.tool,
    required this.onTap,
  });

@override
Widget build(BuildContext context) {

  final card = Container(

    margin: const EdgeInsets.only(bottom: 14),

    padding: const EdgeInsets.all(16),

    decoration: BoxDecoration(

      color: Colors.grey.shade100,

      borderRadius: BorderRadius.circular(20),

      border: Border.all(
        color: AppColors.primary.withOpacity(.15),
      ),

    ),

    child: Row(

      children: [

        Container(

          width: 58,

          height: 58,

          decoration: BoxDecoration(

            color: AppColors.primary.withOpacity(.10),

            borderRadius: BorderRadius.circular(18),

          ),

child: ClipRRect(
  borderRadius: BorderRadius.circular(18),
  child: Image.asset(
    tool.image,
    fit: BoxFit.cover,
  ),
),

        ),

        const SizedBox(width: 14),

        Expanded(

          child: Column(

            crossAxisAlignment: CrossAxisAlignment.start,

            children: [

              Text(

                tool.name,

                style: const TextStyle(

                  fontWeight: FontWeight.bold,

                  fontSize: 16,

                ),

              ),

              const SizedBox(height: 5),

              Text(

                tool.description,

                maxLines: 2,

                overflow: TextOverflow.ellipsis,

                style: TextStyle(

                  color: Colors.grey.shade700,

                  fontSize: 13,

                ),

              ),

              const SizedBox(height: 10),

              Row(

                children: [

                  Icon(
                    Icons.touch_app,
                    size: 16,
                    color: Colors.blue.shade700,
                  ),

                  const SizedBox(width: 6),

                  Text(

                    "Tekan & tahan untuk drag",

                    style: TextStyle(

                      fontSize: 12,

                      color: Colors.blue.shade700,

                      fontWeight: FontWeight.w600,

                    ),

                  ),

                ],

              )

            ],

          ),

        ),

      ],

    ),

  );

  return LongPressDraggable<PlacedObject>(

    data: PlacedObject(

      id: tool.id,

      name: tool.name,
   image: tool.image,

      position: Offset.zero,

      isTool: true,

    ),

    

feedback: Material(
  color: Colors.transparent,
  child: Transform.scale(
    scale: 1.15,
    child: Image.asset(
      tool.image,
      width: 90,
      height: 90,
      fit: BoxFit.contain,
    ),
  ),
),
    childWhenDragging: Opacity(

      opacity: .35,

      child: card,

    ),

    child: card,

  );

}
}