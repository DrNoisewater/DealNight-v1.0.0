using UnityEngine;
using UnityEngine.UIElements;
using DealNight.Core;
namespace DealNight.Presentation {
 public sealed class CardBackground:VisualElement {
  public CardBackground(Card c){
   pickingMode=PickingMode.Ignore;style.position=Position.Absolute;style.left=0;style.right=0;style.top=0;style.bottom=0;
   bool gradient=(c.Kind==Kind.Wild||c.Kind==Kind.Rent)&&c.Colors.Length>1;
   var palette=gradient?System.Array.ConvertAll(c.Colors,x=>Widgets.GroupColor(x)):new[]{Widgets.ColorFor(c)};
   generateVisualContent+=ctx=>{
    var rect=contentRect;if(rect.width<=0||rect.height<=0)return;
    const int n=12;var mesh=ctx.Allocate(n*n*4,n*n*6);
    for(int y=0;y<n;y++)for(int x=0;x<n;x++){
     float x0=x/(float)n,x1=(x+1)/(float)n,y0=y/(float)n,y1=(y+1)/(float)n;
     foreach(var p in new[]{new Vector2(x0,y0),new Vector2(x1,y0),new Vector2(x1,y1),new Vector2(x0,y1)}){
      float t=(p.x+p.y)*.5f*(palette.Length-1);int i=Mathf.FloorToInt(t);
      var color=Color.Lerp(palette[i],palette[Mathf.Min(i+1,palette.Length-1)],t-i);
      color=Color.Lerp(new Color(.96f,.94f,.88f),color,gradient?.48f:.16f);
      mesh.SetNextVertex(new Vertex{position=new Vector3(p.x*rect.width,p.y*rect.height,Vertex.nearZ),tint=color});
     }
     int offset=(y*n+x)*4;foreach(int i in new[]{0,1,2,2,3,0})mesh.SetNextIndex((ushort)(offset+i));
    }
   };
  }
 }
}
