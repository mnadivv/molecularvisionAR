import 'dart:convert';
import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';

/// Interceptor logging kustom yang rapi dan informatif untuk debugging HTTP.
/// Hanya aktif pada mode debug (`kDebugMode`) dan menyamarkan field sensitif.
class LoggingInterceptor extends Interceptor {
  static const List<String> _sensitiveKeys = [
    'password',
    'password_confirmation',
    'token',
    'access_token',
    'secret',
  ];

  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    if (kDebugMode) {
      debugPrint('┌────────────────────────────────────────────────────────');
      debugPrint('│ [DIO REQUEST] ${options.method} ${options.uri}');
      if (options.headers.isNotEmpty) {
        debugPrint('│ Headers: ${_maskHeaders(options.headers)}');
      }
      if (options.queryParameters.isNotEmpty) {
        debugPrint('│ Query Parameters: ${options.queryParameters}');
      }
      if (options.data != null) {
        debugPrint('│ Request Body: ${_maskData(options.data)}');
      }
      debugPrint('└────────────────────────────────────────────────────────');
    }
    return handler.next(options);
  }

  @override
  void onResponse(Response response, ResponseInterceptorHandler handler) {
    if (kDebugMode) {
      debugPrint('┌────────────────────────────────────────────────────────');
      debugPrint(
        '│ [DIO RESPONSE] ${response.statusCode} (${response.statusMessage ?? 'OK'}) '
        '${response.requestOptions.method} ${response.requestOptions.uri}',
      );
      if (response.data != null) {
        final prettyData = _formatResponseData(response.data);
        debugPrint('│ Response Body: $prettyData');
      }
      debugPrint('└────────────────────────────────────────────────────────');
    }
    return handler.next(response);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    if (kDebugMode) {
      debugPrint('┌────────────────────────────────────────────────────────');
      debugPrint(
        '│ [DIO ERROR] ${err.response?.statusCode ?? 'NO_STATUS'} '
        '${err.requestOptions.method} ${err.requestOptions.uri}',
      );
      debugPrint('│ Type: ${err.type}');
      debugPrint('│ Message: ${err.message}');
      if (err.error != null) {
        debugPrint('│ Cause: ${err.error}');
      }
      if (err.response?.data != null) {
        debugPrint('│ Error Response: ${err.response?.data}');
      }
      debugPrint('└────────────────────────────────────────────────────────');
    }
    return handler.next(err);
  }

  Map<String, dynamic> _maskHeaders(Map<String, dynamic> headers) {
    final masked = Map<String, dynamic>.from(headers);
    if (masked.containsKey('Authorization')) {
      final auth = masked['Authorization'].toString();
      if (auth.length > 20) {
        masked['Authorization'] = '${auth.substring(0, 15)}...[MASKED]';
      }
    }
    return masked;
  }

  dynamic _maskData(dynamic data) {
    if (data is Map) {
      final copy = Map<String, dynamic>.from(data);
      for (final key in copy.keys) {
        if (_sensitiveKeys.contains(key.toString().toLowerCase())) {
          copy[key] = '********';
        }
      }
      return copy;
    }
    return data;
  }

  String _formatResponseData(dynamic data) {
    try {
      if (data is Map || data is List) {
        const encoder = JsonEncoder.withIndent('  ');
        final jsonString = encoder.convert(data);
        // Batasi panjang tampilan log agar konsol tidak lagging jika data JSON sangat besar
        if (jsonString.length > 1000) {
          return '${jsonString.substring(0, 1000)}\n│ ... [truncated, total: ${jsonString.length} chars]';
        }
        return jsonString;
      }
      return data.toString();
    } catch (_) {
      return data.toString();
    }
  }
}
