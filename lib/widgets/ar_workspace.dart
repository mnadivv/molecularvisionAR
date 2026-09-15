import 'package:flutter/material.dart';

import '../models/placed_object.dart';

class ARWorkspace extends StatelessWidget {
  final List<PlacedObject> objects;

  final Widget child;

  final Function(PlacedObject object) onAccept;

  final Function(int index, Offset position)? onObjectMoved;

  final Function(int index)? onDeleteObject;

  const ARWorkspace({
    super.key,
    required this.objects,
    required this.child,
    required this.onAccept,
    this.onObjectMoved,
    this.onDeleteObject,
  });

  @override
  Widget build(BuildContext context) {
    return DragTarget<PlacedObject>(
      onAcceptWithDetails: (details) {
        final object = details.data;

        object.position = Offset(
          details.offset.dx - 50,
          details.offset.dy - 120,
        );

        onAccept(object);
      },
      builder: (context, candidate, rejected) {
        return Stack(
          children: [

            Positioned.fill(
              child: child,
            ),

            ...objects.asMap().entries.map((entry) {
              final index = entry.key;
              final object = entry.value;

              return Positioned(
                left: object.position.dx,
                top: object.position.dy,
child: GestureDetector(

onTap: () {

  for (final item in objects) {
    item.selected = false;
  }

  object.selected = true;

  onObjectMoved?.call(index, object.position);

},

  onPanUpdate: (details) {

    final newX = (object.position.dx + details.delta.dx)
        .clamp(
          0.0,
          MediaQuery.of(context).size.width - 100,
        );

    final newY = (object.position.dy + details.delta.dy)
        .clamp(
          90.0,
          MediaQuery.of(context).size.height - 180,
        );

    object.position = Offset(newX, newY);

    onObjectMoved?.call(index, object.position);

  },

onScaleUpdate: (details) {

  object.scale = details.scale.clamp(0.5, 3.0);

  object.rotation = details.rotation;

  onObjectMoved?.call(index, object.position);

},

  child: Container(
decoration: BoxDecoration(

  border: object.selected
      ? Border.all(
          color: Colors.lightBlueAccent,
          width: 2,
        )
      : null,

  borderRadius: BorderRadius.circular(12),

  boxShadow: [
    BoxShadow(
      color: Colors.black.withOpacity(.25),
      blurRadius: 12,
      offset: const Offset(0, 6),
    ),
  ],
),
child: Stack(
  clipBehavior: Clip.none,
  children: [

    Transform.rotate(
      angle: object.rotation,
      child: Transform.scale(
        scale: object.scale,
        child: Image.asset(

  object.reacted
      ? "assets/images/reaction/beaker_hcl.png"
      : object.image,

  width: 100,

  height: 100,

  fit: BoxFit.contain,

)
      ),
    ),

    if (object.selected)
      Positioned(
        top: -10,
        right: -10,
        child: GestureDetector(
          onTap: () {
            onDeleteObject?.call(index);
          },
          child: Container(
            width: 28,
            height: 28,
            decoration: const BoxDecoration(
              color: Colors.red,
              shape: BoxShape.circle,
            ),
            child: const Icon(
              Icons.close,
              color: Colors.white,
              size: 18,
            ),
          ),
        ),
      ),

  ],
), 
                  ),
          
                ),
              );
            }),

          ],
        );
      },
    );
  }
}