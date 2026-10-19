class ApiEndpoints {
  ApiEndpoints._();

  /// Base URL API Backend Laravel.
  /// Dapat di-override saat build/run dengan:
  /// `--dart-define=API_BASE_URL=http://<IP_KOMPUTER>:8000/api`
  /// Default menggunakan `http://localhost:8000/api` (cocok untuk USB debugging dengan `adb reverse tcp:8000 tcp:8000`).
  static const String baseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'https://mvar.my.id/api',
  );

  /// Durasi timeout jaringan
  static const Duration connectTimeout = Duration(seconds: 15);
  static const Duration receiveTimeout = Duration(seconds: 15);
  static const Duration sendTimeout = Duration(seconds: 15);

  // ==========================================
  // Auth & Profile Endpoints
  // ==========================================
  static const String login = '/login';
  static const String register = '/register';
  static const String refresh = '/refresh';
  static const String logout = '/logout';
  static const String me = '/me';
  static const String profile = '/profile';
  static const String updatePassword = '/profile/password';

  // ==========================================
  // Master Data & Modul Pembelajaran
  // ==========================================
  static const String degrees = '/degrees';
  static const String categories = '/categories';
  static const String molecules = '/molecules';
  static String moleculeDetail(int id) => '/molecules/$id';
  static const String materials = '/materials';
  static String materialDetail(int id) => '/materials/$id';

  // ==========================================
  // Progres Belajar (Materi, Molekul, Kategori)
  // ==========================================
  static const String progress = '/progress';
  static const String progressMaterial = '/progress/material';
  static const String progressMolecule = '/progress/molecule';
  static const String progressCategory = '/progress/category';

  // ==========================================
  // Kuis & Latihan Soal
  // ==========================================
  static const String exercises = '/exercises';

  static String exerciseDetail(int categoryId) => '/exercises/$categoryId';
  static String exerciseResult(int categoryId) =>
      '/exercises/$categoryId/result';
}
