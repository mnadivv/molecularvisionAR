import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../../core/services/permission_service.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../core/widgets/camera_permission_dialog.dart';
import '../../../ar/presentation/screens/ar_menu_screen.dart';
import '../../../auth/presentation/bloc/auth_bloc.dart';
import '../../../auth/presentation/bloc/auth_event.dart';
import '../../../auth/presentation/bloc/auth_state.dart';
import '../../../auth/presentation/screens/login_screen.dart';
import '../../../materi/presentation/screens/materi_screen.dart';
import '../../../molecule/presentation/screens/molecule_list_screen.dart';
import '../../../profile/presentation/screens/profile_screen.dart';
import '../../../quiz/presentation/screens/quiz_material_screen.dart';
import '../../../../data/repositories/progress_repository.dart';
import '../bloc/progress/progress_bloc.dart';
import '../bloc/progress/progress_event.dart';
import '../bloc/progress/progress_state.dart';
import '../widgets/home_progress_card.dart';
import '../widgets/menu_card.dart';

class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (ctx) => ProgressBloc(
        repository: ctx.read<ProgressRepository>(),
      )..add(const ProgressFetchRequested()),
      child: const _HomeView(),
    );
  }
}

class _HomeView extends StatefulWidget {
  const _HomeView();

  @override
  State<_HomeView> createState() => _HomeViewState();
}

