import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_widget_from_html/flutter_widget_from_html.dart';

import '../../../../core/models/chemistry_topic.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../data/models/material/material_model.dart';
import '../../../ar/presentation/screens/ar_menu_screen.dart';
import '../../../molecule/presentation/screens/molecule_screen.dart';
import '../../../quiz/presentation/screens/quiz_screen.dart';
import '../../data/repositories/materi_repository.dart';

/// Halaman Detail Konten Teori Pembelajaran Kimia
class MateriDetailScreen extends StatefulWidget {
  final MaterialModel? material;
  final ChemistryTopic? topic;

  const MateriDetailScreen({
    super.key,
    this.material,
    this.topic,
  }) : assert(
          material != null || topic != null,
          'Either material or topic must be provided',
        );

  @override
  State<MateriDetailScreen> createState() => _MateriDetailScreenState();
}

class _MateriDetailScreenState extends State<MateriDetailScreen> {
  MaterialModel? _detailedMaterial;
  bool _isLoadingContent = false;

  @override
  void initState() {
    super.initState();
    _detailedMaterial = widget.material;

    // Jika materi belum memiliki content lengkap, fetch detail dari API
    if (widget.material != null &&
        (widget.material!.content == null ||
            widget.material!.content!.isEmpty)) {
      _loadDetailContent();
    }
  }

  Future<void> _loadDetailContent() async {
    setState(() {
      _isLoadingContent = true;
    });

    try {
      final repository = context.read<MateriRepository>();
      final detail = await repository.getMaterialDetail(
        widget.material!.idMaterial,
      );
      if (mounted) {
        setState(() {
          _detailedMaterial = detail;
          _isLoadingContent = false;
        });
      }
    } catch (_) {
      if (mounted) {
        setState(() {
          _isLoadingContent = false;
        });
      }
    }
  }

  String _getTopicDescription(String title) {
    switch (title) {
      case "Ikatan Kimia":
        return "Ikatan kimia merupakan gaya tarik-menarik antar atom yang menyebabkan atom-atom bergabung membentuk molekul atau senyawa yang stabil.";
      case "Termokimia":
        return "Termokimia mempelajari perubahan energi panas yang terjadi selama reaksi kimia berlangsung.";
      case "Kalorimetri":
        return "Kalorimetri mempelajari cara mengukur jumlah kalor yang dilepas atau diserap suatu reaksi.";
      case "Laju Reaksi":
        return "Laju reaksi adalah besarnya kecepatan berlangsungnya suatu reaksi kimia. Laju reaksi dapat dinyatakan sebagai laju berkurangnya pereaksi (reaktan) atau bertambahnya produk per satuan waktu.";
      case "Asam Basa":
        return "Asam basa mempelajari sifat larutan berdasarkan teori Arrhenius, Bronsted-Lowry, dan Lewis.";
      case "Hidrolisis":
        return "Hidrolisis merupakan reaksi ion garam dengan air yang menghasilkan sifat asam ataupun basa.";
      case "Hasil Kali Kelarutan":
        return "Konsep Ksp digunakan untuk menentukan kelarutan suatu senyawa ionik.";
      case "Kesetimbangan Kimia":
        return "Kesetimbangan kimia terjadi ketika laju reaksi maju sama dengan laju reaksi balik.";
      case "Sel Volta":
        return "Sel Volta menghasilkan energi listrik melalui reaksi redoks spontan.";
      case "Elektrolisis":
        return "Elektrolisis menggunakan energi listrik untuk menjalankan reaksi redoks yang tidak spontan.";
      case "Hidrokarbon":
        return "Hidrokarbon merupakan senyawa yang hanya tersusun dari atom karbon dan hidrogen.";
      case "Daur Ulang Polimer":
        return "Daur ulang polimer bertujuan mengurangi limbah plastik dengan memanfaatkan kembali material polimer.";
      default:
        return "Materi Pembelajaran Kimia.";
    }
  }

