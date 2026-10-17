import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'core/network/dio_client.dart';
import 'core/storage/token_storage_service.dart';
import 'features/auth/data/repositories/auth_repository.dart';
import 'features/auth/presentation/bloc/auth_bloc.dart';
import 'features/auth/presentation/bloc/auth_event.dart';
import 'features/home/presentation/screens/splash_screen.dart';

import 'core/theme/app_theme.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  final tokenStorage = TokenStorageService();
  final dioClient = DioClient(tokenStorage: tokenStorage);
  final authRepository = AuthRepository(
    dioClient: dioClient,
    tokenStorage: tokenStorage,
  );

  runApp(
    MultiRepositoryProvider(
      providers: [
        RepositoryProvider<TokenStorageService>.value(value: tokenStorage),
        RepositoryProvider<DioClient>.value(value: dioClient),
        RepositoryProvider<AuthRepository>.value(value: authRepository),
      ],
      child: MultiBlocProvider(
        providers: [
          BlocProvider<AuthBloc>(
            create: (context) => AuthBloc(
              authRepository: context.read<AuthRepository>(),
            )..add(const AuthCheckRequested()),
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