class _HomeViewState extends State<_HomeView> with TickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _fade;
  late Animation<Offset> _slide;
  late int _factIndex;

  final List<String> chemistryFacts = [
    "Air (H₂O) memiliki sudut ikatan sekitar 104,5°.",
    "Grafit dapat menghantarkan listrik karena memiliki elektron bebas.",
    "Intan dan grafit tersusun dari unsur karbon yang sama.",
    "Asam kuat terionisasi sempurna di dalam air.",
    "Reaksi eksoterm melepaskan kalor ke lingkungan.",
    "Katalis mempercepat reaksi tanpa ikut habis.",
    "Atom selalu berusaha mencapai konfigurasi elektron stabil.",
    "Ikatan kovalen terbentuk karena penggunaan pasangan elektron bersama.",
    "Larutan buffer menjaga pH tetap stabil.",
    "Elektrolisis mengubah energi listrik menjadi energi kimia.",
  ];

  @override
  void initState() {
    super.initState();
    _factIndex = DateTime.now().day % chemistryFacts.length;

    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 900),
    );

    _fade = CurvedAnimation(parent: _controller, curve: Curves.easeIn);

    _slide = Tween<Offset>(
      begin: const Offset(0, .12),
      end: Offset.zero,
    ).animate(CurvedAnimation(parent: _controller, curve: Curves.easeOut));

    _controller.forward();

    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      final authBloc = context.read<AuthBloc>();
      if (authBloc.state is! Authenticated) {
        authBloc.add(const AuthCheckRequested());
      }
    });
  }

  Future<void> _handleRefresh() async {
    final authBloc = context.read<AuthBloc>();
    authBloc.add(const AuthCheckRequested());
    context.read<ProgressBloc>().add(const ProgressRefreshRequested());

    setState(() {
      _factIndex = (_factIndex + 1) % chemistryFacts.length;
    });

    try {
      await authBloc.stream
          .firstWhere((s) => s is Authenticated || s is Unauthenticated)
          .timeout(const Duration(seconds: 4));
    } catch (_) {
      // Refresh gracefully completes even on network timeout
    }
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  Future<void> _navigateToARMenu() async {
    final hasPermission = await PermissionService.hasCameraPermission();
    if (!mounted) return;

    if (hasPermission) {
      Navigator.push(
        context,
        MaterialPageRoute(builder: (_) => const ARMenuScreen()),
      );
    } else {
      await CameraPermissionDialog.show(
        context,
        onGranted: () {
          Navigator.push(
            context,
            MaterialPageRoute(builder: (_) => const ARMenuScreen()),
          );
        },
      );
    }
  }

  void _confirmLogout() {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: const Color(0xFF0C2454),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        title: const Row(
          children: [
            Icon(Icons.logout_rounded, color: Colors.redAccent),
            SizedBox(width: 10),
            Text(
              'Keluar Akun',
              style: TextStyle(
                color: Colors.white,
                fontSize: 18,
                fontWeight: FontWeight.bold,
              ),
            ),
          ],
        ),
        content: const Text(
          'Apakah Anda yakin ingin keluar dari sesi akun siswa ini?',
          style: TextStyle(color: Colors.white70, fontSize: 14),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(),
            child: const Text('Batal', style: TextStyle(color: Colors.white60)),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(
              backgroundColor: Colors.redAccent,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(10),
              ),
            ),
            onPressed: () {
              Navigator.of(ctx).pop();
              context.read<AuthBloc>().add(const AuthLogoutRequested());
            },
            child: const Text(
              'Keluar',
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

  void _showAbout() {
    showModalBottomSheet(
      context: context,
      backgroundColor: AppColors.background,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
      ),
      builder: (_) {
        return Padding(
          padding: const EdgeInsets.all(25),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Image.asset("assets/images/logo_circle.png", width: 80),

              const SizedBox(height: 16),

              const Text(
                "Molecular Vision AR",
                style: TextStyle(
                  color: Colors.white,
                  fontSize: 22,
                  fontWeight: FontWeight.bold,
                ),
              ),

              const SizedBox(height: 8),

              Text("Version 1.0.0", style: TextStyle(color: Colors.white70)),

              const SizedBox(height: 20),

              const Divider(),

              const SizedBox(height: 20),

              const ListTile(
                leading: Icon(Icons.school, color: Colors.white),
                title: Text(
                  "Universitas Jember",
                  style: TextStyle(color: Colors.white),
                ),
              ),

              const ListTile(
                leading: Icon(Icons.emoji_events, color: Colors.white),
                title: Text("LIDM 2026", style: TextStyle(color: Colors.white)),
              ),

              const SizedBox(height: 20),
            ],
          ),
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    final fact = chemistryFacts[_factIndex % chemistryFacts.length];

    return Scaffold(
      backgroundColor: AppColors.background,
      body: BlocListener<AuthBloc, AuthState>(
        listener: (context, state) {
          if (state is Unauthenticated) {
            Navigator.of(context).pushAndRemoveUntil(
              MaterialPageRoute(builder: (_) => const LoginScreen()),
              (route) => false,
            );
          }
        },
        child: Stack(
          children: [
            /// Background Molekul Kiri Atas
            Positioned(
              top: -70,
              left: -60,
              child: Opacity(
                opacity: 0.08,
                child: Image.asset("assets/images/bg_molecule.png", width: 220),
              ),
            ),

            /// Background Molekul Kanan Bawah
            Positioned(
              bottom: -80,
              right: -70,
              child: Opacity(
                opacity: 0.08,
                child: Transform.rotate(
                  angle: 0.5,
                  child: Image.asset(
                    "assets/images/bg_molecule.png",
                    width: 250,
                  ),
                ),
              ),
            ),

            SafeArea(
              child: FadeTransition(
                opacity: _fade,
                child: SlideTransition(
                  position: _slide,
                  child: RefreshIndicator(
                    color: AppColors.accent,
                    backgroundColor: AppColors.surface,
                    displacement: 30,
                    strokeWidth: 2.5,
                    onRefresh: _handleRefresh,
                    child: ListView(
                      physics: const AlwaysScrollableScrollPhysics(
                        parent: BouncingScrollPhysics(),
                      ),
                      padding: const EdgeInsets.fromLTRB(22, 10, 22, 30),
                      children: [
                        /// Top Bar (User Profile & Actions)
                        BlocBuilder<AuthBloc, AuthState>(
                          builder: (context, authState) {
                            final user = authState is Authenticated
                                ? authState.user
                                : null;
                            final userName =
                                (user?.name != null &&
                                    user!.name.trim().isNotEmpty)
                                ? user.name
                                : 'Siswa MVAR';
                            final subtitleParts = <String>[];
                            if (user?.degree?.name != null &&
                                user!.degree!.name.trim().isNotEmpty) {
                              subtitleParts.add(user.degree!.name);
                            }
                            if (user?.school != null &&
                                user!.school!.trim().isNotEmpty) {
                              subtitleParts.add(user.school!);
                            }
                            final userSubtitle = subtitleParts.isNotEmpty
                                ? subtitleParts.join(' • ')
                                : 'Siswa • MVAR Chemistry';

                            return Container(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 14,
                                vertical: 10,
                              ),
                              margin: const EdgeInsets.only(bottom: 16),
                              decoration: BoxDecoration(
                                borderRadius: BorderRadius.circular(20),
                                gradient: const LinearGradient(
                                  begin: Alignment.topLeft,
                                  end: Alignment.bottomRight,
                                  colors: [
                                    Color(0xFF38438A),
                                    Color(0xFF252F6B),
                                  ],
                                ),
                                border: Border.all(
                                  color: Colors.white.withValues(alpha: 0.16),
                                  width: 1.2,
                                ),
                                boxShadow: [
                                  BoxShadow(
                                    color: Colors.black.withValues(alpha: 0.22),
                                    blurRadius: 14,
                                    offset: const Offset(0, 6),
                                  ),
                                ],
                              ),
                              child: Row(
                                children: [
                                  Expanded(
                                    child: InkWell(
                                      borderRadius: BorderRadius.circular(14),
                                      onTap: () async {
                                        await Navigator.push(
                                          context,
                                          MaterialPageRoute(
                                            builder: (_) =>
                                                const ProfileScreen(),
                                          ),
                                        );
                                        if (context.mounted) {
                                          context.read<AuthBloc>().add(
                                            const AuthCheckRequested(),
                                          );
                                        }
                                      },
                                      child: Row(
                                        children: [
                                          Expanded(
                                            child: Column(
                                              crossAxisAlignment:
                                                  CrossAxisAlignment.start,
                                              children: [
                                                Text(
                                                  userName,
                                                  style: const TextStyle(
                                                    color: Colors.white,
                                                    fontSize: 14.5,
                                                    fontWeight: FontWeight.bold,
                                                    letterSpacing: 0.2,
                                                  ),
                                                  maxLines: 1,
                                                  overflow:
                                                      TextOverflow.ellipsis,
                                                ),
                                                const SizedBox(height: 2),
                                                Text(
                                                  userSubtitle,
                                                  style: TextStyle(
                                                    color: Colors.white
                                                        .withValues(alpha: 0.7),
                                                    fontSize: 11,
                                                  ),
                                                  maxLines: 1,
                                                  overflow:
                                                      TextOverflow.ellipsis,
                                                ),
                                              ],
                                            ),
                                          ),
                                        ],
                                      ),
                                    ),
                                  ),
                                  SizedBox(width: 10),
                                  Container(
                                    padding: const EdgeInsets.symmetric(
                                      horizontal: 4,
                                      vertical: 2,
                                    ),
                                    decoration: BoxDecoration(
                                      color: Colors.white.withValues(
                                        alpha: 0.08,
                                      ),
                                      borderRadius: BorderRadius.circular(14),
                                    ),
                                    child: Row(
                                      mainAxisSize: MainAxisSize.min,
                                      children: [
                                        IconButton(
                                          tooltip: 'Profil & Pengaturan',
                                          onPressed: () async {
                                            await Navigator.push(
                                              context,
                                              MaterialPageRoute(
                                                builder: (_) =>
                                                    const ProfileScreen(),
                                              ),
                                            );
                                            if (context.mounted) {
                                              context.read<AuthBloc>().add(
                                                const AuthCheckRequested(),
                                              );
                                            }
                                          },
                                          splashRadius: 18,
                                          constraints: const BoxConstraints(
                                            minWidth: 36,
                                            minHeight: 36,
                                          ),
                                          padding: EdgeInsets.zero,
                                          icon: const Icon(
                                            Icons.account_circle_outlined,
                                            color: AppColors.accent,
                                            size: 21,
                                          ),
                                        ),
                                        IconButton(
                                          tooltip: 'Tentang Aplikasi',
                                          onPressed: _showAbout,
                                          splashRadius: 18,
                                          constraints: const BoxConstraints(
                                            minWidth: 36,
                                            minHeight: 36,
                                          ),
                                          padding: EdgeInsets.zero,
                                          icon: const Icon(
                                            Icons.info_outline_rounded,
                                            color: Colors.white70,
                                            size: 21,
                                          ),
                                        ),
                                        IconButton(
                                          tooltip: 'Keluar',
                                          onPressed: _confirmLogout,
                                          splashRadius: 18,
                                          constraints: const BoxConstraints(
                                            minWidth: 36,
                                            minHeight: 36,
                                          ),
                                          padding: EdgeInsets.zero,
                                          icon: const Icon(
                                            Icons.logout_rounded,
                                            color: Colors.redAccent,
                                            size: 21,
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                ],
                              ),
                            );
                          },
                        ),

                        const SizedBox(height: 50),

                        /// Logo Circle
                        Hero(
                          tag: "logo",
                          child: Center(
                            child: Image.asset(
                              "assets/images/logo_circle.png",
                              width: 110,
                            ),
                          ),
                        ),

                        const SizedBox(height: 18),

                        /// Logo Text
                        Center(
                          child: Image.asset(
                            "assets/images/logo_text.png",
                            width: 230,
                          ),
                        ),

                        const SizedBox(height: 10),

                        const Center(
                          child: Text(
                            "Bring Chemistry To Life",
                            style: TextStyle(
                              color: Colors.white70,
                              fontSize: 16,
                              letterSpacing: 1,
                            ),
                          ),
                        ),

                        const SizedBox(height: 50),

                        /// Chemistry Fact
                        Container(
                          padding: const EdgeInsets.symmetric(
                            horizontal: 18,
                            vertical: 16,
                          ),
                          decoration: BoxDecoration(
                            borderRadius: BorderRadius.circular(20),
                            gradient: const LinearGradient(
                              begin: Alignment.topLeft,
                              end: Alignment.bottomRight,
                              colors: [Color(0xFF38438A), Color(0xFF252F6B)],
                            ),
                            border: Border.all(
                              color: Colors.white.withValues(alpha: 0.16),
                              width: 1.2,
                            ),
                            boxShadow: [
                              BoxShadow(
                                color: Colors.black.withValues(alpha: 0.22),
                                blurRadius: 14,
                                offset: const Offset(0, 6),
                              ),
                            ],
                          ),
                          child: Row(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Expanded(
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Row(
                                      children: [
                                        Text(
                                          "Chemistry Fact",
                                          style: TextStyle(
                                            color: Colors.white,
                                            fontSize: 16,
                                            fontWeight: FontWeight.bold,
                                            letterSpacing: 0.2,
                                          ),
                                        ),
                                        SizedBox(width: 2),
                                        Icon(Icons.star, color: Colors.amber),
                                      ],
                                    ),
                                    const SizedBox(height: 6),
                                    Text(
                                      fact,
                                      style: TextStyle(
                                        color: Colors.white.withValues(
                                          alpha: .8,
                                        ),
                                        height: 1.45,
                                        fontSize: 13,
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                            ],
                          ),
                        ),

                        const SizedBox(height: 18),

                        /// Progres Belajar Kamu (Materi, Molekul, Latihan Soal)
                        BlocBuilder<ProgressBloc, ProgressState>(
                          builder: (context, state) {
                            final progress =
                                state is ProgressLoaded ? state.progress : null;
                            final isLoading = state is ProgressLoading;
                            return HomeProgressCard(
                              progress: progress,
                              isLoading: isLoading,
                              onRetry: () => context
                                  .read<ProgressBloc>()
                                  .add(const ProgressFetchRequested()),
                            );
                          },
                        ),

                        const SizedBox(height: 20),

                        MenuCard(
                          icon: Icons.menu_book_rounded,
                          title: "Materi Pembelajaran",
                          subtitle:
                              "Pelajari materi kimia secara interaktif.",
                          iconColor: Colors.orange,
                          onTap: () async {
                            await Navigator.push(
                              context,
                              MaterialPageRoute(
                                builder: (_) => const MateriScreen(),
                              ),
                            );
                            if (context.mounted) {
                              context
                                  .read<ProgressBloc>()
                                  .add(const ProgressRefreshRequested());
                            }
                          },
                        ),
                        MenuCard(
                          icon: Icons.science_rounded,
                          title: "Daftar Molekul",
                          subtitle:
                              "Pelajari struktur, bentuk, dan sifat molekul.",
                          iconColor: Colors.deepPurpleAccent,
                          onTap: () async {
                            await Navigator.push(
                              context,
                              MaterialPageRoute(
                                builder: (_) => const MoleculeListScreen(),
                              ),
                            );
                            if (context.mounted) {
                              context
                                  .read<ProgressBloc>()
                                  .add(const ProgressRefreshRequested());
                            }
                          },
                        ),

                        /// =========================
                        /// Menu Quiz
                        /// =========================
                        MenuCard(
                          icon: Icons.quiz_rounded,
                          title: "Latihan Soal",
                          subtitle:
                              "Uji pemahamanmu dengan soal-soal interaktif.",
                          iconColor: Colors.green,
                          onTap: () async {
                            await Navigator.push(
                              context,
                              MaterialPageRoute(
                                builder: (_) => const QuizMaterialScreen(),
                              ),
                            );
                            if (context.mounted) {
                              context
                                  .read<ProgressBloc>()
                                  .add(const ProgressRefreshRequested());
                            }
                          },
                        ),
                        MenuCard(
                          icon: Icons.view_in_ar_rounded,
                          title: "Virtual Laboratory AR",
                          subtitle:
                              "Visualisasikan molekul dalam bentuk 3D menggunakan kamera.",
                          iconColor: Colors.cyan,
                          onTap: _navigateToARMenu,
                        ),

                        const SizedBox(height: 25),

                        Center(
                          child: Text(
                            "© 2026 Molecular Vision AR",
                            style: TextStyle(
                              color: Colors.white.withValues(alpha: .55),
                              fontSize: 13,
                            ),
                          ),
                        ),

                        const SizedBox(height: 10),
                      ],
                    ),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
