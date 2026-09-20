using System;
using System.Linq;
using DealNight.Core;
using UnityEngine;
using UnityEngine.UIElements;
namespace DealNight.Presentation
{
 public sealed partial class TableApp
 {
  void Reaction()
  {
   var e=game.Pending;var definition=e.Card>=0?game.Cards[e.Card]:game.Cards.First(c=>c.Kind==e.Kind);
   var w=Modal(e.Cancelled?"Action Rejected":"Incoming action",false);
   var route=Widgets.Box("reaction-route");route.Add(PlayerBadge(e.Source));route.Add(Widgets.Text(" → "));route.Add(PlayerBadge(e.Target));w.Add(route);
   var row=Widgets.Box("reaction-layout");w.Add(row);var action=Widgets.Card(definition,()=>{});HoverHelp(action,e.Cancelled?RejectedExplanation(e):IncomingExplanation(e));if(e.Cancelled)action.AddToClassList("rejected-card");row.Add(action);var detail=Widgets.Box("reaction-detail");row.Add(detail);
   detail.Add(Widgets.Text(definition.Name,"modal-title"));detail.Add(Widgets.Text(TouchMode?"Tap the action card for details":"Hover over the action card for details","muted"));detail.Add(Widgets.Text(e.Cancelled?RejectedExplanation(e):IncomingExplanation(e),"reaction-explanation"));
   if(e.Amount>0){detail.Add(Widgets.Text(e.Cancelled?"No payment if rejection stands":e.Amount+"M requested"+(e.AllOpponents?" • EACH opponent":" • ONE opponent"),"reaction-amount"));detail.Add(Widgets.Text("Your bank: "+game.BankTotal(0)+"M","bank-available"));}
   if(e.Kind==Kind.DealBreaker){detail.Add(Widgets.Text(e.Cancelled?"Set protected if rejection stands":e.Target==0?"Your set at stake":"Set at stake","eyebrow"));var set=game.Players.SelectMany(p=>p.Sets).First(s=>s.Id==e.Set);MiniCards(detail,set.Cards.Concat(new[]{set.House,set.Hotel}.Where(id=>id>=0)));}
   else if(e.Take>=0){detail.Add(Widgets.Text(e.Cancelled?"Property protected if rejection stands":e.Target==0?"Your property at stake":"Property at stake","eyebrow"));MiniCards(detail,new[]{e.Take});if(e.Kind==Kind.ForcedDeal&&e.Give>=0){detail.Add(Widgets.Text(e.Target==0?"You receive":"In exchange","eyebrow"));MiniCards(detail,new[]{e.Give});}}
   foreach(var cmd in game.Legal(0)){var c=cmd;w.Add(Widgets.Button(c.Op==Op.No?(e.Cancelled?"Counter • restore original action":"Just Say No"):e.Cancelled?"Continue • action stays rejected":"Allow action",()=>Send(c),c.Op==Op.No?"primary":""));}
  }
  void ShowNo(string actor)
  {
   var shade=Widgets.Box("no-overlay");root.Add(shade);
   var show=Widgets.Box("no-show");shade.Add(show);
   show.Add(PlayerText(actor+" played","no-actor"));
   show.Add(CardArt.Glove());
   show.Add(Widgets.Text("JUST SAY NO!","no-title"));
   float start=Time.unscaledTime;float duration=reduced?.65f:1.25f*animationPace;bool done=false;
   show.schedule.Execute(()=>{
    float t=Mathf.Clamp01((Time.unscaledTime-start)/duration);
    if(!reduced){float enter=Mathf.Clamp01(t/.23f);float scale=.65f+.35f*(1-Mathf.Pow(1-enter,3));show.style.scale=new Scale(new Vector3(scale,scale,1));shade.style.opacity=t>.8f?1-(t-.8f)/.2f:1;}
    if(t>=1&&!done){done=true;noAnimation=false;shade.RemoveFromHierarchy();Render();if(paused)Pause();nextAI=Time.unscaledTime+.2f;}
   }).Every(16).Until(()=>done||shade.parent==null);
  }
 }
}
