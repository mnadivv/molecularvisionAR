# Flutter Proguard Rules

# Suppress missing Unity classes when using Option 2 (App-to-App launcher without embedded unityLibrary)
-dontwarn com.unity3d.player.**
-dontwarn com.learntoflutter.flutter_embed_unity_android.**

# Keep Unity Player classes for JNI reflection and Flutter Embed bridge
-keep class com.unity3d.player.** { *; }
-keep class com.learntoflutter.flutter_embed_unity_android.** { *; }
