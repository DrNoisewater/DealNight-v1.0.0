using UnityEngine;
using UnityEngine.UIElements;
namespace DealNight.Presentation {
 public sealed partial class TableApp {
  bool touchPeek;VisualElement touchPeekButton;Rect previousSafe;float previousWidth,previousHeight;int peekPointer=-1;
  bool TouchMode { get { return Application.isMobilePlatform; } }
  void RemoveTouchPeek(){EndTouchPeek();if(touchPeekButton!=null)touchPeekButton.RemoveFromHierarchy();touchPeekButton=null;}
  void AddTouchPeek(){
   RemoveTouchPeek();if(!TouchMode||game==null)return;
   // A plain element avoids Button.Clickable competing for touch capture.
   var button=new VisualElement();button.AddToClassList("touch-peek");
   var label=Widgets.Text("Hold to view board", "");label.pickingMode=PickingMode.Ignore;button.Add(label);
   touchPeekButton=button;root.Add(button);
   button.RegisterCallback<PointerDownEvent>(e=>{
    if(peekPointer!=-1)return;peekPointer=e.pointerId;touchPeek=true;HideHint();
    button.CapturePointer(e.pointerId);if(overlay!=null)overlay.style.opacity=0;
    e.StopImmediatePropagation();
   });
   button.RegisterCallback<PointerUpEvent>(e=>{if(e.pointerId==peekPointer){EndTouchPeek();button.ReleasePointer(e.pointerId);}e.StopPropagation();});
   button.RegisterCallback<PointerCancelEvent>(e=>{EndTouchPeek();button.ReleasePointer(e.pointerId);});
   button.RegisterCallback<PointerCaptureOutEvent>(_=>EndTouchPeek());
  }
  void EndTouchPeek(){touchPeek=false;peekPointer=-1;if(overlay!=null)overlay.style.opacity=1;}
  void UpdateMobile(){if(!TouchMode)return;Rect safe=Screen.safeArea;float width=root.resolvedStyle.width;if(width>0&&(safe!=previousSafe||width!=previousWidth||root.resolvedStyle.height!=previousHeight)){previousSafe=safe;previousWidth=width;previousHeight=root.resolvedStyle.height;float sx=width/Mathf.Max(1,Screen.width),sy=root.resolvedStyle.height/Mathf.Max(1,Screen.height);root.style.paddingLeft=safe.xMin*sx+3;root.style.paddingRight=(Screen.width-safe.xMax)*sx+3;root.style.paddingTop=(Screen.height-safe.yMax)*sy+3;root.style.paddingBottom=safe.yMin*sy+3;}if(touchPeekButton!=null)touchPeekButton.style.bottom=4+safe.yMin*root.resolvedStyle.height/Mathf.Max(1,Screen.height);if(Input.GetKeyDown(KeyCode.Escape)&&game!=null&&!noAnimation){if(overlay!=null&&!paused)Close();else if(!paused)Pause();else{paused=false;Render();}}}
  void OnApplicationPause(bool suspended){if(!suspended)return;EndTouchPeek();HideHint();PlayerPrefs.Save();if(game!=null&&!paused){paused=true;if(!noAnimation)Pause();}}
 }
}
