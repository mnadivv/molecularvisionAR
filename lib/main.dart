import 'package:flutter/material.dart';

import 'screens/splash_screen.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  runApp(const MVARApp());
}

class MVARApp extends StatelessWidget {
  const MVARApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Molecular Vision AR',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        useMaterial3: true,
        fontFamily: 'Poppins',
      ),
      home: const SplashScreen(),
    );
  }
}