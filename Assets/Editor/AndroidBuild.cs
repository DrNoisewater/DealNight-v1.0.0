using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace DealNight.Editor {
 [InitializeOnLoad] public static class AndroidBuild {
  const string Pending="DealNight.AndroidBuild.Pending",Run="DealNight.AndroidBuild.Run";
  static AndroidBuild(){EditorApplication.update+=Resume;}
  [MenuItem("Deal Night/Build Android APK")] public static void APK(){Request(false);}
  [MenuItem("Deal Night/Build Android and Run on Phone")] public static void Phone(){Request(true);}
  static void Request(bool run){if(EditorApplication.isPlayingOrWillChangePlaymode){EditorUtility.DisplayDialog("Stop Play Mode","Stop Play Mode before building.","OK");return;}if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android)){EditorUtility.DisplayDialog("Install Android support","Unity Hub > Installs > your Editor > Add modules: Android Build Support, Android SDK & NDK Tools, OpenJDK. Then reopen this project.","OK");return;}SessionState.SetBool(Run,run);SessionState.SetBool(Pending,true);if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android&&!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,BuildTarget.Android)){SessionState.SetBool(Pending,false);Debug.LogError("Could not switch to Android. Check the Unity Console.");}}
  static void Resume(){if(!SessionState.GetBool(Pending,false)||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)return;bool run=SessionState.GetBool(Run,false);SessionState.SetBool(Pending,false);try{Build(run);}catch(Exception e){Debug.LogException(e);EditorUtility.DisplayDialog("Android build failed",e.Message+"\nSee the Console for details.","OK");}}
  static void Build(bool run){IconSetup.Apply();PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.noisewaterlabs.dealnight");PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64|AndroidArchitecture.ARMv7;PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;PlayerSettings.Android.bundleVersionCode=201;PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;PlayerSettings.allowedAutorotateToPortrait=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;EditorUserBuildSettings.buildAppBundle=false;EditorUserBuildSettings.exportAsGoogleAndroidProject=false;PlayerSettings.Android.splitApplicationBinary=false;
   Directory.CreateDirectory("Builds/Android");string path="Builds/Android/DealNight-"+PlayerSettings.bundleVersion+".apk";var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Main.unity"},locationPathName=path,target=BuildTarget.Android,options=run?BuildOptions.AutoRunPlayer:BuildOptions.None});if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build result: "+report.summary.result);EditorUtility.RevealInFinder(Path.GetFullPath(path));}
 }
}

