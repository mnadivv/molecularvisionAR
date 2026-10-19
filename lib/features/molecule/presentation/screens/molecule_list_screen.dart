import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../../core/theme/app_colors.dart';
import '../../data/repositories/molecule_repository.dart';
import '../bloc/molecule_bloc.dart';
import '../bloc/molecule_event.dart';
import '../bloc/molecule_state.dart';
import '../widgets/molecule_banner_card.dart';
import '../widgets/molecule_card.dart';
import '../widgets/molecule_search_bar.dart';
import '../widgets/molecule_shape_chips.dart';
import 'molecule_detail_screen.dart';

/// Halaman Daftar Molekul Kimia 3D persis sesuai desain sistem modul Materi
class MoleculeListScreen extends StatelessWidget {
  const MoleculeListScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) => MoleculeBloc(
        repository: context.read<MoleculeRepository>(),
      )..add(const MoleculeFetchRequested()),
      child: const _MoleculeListView(),
    );
  }
}

class _MoleculeListView extends StatefulWidget {
  const _MoleculeListView();

  @override
  State<_MoleculeListView> createState() => _MoleculeListViewState();
}

class _MoleculeListViewState extends State<_MoleculeListView> {
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
                        'Daftar Molekul',
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
                  child: BlocConsumer<MoleculeBloc, MoleculeState>(
                    listener: (context, state) {
                      if (state is MoleculeFailure) {
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
                      if (state is MoleculeLoading) {
                        return const Center(
                          child: CircularProgressIndicator(
                            color: AppColors.accent,
                          ),
                        );
                      }

                      if (state is MoleculeFailure && state is! MoleculeLoaded) {
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
                                    context.read<MoleculeBloc>().add(
                                          const MoleculeFetchRequested(),
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

                      if (state is MoleculeLoaded) {
                        return RefreshIndicator(
                          color: AppColors.accent,
                          backgroundColor: AppColors.surface,
                          onRefresh: () async {
                            final bloc = context.read<MoleculeBloc>();
                            bloc.add(const MoleculeRefreshRequested());
                            await bloc.stream.firstWhere(
                              (s) => s is! MoleculeLoading,
                            );
                          },
                          child: ListView(
                            physics: const AlwaysScrollableScrollPhysics(
                              parent: BouncingScrollPhysics(),
                            ),
                            padding: const EdgeInsets.fromLTRB(20, 10, 20, 30),
                            children: [
                              /// 1. Search Bar
                              MoleculeSearchBar(
                                initialValue: state.searchQuery,
                                onSearch: (query) {
                                  context.read<MoleculeBloc>().add(
                                        MoleculeSearchSubmitted(query),
                                      );
                                },
                                onClear: () {
                                  context.read<MoleculeBloc>().add(
                                        const MoleculeSearchSubmitted(''),
                                      );
                                },
                              ),

                              const SizedBox(height: 20),

                              /// 2. Card di bawah search (Banner Card Molekul)
                              MoleculeBannerCard(
                                completedCount: state.completedMoleculesCount,
                                totalCount: state.totalMoleculesCount,
                              ),

                              const SizedBox(height: 24),

                              /// 3. Sub Judul Kategori & 4. Kategori Filter Chips
                              MoleculeShapeChips(
                                shapes: state.availableShapes,
                                selectedShape: state.selectedShape,
                                onShapeSelected: (shape) {
                                  context.read<MoleculeBloc>().add(
                                        MoleculeShapeSelected(shape),
                                      );
                                },
                              ),

                              const SizedBox(height: 26),

                              /// 5. Sub Judul untuk Daftar Molekul
                              Row(
                                mainAxisAlignment:
                                    MainAxisAlignment.spaceBetween,
                                children: [
                                  const Text(
                                    'Daftar Molekul',
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
                                    ),
                                ],
                              ),

                              const SizedBox(height: 14),

                              /// 6. Card Molekul
                              if (state.molecules.isEmpty) ...[
                                Container(
                                  padding: const EdgeInsets.symmetric(
                                    vertical: 36,
                                    horizontal: 20,
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
                                      const Icon(
                                        Icons.science_outlined,
                                        color: Colors.white38,
                                        size: 44,
                                      ),
                                      const SizedBox(height: 12),
                                      const Text(
                                        'Molekul Tidak Ditemukan',
                                        style: TextStyle(
                                          color: Colors.white,
                                          fontSize: 15,
                                          fontWeight: FontWeight.bold,
                                        ),
                                      ),
                                      const SizedBox(height: 6),
                                      Text(
                                        state.searchQuery.isNotEmpty
                                            ? 'Tidak ada molekul yang cocok dengan kata kunci "${state.searchQuery}".'
                                            : 'Belum ada molekul untuk kategori bentuk ini.',
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
                                  physics:
                                      const NeverScrollableScrollPhysics(),
                                  itemCount: state.molecules.length,
                                  itemBuilder: (context, index) {
                                    final molecule = state.molecules[index];
                                    final isCompleted = state
                                        .completedMoleculeIds
                                        .contains(molecule.idMolecule);

                                    return MoleculeCard(
                                      molecule: molecule,
                                      isCompleted: isCompleted,
                                      onTap: () {
                                        Navigator.of(context).push(
                                          MaterialPageRoute(
                                            builder: (_) => BlocProvider.value(
                                              value:
                                                  context.read<MoleculeBloc>(),
                                              child: MoleculeDetailScreen(
                                                molecule: molecule,
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
