import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../../core/theme/app_colors.dart';
import '../../data/repositories/quiz_repository.dart';
import '../bloc/quiz_category/quiz_category_bloc.dart';
import '../bloc/quiz_category/quiz_category_event.dart';
import '../bloc/quiz_category/quiz_category_state.dart';
import '../widgets/quiz_banner_card.dart';
import '../widgets/quiz_category_card.dart';
import '../widgets/quiz_result_bottom_sheet.dart';
import 'quiz_review_screen.dart';
import 'quiz_screen.dart';

/// Halaman Katalog Kategori Kuis & Latihan Soal Kimia (Desain konsisten dengan Materi & Molekul)
class QuizMaterialScreen extends StatelessWidget {
  const QuizMaterialScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) => QuizCategoryBloc(
        repository: context.read<QuizRepository>(),
      )..add(const QuizCategoryFetchRequested()),
      child: const _QuizMaterialView(),
    );
  }
}

class _QuizMaterialView extends StatefulWidget {
  const _QuizMaterialView();

  @override
  State<_QuizMaterialView> createState() => _QuizMaterialViewState();
}

class _QuizMaterialViewState extends State<_QuizMaterialView> {
  final TextEditingController _searchController = TextEditingController();
  bool _hasSearchText = false;

  @override
  void initState() {
    super.initState();
    _searchController.addListener(() {
      final hasText = _searchController.text.trim().isNotEmpty;
      if (hasText != _hasSearchText) {
        setState(() {
          _hasSearchText = hasText;
        });
      }
    });
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  void _onSearch(String query) {
    context
        .read<QuizCategoryBloc>()
        .add(QuizCategoryFetchRequested(search: query));
  }

  void _onClearSearch() {
    _searchController.clear();
    context
        .read<QuizCategoryBloc>()
        .add(const QuizCategoryFetchRequested(search: ''));
  }

  void _openQuiz(int idCategory, String categoryName) async {
    await Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) => QuizScreen(
          idCategory: idCategory,
          categoryName: categoryName,
        ),
      ),
    );

    // Refresh status pengerjaan kuis ketika kembali ke halaman katalog
    if (mounted) {
      context
          .read<QuizCategoryBloc>()
          .add(const QuizCategoryRefreshRequested());
    }
  }

  void _openReview(int idCategory, String categoryName) {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) => QuizReviewScreen(
          idCategory: idCategory,
          categoryName: categoryName,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: Stack(
        children: [
          // Background Molekul Kiri Atas
          Positioned(
            top: -60,
            left: -60,
            child: Opacity(
              opacity: 0.08,
              child: Image.asset(
                'assets/images/bg_molecule.png',
                width: 220,
              ),
            ),
          ),

          // Background Molekul Kanan Bawah
          Positioned(
            bottom: -60,
            right: -60,
            child: Opacity(
              opacity: 0.08,
              child: Transform.rotate(
                angle: 0.5,
                child: Image.asset(
                  'assets/images/bg_molecule.png',
                  width: 240,
                ),
              ),
            ),
          ),

          SafeArea(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Top Header
                Padding(
                  padding: const EdgeInsets.fromLTRB(14, 10, 20, 10),
                  child: Row(
                    children: [
                      IconButton(
                        onPressed: () => Navigator.of(context).pop(),
                        icon: const Icon(
                          Icons.arrow_back_ios_new_rounded,
                          color: Colors.white,
                          size: 20,
                        ),
                        splashRadius: 22,
                      ),
                      const SizedBox(width: 8),
                      const Text(
                        'Kuis STEM',
                        style: TextStyle(
                          color: Colors.white,
                          fontSize: 20,
                          fontWeight: FontWeight.bold,
                          letterSpacing: 0.2,
                        ),
                      ),
                    ],
                  ),
                ),

                // Main Content
                Expanded(
                  child: BlocConsumer<QuizCategoryBloc, QuizCategoryState>(
                    listener: (context, state) {
                      if (state is QuizCategoryFailure) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          SnackBar(
                            content: Text(state.message),
                            backgroundColor: Colors.redAccent.shade700,
                            behavior: SnackBarBehavior.floating,
                          ),
                        );
                      }
                    },
                    builder: (context, state) {
                      if (state is QuizCategoryLoading) {
                        return const Center(
                          child: CircularProgressIndicator(
                            color: AppColors.accent,
                          ),
                        );
                      }

                      if (state is QuizCategoryFailure &&
                          state is! QuizCategoryLoaded) {
                        return Center(
                          child: Padding(
                            padding: const EdgeInsets.all(24),
                            child: Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                const Icon(
                                  Icons.cloud_off_rounded,
                                  color: Colors.white54,
                                  size: 56,
                                ),
                                const SizedBox(height: 16),
                                Text(
                                  state.message,
                                  textAlign: TextAlign.center,
                                  style: const TextStyle(
                                    color: Colors.white70,
                                    fontSize: 14,
                                  ),
                                ),
                                const SizedBox(height: 20),
                                ElevatedButton.icon(
                                  onPressed: () => context
                                      .read<QuizCategoryBloc>()
                                      .add(const QuizCategoryFetchRequested()),
                                  icon: const Icon(Icons.refresh_rounded),
                                  label: const Text('Coba Lagi'),
                                  style: ElevatedButton.styleFrom(
                                    backgroundColor: AppColors.accent,
                                    foregroundColor: const Color(0xFF0F172A),
                                  ),
                                ),
                              ],
                            ),
                          ),
                        );
                      }

                      final loadedState = state is QuizCategoryLoaded
                          ? state
                          : const QuizCategoryLoaded(
                              categories: [],
                              completedCategoryIds: {},
                            );

                      final categories = loadedState.filteredCategories;

                      return RefreshIndicator(
                        color: AppColors.accent,
                        backgroundColor: const Color(0xFF1E2766),
                        onRefresh: () async {
                          context
                              .read<QuizCategoryBloc>()
                              .add(const QuizCategoryRefreshRequested());
                        },
                        child: ListView(
                          padding: const EdgeInsets.fromLTRB(20, 10, 20, 30),
                          children: [
                            // Search Bar
                            Row(
                              children: [
                                Expanded(
                                  child: Container(
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
                                      controller: _searchController,
                                      textInputAction: TextInputAction.search,
                                      onSubmitted: _onSearch,
                                      style: const TextStyle(
                                        color: Color(0xFF1E293B),
                                        fontSize: 14,
                                        fontWeight: FontWeight.w500,
                                      ),
                                      decoration: InputDecoration(
                                        hintText: 'Cari kategori kuis...',
                                        hintStyle: TextStyle(
                                          color: Colors.grey.shade500,
                                          fontSize: 14,
                                        ),
                                        prefixIcon: Icon(
                                          Icons.search_rounded,
                                          color: Colors.grey.shade600,
                                          size: 20,
                                        ),
                                        suffixIcon: _hasSearchText
                                            ? IconButton(
                                                icon: const Icon(
                                                  Icons.close_rounded,
                                                  size: 18,
                                                ),
                                                color: Colors.grey.shade600,
                                                splashRadius: 18,
                                                onPressed: _onClearSearch,
                                              )
                                            : null,
                                        contentPadding:
                                            const EdgeInsets.symmetric(
                                          horizontal: 14,
                                          vertical: 12,
                                        ),
                                        border: InputBorder.none,
                                        enabledBorder: InputBorder.none,
                                        focusedBorder: InputBorder.none,
                                      ),
                                    ),
                                  ),
                                ),
                                const SizedBox(width: 10),
                                SizedBox(
                                  height: 48,
                                  child: ElevatedButton(
                                    onPressed: () {
                                      FocusScope.of(context).unfocus();
                                      _onSearch(_searchController.text.trim());
                                    },
                                    style: ElevatedButton.styleFrom(
                                      backgroundColor: Colors.white,
                                      foregroundColor: const Color(0xFF0F172A),
                                      elevation: 4,
                                      shadowColor: Colors.black.withValues(alpha: 0.2),
                                      padding: const EdgeInsets.symmetric(
                                        horizontal: 20,
                                      ),
                                      shape: RoundedRectangleBorder(
                                        borderRadius:
                                            BorderRadius.circular(16),
                                      ),
                                    ),
                                    child: const Text(
                                      'Cari',
                                      style: TextStyle(
                                        fontSize: 14,
                                        fontWeight: FontWeight.bold,
                                        letterSpacing: 0.3,
                                      ),
                                    ),
                                  ),
                                ),
                              ],
                            ),

                            const SizedBox(height: 18),

                            // Hero Banner Card
                            QuizBannerCard(
                              completedCount:
                                  loadedState.completedCategoriesCount,
                              totalCount: loadedState.totalCategoriesCount,
                            ),

                            const SizedBox(height: 24),

                            // Subtitle Section
                            const Text(
                              'Pilih Materi Kuis',
                              style: TextStyle(
                                color: Colors.white,
                                fontSize: 18,
                                fontWeight: FontWeight.bold,
                                letterSpacing: 0.2,
                              ),
                            ),
                            const SizedBox(height: 4),
                            Text(
                              'Uji pemahamanmu setelah membaca materi kimia',
                              style: TextStyle(
                                color: Colors.white.withValues(alpha: 0.7),
                                fontSize: 12.5,
                              ),
                            ),

                            const SizedBox(height: 16),

                            // Daftar Kategori Soal
                            if (categories.isEmpty)
                              Padding(
                                padding:
                                    const EdgeInsets.symmetric(vertical: 40),
                                child: Center(
                                  child: Column(
                                    children: [
                                      Icon(
                                        Icons.quiz_outlined,
                                        color: Colors.white.withValues(alpha: 0.4),
                                        size: 48,
                                      ),
                                      const SizedBox(height: 12),
                                      Text(
                                        _searchController.text.isNotEmpty
                                            ? 'Tidak ada kategori kuis ditemukan'
                                            : 'Belum ada latihan kuis tersedia',
                                        style: TextStyle(
                                          color: Colors.white.withValues(alpha: 0.7),
                                          fontSize: 14,
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                              )
                            else
                              ...categories.asMap().entries.map((entry) {
                                final index = entry.key;
                                final cat = entry.value;
                                final isCompleted = loadedState
                                    .completedCategoryIds
                                    .contains(cat.idCategory);

                                return QuizCategoryCard(
                                  category: cat,
                                  index: index,
                                  isCompleted: isCompleted,
                                  onTap: () {
                                    if (isCompleted) {
                                      QuizResultBottomSheet.show(
                                        context: context,
                                        idCategory: cat.idCategory,
                                        categoryName: cat.name,
                                        onRetry: () => _openQuiz(
                                          cat.idCategory,
                                          cat.name,
                                        ),
                                        onReview: () => _openReview(
                                          cat.idCategory,
                                          cat.name,
                                        ),
                                      );
                                    } else {
                                      _openQuiz(cat.idCategory, cat.name);
                                    }
                                  },
                                );
                              }),
                          ],
                        ),
                      );
                    },
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}