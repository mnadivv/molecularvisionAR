import 'dart:convert';
import 'dart:io';
import 'package:crypto/crypto.dart';
import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:path_provider/path_provider.dart';

typedef DownloadProgressCallback = void Function(
  int receivedBytes,
  int totalBytes,
  double progress,
);

/// Service cerdas untuk mengunduh, caching, dan memvalidasi file 3D (.glb / .gltf)
/// Mencegah pengunduhan berulang (0ms load dari cache lokal) dan mendukung download progress visual.
class Molecule3DCacheService {
  static final Molecule3DCacheService _instance =
      Molecule3DCacheService._internal();

  factory Molecule3DCacheService() => _instance;

  Molecule3DCacheService._internal({Dio? dio}) : _dio = dio ?? Dio() {
    _dio.options.connectTimeout = const Duration(seconds: 15);
    _dio.options.receiveTimeout = const Duration(minutes: 3);
  }

  final Dio _dio;
  static const String _cacheFolder = 'molecule_3d_models';

  HttpServer? _localServer;
  int? _localPort;

  /// Memulai local HTTP server untuk menyajikan file GLB ke WebView tanpa CORS error
  Future<int> _ensureLocalServerRunning() async {
    if (_localServer != null && _localPort != null) {
      return _localPort!;
    }

    final server = await HttpServer.bind(InternetAddress.loopbackIPv4, 0);
    _localServer = server;
    _localPort = server.port;

    server.listen((HttpRequest request) async {
      try {
        final path = request.uri.path;
        final dir = await _getCacheDirectory();
        // Ambil nama file dari path (misal: /abc123.glb)
        final fileName = path.replaceFirst('/', '');
        final file = File('${dir.path}/$fileName');

        if (await file.exists()) {
          request.response.headers.set('Access-Control-Allow-Origin', '*');
          request.response.headers.set('Access-Control-Allow-Methods', 'GET, OPTIONS');
          request.response.headers.set(
            'Access-Control-Allow-Headers',
            'Origin, X-Requested-With, Content-Type, Accept',
          );
          request.response.headers.contentType =
              ContentType('model', 'gltf-binary');

          final fileLength = await file.length();
          request.response.headers.contentLength = fileLength;

          await file.openRead().pipe(request.response);
        } else {
          request.response.statusCode = HttpStatus.notFound;
          await request.response.close();
        }
      } catch (e) {
        debugPrint('[Molecule3DCacheService] Local server error: $e');
        try {
          request.response.statusCode = HttpStatus.internalServerError;
          await request.response.close();
        } catch (_) {}
      }
    });

    return _localPort!;
  }

  /// Mendapatkan URL HTTP localhost untuk file model yang dicache
  Future<String?> getCachedHttpUrl(String url) async {
    final file = await getCachedModelFile(url);
    if (file == null) return null;
    final port = await _ensureLocalServerRunning();
    final fileName = _getFileNameForUrl(url);
    return 'http://127.0.0.1:$port/$fileName';
  }

  /// Mendapatkan direktori penyimpanan cache lokal untuk 3D models
  Future<Directory> _getCacheDirectory() async {
    final baseDir = await getApplicationDocumentsDirectory();
    final cacheDir = Directory('${baseDir.path}/$_cacheFolder');
    if (!await cacheDir.exists()) {
      await cacheDir.create(recursive: true);
    }
    return cacheDir;
  }

  /// Menghasilkan nama file unik berdasarkan hash URL
  String _getFileNameForUrl(String url) {
    final hash = md5.convert(utf8.encode(url.trim())).toString();
    final cleanExt = url.toLowerCase().contains('.gltf') ? '.gltf' : '.glb';
    return '$hash$cleanExt';
  }

  /// Memeriksa apakah URL model 3D sudah tersimpan di cache lokal
  Future<File?> getCachedModelFile(String url) async {
    if (url.trim().isEmpty) return null;
    try {
      final dir = await _getCacheDirectory();
      final fileName = _getFileNameForUrl(url);
      final file = File('${dir.path}/$fileName');
      if (await file.exists() && await file.length() > 0) {
        return file;
      }
    } catch (e) {
      debugPrint('[Molecule3DCacheService] Error checking cache: $e');
    }
    return null;
  }

  /// Mengunduh model 3D dengan progress listener dan menyimpannya ke cache lokal
  /// Mengembalikan [File] lokal jika berhasil, atau null jika gagal.
  Future<File?> downloadAndCacheModel(
    String url, {
    DownloadProgressCallback? onProgress,
    CancelToken? cancelToken,
  }) async {
    final trimmedUrl = url.trim();
    if (trimmedUrl.isEmpty) return null;

    // Periksa apakah sudah ada di cache terlebih dahulu
    final existingFile = await getCachedModelFile(trimmedUrl);
    if (existingFile != null) {
      if (onProgress != null) {
        final len = await existingFile.length();
        onProgress(len, len, 1.0);
      }
      return existingFile;
    }

    try {
      final dir = await _getCacheDirectory();
      final fileName = _getFileNameForUrl(trimmedUrl);
      final targetFile = File('${dir.path}/$fileName');
      final tempFile = File('${dir.path}/$fileName.tmp');

      if (await tempFile.exists()) {
        await tempFile.delete();
      }

      await _dio.download(
        trimmedUrl,
        tempFile.path,
        cancelToken: cancelToken,
        onReceiveProgress: (received, total) {
          if (total > 0 && onProgress != null) {
            final progress = (received / total).clamp(0.0, 1.0);
            onProgress(received, total, progress);
          } else if (onProgress != null) {
            onProgress(received, -1, 0.5);
          }
        },
      );

      // Verifikasi integritas file yang diunduh
      if (await tempFile.exists() && await tempFile.length() > 0) {
        if (await targetFile.exists()) {
          await targetFile.delete();
        }
        await tempFile.rename(targetFile.path);
        return targetFile;
      }
    } catch (e) {
      debugPrint('[Molecule3DCacheService] Error downloading model: $e');
      rethrow;
    }
    return null;
  }

  /// Menghapus cache spesifik untuk satu URL
  Future<void> removeCachedModel(String url) async {
    try {
      final dir = await _getCacheDirectory();
      final fileName = _getFileNameForUrl(url);
      final file = File('${dir.path}/$fileName');
      if (await file.exists()) {
        await file.delete();
      }
    } catch (e) {
      debugPrint('[Molecule3DCacheService] Error removing cache: $e');
    }
  }

  /// Menghapus semua file 3D yang tersimpan di cache
  Future<void> clearAllCache() async {
    try {
      final dir = await _getCacheDirectory();
      if (await dir.exists()) {
        await dir.delete(recursive: true);
      }
    } catch (e) {
      debugPrint('[Molecule3DCacheService] Error clearing all cache: $e');
    }
  }
}
