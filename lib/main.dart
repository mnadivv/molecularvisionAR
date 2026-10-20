import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'core/network/dio_client.dart';
import 'core/storage/token_storage_service.dart';
import 'features/auth/data/repositories/auth_repository.dart';
import 'features/auth/presentation/bloc/auth_bloc.dart';
import 'features/auth/presentation/bloc/auth_event.dart';
import 'features/home/presentation/screens/splash_screen.dart';

import 'core/theme/app_theme.dart';

import 'data/repositories/progress_repository.dart';
import 'features/materi/data/repositories/materi_repository.dart';
import 'features/molecule/data/repositories/molecule_repository.dart';
import 'features/profile/data/repositories/profile_repository.dart';
import 'features/profile/presentation/bloc/profile_bloc.dart';
import 'features/quiz/data/repositories/quiz_repository.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await SystemChrome.setPreferredOrientations([
    DeviceOrientation.portraitUp,
  ]);

  final tokenStorage = TokenStorageService();
  final dioClient = DioClient(tokenStorage: tokenStorage);
  final authRepository = AuthRepository(
    dioClient: dioClient,
    tokenStorage: tokenStorage,
  );
  final profileRepository = ProfileRepository(
    dioClient: dioClient,
  );
  final materiRepository = MateriRepository(
    dioClient: dioClient,
  );
  final moleculeRepository = MoleculeRepository(
    dioClient: dioClient,
  );
  final quizRepository = QuizRepository(
    dioClient: dioClient,
  );
  final progressRepository = ProgressRepository(
    dioClient: dioClient,
  );

  runApp(
    MultiRepositoryProvider(
      providers: [
        RepositoryProvider<TokenStorageService>.value(value: tokenStorage),
        RepositoryProvider<DioClient>.value(value: dioClient),
        RepositoryProvider<AuthRepository>.value(value: authRepository),
        RepositoryProvider<ProfileRepository>.value(value: profileRepository),
        RepositoryProvider<MateriRepository>.value(value: materiRepository),
        RepositoryProvider<MoleculeRepository>.value(value: moleculeRepository),
        RepositoryProvider<QuizRepository>.value(value: quizRepository),
        RepositoryProvider<ProgressRepository>.value(value: progressRepository),
      ],
      child: MultiBlocProvider(
        providers: [
          BlocProvider<AuthBloc>(
            create: (context) => AuthBloc(
              authRepository: context.read<AuthRepository>(),
            )..add(const AuthCheckRequested()),
          ),
          BlocProvider<ProfileBloc>(
            create: (context) => ProfileBloc(
              repository: context.read<ProfileRepository>(),
            ),
          ),
        ],
        child: const MVARApp(),
      ),
    ),
  );
}

class MVARApp extends StatelessWidget {
  const MVARApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Molecular Vision AR',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.darkTheme,
      home: const SplashScreen(),
    );
  }
}