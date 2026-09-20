using UnityEngine;
using UnityEngine.UIElements;
namespace DealNight.Presentation {
 public sealed class Boot:MonoBehaviour {
  void Awake(){Application.targetFrameRate=60;var cameraObject=new GameObject("Table Camera",typeof(Camera),typeof(AudioListener));var cam=cameraObject.GetComponent<Camera>();cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.06f,.12f,.15f);cam.cullingMask=0;var ui=new GameObject("Deal Night UI");var doc=ui.AddComponent<UIDocument>();var panel=ScriptableObject.CreateInstance<PanelSettings>();panel.scaleMode=PanelScaleMode.ScaleWithScreenSize;panel.referenceResolution=Application.isMobilePlatform?new Vector2Int(1280,720):new Vector2Int(1600,900);panel.screenMatchMode=PanelScreenMatchMode.MatchWidthOrHeight;panel.match=.5f;panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("Theme");doc.panelSettings=panel;ui.AddComponent<TableApp>();}
 }
}
