using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace DealNight.Editor {
 [InitializeOnLoad] public static class BuildProject {
  const string Pending="DealNight.WindowsBuild.Pending";
  static BuildProject(){EditorApplication.update+=Resume;}
  [MenuItem("Deal Night/Build Windows")]
  public static void Windows(){if(EditorApplication.isPlayingOrWillChangePlaymode){EditorUtility.DisplayDialog("Stop Play Mode","Stop Play Mode before building.","OK");return;}if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows64)){EditorUtility.DisplayDialog("Windows support required","Install Windows build support for this Unity Editor through Unity Hub.","OK");return;}SessionState.SetBool(Pending,true);if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.StandaloneWindows64&&!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows64)){SessionState.SetBool(Pending,false);Debug.LogError("Could not switch to Windows. See the Console.");}}
  static void Resume(){if(!SessionState.GetBool(Pending,false)||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorUserBuildSettings.activeBuildTarget!=BuildTarget.StandaloneWindows64)return;SessionState.SetBool(Pending,false);try{IconSetup.Apply();Directory.CreateDirectory("Builds/Windows");string file="Builds/Windows/DealNight.exe";var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Main.unity"},locationPathName=file,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Windows build failed: "+report.summary.result);EditorUtility.RevealInFinder(Path.GetFullPath(file));}catch(Exception e){Debug.LogException(e);EditorUtility.DisplayDialog("Windows build failed",e.Message+"\nSee the Console for details.","OK");}}
 }
}
