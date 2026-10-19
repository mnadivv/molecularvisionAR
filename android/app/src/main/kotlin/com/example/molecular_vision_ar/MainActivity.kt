package com.example.molecular_vision_ar

import android.content.Intent
import android.content.pm.PackageManager
import android.net.Uri
import io.flutter.embedding.android.FlutterActivity
import io.flutter.embedding.engine.FlutterEngine
import io.flutter.plugin.common.MethodChannel

class MainActivity : FlutterActivity() {
    private val CHANNEL = "com.mvar.molecularvisionAR/unity_launcher"

    override fun configureFlutterEngine(flutterEngine: FlutterEngine) {
        super.configureFlutterEngine(flutterEngine)

        // Pastikan plugin krusial tetap terdaftar jika terjadi error pada plugin unity
        try {
            if (!flutterEngine.plugins.has(com.it_nomads.fluttersecurestorage.FlutterSecureStoragePlugin::class.java)) {
                flutterEngine.plugins.add(com.it_nomads.fluttersecurestorage.FlutterSecureStoragePlugin())
            }
        } catch (t: Throwable) {
            android.util.Log.e("MainActivity", "Fallback register FlutterSecureStoragePlugin failed", t)
        }

        try {
            if (!flutterEngine.plugins.has(io.flutter.plugins.flutter_plugin_android_lifecycle.FlutterAndroidLifecyclePlugin::class.java)) {
                flutterEngine.plugins.add(io.flutter.plugins.flutter_plugin_android_lifecycle.FlutterAndroidLifecyclePlugin())
            }
        } catch (t: Throwable) {
            android.util.Log.e("MainActivity", "Fallback register FlutterAndroidLifecyclePlugin failed", t)
        }

        try {
            if (!flutterEngine.plugins.has(io.flutter.plugins.pathprovider.PathProviderPlugin::class.java)) {
                flutterEngine.plugins.add(io.flutter.plugins.pathprovider.PathProviderPlugin())
            }
        } catch (t: Throwable) {
            android.util.Log.e("MainActivity", "Fallback register PathProviderPlugin failed", t)
        }

        try {
            if (!flutterEngine.plugins.has(io.flutter.plugins.sharedpreferences.SharedPreferencesPlugin::class.java)) {
                flutterEngine.plugins.add(io.flutter.plugins.sharedpreferences.SharedPreferencesPlugin())
            }
        } catch (t: Throwable) {
            android.util.Log.e("MainActivity", "Fallback register SharedPreferencesPlugin failed", t)
        }

        try {
            if (!flutterEngine.plugins.has(io.flutter.plugins.webviewflutter.WebViewFlutterPlugin::class.java)) {
                flutterEngine.plugins.add(io.flutter.plugins.webviewflutter.WebViewFlutterPlugin())
            }
        } catch (t: Throwable) {
            android.util.Log.e("MainActivity", "Fallback register WebViewFlutterPlugin failed", t)
        }

        MethodChannel(flutterEngine.dartExecutor.binaryMessenger, CHANNEL).setMethodCallHandler { call, result ->
            when (call.method) {
                "isAppInstalled" -> {
                    val packageName = call.argument<String>("packageName") ?: ""
                    result.success(isPackageInstalled(packageName))
                }
                "launchUnityApp" -> {
                    val packageName = call.argument<String>("packageName") ?: ""
                    val topicId = call.argument<String>("topicId") ?: ""
                    val topicTitle = call.argument<String>("topicTitle") ?: ""
                    val success = launchPackage(packageName, topicId, topicTitle)
                    result.success(success)
                }
                "launchDeepLink" -> {
                    val uriString = call.argument<String>("uri") ?: ""
                    val success = launchUri(uriString)
                    result.success(success)
                }
                else -> result.notImplemented()
            }
        }
    }

    private fun isPackageInstalled(packageName: String): Boolean {
        if (packageName.isBlank()) return false
        return try {
            packageManager.getPackageInfo(packageName, 0)
            true
        } catch (e: PackageManager.NameNotFoundException) {
            false
        }
    }

    private fun launchPackage(packageName: String, topicId: String, topicTitle: String): Boolean {
        return try {
            val intent = packageManager.getLaunchIntentForPackage(packageName)
            if (intent != null) {
                intent.putExtra("topic_id", topicId)
                intent.putExtra("topic_title", topicTitle)
                intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
                startActivity(intent)
                true
            } else {
                false
            }
        } catch (e: Exception) {
            e.printStackTrace()
            false
        }
    }

    private fun launchUri(uriString: String): Boolean {
        return try {
            val intent = Intent(Intent.ACTION_VIEW, Uri.parse(uriString))
            intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
            startActivity(intent)
            true
        } catch (e: Exception) {
            e.printStackTrace()
            false
        }
    }
}
