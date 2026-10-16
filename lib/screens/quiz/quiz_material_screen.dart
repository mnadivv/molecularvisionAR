import 'package:flutter/material.dart';

import '../../core/constants/app_colors.dart';
import 'quiz_screen.dart';

class QuizMaterialScreen extends StatelessWidget {
  const QuizMaterialScreen({
    super.key,
  });


  static const List<Map<String, String>> materials = [
    {
      'id': 'ikatan_kimia',
      'number': '01',
      'name': 'Ikatan Kimia',
    },
    {
      'id': 'termokimia',
      'number': '02',
      'name': 'Termokimia',
    },
    {
      'id': 'laju_reaksi',
      'number': '03',
      'name': 'Laju Reaksi',
    },
    {
      'id': 'stoikiometri',
      'number': '04',
      'name': 'Stokiometri',
    },
    {
      'id': 'elektrolisis_asam_basa',
      'number': '05',
      'name': 'Elektrolisis dan Asam Basa',
    },
    {
      'id': 'hidrolisis_garam',
      'number': '06',
      'name': 'Hidrolisis Garam',
    },
    {
      'id': 'hasil_kali_kel arutan',
      'number': '07',
      'name': 'Hasil Kali Kelarutan',
    },
    {
      'id': 'kesetimbangan_kimia',
      'number': '08',
      'name': 'Kesetimbangan Kimia',
    },
    {
      'id': 'sel_volta',
      'number': '09',
      'name': 'Sel Volta',
    },
    {
      'id': 'hidrokarbon',
      'number': '10',
      'name': 'Hidrokarbon',
    },
    {
      'id': 'daur_ulang_polimer',
      'number': '11',
      'name': 'Daur Ulang Polimer',
    },
  ];

  void openQuiz(
    BuildContext context,
    String materialId,
    String materialName,
  ) {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) => QuizScreen(
          materialId: materialId,
          materialName: materialName,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        elevation: 0,
        centerTitle: true,
        title: const Text(
          'Kuis STEM',
          style: TextStyle(
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
      body: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          Container(
            padding: const EdgeInsets.all(22),
            decoration: BoxDecoration(
              gradient: const LinearGradient(
                colors: [
                  AppColors.primary,
                  AppColors.secondary,
                ],
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
              ),
              borderRadius: BorderRadius.circular(24),
            ),
            child: const Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Icon(
                  Icons.quiz_rounded,
                  color: Colors.white,
                  size: 42,
                ),
                SizedBox(height: 16),
                Text(
                  'Kuis STEM',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 25,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                SizedBox(height: 8),
                Text(
                  'Pilih materi kimia yang ingin kamu kerjakan.',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 15,
                    height: 1.4,
                  ),
                ),
              ],
            ),
          ),

          const SizedBox(height: 24),

          const Text(
            'Pilih Materi',
            style: TextStyle(
              color: AppColors.textPrimary,
              fontSize: 21,
              fontWeight: FontWeight.bold,
            ),
          ),

          const SizedBox(height: 6),

          const Text(
            'Semua materi dapat dipilih.',
            style: TextStyle(
              color: AppColors.textSecondary,
            ),
          ),

          const SizedBox(height: 16),

          ...materials.map(
            (material) {
              final id = material['id']!;
              final number = material['number']!;
              final name = material['name']!;

              final isAvailable =
                  id == 'laju_reaksi';

              return Padding(
                padding: const EdgeInsets.only(
                  bottom: 12,
                ),
                child: Material(
                  color: AppColors.surface,
                  borderRadius:
                      BorderRadius.circular(18),
                  child: InkWell(
                    borderRadius:
                        BorderRadius.circular(18),
                    onTap: () {
                      openQuiz(
                        context,
                        id,
                        name,
                      );
                    },
                    child: Container(
                      padding:
                          const EdgeInsets.all(16),
                      decoration: BoxDecoration(
                        borderRadius:
                            BorderRadius.circular(18),
                        border: Border.all(
                          color: isAvailable
                              ? AppColors.primary
                                  .withOpacity(0.25)
                              : AppColors.border,
                        ),
                      ),
                      child: Row(
                        children: [
                          Container(
                            width: 50,
                            height: 50,
                            decoration:
                                BoxDecoration(
                              color: isAvailable
                                  ? AppColors.primary
                                      .withOpacity(0.1)
                                  : AppColors.background,
                              borderRadius:
                                  BorderRadius.circular(
                                14,
                              ),
                            ),
                            alignment:
                                Alignment.center,
                            child: Text(
                              number,
                              style: TextStyle(
                                color: isAvailable
                                    ? AppColors.primary
                                    : AppColors
                                        .textSecondary,
                                fontWeight:
                                    FontWeight.bold,
                              ),
                            ),
                          ),

                          const SizedBox(width: 14),

                          Expanded(
                            child: Column(
                              crossAxisAlignment:
                                  CrossAxisAlignment.start,
                              children: [
                                Text(
                                  name,
                                  style:
                                      const TextStyle(
                                    color: AppColors
                                        .textPrimary,
                                    fontSize: 16,
                                    fontWeight:
                                        FontWeight.bold,
                                  ),
                                ),
                                const SizedBox(height: 4),
                                Text(
                                  isAvailable
                                      ? 'Kuis tersedia'
                                      : 'Kuis dapat dipilih',
                                  style:
                                      const TextStyle(
                                    color: AppColors
                                        .textSecondary,
                                    fontSize: 13,
                                  ),
                                ),
                              ],
                            ),
                          ),

                          const Icon(
                            Icons.arrow_forward_ios_rounded,
                            size: 17,
                            color:
                                AppColors.textSecondary,
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
              );
            },
          ),
        ],
      ),
    );
  }
}