using System;
using System.Collections.Generic;
using System.Linq;
using DealNight.Core;
using UnityEngine;
using UnityEngine.UIElements;
namespace DealNight.Presentation
{
 public sealed partial class TableApp
 {
  readonly Dictionary<int,float> arrivals=new Dictionary<int,float>();
  void RevealArrival(int id){arrivals.Remove(id);var card=root.Q("handcard-"+id);if(card!=null){card.style.opacity=1;card.SetEnabled(true);}}
  void MarkArrivals(Dictionary<int,CardPosition> before){if(reduced)return;var arrivalGame=game;int changed=CaptureCards().Count(pair=>before.ContainsKey(pair.Key)&&(before[pair.Key].Zone!=pair.Value.Zone||before[pair.Key].Owner!=pair.Value.Owner));foreach(int id in game.Players[0].Hand){CardPosition old;if(!before.TryGetValue(id,out old)||(old.Zone=="hand"&&old.Owner==0))continue;int card=id;float expires=Time.unscaledTime+animationPace*(1+changed*.069f)+.2f;arrivals[id]=expires;root.schedule.Execute(()=>{if(game==arrivalGame&&arrivals.ContainsKey(card)&&arrivals[card]<=Time.unscaledTime)RevealArrival(card);}).StartingIn((long)((expires-Time.unscaledTime)*1000)+30);}}
  void BankGain(int player,int amount){var point=PointOf(root.Q("player-"+player)?.Q(className:"bank-pill"));var popup=Widgets.Text("+"+amount+"M","bank-gain");popup.pickingMode=PickingMode.Ignore;root.Add(popup);float began=Time.unscaledTime;popup.schedule.Execute(()=>{float t=(Time.unscaledTime-began)/(reduced?2f:Mathf.Max(2.8f,animationPace*2f));popup.style.left=point.x-10;popup.style.top=point.y-35-t*35;popup.style.opacity=t<.65f?1:Mathf.Clamp01((1-t)/.35f);if(t>=1)popup.RemoveFromHierarchy();}).Every(16).Until(()=>popup.parent==null);}
  sealed class CardPosition
  {
   public string Zone; public int Owner; public Vector2 Point; public bool Hidden;
  }
  Vector2 PointOf(VisualElement element)
  {
   if(element==null||element.worldBound.width<=0)return new Vector2(root.resolvedStyle.width*.5f,root.resolvedStyle.height*.45f);
   return root.WorldToLocal(element.worldBound.center);
  }
  Dictionary<int,CardPosition> CaptureCards()
  {
   var result=new Dictionary<int,CardPosition>();
   Action<int,string,int,VisualElement,bool> add=(id,zone,owner,element,hidden)=>result[id]=new CardPosition{Zone=zone,Owner=owner,Point=PointOf(element),Hidden=hidden};
   foreach(int id in game.Deck)add(id,"deck",-1,root.Q(className:"pile"),true);
   foreach(int id in game.Discard)add(id,"discard",-1,root.Q(className:"discard-pile"),false);
   for(int i=0;i<game.Players.Count;i++)
   {
    var p=game.Players[i];var area=root.Q("player-"+i);
    foreach(int id in p.Hand)add(id,"hand",i,i==0?root.Q("handcard-"+id):area,i!=0);
    foreach(int id in p.Bank)add(id,"bank",i,area,false);
    foreach(var set in p.Sets)foreach(int id in set.Cards.Concat(new[]{set.House,set.Hotel}.Where(x=>x>=0)))add(id,"set-"+set.Id,i,root.Q("set-"+set.Id),false);
   }
   return result;
  }
  void IgnorePointer(VisualElement e){e.pickingMode=PickingMode.Ignore;foreach(var child in e.Children())IgnorePointer(child);}
  void AnimateChanges(Command command,Dictionary<int,CardPosition> before)
  {
   // Wait one layout pass to obtain destination positions; gameplay never waits on animations.
   var match=game;float pace=animationPace;
   root.schedule.Execute(()=>{
    if(game!=match||reduced)return;
    var after=CaptureCards();int order=0;
    foreach(var entry in after)
    {
     CardPosition old;if(!before.TryGetValue(entry.Key,out old))continue;var next=entry.Value;
     if(next.Zone=="deck"||(old.Zone==next.Zone&&old.Owner==next.Owner))continue;
     int id=entry.Key;var start=old;var end=next;int delay=(int)(order++*69*pace);
     root.schedule.Execute(()=>{if(game==match&&!reduced)FlyCard(id,start,end,pace);}).StartingIn(delay);
    }
    nextAI=Mathf.Max(nextAI,Time.unscaledTime+(1f+order*.069f)*pace+.1f);
   }).StartingIn(16);
  }
  void FlyCard(int id,CardPosition from,CardPosition to,float duration)
  {
   if(reduced||game==null)return;
   bool hidden=to.Hidden; // A draw to an opponent always uses a card back.
   var fly=hidden?Widgets.CardBack():Widgets.Card(game.Cards[id],()=>{});
   fly.AddToClassList("motion-card");IgnorePointer(fly);root.Add(fly);
   float began=Time.unscaledTime;
   fly.schedule.Execute(()=>{
    float t=Mathf.Clamp01((Time.unscaledTime-began)/duration);float eased=1-Mathf.Pow(1-t,3);
    Vector2 point=Vector2.Lerp(from.Point,to.Point,eased);point.y-=Mathf.Sin(t*Mathf.PI)*38;
    fly.style.left=point.x-70;fly.style.top=point.y-88;fly.style.opacity=t>.85f?(1-t)/.15f:1;
    if(t>=1){if(to.Zone=="hand"&&to.Owner==0)RevealArrival(id);fly.RemoveFromHierarchy();}
   }).Every(16).Until(()=>fly.parent==null);
  }
 }
}
