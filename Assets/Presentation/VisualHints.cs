using System;
using System.Linq;
using System.Collections.Generic;
using DealNight.Core;
using UnityEngine;
using UnityEngine.UIElements;
namespace DealNight.Presentation
{
 public sealed partial class TableApp
 {
  void AddBoardSettings(VisualElement window)
  {
   var automatic=new Toggle("Smart automatic color / group choices"){value=autoChoices};automatic.RegisterValueChangedCallback(e=>{autoChoices=e.newValue;PlayerPrefs.SetInt("autoChoices",autoChoices?1:0);});window.Add(automatic);
   var completed=new Toggle("Move complete sets to the left"){value=completeFirst};
   completed.RegisterValueChangedCallback(e=>{completeFirst=e.newValue;PlayerPrefs.SetInt("completeFirst",completeFirst?1:0);});window.Add(completed);
   int current=Array.IndexOf(PresentationOptions.Durations,animationPace);
   var pace=new SliderInt("Animation speed",0,4){value=Mathf.Max(0,current)};
   var label=Widgets.Text("","muted");Action<int> describe=step=>label.text=(step+1)+" / 5 • "+PresentationOptions.SpeedNames[step]+" • Card travel "+PresentationOptions.Durations[step].ToString("0.00")+"s";
   pace.RegisterValueChangedCallback(e=>{int step=Mathf.Clamp(e.newValue,0,4);animationPace=PresentationOptions.Durations[step];PlayerPrefs.SetInt("animationStep15",step);PlayerPrefs.SetFloat("animationPace",animationPace/.65f);describe(step);});
   window.Add(pace);var stops=Widgets.Box("speed-stops");for(int i=0;i<5;i++){int step=i;stops.Add(Widgets.Button((i+1).ToString(),()=>pace.value=step,"speed-stop"));}window.Add(stops);window.Add(label);describe(pace.value);
  }
  Action activeHintHide;
  void HideHint(){var hide=activeHintHide;activeHintHide=null;if(hide!=null)hide();}
  void HintBadge(VisualElement parent,string icon,string text)
  {
   var badge=Widgets.Button(icon,()=>{},"hint-badge");parent.Add(badge);
   VisualElement tip=null;IVisualElementScheduledItem expiry=null;
   Action hide=null;hide=()=>{if(expiry!=null)expiry.Pause();expiry=null;if(tip!=null)tip.RemoveFromHierarchy();tip=null;if(activeHintHide==hide)activeHintHide=null;};
   Action show=()=>{HideHint();hide();activeHintHide=hide;expiry=root.schedule.Execute(hide).StartingIn(5000);tip=PlayerText(text,"hint-popover");tip.pickingMode=PickingMode.Ignore;root.Add(tip);var p=root.WorldToLocal(badge.worldBound.center);tip.style.left=Mathf.Clamp(p.x-120,8,Mathf.Max(8,root.resolvedStyle.width-288));tip.style.top=Mathf.Clamp(p.y+15,8,Mathf.Max(8,root.resolvedStyle.height-125));};
   badge.clicked+=()=>{if(tip==null)show();else hide();};
   badge.RegisterCallback<MouseEnterEvent>(_=>show());badge.RegisterCallback<MouseLeaveEvent>(_=>hide());
   badge.RegisterCallback<FocusInEvent>(_=>show());badge.RegisterCallback<FocusOutEvent>(_=>hide());
   badge.RegisterCallback<ClickEvent>(e=>e.StopPropagation());parent.RegisterCallback<DetachFromPanelEvent>(_=>hide());
  }
  void AddSharedColorBadge(VisualElement tile,Group color)
  {
   if(game.Players[0].Sets.Any(s=>s.Color==color&&game.Complete(s)))return;
   var owners=game.Players.Skip(1).Where(p=>p.Sets.Any(s=>s.Color==color&&s.Cards.Count>0&&!game.Complete(s))).Select(p=>p.Name+": "+p.Sets.Where(s=>s.Color==color&&!game.Complete(s)).Sum(s=>s.Cards.Count));
   string list=string.Join("\n",owners.ToArray());if(list.Length>0)HintBadge(tile,"♟",Catalog.Labels[(int)color]+" on other tables\n"+list);
  }
  void AddPlayerMatchBadge(VisualElement tile,int player)
  {
   var yours=game.Players[0].Sets.Select(s=>s.Color).ToArray();var matches=game.Players[player].Sets.Where(s=>yours.Contains(s.Color)&&!game.Complete(s)&&!game.Players[0].Sets.Any(own=>own.Color==s.Color&&game.Complete(own))).Select(s=>Catalog.Labels[(int)s.Color]).Distinct().ToArray();
   if(matches.Length>0)HintBadge(tile,"+","Has colors you already own:\n"+string.Join(", ",matches));
  }
  Group? AssignedColor(int id)
  {
   var set=game.Players.SelectMany(p=>p.Sets).FirstOrDefault(s=>s.Cards.Contains(id));return set==null?(Group?)null:set.Color;
  }
  void AddMatchBadge(VisualElement tile,int id)
  {
   var color=AssignedColor(id);if(!color.HasValue)return;if(game.Players.SelectMany(p=>p.Sets).Any(s=>s.Cards.Contains(id)&&game.Complete(s))||game.Players[0].Sets.Any(s=>s.Color==color.Value&&game.Complete(s)))return;
   int count=game.Players[0].Sets.Where(s=>s.Color==color.Value).Sum(s=>s.Cards.Count);
   if(count>0)HintBadge(tile,"+", "Matches your "+Catalog.Labels[(int)color.Value]+" properties ("+count+")."+(game.Cards[id].Kind==Kind.Wild?"\nArrives as "+Catalog.Labels[(int)color.Value]+". Rotate later for 1 play.":""));
  }
  VisualElement TableCard(int id,Action click)
  {
   var card=Widgets.Card(game.Cards[id],click);var color=AssignedColor(id);
   if(color.HasValue){card.AddToClassList("has-rent");Widgets.Tint(card,Widgets.GroupColor(color.Value));card.Q(className:"card-stripe").style.backgroundColor=Widgets.GroupColor(color.Value);var group=game.Players.SelectMany(p=>p.Sets).First(s=>s.Cards.Contains(id));card.Add(Widgets.Text("Set rent "+game.Rent(group)+"M","card-rent"));if(game.Complete(group)){var glow=Widgets.Box("complete-glow");glow.pickingMode=PickingMode.Ignore;card.Add(glow);card.tooltip+="\nPart of a complete set.";}if(game.Cards[id].Kind!=Kind.Wild)card.Q<Label>(className:"card-kind").text=Catalog.Labels[(int)color.Value];}
   return card;
  }
  void ColorKey(VisualElement parent,IEnumerable<Group> colors)
  {
   var row=Widgets.Box("color-key");var list=colors.Distinct().ToArray();
   foreach(var color in list){var chip=Widgets.Text(Catalog.Labels[(int)color],"color-chip");Widgets.Tint(chip,Widgets.GroupColor(color));chip.tooltip=Catalog.Labels[(int)color];row.Add(chip);}parent.Add(row);
   FitChildren(row,67,21,1);
  }
 }
}
