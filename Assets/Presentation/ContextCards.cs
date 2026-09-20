using System;
using System.Linq;
using System.Collections.Generic;
using DealNight.Core;
using UnityEngine;
using UnityEngine.UIElements;
namespace DealNight.Presentation {
 public sealed partial class TableApp {
  void HoverHelp(VisualElement target,string text){target.tooltip="";VisualElement tip=null;IVisualElementScheduledItem timer=null;Action hide=null;hide=()=>{if(timer!=null)timer.Pause();if(tip!=null)tip.RemoveFromHierarchy();tip=null;if(activeHintHide==hide)activeHintHide=null;};Action show=()=>{HideHint();activeHintHide=hide;tip=PlayerText(text,"hint-popover");tip.pickingMode=PickingMode.Ignore;root.Add(tip);var point=root.WorldToLocal(target.worldBound.center);tip.style.left=Mathf.Clamp(point.x,8,Mathf.Max(8,root.resolvedStyle.width-290));tip.style.top=Mathf.Clamp(point.y,8,Mathf.Max(8,root.resolvedStyle.height-140));timer=root.schedule.Execute(hide).StartingIn(5000);};target.RegisterCallback<MouseEnterEvent>(_=>show());target.RegisterCallback<MouseLeaveEvent>(_=>hide());target.RegisterCallback<FocusInEvent>(_=>show());target.RegisterCallback<FocusOutEvent>(_=>hide());target.RegisterCallback<ClickEvent>(e=>{if(TouchMode){show();e.StopPropagation();}});target.RegisterCallback<DetachFromPanelEvent>(_=>hide());}
  string IncomingExplanation(Effect e){string target=e.Target==0?"YOU":game.Players[e.Target].Name;string actor=e.Source==0?"You":game.Players[e.Source].Name;switch(e.Kind){case Kind.DealBreaker:return actor+" will steal the complete set shown from "+target+", including its buildings. Just Say No cancels this action.";case Kind.SlyDeal:return actor+" will take the property shown from "+target+". It is outside a complete set.";case Kind.ForcedDeal:return actor+" will take the first property shown from "+target+" and give the second property in exchange.";case Kind.Rent:case Kind.DebtCollector:case Kind.Birthday:return target+" must pay "+e.Amount+"M to "+actor+" from banked money or laid-down properties. "+(e.AllOpponents?"Every opponent is charged separately. ":"Only the selected opponent is charged. ")+"Never pay from your hand. No change is returned.";default:return Catalog.Describe(game.Cards.First(c=>c.Kind==e.Kind));}}
  void MiniCards(VisualElement parent,IEnumerable<int> ids){var row=Widgets.Box("mini-previews");parent.Add(row);foreach(var id in ids)row.Add(Widgets.Card(game.Cards[id],()=>{}));FitChildren(row,170,215,.55f);}
 }
}
