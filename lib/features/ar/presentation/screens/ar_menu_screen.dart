import 'package:flutter/material.dart';

import '../../../../core/models/chemistry_topic.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../home/presentation/widgets/menu_card.dart';

import '../../services/unity_launcher_service.dart';
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
    final topicId = topic.unityScene ??
        topic.title.toLowerCase().trim().replaceAll(' ', '_');
    final displayTitle = topic.practicumTitle ?? topic.title;

    final isInstalled = await UnityLauncherService.isUnityInstalled();
    if (!mounted) return;

    if (isInstalled) {
      final success = await UnityLauncherService.launchUnityApp(
        topicId: topicId,
        topicTitle: displayTitle,
      );
      if (!success && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text("Gagal membuka aplikasi AR Unity"),
            backgroundColor: Colors.redAccent,
          ),
        );
      }
    } else {
      _showUnityNotInstalledDialog(topic, displayTitle);
    }
  }

  void _showUnityNotInstalledDialog(ChemistryTopic topic, String displayTitle) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: const Color(0xFF161E31),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(20),
          side: BorderSide(
            color: Colors.white.withValues(alpha: 0.1),
          ),
        ),
        title: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(10),
              decoration: BoxDecoration(
                color: AppColors.primary.withValues(alpha: 0.2),
                shape: BoxShape.circle,
              ),
              child: const Icon(
                Icons.view_in_ar_rounded,
                color: AppColors.primary,
                size: 26,
              ),
            ),
            const SizedBox(width: 12),
            const Expanded(
              child: Text(
                "Aplikasi AR Belum Terpasang",
                style: TextStyle(
                  color: Colors.white,
                  fontSize: 17,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
          ],
        ),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              "Untuk membuka visualisasi 3D AR '$displayTitle', aplikasi modul Unity AR perlu dipasang di HP Android ini terlebih dahulu.",
              style: const TextStyle(
                color: Colors.white70,
                fontSize: 14,
                height: 1.4,
              ),
            ),
            const SizedBox(height: 16),
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: Colors.white.withValues(alpha: 0.05),
                borderRadius: BorderRadius.circular(12),
                border: Border.all(
                  color: Colors.white.withValues(alpha: 0.08),
                ),
              ),
              child: const Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    "Langkah Pemasangan (Opsi 2):",
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 13,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  SizedBox(height: 6),
                  Text(
                    "1. Buka project Unity (unity/MVAR)\n"
                    "2. Build APK (Package: com.mvar.unityar)\n"
                    "3. Install file APK ke HP Android Anda",
                    style: TextStyle(
                      color: Colors.white60,
                      fontSize: 12,
                      height: 1.4,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.pop(ctx);
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (_) => ARUnityScreen(topic: topic),
                ),
              );
            },
            child: const Text(
              "Mode Internal (Eksperimental)",
              style: TextStyle(color: Colors.white38, fontSize: 12),
            ),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.primary,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
              ),
            ),
            onPressed: () => Navigator.pop(ctx),
            child: const Text(
              "Mengerti",
              style: TextStyle(
                color: Colors.white,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        ],
      ),
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
        title: const Text(
          "Augmented Reality",
        ),
        backgroundColor: Colors.transparent,
        elevation: 0,
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
            TextField(
              controller: searchController,
              style: const TextStyle(
                color: Colors.white,
              ),
              decoration: InputDecoration(
                hintText: "Cari materi Augmented Reality",
                hintStyle: TextStyle(
                  color: Colors.white.withValues(alpha: .5),
                ),
                prefixIcon: const Icon(
                  Icons.search,
                  color: Colors.white,
                ),
                filled: true,
                fillColor: Colors.white.withValues(alpha: .08),
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(18),
                  borderSide: BorderSide.none,
                ),
              ),
            ),
            const SizedBox(height: 25),
            Expanded(
              child: ListView.builder(
                itemCount: filteredTopics.length,
                itemBuilder: (context, index) {
                  final topic = filteredTopics[index];
                  final displayTitle =
                      topic.practicumTitle ?? topic.title;

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