  @override
  Widget build(BuildContext context) {
    final title = _detailedMaterial?.name ?? widget.topic?.title ?? 'Materi Kimia';
    final categoryName = _detailedMaterial?.category?.name;
    final description = _detailedMaterial?.description ??
        (widget.topic != null ? _getTopicDescription(widget.topic!.title) : '');
    final content = _detailedMaterial?.content;

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(
            Icons.arrow_back_ios_new_rounded,
            color: Colors.white,
            size: 20,
          ),
          onPressed: () => Navigator.of(context).pop(),
        ),
        title: Text(
          title,
          style: const TextStyle(
            color: Colors.white,
            fontSize: 18,
            fontWeight: FontWeight.bold,
          ),
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
        ),
        centerTitle: false,
      ),
      body: Stack(
        children: [
          /// Background Molekul halus
          Positioned(
            top: -40,
            right: -40,
            child: Opacity(
              opacity: 0.06,
              child: Image.asset(
                'assets/images/bg_molecule.png',
                width: 200,
              ),
            ),
          ),

          ListView(
            padding: const EdgeInsets.symmetric(horizontal: 22, vertical: 16),
            children: [
              // Header Card
              Container(
                padding: const EdgeInsets.all(20),
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(22),
                  gradient: const LinearGradient(
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                    colors: [
                      Color(0xFF38438A),
                      Color(0xFF222B60),
                    ],
                  ),
                  border: Border.all(
                    color: Colors.white.withValues(alpha: 0.15),
                  ),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    if (categoryName != null) ...[
                      Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 10,
                          vertical: 4,
                        ),
                        decoration: BoxDecoration(
                          color: AppColors.accent.withValues(alpha: 0.15),
                          borderRadius: BorderRadius.circular(10),
                          border: Border.all(
                            color: AppColors.accent.withValues(alpha: 0.3),
                          ),
                        ),
                        child: Text(
                          categoryName,
                          style: const TextStyle(
                            color: AppColors.accent,
                            fontSize: 11,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                      const SizedBox(height: 12),
                    ],
                    Text(
                      title,
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 22,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    if (description.isNotEmpty) ...[
                      const SizedBox(height: 10),
                      Text(
                        description,
                        style: TextStyle(
                          color: Colors.white.withValues(alpha: 0.8),
                          fontSize: 13.5,
                          height: 1.5,
                        ),
                      ),
                    ],
                  ],
                ),
              ),

              const SizedBox(height: 24),

              // Content / Penjelasan Teori
              const Text(
                'Penjelasan Materi',
                style: TextStyle(
                  color: Colors.white,
                  fontSize: 17,
                  fontWeight: FontWeight.bold,
                  letterSpacing: 0.3,
                ),
              ),

              const SizedBox(height: 12),

              Container(
                padding: const EdgeInsets.all(20),
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.06),
                  borderRadius: BorderRadius.circular(18),
                  border: Border.all(
                    color: Colors.white.withValues(alpha: 0.1),
                  ),
                ),
                child: _isLoadingContent
                    ? const Center(
                        child: Padding(
                          padding: EdgeInsets.symmetric(vertical: 24),
                          child: CircularProgressIndicator(
                            strokeWidth: 2.5,
                            color: AppColors.accent,
                          ),
                        ),
                      )
                    : _buildRichContent(content, description),
              ),

              const SizedBox(height: 30),

              // Menu Pembelajaran & Interaksi Cepat
              const Text(
                'Menu Pembelajaran Terkait',
                style: TextStyle(
                  color: Colors.white,
                  fontSize: 17,
                  fontWeight: FontWeight.bold,
                  letterSpacing: 0.3,
                ),
              ),

              const SizedBox(height: 16),

              ElevatedButton.icon(
                icon: const Icon(Icons.science_rounded),
                label: const Text('Lihat Detail Molekul 3D'),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.primary,
                  foregroundColor: Colors.white,
                  minimumSize: const Size.fromHeight(52),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(14),
                  ),
                  elevation: 4,
                ),
                onPressed: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (_) => const MoleculeScreen(),
                    ),
                  );
                },
              ),

              const SizedBox(height: 12),

              ElevatedButton.icon(
                icon: const Icon(Icons.view_in_ar_rounded),
                label: const Text('Mulai Virtual Lab AR'),
                style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xFF0288D1),
                  foregroundColor: Colors.white,
                  minimumSize: const Size.fromHeight(52),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(14),
                  ),
                  elevation: 4,
                ),
                onPressed: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (_) => const ARMenuScreen(),
                    ),
                  );
                },
              ),

              const SizedBox(height: 12),

              ElevatedButton.icon(
                icon: const Icon(Icons.quiz_rounded),
                label: const Text('Kerjakan Quiz Materi Ini'),
                style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xFF2E7D32),
                  foregroundColor: Colors.white,
                  minimumSize: const Size.fromHeight(52),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(14),
                  ),
                  elevation: 4,
                ),
                onPressed: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (_) => QuizScreen(
                        materialId: title,
                        materialName: title,
                      ),
                    ),
                  );
                },
              ),

              const SizedBox(height: 30),
            ],
          ),
        ],
      ),
    );
  }

  /// Merender konten HTML/Rich Text yang dihasilkan oleh editor website (TipTap)
  Widget _buildRichContent(String? content, String fallbackDescription) {
    final rawText = (content != null && content.trim().isNotEmpty)
        ? content
        : fallbackDescription;

    if (rawText.trim().isEmpty) {
      return Text(
        'Konten pembelajaran lengkap sedang disiapkan.',
        style: TextStyle(
          color: Colors.white.withValues(alpha: 0.65),
          fontSize: 14,
          fontStyle: FontStyle.italic,
        ),
      );
    }

    return HtmlWidget(
      rawText,
      baseUrl: Uri.parse('https://mvar.my.id'),
      textStyle: TextStyle(
        color: Colors.white.withValues(alpha: 0.88),
        fontSize: 14.5,
        height: 1.6,
      ),
      customStylesBuilder: (element) {
        if (element.localName == 'h1') {
          return {
            'color': '#4DD0E1',
            'font-size': '20px',
            'font-weight': 'bold',
            'margin-bottom': '10px',
          };
        }
        if (element.localName == 'h2') {
          return {
            'color': '#FFFFFF',
            'font-size': '18px',
            'font-weight': 'bold',
            'margin-top': '14px',
            'margin-bottom': '8px',
          };
        }
        if (element.localName == 'h3') {
          return {
            'color': '#4DD0E1',
            'font-size': '16px',
            'font-weight': '600',
            'margin-top': '12px',
            'margin-bottom': '6px',
          };
        }
        if (element.localName == 'p') {
          return {
            'margin-bottom': '10px',
          };
        }
        if (element.localName == 'blockquote') {
          return {
            'border-left': '3px solid #4DD0E1',
            'padding-left': '12px',
            'color': '#B0BEC5',
            'font-style': 'italic',
            'margin': '12px 0',
          };
        }
        if (element.localName == 'code') {
          return {
            'background-color': 'rgba(255, 255, 255, 0.1)',
            'padding': '2px 6px',
            'border-radius': '4px',
            'font-family': 'monospace',
            'color': '#80D8FF',
          };
        }
        if (element.localName == 'pre') {
          return {
            'background-color': 'rgba(0, 0, 0, 0.35)',
            'padding': '12px',
            'border-radius': '8px',
            'margin': '12px 0',
          };
        }
        if (element.localName == 'a') {
          return {
            'color': '#4DD0E1',
            'text-decoration': 'underline',
          };
        }
        if (element.localName == 'img') {
          return {
            'border-radius': '12px',
            'margin': '12px 0',
          };
        }
        return null;
      },
      renderMode: RenderMode.column,
    );
  }
}