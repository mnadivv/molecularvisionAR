import 'package:flutter/material.dart';

import '../../data/practicum/reaction_rate.dart';
import '../../widgets/tool_drawer.dart';
import '../../widgets/progress_header.dart';
import '../../widgets/assistant_bubble.dart';
import '../../widgets/bottom_toolbar.dart';
import '../../widgets/material_drawer.dart';
import '../../services/practicum_engine.dart';
import '../../models/placed_object.dart';
import '../../widgets/ar_workspace.dart';
import '../../services/camera_service.dart';
import 'package:camera/camera.dart';
import 'package:flutter/services.dart';

class PracticumScreen extends StatefulWidget {
  const PracticumScreen({super.key});

  @override
  State<PracticumScreen> createState() => _PracticumScreenState();
}

class _PracticumScreenState extends State<PracticumScreen> {

@override
void dispose() {

  CameraService.dispose();

  // Kembalikan portrait setelah keluar praktikum
  SystemChrome.setPreferredOrientations([
    DeviceOrientation.portraitUp,
  ]);

  super.dispose();
}

late PracticumEngine engine;

bool cameraReady = false;

bool reactionDone = false;
final List<String> completedSteps = [];

  bool showTools = false;

  bool showMaterials = false;

  bool stepCompleted = false;
  final List<PlacedObject> placedObjects = [];

  String? assistantMessage;

  final practicum = reactionRatePracticum;

Future<void> _initCamera() async {
  await CameraService.initialize();

  if (mounted) {
    setState(() {
      cameraReady = true;
    });
  }
}

void _checkInteraction() {

  if (reactionDone) return;

  if (placedObjects.length < 2) return;

  for (int i = 0; i < placedObjects.length; i++) {

    for (int j = i + 1; j < placedObjects.length; j++) {

      final a = placedObjects[i];
      final b = placedObjects[j];

      final distance =
          (a.position - b.position).distance;

      if (distance < 90) {

        _handleInteraction(a, b);

      }

    }

  }

}
void _handleInteraction(
  PlacedObject a,
  PlacedObject b,
) {

  if (
      (a.id == "gelas_kimia" && b.id == "hcl") ||
      (b.id == "gelas_kimia" && a.id == "hcl")
  ) {

    reactionDone = true;

    setState(() {

      a.reacted = true;
      b.reacted = true;

      assistantMessage =
          "🧪 HCl berhasil dimasukkan.";

    });

  }

}

bool _validateStep(String objectId) {

  final currentStep = engine.current.requiredItemId;

  if (objectId != currentStep) {

    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        backgroundColor: Colors.red,
        content: Text("❌ Urutan praktikum salah"),
      ),
    );

    return false;
  }

  return true;
}

@override
void initState() {
  super.initState();

  engine = PracticumEngine(practicum);

  SystemChrome.setPreferredOrientations([
    DeviceOrientation.landscapeLeft,
    DeviceOrientation.landscapeRight,
  ]);

  _initCamera();
}

  @override
  Widget build(BuildContext context) {

    final step = engine.current;

    return Scaffold(

      backgroundColor: Colors.black,

      body: SafeArea(

        child: Stack(

          children: [

            /// ============================
            /// CAMERA PLACEHOLDER
            /// ============================

Positioned.fill(

  child: ARWorkspace(

    objects: placedObjects,

onObjectMoved: (index, position) {

  setState(() {

placedObjects[index].position = position;

_checkInteraction();
  });

},

onDeleteObject: (index) {

  setState(() {

    placedObjects.removeAt(index);

    reactionDone = false;

    assistantMessage = null;

  });

},
onAccept: (object) {

  setState(() {

    placedObjects.add(object);

  });

  _checkInteraction();

},
child: Stack(
  children: [

cameraReady
    ? SizedBox.expand(
        child: CameraPreview(
          CameraService.controller!,
        ),
      )
    : const Center(
        child: CircularProgressIndicator(),
      ),

  ],
),

  ),

),

// progres holder
            Positioned(
              top: 0,
              left: 0,
              right: 0,
              child: ProgressHeader(
                title: practicum.title,
                currentStep: engine.currentStep + 1,
                totalStep: practicum.steps.length,
onBack: () async {
  await SystemChrome.setPreferredOrientations([
    DeviceOrientation.portraitUp,
  ]);

  if (mounted) {
    Navigator.pop(context);
  }
},
              ),
            ),
            /// ============================
            /// HEADER
            /// ============================

            Positioned(

              left: 0,

              right: 0,

              bottom: 0,

              child: BottomToolbar(

              canNext: stepCompleted,

                onTool: () {

                  setState(() {

                    showTools = !showTools;

                    showMaterials = false;

                  });

                },

                onMaterial: () {

                  setState(() {

                    showMaterials = !showMaterials;

                    showTools = false;

                  });

                },

                onData: () {

                },

                onHint: () {

                },

                onNext: () {

                  if(engine.next()){

                    setState((){

                      stepCompleted = false;
reactionDone = false;
assistantMessage = null;
placedObjects.clear();

                    });

                  }else{

                    ScaffoldMessenger.of(context).showSnackBar(

                      const SnackBar(

                        content: Text(
                          "🎉 Praktikum selesai",
                        ),

                      ),

                    );

                  }

                },

              ),

            ),

            Positioned(

              right: 20,

              top: 110,

              child: AssistantBubble(

              title: assistantMessage ?? step.title,

              instruction: assistantMessage ?? step.instruction,

              hint: step.hint,

              ),

            ),

            ToolDrawer(
              visible: showTools,
              tools: practicum.tools,
              onSelected: (tool) {

if (_validateStep(tool.id) &&
    engine.validate(tool.id)) {

  setState(() {

    stepCompleted = true;

    showTools = false;

    assistantMessage =
        "✅ ${tool.name} berhasil digunakan.\nTekan Next untuk melanjutkan.";

  });

                } else {

                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(
                      backgroundColor: Colors.red,
                      content: Text(
                        "❌ Alat tidak sesuai",
                      ),
                    ),
                  );

                }

              },
            ),

            MaterialDrawer(
              visible: showMaterials,
              materials: practicum.materials,
              onSelected: (material){

if (_validateStep(material.id) &&
    engine.validate(material.id)) {

  setState(() {

    stepCompleted = true;

    showMaterials = false;

    assistantMessage =
        "✅ ${material.name} berhasil digunakan.";

  });

                }else{

                  ScaffoldMessenger.of(context).showSnackBar(

                    SnackBar(
                      backgroundColor: Colors.red,
                      content: Text(
                        "❌ Bahan belum sesuai",
                      ),

                    ),

                  );

                }

              },
            ),

          ],
        ),
      ),
    );
  }

}