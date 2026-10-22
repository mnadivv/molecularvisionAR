import 'package:flutter/material.dart';

import '../../../../core/models/chemistry_topic.dart';
import '../../../../core/services/permission_service.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../core/widgets/camera_permission_dialog.dart';
import '../../../home/presentation/widgets/menu_card.dart';
import '../widgets/ar_module_banner_card.dart';
import '../../../../core/constants/app_links.dart';
import 'ar_unity_screen.dart';

class ARMenuScreen extends StatefulWidget {
  const ARMenuScreen({super.key});

  @override
  State<ARMenuScreen> createState() => _ARMenuScreenState();
}

class _ARMenuScreenState extends State<ARMenuScreen> {
  final TextEditingController searchController = TextEditingController();

  List<ChemistryTopic> filteredTopics = List.from(chemistryTopics);

  @override
  void initState() {
    super.initState();
    searchController.addListener(searchTopic);
  }

  void searchTopic() {
    final keyword = searchController.text.toLowerCase();

    setState(() {
      filteredTopics = chemistryTopics.where((topic) {
        return topic.title.toLowerCase().contains(keyword) ||
            (topic.practicumTitle != null &&
                topic.practicumTitle!.toLowerCase().contains(keyword)) ||
            topic.subtitle.toLowerCase().contains(keyword);
      }).toList();
    });
  }

  Future<void> _handleTopicTap(ChemistryTopic topic) async {
    final hasPermission = await PermissionService.hasCameraPermission();
    if (!mounted) return;

    if (!hasPermission) {
      final granted = await CameraPermissionDialog.show(context);
      if (!granted || !mounted) return;
    }

    Navigator.push(
      context,
      MaterialPageRoute(builder: (_) => ARUnityScreen(topic: topic)),
    );
  }

  @override
  void dispose() {
    searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        leading: IconButton(
          icon: const Icon(
            Icons.arrow_back_ios_new_rounded,
            color: Colors.white,
            size: 20,
          ),
          onPressed: () => Navigator.of(context).pop(),
        ),
        title: const Text("Augmented Reality"),
        backgroundColor: Colors.transparent,
        elevation: 0,
        centerTitle: false,
        iconTheme: const IconThemeData(color: Colors.white),
        titleTextStyle: const TextStyle(
          color: Colors.white,
          fontSize: 20,
          fontWeight: FontWeight.bold,
        ),
      ),
      body: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          children: [
            Container(
              height: 48,
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(16),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withValues(alpha: 0.15),
                    blurRadius: 10,
                    offset: const Offset(0, 4),
                  ),
                ],
              ),
              child: TextField(
                controller: searchController,
                textInputAction: TextInputAction.search,
                style: const TextStyle(
                  color: Color(0xFF1E293B),
                  fontSize: 14,
                  fontWeight: FontWeight.w500,
                ),
                decoration: InputDecoration(
                  hintText: "Cari materi Augmented Reality...",
                  hintStyle: TextStyle(
                    color: Colors.grey.shade500,
                    fontSize: 14,
                  ),
                  prefixIcon: Icon(
                    Icons.search_rounded,
                    color: Colors.grey.shade600,
                    size: 20,
                  ),
                  suffixIcon: searchController.text.isNotEmpty
                      ? IconButton(
                          icon: const Icon(Icons.close_rounded, size: 18),
                          color: Colors.grey.shade600,
                          onPressed: () {
                            searchController.clear();
                          },
                        )
                      : null,
                  contentPadding: const EdgeInsets.symmetric(
                    horizontal: 14,
                    vertical: 12,
                  ),
                  border: InputBorder.none,
                  enabledBorder: InputBorder.none,
                  focusedBorder: InputBorder.none,
                ),
              ),
            ),
            const SizedBox(height: 18),
            const ARModuleBannerCard(
              driveUrl: AppLinks.practicumModuleDriveUrl,
            ),
            const SizedBox(height: 20),
            Expanded(
              child: ListView.builder(
                itemCount: filteredTopics.length,
                itemBuilder: (context, index) {
                  final topic = filteredTopics[index];
                  final displayTitle = topic.practicumTitle ?? topic.title;

                  return MenuCard(
                    icon: topic.icon,
                    title: displayTitle,
                    subtitle: "Buka visualisasi 3D AR ${topic.title}",
                    iconColor: topic.color,
                    onTap: () => _handleTopicTap(topic),
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }
}
