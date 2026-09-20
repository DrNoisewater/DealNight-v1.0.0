using DealNight.Core;
using UnityEngine;
using UnityEngine.UIElements;
namespace DealNight.Presentation
{
 // Original vector-like artwork made from UI geometry: resolution-independent,
 // no image downloads, sprite atlas, or third-party art package required.
 public static class CardArt
 {
  public static VisualElement Glove(){
   var hand=Widgets.Box("stop-hand");
   var sleeve=Widgets.Box("glove-sleeve");hand.Add(sleeve);
   // Original five-digit white cartoon glove, rendered with vector-like UI geometry.
   int[] tops={52,19,7,31};int[] lengths={130,163,175,151};
   for(int i=0;i<4;i++){var finger=Widgets.Box("glove-finger");finger.style.left=63+i*45;finger.style.top=tops[i];finger.style.height=lengths[i];hand.Add(finger);}
   var thumb=Widgets.Box("glove-thumb");hand.Add(thumb);
   var palm=Widgets.Box("glove-palm");hand.Add(palm);
   for(int i=0;i<3;i++){var seam=Widgets.Box("glove-seam");seam.style.left=43+i*36;palm.Add(seam);}
   var cuff=Widgets.Box("glove-cuff");hand.Add(cuff);
   return hand;
  }
  public static VisualElement Create(Card card)
  {
   var art=Widgets.Box("card-art");art.pickingMode=PickingMode.Ignore;
   if(card.Property)
   {
    var sun=Widgets.Box("art-sun");art.Add(sun);
    for(int i=0;i<4;i++)
    {
     var building=Widgets.Box("art-building");building.style.left=8+i*24;building.style.width=20;
     int height=25+(card.Id*13+i*17)%35;building.style.height=height;building.style.bottom=5;
     building.style.backgroundColor=Color.Lerp(Widgets.ColorFor(card),new Color(.05f,.12f,.16f),i*.14f);
     for(int floor=0;floor<height/13;floor++)for(int x=0;x<2;x++)
     {
      var window=Widgets.Box("art-window");window.style.left=4+x*9;window.style.bottom=6+floor*12;building.Add(window);
     }
     art.Add(building);
    }
    var road=Widgets.Box("art-road");art.Add(road);
    if(card.Kind==Kind.Wild)art.Add(Widgets.Text("WILD","art-tag"));
   }
   else
   {
    if(card.Kind==Kind.JustSayNo){var hand=Glove();hand.style.position=Position.Absolute;hand.style.left=35;hand.style.top=0;hand.style.transformOrigin=new TransformOrigin(Length.Percent(0),Length.Percent(0));hand.style.scale=new Scale(new Vector3(.21f,.21f,1));art.Add(hand);return art;}
    var picture=new Image();picture.image=Resources.Load<Texture2D>("Art/"+card.Kind);picture.scaleMode=ScaleMode.ScaleToFit;picture.style.flexGrow=1;picture.pickingMode=PickingMode.Ignore;art.Add(picture);

   }
   return art;
  }
 }
}
