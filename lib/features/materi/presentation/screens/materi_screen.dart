import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../../core/theme/app_colors.dart';
import '../../data/repositories/materi_repository.dart';
import '../bloc/materi_bloc.dart';
import '../bloc/materi_event.dart';
import '../bloc/materi_state.dart';
import '../widgets/materi_banner_card.dart';
import '../widgets/materi_category_chips.dart';
import '../widgets/materi_list_card.dart';
import '../widgets/materi_search_bar.dart';
import 'materi_detail_screen.dart';

/// Halaman Daftar Materi Kimia persis sesuai desain Figma (node-id=65-123)
class MateriScreen extends StatelessWidget {
  const MateriScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) => MateriBloc(
        repository: context.read<MateriRepository>(),
      )..add(const MateriFetchRequested()),
      child: const _MateriView(),
    );
  }
}

class _MateriView extends StatefulWidget {
  const _MateriView();

  @override
  State<_MateriView> createState() => _MateriViewState();
}

class _MateriViewState extends State<_MateriView> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: Stack(
        children: [
          /// Background Molekul Kiri Atas
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

          /// Background Molekul Kanan Bawah
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
                /// Custom Top Header (Back + Title)
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
                        'Materi Kimia',
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

                /// Main Content with BlocConsumer & Pull-To-Refresh
                Expanded(
                  child: BlocConsumer<MateriBloc, MateriState>(
                    listener: (context, state) {
                      if (state is MateriFailure) {
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
                      if (state is MateriLoading) {
                        return const Center(
                          child: CircularProgressIndicator(
                            color: AppColors.accent,
                          ),
                        );
                      }

                      if (state is MateriFailure && state is! MateriLoaded) {
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
                                  style: ElevatedButton.styleFrom(
                                    backgroundColor: AppColors.primary,
                                    shape: RoundedRectangleBorder(
                                      borderRadius: BorderRadius.circular(12),
                                    ),
                                  ),
                                  onPressed: () {
                                    context.read<MateriBloc>().add(
                                          const MateriFetchRequested(),
                                        );
                                  },
                                  icon: const Icon(
                                    Icons.refresh_rounded,
                                    color: Colors.white,
                                  ),
                                  label: const Text(
                                    'Coba Lagi',
                                    style: TextStyle(
                                      color: Colors.white,
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                                ),
                              ],
                            ),
                          ),
                        );
                      }

                      if (state is MateriLoaded) {
                        return RefreshIndicator(
                          color: AppColors.accent,
                          backgroundColor: AppColors.surface,
                          onRefresh: () async {
                            final bloc = context.read<MateriBloc>();
                            bloc.add(const MateriRefreshRequested());
                            await bloc.stream.firstWhere(
                              (s) => s is! MateriLoading,
                            );
                          },
                          child: ListView(
                            physics: const AlwaysScrollableScrollPhysics(
                              parent: BouncingScrollPhysics(),
                            ),
                            padding: const EdgeInsets.fromLTRB(20, 10, 20, 30),
                            children: [
                              /// Search Bar
                              MateriSearchBar(
                                initialValue: state.searchQuery,
                                onSearch: (query) {
                                  context.read<MateriBloc>().add(
                                        MateriSearchSubmitted(query),
                                      );
                                },
                                onClear: () {
                                  context.read<MateriBloc>().add(
                                        const MateriSearchSubmitted(''),
                                      );
                                },
                              ),

                              const SizedBox(height: 20),

                              /// Hero Banner
                              const MateriBannerCard(),

                              const SizedBox(height: 24),

                              /// Kategori Materi Horizontal Chips
                              MateriCategoryChips(
                                categories: state.categories,
                                selectedCategoryId: state.selectedCategoryId,
                                onCategorySelected: (idCategory) {
                                  context.read<MateriBloc>().add(
                                        MateriCategorySelected(idCategory),
                                      );
                                },
                              ),

                              const SizedBox(height: 26),

                              /// Section Header: Daftar Materi
                              Row(
                                mainAxisAlignment:
                                    MainAxisAlignment.spaceBetween,
                                children: [
                                  const Text(
                                    'Daftar Materi',
                                    style: TextStyle(
                                      color: Colors.white,
                                      fontSize: 16,
                                      fontWeight: FontWeight.bold,
                                      letterSpacing: 0.3,
                                    ),
                                  ),
                                  if (state.isFiltering)
                                    const SizedBox(
                                      width: 16,
                                      height: 16,
                                      child: CircularProgressIndicator(
                                        strokeWidth: 2,
                                        color: AppColors.accent,
                                      ),
                                    )
                                  else
                                    Text(
                                      '${state.materials.length} Materi',
                                      style: TextStyle(
                                        color:
                                            Colors.white.withValues(alpha: 0.6),
                                        fontSize: 12,
                                      ),
                                    ),
                                ],
                              ),

                              const SizedBox(height: 14),

                              /// List of Materials or Empty State
                              if (state.materials.isEmpty) ...[
                                Container(
                                  padding: const EdgeInsets.symmetric(
                                    horizontal: 20,
                                    vertical: 36,
                                  ),
                                  decoration: BoxDecoration(
                                    color: Colors.white.withValues(alpha: 0.05),
                                    borderRadius: BorderRadius.circular(18),
                                    border: Border.all(
                                      color:
                                          Colors.white.withValues(alpha: 0.08),
                                    ),
                                  ),
                                  child: Column(
                                    children: [
                                      Icon(
                                        Icons.search_off_rounded,
                                        size: 48,
                                        color: Colors.white
                                            .withValues(alpha: 0.4),
                                      ),
                                      const SizedBox(height: 12),
                                      const Text(
                                        'Materi tidak ditemukan',
                                        style: TextStyle(
                                          color: Colors.white,
                                          fontSize: 15,
                                          fontWeight: FontWeight.bold,
                                        ),
                                      ),
                                      const SizedBox(height: 6),
                                      Text(
                                        state.searchQuery.isNotEmpty
                                            ? 'Tidak ada materi kimia yang cocok dengan kata kunci "${state.searchQuery}".'
                                            : 'Belum ada materi untuk kategori ini.',
                                        textAlign: TextAlign.center,
                                        style: TextStyle(
                                          color: Colors.white
                                              .withValues(alpha: 0.65),
                                          fontSize: 12,
                                          height: 1.4,
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                              ] else ...[
                                ListView.builder(
                                  shrinkWrap: true,
                                  physics: const NeverScrollableScrollPhysics(),
                                  itemCount: state.materials.length,
                                  itemBuilder: (context, index) {
                                    final material = state.materials[index];
                                    final isCompleted = state.completedMaterialIds
                                        .contains(material.idMaterial);
                                    return MateriListCard(
                                      material: material,
                                      index: index,
                                      isCompleted: isCompleted,
                                      onTap: () {
                                        Navigator.push(
                                          context,
                                          MaterialPageRoute(
                                            builder: (_) => BlocProvider.value(
                                              value: context.read<MateriBloc>(),
                                              child: MateriDetailScreen(
                                                material: material,
                                              ),
                                            ),
                                          ),
                                        );
                                      },
                                    );
                                  },
                                ),
                              ],
                            ],
                          ),
                        );
                      }

                      return const SizedBox.shrink();
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