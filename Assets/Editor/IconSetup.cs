using UnityEditor;
using UnityEngine;
namespace DealNight.Editor {
 [InitializeOnLoad] public static class IconSetup {
  static IconSetup(){EditorApplication.delayCall+=Apply;}
  public static void Apply(){var icon=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Art/AppIcon.png");if(icon==null)return;var current=PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown);if(current.Length==1&&current[0]==icon)return;PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,new[]{icon});}
 }
}
