import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../features/auth/presentation/bloc/auth_bloc.dart';
import '../features/auth/presentation/bloc/auth_event.dart';
import '../features/auth/presentation/bloc/auth_state.dart';
import '../features/auth/presentation/screens/login_screen.dart';
import '../utils/app_colors.dart';
import '../widgets/menu_card.dart';

import 'materi/materi_screen.dart';
import 'ar/ar_menu_screen.dart';
import 'molecule/molecule_screen.dart';
import 'quiz/quiz_material_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen>
    with TickerProviderStateMixin {

  late AnimationController _controller;

  late Animation<double> _fade;

  late Animation<Offset> _slide;

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

    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 900),
    );

    _fade = CurvedAnimation(
      parent: _controller,
      curve: Curves.easeIn,
    );

    _slide = Tween<Offset>(
      begin: const Offset(0, .12),
      end: Offset.zero,
    ).animate(
      CurvedAnimation(
        parent: _controller,
        curve: Curves.easeOut,
      ),
    );

    _controller.forward();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
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
            Text('Keluar Akun', style: TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.bold)),
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
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
            ),
            onPressed: () {
              Navigator.of(ctx).pop();
              context.read<AuthBloc>().add(const AuthLogoutRequested());
            },
            child: const Text('Keluar', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
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
        borderRadius: BorderRadius.vertical(
          top: Radius.circular(28),
        ),
      ),
      builder: (_) {
        return Padding(
          padding: const EdgeInsets.all(25),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [

              Image.asset(
                "assets/images/logo_circle.png",
                width: 80,
              ),

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

              Text(
                "Version 1.0.0",
                style: TextStyle(
                  color: Colors.white70,
                ),
              ),

              const SizedBox(height: 20),

              const Divider(),

              const SizedBox(height: 20),

              const ListTile(
                leading: Icon(
                  Icons.school,
                  color: Colors.white,
                ),
                title: Text(
                  "Universitas Jember",
                  style: TextStyle(
                    color: Colors.white,
                  ),
                ),
              ),

              const ListTile(
                leading: Icon(
                  Icons.emoji_events,
                  color: Colors.white,
                ),
                title: Text(
                  "LIDM 2026",
                  style: TextStyle(
                    color: Colors.white,
                  ),
                ),
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
    final fact =
        chemistryFacts[DateTime.now().day % chemistryFacts.length];

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
              child: Image.asset(
                "assets/images/bg_molecule.png",
                width: 220,
              ),
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
                child: ListView(
                  padding: const EdgeInsets.fromLTRB(
                    22,
                    10,
                    22,
                    30,
                  ),
                  children: [

                    /// Top Bar (User Profile & Actions)
                    BlocBuilder<AuthBloc, AuthState>(
                      builder: (context, authState) {
                        final user = authState is Authenticated ? authState.user : null;

                        return Container(
                          padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                          margin: const EdgeInsets.only(bottom: 12),
                          decoration: BoxDecoration(
                            color: Colors.white.withValues(alpha: 0.08),
                            borderRadius: BorderRadius.circular(18),
                            border: Border.all(
                              color: Colors.white.withValues(alpha: 0.12),
                            ),
                          ),
                          child: Row(
                            children: [
                              CircleAvatar(
                                radius: 20,
                                backgroundColor: AppColors.accent.withValues(alpha: 0.2),
                                child: const Icon(
                                  Icons.person_rounded,
                                  color: AppColors.accent,
                                  size: 22,
                                ),
                              ),
                              const SizedBox(width: 12),
                              Expanded(
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Text(
                                      user?.name ?? 'Siswa MVAR',
                                      style: const TextStyle(
                                        color: Colors.white,
                                        fontSize: 14,
                                        fontWeight: FontWeight.bold,
                                      ),
                                      maxLines: 1,
                                      overflow: TextOverflow.ellipsis,
                                    ),
                                    Text(
                                      '${user?.degree?.name ?? 'Siswa'} • ${user?.school ?? 'MVAR Chemistry'}',
                                      style: TextStyle(
                                        color: Colors.white.withValues(alpha: 0.6),
                                        fontSize: 11,
                                      ),
                                      maxLines: 1,
                                      overflow: TextOverflow.ellipsis,
                                    ),
                                  ],
                                ),
                              ),
                              IconButton(
                                onPressed: _showAbout,
                                splashRadius: 20,
                                icon: const Icon(
                                  Icons.info_outline_rounded,
                                  color: Colors.white70,
                                  size: 22,
                                ),
                              ),
                              IconButton(
                                onPressed: _confirmLogout,
                                splashRadius: 20,
                                icon: const Icon(
                                  Icons.logout_rounded,
                                  color: Colors.redAccent,
                                  size: 22,
                                ),
                              ),
                            ],
                          ),
                        );
                      },
                    ),

                    const SizedBox(height: 8),

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

                    const SizedBox(height: 30),

                    /// Chemistry Fact
                    Container(
                      padding: const EdgeInsets.all(18),
                      decoration: BoxDecoration(
                        color: Colors.white.withValues(alpha: .08),
                        borderRadius: BorderRadius.circular(22),
                        border: Border.all(
                          color: Colors.white.withValues(alpha: .12),
                        ),
                      ),
                      child: Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [

                          const Icon(
                            Icons.lightbulb_rounded,
                            color: Colors.amber,
                            size: 30,
                          ),

                          const SizedBox(width: 15),

                          Expanded(
                            child: Column(
                              crossAxisAlignment:
                                  CrossAxisAlignment.start,
                              children: [

                                const Text(
                                  "Chemistry Fact",
                                  style: TextStyle(
                                    color: Colors.white,
                                    fontSize: 16,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),

                                const SizedBox(height: 8),

                                Text(
                                  fact,
                                  style: TextStyle(
                                    color: Colors.white.withValues(
                                      alpha: .75,
                                    ),
                                    height: 1.5,
                                    fontSize: 14,
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ],
                      ),
                    ),

                    const SizedBox(height: 30),
                    /// =========================
                    /// Menu Materi
                    /// =========================
                    MenuCard(
                      icon: Icons.menu_book_rounded,
                      title: "Materi Pembelajaran",
                      subtitle:
                          "Pelajari 12 materi kimia secara interaktif.",
                      iconColor: Colors.orange,
                      onTap: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (_) => const MateriScreen(),
                          ),
                        );
                      },
                    ),

                    /// =========================
                    /// Menu AR
                    /// =========================
                    MenuCard(
                      icon: Icons.view_in_ar_rounded,
                      title: "Augmented Reality",
                      subtitle:
                          "Visualisasikan molekul dalam bentuk 3D menggunakan kamera.",
                      iconColor: Colors.cyan,
                      onTap: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (_) => const ARMenuScreen(),
                          ),
                        );
                      },
                    ),

                    /// =========================
                    /// Menu Molekul
                    /// =========================
                    MenuCard(
                      icon: Icons.science_rounded,
                      title: "Detail Molekul",
                      subtitle:
                          "Pelajari struktur, bentuk, dan sifat molekul.",
                      iconColor: Colors.deepPurpleAccent,
                      onTap: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (_) => const MoleculeScreen(),
                          ),
                        );
                      },
                    ),

                    /// =========================
                    /// Menu Quiz
                    /// =========================
                    MenuCard(
                      icon: Icons.quiz_rounded,
                      title: "Quiz",
                      subtitle:
                          "Uji pemahamanmu dengan soal-soal interaktif.",
                      iconColor: Colors.green,
                      onTap: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (_) => const QuizMaterialScreen(),
                          ),
                        );
                      },
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
        ],
        ),
      ),
    );
  }
}