using System;
using System.Linq;
using DealNight.Core;
using UnityEngine;
using UnityEngine.UIElements;
namespace DealNight.Presentation
{
 public sealed partial class TableApp
 {
  readonly System.Collections.Generic.Dictionary<int,int> mobilePages=new System.Collections.Generic.Dictionary<int,int>();
  void FitChildren(VisualElement host,float width,float height,float maxScale=1)
  {
   Action fit=()=>{
    var children=host.Children().ToArray();if(children.Length==0)return;
    var layout=PresentationMath.Fit(children.Length,host.contentRect.width,host.contentRect.height,width,height,maxScale);
    if(layout.Scale<=0)return;
    for(int i=0;i<children.Length;i++)
    {
     var child=children[i];child.style.position=Position.Absolute;child.style.width=width;child.style.height=height;
     child.style.minWidth=0;child.style.maxWidth=StyleKeyword.None;child.style.marginLeft=0;child.style.marginRight=0;
     child.style.marginTop=0;child.style.marginBottom=0;
     child.style.transformOrigin=new TransformOrigin(Length.Percent(0),Length.Percent(0));
     child.style.scale=new Scale(new Vector3(layout.Scale,layout.Scale,1));
     child.style.left=(i%layout.Columns)*layout.CellWidth+layout.OffsetX;
     child.style.top=(i/layout.Columns)*layout.CellHeight+layout.OffsetY;
    }
   };
   host.RegisterCallback<GeometryChangedEvent>(_=>fit());host.schedule.Execute(fit).StartingIn(16);
  }
  VisualElement PlayerArea(int index)
  {
   var p=game.Players[index];var area=Widgets.Box("player-area");area.name="player-"+index;
   if(index==0)area.AddToClassList("self");if(game.Current==index)area.AddToClassList("active-player");
   var heading=Widgets.Box("player-heading");area.Add(heading);
   heading.Add(PlayerBadge(index,"player-name"));
   heading.Add(Widgets.Text(game.CompleteCount(index)+"/3 sets • "+p.Hand.Count+" cards","player-summary"));
   heading.Add(Widgets.Button(game.BankTotal(index)+"M",()=>BankInspect(index),"bank-pill"));
   var grid=Widgets.Box("fit-properties");area.Add(grid);
   if(p.Sets.Count==0){grid.Add(Widgets.Text("No properties yet","empty"));return area;}
   var ordered=completeFirst?p.Sets.OrderByDescending(game.Complete).ToList():p.Sets;
   int pageSize=index==0?8:(game.Players.Count>3?3:6);
   int page=0,pages=(ordered.Count+pageSize-1)/pageSize;
   if(TouchMode){mobilePages.TryGetValue(index,out page);page=Mathf.Clamp(page,0,pages-1);}
   if(TouchMode&&pages>1){
    var pager=Widgets.Box("mobile-group-pages");area.Add(pager);
    pager.Add(Widgets.Button("‹",()=>{mobilePages[index]=(page+pages-1)%pages;Render();}));
    pager.Add(Widgets.Text("Groups "+(page*pageSize+1)+"–"+Math.Min(ordered.Count,(page+1)*pageSize)+" / "+ordered.Count,"muted"));
    pager.Add(Widgets.Button("›",()=>{mobilePages[index]=(page+1)%pages;Render();}));
   }
   foreach(var set in TouchMode?ordered.Skip(page*pageSize).Take(pageSize):ordered)
   {
    var s=set;var tile=Widgets.Box("set");tile.name="set-"+s.Id;tile.focusable=true;tile.tabIndex=0;
    Color col;ColorUtility.TryParseHtmlString(Widgets.Colors[(int)s.Color],out col);Widgets.Tint(tile,col);if(game.Complete(s)){var glow=Widgets.Box("complete-glow");glow.pickingMode=PickingMode.Ignore;tile.Add(glow);}
    tile.Add(Widgets.Text(Catalog.Labels[(int)s.Color],"set-name"));
    tile.Add(Widgets.Text(string.Join("\n",s.Cards.Select(id=>game.Cards[id].Name).ToArray()),"property-names"));
    tile.Add(Widgets.Text(s.Cards.Count+" / "+Catalog.Sizes[(int)s.Color]+(game.Complete(s)?(TouchMode?" ✓":" • COMPLETE"):""),"set-progress"));
    tile.Add(Widgets.Text("Rent "+game.Rent(s)+"M","rent-corner"));
    if(index==0&&!game.Complete(s))AddSharedColorBadge(tile,s.Color);
    tile.tooltip=SetName(s.Id);tile.RegisterCallback<ClickEvent>(evt=>{if(evt.button==0)SetInspect(index,s);});
    tile.RegisterCallback<KeyDownEvent>(e=>{if(e.keyCode==KeyCode.Return||e.keyCode==KeyCode.Space){SetInspect(index,s);e.StopPropagation();}});
    grid.Add(tile);
   }
   if(TouchMode)FitChildren(grid,150,76,1.25f);else FitChildren(grid,205,142,1.15f);return area;
  }
 }
}
