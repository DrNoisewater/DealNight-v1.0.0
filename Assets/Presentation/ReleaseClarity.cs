using System;
using System.Linq;
using DealNight.Core;
using UnityEngine.UIElements;
namespace DealNight.Presentation {
 public sealed partial class TableApp {
  void AddPeekHint(VisualElement window){
   var hint=Widgets.Box("peek-hint-row");window.Add(hint);
   if(TouchMode){hint.Add(Widgets.Text("Hold the board button to view board","peek-hint"));return;}
   hint.Add(Widgets.Text("Hold","peek-hint"));var mouse=Widgets.Box("right-mouse-icon");mouse.tooltip="Right mouse button";
   mouse.Add(Widgets.Box("right-mouse-button"));hint.Add(mouse);hint.Add(Widgets.Text("to view board","peek-hint"));
  }
  string ChargeSummary(Command c){int due=ChargeAdvice.Amount(game,c);string scope=c.Target>=0?"ONE opponent: "+game.Players[c.Target].Name:"ALL "+(game.Players.Count-1)+" opponents, separately";
   string text=game.Cards[c.Card].Name+" • "+scope+"\n"+due+"M "+(c.Target<0?"EACH":"due");
   if(game.Cards[c.Card].Kind==Kind.Rent)text+="\nBased on your "+SetName(c.Set)+(c.Extras.Length>0?" • "+(1<<c.Extras.Length)+"× rent":"");
   foreach(int target in c.Target>=0?new[]{c.Target}:Enumerable.Range(1,game.Players.Count-1).ToArray())text+="\n"+game.Players[target].Name+": "+game.BankTotal(target)+"M bank. "+ChargeAdvice.Outlook(game,target,due);
   return text+"\nThey choose which assets to pay; a particular property is not guaranteed.";
  }
  string RejectedExplanation(Effect e){string name=e.Card>=0?game.Cards[e.Card].Name:e.Kind.ToString();string who=e.Target==0?"YOU":game.Players[e.Target].Name;
   return name+" was rejected. "+(e.Amount>0?who+" will not pay "+e.Amount+"M.":who+" keeps the targeted property or set; no exchange happens.")+" Continue to accept this result, or counter with Just Say No to restore the original action.";
  }
 }
}
