import 'package:flutter/material.dart';

import '../../core/models/chemistry_topic.dart';
import '../../utils/app_colors.dart';
import '../../widgets/menu_card.dart';

import 'materi_detail_screen.dart';

class MateriScreen extends StatefulWidget {
  const MateriScreen({super.key});

  @override
  State<MateriScreen> createState() => _MateriScreenState();
}

class _MateriScreenState extends State<MateriScreen> {

  final TextEditingController searchController =
      TextEditingController();

  List<ChemistryTopic> filteredTopics =
      List.from(chemistryTopics);

  @override
  void initState() {
    super.initState();

    searchController.addListener(_searchTopic);
  }

  void _searchTopic() {

    final keyword =
        searchController.text.toLowerCase();

    setState(() {

      filteredTopics = chemistryTopics.where((topic) {

        return topic.title
                .toLowerCase()
                .contains(keyword) ||

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

        backgroundColor: Colors.transparent,

        elevation: 0,

        centerTitle: true,

        title: const Text(

          "Materi Pembelajaran",

          style: TextStyle(

            color: Colors.white,

            fontWeight: FontWeight.bold,

          ),

        ),

        iconTheme:
            const IconThemeData(color: Colors.white),

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

                hintText: "Cari materi...",

                hintStyle: TextStyle(

                  color: Colors.white.withValues(alpha: .55),

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

                itemBuilder: (context, index) {

                  final topic =
                      filteredTopics[index];
                      
                    return MenuCard(

                    icon: topic.icon,

                    title: topic.title,

                    subtitle: topic.subtitle,

                    iconColor: topic.color,

                    onTap: () {

                      Navigator.push(

                        context,

                        MaterialPageRoute(

                          builder: (_) =>
                              MateriDetailScreen(
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