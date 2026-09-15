import 'package:flutter/material.dart';

import '../../core/constants/app_colors.dart';
import '../../data/practicum/reaction_rate.dart';
import 'practicum_screen.dart';

class PracticumIntroScreen extends StatelessWidget {
  const PracticumIntroScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final practicum = reactionRatePracticum;

    return Scaffold(
      backgroundColor: AppColors.background,

      appBar: AppBar(
        backgroundColor: AppColors.background,
        elevation: 0,
        centerTitle: true,
        title: const Text(
          "Virtual Chemistry Laboratory",
          style: TextStyle(
            fontWeight: FontWeight.bold,
          ),
        ),
      ),

      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(20),

          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,

            children: [

              Center(
                child: Container(
                  width: 110,
                  height: 110,

                  decoration: BoxDecoration(
                    color: AppColors.primary.withOpacity(.08),
                    borderRadius: BorderRadius.circular(28),
                  ),

                  child: Icon(
                    Icons.science,
                    size: 60,
                    color: AppColors.primary,
                  ),
                ),
              ),

              const SizedBox(height: 28),

              Text(
                practicum.title,
                style: const TextStyle(
                  fontSize: 28,
                  fontWeight: FontWeight.bold,
                ),
              ),

              const SizedBox(height: 10),

              Text(
                practicum.description,
                style: const TextStyle(
                  fontSize: 15,
                  height: 1.6,
                  color: Colors.grey,
                ),
              ),

              const SizedBox(height: 30),

              _sectionTitle("Tujuan Praktikum"),

              const SizedBox(height: 10),

              _infoCard(
                icon: Icons.flag,
                text: practicum.objective,
              ),

              const SizedBox(height: 25),

              Row(
                children: [

                  Expanded(
                    child: _smallCard(
                      Icons.timer,
                      "${practicum.estimatedMinutes} Menit",
                    ),
                  ),

                  const SizedBox(width: 12),

                  Expanded(
                    child: _smallCard(
                      Icons.workspace_premium,
                      "Level ${practicum.difficulty}",
                    ),
                  ),

                ],
              ),

              const SizedBox(height: 30),

              _sectionTitle("Peralatan"),

              const SizedBox(height: 10),
              GridView.builder(

                shrinkWrap: true,

                physics: const NeverScrollableScrollPhysics(),

                itemCount: practicum.tools.length,

                gridDelegate:
                    const SliverGridDelegateWithFixedCrossAxisCount(
                  crossAxisCount: 2,
                  childAspectRatio: 2.9,
                  crossAxisSpacing: 10,
                  mainAxisSpacing: 10,
                ),

                itemBuilder: (context, index) {

                  final tool = practicum.tools[index];

                  return Container(

                    padding: const EdgeInsets.symmetric(
                      horizontal: 12,
                    ),

                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(16),

                      boxShadow: [

                        BoxShadow(
                          blurRadius: 10,
                          color: Colors.black.withOpacity(.05),
                        )

                      ],
                    ),

                    child: Row(

                      children: [

                        Icon(
                          Icons.science_outlined,
                          color: AppColors.primary,
                        ),

                        const SizedBox(width: 10),

                        Expanded(
                          child: Text(
                            tool.name,
                            style: const TextStyle(
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),

                      ],
                    ),
                  );
                },
              ),

              const SizedBox(height: 30),

              _sectionTitle("Bahan"),

              const SizedBox(height: 10),

              GridView.builder(

                shrinkWrap: true,

                physics: const NeverScrollableScrollPhysics(),

                itemCount: practicum.materials.length,

                gridDelegate:
                    const SliverGridDelegateWithFixedCrossAxisCount(
                  crossAxisCount: 2,
                  childAspectRatio: 2.9,
                  crossAxisSpacing: 10,
                  mainAxisSpacing: 10,
                ),

                itemBuilder: (context, index) {

                  final material = practicum.materials[index];

                  return Container(

                    padding: const EdgeInsets.symmetric(horizontal: 12),

                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(16),

                      boxShadow: [

                        BoxShadow(
                          blurRadius: 10,
                          color: Colors.black.withOpacity(.05),
                        )

                      ],
                    ),

                    child: Row(

                      children: [

                        Icon(
                          Icons.biotech,
                          color: Colors.orange,
                        ),

                        const SizedBox(width: 10),

                        Expanded(
                          child: Text(
                            material.name,
                            style: const TextStyle(
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),

                      ],
                    ),
                  );
                },
              ),
              const SizedBox(height: 35),

              SizedBox(

                width: double.infinity,

                height: 60,

                child: ElevatedButton(

                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppColors.primary,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(18),
                    ),
                  ),

                  onPressed: () {

                    Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (_) => const PracticumScreen(),
                      ),
                    );

                  },

                  child: const Text(
                    "Mulai Praktikum",
                    style: TextStyle(
                      fontSize: 18,
                      color: Colors.white,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
              ),

              const SizedBox(height: 40),

            ],
          ),
        ),
      ),
    );
  }

  Widget _sectionTitle(String title) {

    return Text(
      title,
      style: const TextStyle(
        fontSize: 19,
        fontWeight: FontWeight.bold,
      ),
    );
  }

  Widget _infoCard({

    required IconData icon,

    required String text,

  }) {

    return Container(

      padding: const EdgeInsets.all(16),

      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(18),
      ),

      child: Row(

        crossAxisAlignment: CrossAxisAlignment.start,

        children: [

          Icon(icon),

          const SizedBox(width: 12),

          Expanded(
            child: Text(
              text,
              style: const TextStyle(
                height: 1.6,
              ),
            ),
          )

        ],
      ),
    );
  }

  Widget _smallCard(
    IconData icon,
    String text,
  ) {

    return Container(

      height: 70,

      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(18),
      ),

      child: Row(

        mainAxisAlignment: MainAxisAlignment.center,

        children: [

          Icon(
            icon,
            color: AppColors.primary,
          ),

          const SizedBox(width: 10),

          Text(
            text,
            style: const TextStyle(
              fontWeight: FontWeight.bold,
            ),
          )

        ],
      ),
    );
  }
}