import 'package:flutter/material.dart';

import '../../core/models/chemistry_topic.dart';
import '../../utils/app_colors.dart';
import '../../widgets/menu_card.dart';

import 'ar_unity_screen.dart';

class ARMenuScreen extends StatefulWidget {
  const ARMenuScreen({super.key});

  @override
  State<ARMenuScreen> createState() => _ARMenuScreenState();
}

class _ARMenuScreenState extends State<ARMenuScreen> {

  final TextEditingController searchController =
      TextEditingController();

  List<ChemistryTopic> filteredTopics =
      List.from(chemistryTopics);

  @override
  void initState() {
    super.initState();

    searchController.addListener(searchTopic);
  }

  void searchTopic() {

    final keyword =
        searchController.text.toLowerCase();

    setState(() {

      filteredTopics = chemistryTopics.where((topic) {

        return topic.title
                .toLowerCase()
                .contains(keyword) ||

            (topic.practicumTitle != null &&
                topic.practicumTitle!
                    .toLowerCase()
                    .contains(keyword)) ||

            topic.subtitle
                .toLowerCase()
                .contains(keyword);

      }).toList();

    });

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

        iconTheme:
            const IconThemeData(color: Colors.white),

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

                hintText:
                    "Cari materi Augmented Reality",

                hintStyle: TextStyle(

                  color:
                      Colors.white.withValues(alpha: .5),

                ),

                prefixIcon: const Icon(

                  Icons.search,

                  color: Colors.white,

                ),

                filled: true,

                fillColor:
                    Colors.white.withValues(alpha: .08),

                border: OutlineInputBorder(

                  borderRadius:
                      BorderRadius.circular(18),

                  borderSide: BorderSide.none,

                ),

              ),

            ),

            const SizedBox(height: 25),

            Expanded(

              child: ListView.builder(

                itemCount: filteredTopics.length,

                itemBuilder: (context,index){

                  final topic =
                      filteredTopics[index];
                  final displayTitle =
                      topic.practicumTitle ?? topic.title;

                  return MenuCard(

                    icon: topic.icon,

                    title: displayTitle,

                    subtitle:
                        "Buka visualisasi 3D AR ${topic.title}",

                    iconColor: topic.color,

                    onTap: () {

                      Navigator.push(

                        context,

                        MaterialPageRoute(

                          builder: (_) =>
                              ARUnityScreen(
                            topic: topic,
                          ),

                        ),

                      );

                    },

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