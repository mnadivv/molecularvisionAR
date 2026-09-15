import 'package:flutter/material.dart';

import '../core/constants/app_colors.dart';
import '../models/material_item.dart';
import '../models/placed_object.dart';

class MaterialDrawer extends StatelessWidget {
  final bool visible;
  final List<MaterialItem> materials;
  final Function(MaterialItem) onSelected;

  const MaterialDrawer({
    super.key,
    required this.visible,
    required this.materials,
    required this.onSelected,
  });

  @override
  Widget build(BuildContext context) {
    return AnimatedPositioned(
      duration: const Duration(milliseconds: 350),
      curve: Curves.easeInOut,

      right: visible ? 20 : -330,

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
                    Icons.biotech,
                    color: Colors.orange,
                  ),

                  SizedBox(width: 10),

                  Text(
                    "MATERIALS",
                    style: TextStyle(
                      fontWeight: FontWeight.bold,
                      fontSize: 22,
                    ),
                  )

                ],
              ),

              const SizedBox(height: 20),

              Expanded(
                child: ListView.builder(
                  padding: const EdgeInsets.all(16),

                  itemCount: materials.length,

                  itemBuilder: (context, index) {

                    final material = materials[index];

                    return _MaterialCard(
                      material: material,
                      onTap: () => onSelected(material),
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

class _MaterialCard extends StatelessWidget {

  final MaterialItem material;
  final VoidCallback onTap;

  const _MaterialCard({
    required this.material,
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
          color: Colors.orange.withOpacity(.15),
        ),
      ),

      child: Row(

        children: [

          Container(
            width: 58,
            height: 58,

            decoration: BoxDecoration(
              color: Colors.orange.withOpacity(.10),
              borderRadius: BorderRadius.circular(18),
            ),

            child: ClipRRect(
              borderRadius: BorderRadius.circular(18),
              child: Image.asset(
                material.image,
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
                  material.name,
                  style: const TextStyle(
                    fontWeight: FontWeight.bold,
                    fontSize: 16,
                  ),
                ),

                const SizedBox(height: 4),

                Text(
                  material.formula,
                  style: TextStyle(
                    color: Colors.grey.shade600,
                  ),
                ),

                const SizedBox(height: 4),

                Text(
                  material.description,
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(
                    color: Colors.grey.shade700,
                    fontSize: 12,
                  ),
                ),

                const SizedBox(height: 10),

                Row(
                  children: [

                    Icon(
                      Icons.touch_app,
                      size: 16,
                      color: Colors.orange.shade700,
                    ),

                    const SizedBox(width: 6),

                    Text(
                      "Tekan & tahan untuk drag",
                      style: TextStyle(
                        fontSize: 12,
                        color: Colors.orange.shade700,
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

      id: material.id,

      name: material.name,

      image: material.image,

      position: Offset.zero,

      isTool: false,

    ),

    feedback: Material(

      color: Colors.transparent,

      child: SizedBox(

        width: 240,

        child: card,

      ),

    ),

    childWhenDragging: Opacity(

      opacity: .35,

      child: card,

    ),

    child: InkWell(

      borderRadius: BorderRadius.circular(20),

      onTap: onTap,

      child: card,

    ),

  );

}

}