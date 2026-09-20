using System;
using System.Collections.Generic;
using System.Linq;
using DealNight.Core;
using UnityEngine.UIElements;
namespace DealNight.Presentation
{
 public sealed partial class TableApp
 {
  string SetName(int id)
  {
   var set=game.Players.SelectMany(p=>p.Sets).FirstOrDefault(s=>s.Id==id);
   return set==null?"New property group":Catalog.Labels[(int)set.Color]+" • "+string.Join(", ",set.Cards.Select(c=>game.Cards[c].Name).ToArray());
  }
  string ActionSummary(Command c)
  {
   if(c.Op==Op.Play&&(game.Cards[c.Card].Kind==Kind.Rent||game.Cards[c.Card].Kind==Kind.Birthday||game.Cards[c.Card].Kind==Kind.DebtCollector))return ChargeSummary(c);
   if(c.Op==Op.Move)return "Move "+game.Cards[c.Card].Name+" to "+(c.Set<0?Catalog.Labels[(int)c.Color]:SetName(c.Set));
   string text=game.Cards[c.Card].Name+"\n"+Catalog.Describe(game.Cards[c.Card]);
   if(c.Target>=0)text+="\nPlayer: "+game.Players[c.Target].Name;
   if(c.Take>=0)text+="\nYou receive: "+game.Cards[c.Take].Name;
   if(c.Give>=0)text+="\nYou give: "+game.Cards[c.Give].Name;
   if(c.Set>=0)text+="\n"+SetName(c.Set);
   if(c.Extras.Length>0)text+="\nRent ×"+(1<<c.Extras.Length);
   return text;
  }
  VisualElement Grid(VisualElement parent,bool cards=true)
  {
   var grid=Widgets.Box("fit-modal-grid");parent.Add(grid);FitChildren(grid,cards?170:235,cards?215:175,1);return grid;
  }
  void Inspect(int id)
  {
   selected=id;var card=game.Cards[id];var w=Modal(card.Name);
   var row=Widgets.Box("inspect-row");w.Add(row);row.Add(Widgets.Card(card,()=>{}));
   var detail=Widgets.Box("inspect-detail");row.Add(detail);detail.Add(Widgets.Text(Catalog.Describe(card),"description"));
   var options=game.Legal(0).Where(c=>c.Card==id&&c.Op!=Op.Move).ToList();
   var play=options.Where(c=>c.Op==Op.Play).ToList();
   if(play.Count>0)detail.Add(Widgets.Button("Play card",()=>Choose(play,()=>Inspect(id)),"primary"));
   var bank=options.FirstOrDefault(c=>c.Op==Op.Bank);
   if(bank!=null)detail.Add(Widgets.Button("Bank • "+card.Value+"M",()=>Send(bank)));
   if(play.Count==0)detail.Add(Widgets.Text(card.Kind==Kind.JustSayNo?"Use this when another player acts against you.":card.Kind==Kind.DoubleRent?"Play a Rent card to attach this modifier.":"No legal play right now. Check your remaining plays and available properties.","muted"));
   w.Add(Widgets.Button("Back to table",Close));
  }
  // Reduce the legal command set at each step. Choices never mutate the game.
  // A final command is still checked by Game.Execute immediately before use.
  void Choose(List<Command> choices,Action back,int stage=0)
  {
   if(choices.Count==0){feedback="This choice is no longer available.";Render();return;}
   if(stage>5){Confirm(choices[0],back);return;}
   Func<Command,int> key;
   switch(stage){case 0:key=c=>c.Target;break;case 1:key=c=>c.Take;break;case 2:key=c=>c.Give;break;case 3:key=c=>(int)c.Color;break;case 4:key=c=>c.Set;break;default:key=c=>c.Extras.Length;break;}
   if(autoChoices&&stage==4&&(game.Cards[choices[0].Card].Property||choices[0].Op==Op.Move))
   {
    int auto=PlacementPolicy.AutomaticDestination(game.Players[0].Sets,choices.Select(c=>c.Set));
    if(auto>=0){Choose(choices.Where(c=>c.Set==auto).ToList(),back,stage+1);return;}
   }
   if(autoChoices&&stage==3&&choices[0].Op==Op.Play&&game.Cards[choices[0].Card].Kind==Kind.Wild){var preferred=choices.Where(c=>!game.Players[0].Sets.Any(s=>s.Color==c.Color&&game.Complete(s))).ToList();if(preferred.Count>0)choices=preferred;}
   var groups=choices.GroupBy(key).ToList();
   if(groups.Count==1&&!(stage<=2&&groups[0].Key>=0)){Choose(choices,back,stage+1);return;}
   string[] headings={"Choose a player","Choose their property","Choose your property to give","Choose a color","Choose a property group","Boost your rent?"};
   var w=Modal(headings[stage]);w.Add(Widgets.Text(game.Cards[choices[0].Card].Name+" • "+(stage==1?"Select the card you want to receive.":stage==2?"Select the card you want to offer.":"Choose one option to continue."),"muted"));
   var grid=Grid(w,stage==1||stage==2);
   bool charge=choices[0].Op==Op.Play&&(game.Cards[choices[0].Card].Kind==Kind.Rent||game.Cards[choices[0].Card].Kind==Kind.DebtCollector);
   int recommended=charge?key(choices.OrderByDescending(c=>ChargeAdvice.Score(game,c)).First()):-999;
   foreach(var group in groups)
   {
    var subset=group.ToList();int value=group.Key;
    Action next=()=>Choose(subset,()=>Choose(choices,back,stage),stage+1);
    if(stage==1||stage==2){var preview=TableCard(value,next);if(stage==1)AddMatchBadge(preview,value);else if(subset[0].Target>=0)AddRecipientWarning(preview,value,subset[0].Target);grid.Add(preview);continue;}
    string title,detail;
    if(stage==0){title=game.Players[value].Name;detail=game.CompleteCount(value)+" / 3 complete sets";}
    else if(stage==3){title=Catalog.Labels[value];detail="Assign this property to "+Catalog.Labels[value]+".";}
    else if(stage==4){if(value<0){title="Start a new group";detail="Keep it separate";}else{var targetSet=game.Players.SelectMany(p=>p.Sets).First(x=>x.Id==value);title=Catalog.Labels[(int)targetSet.Color];detail=targetSet.Cards.Count+" / "+Catalog.Sizes[(int)targetSet.Color]+" properties • Rent "+game.Rent(targetSet)+"M";}}
    else {title=value==0?"Normal rent":(1<<value)+"× rent";detail=(value+1)+" card play"+(value==0?"":"s");}
    var tile=Widgets.Button("",next,"choice-tile");if(stage==0)tile.Add(PlayerBadge(value,"choice-name"));else tile.Add(Widgets.Text(title,"choice-name"));tile.Add(Widgets.Text(detail,"choice-summary"));
    if(stage==0){tile.AddToClassList("player-choice");tile.Add(Widgets.Text(game.BankTotal(value)+"M","bank-corner"));ColorKey(tile,game.Players[value].Sets.Select(s=>s.Color));AddPlayerMatchBadge(tile,value);}
    if(stage==3){Widgets.Tint(tile,Widgets.GroupColor((Group)value));var preview=Widgets.Card(new Card(-1,Kind.Property,Catalog.Labels[value],Catalog.Values[value],(Group)value),()=>{});preview.AddToClassList("assignment-preview");IgnorePointer(preview);tile.Add(preview);}
    if(stage==4&&value>=0){var set=game.Players.SelectMany(p=>p.Sets).First(x=>x.Id==value);Widgets.Tint(tile,Widgets.GroupColor(set.Color));ColorKey(tile,new[]{set.Color});}
    if(charge&&(stage==0||stage==4||stage==5)){
     var best=subset.OrderByDescending(c=>ChargeAdvice.Score(game,c)).First();int due=ChargeAdvice.Amount(game,best);tile.AddToClassList("charge-choice");
     string advice=(value==recommended?"Suggested • ":"")+due+"M"+(best.Target<0?" each • ALL opponents":" • ONE opponent");
     if(best.Target>=0)advice+="\n"+ChargeAdvice.Outlook(game,best.Target,due);
     if(best.Extras.Length>0)advice+="\nIncludes "+best.Extras.Length+" double(s); "+(1+best.Extras.Length)+" plays";
     tile.Add(Widgets.Text(advice,"charge-advice"));if(value==recommended)tile.AddToClassList("recommended-charge");
    }
    grid.Add(tile);
   }
   w.Add(Widgets.Button("Back",back));
  }
  void SetInspect(int player,PropertySet set)
  {
   var w=Modal(Catalog.Labels[(int)set.Color]);w.Add(PlayerBadge(player));
   w.Add(Widgets.Text(set.Cards.Count+" of "+Catalog.Sizes[(int)set.Color]+" properties • Rent "+game.Rent(set)+"M","description"));
   var grid=Grid(w);
   foreach(int id in set.Cards.Concat(new[]{set.House,set.Hotel}.Where(x=>x>=0)))
   {
    int card=id;
    grid.Add(TableCard(id,()=>{
     var detail=Modal(game.Cards[card].Name);detail.Add(Widgets.Text(Catalog.Describe(game.Cards[card]),"description"));
     var moves=player==0?game.Legal(0).Where(c=>c.Op==Op.Move&&c.Card==card).ToList():new List<Command>();
     if(moves.Count>0)detail.Add(Widgets.Button(game.Cards[card].Kind==Kind.Wild?"Rotate / regroup wildcard":"Move property",()=>Choose(moves,()=>SetInspect(player,set)),"primary"));
     detail.Add(Widgets.Button("Back",()=>SetInspect(player,set)));
    }));
   }
   w.Add(Widgets.Button("Back to table",Close));
  }
  void Payment()
  {
   var w=Modal("Pay "+game.Pending.Amount+"M",false);
   w.Add(PlayerBadge(game.Pending.Source,"payment-recipient"," • receiving payment"));w.Add(Widgets.Text("Suggested cards selected. Click to adjust. No change is returned.","description"));
   var selectedAssets=new HashSet<int>();var tiles=new Dictionary<int,VisualElement>();
   var status=Widgets.Text("","payment-total");w.Add(status);
   var paymentGrid=Grid(w);
   var confirm=Widgets.Button("Pay selected cards",()=>Send(new Command(Op.Pay,0){Extras=selectedAssets.OrderBy(x=>x).ToArray()}),"primary");
   Action refresh=()=>{
    int total=selectedAssets.Sum(id=>game.Cards[id].Value);int due=game.Pending.Amount;
    string error=game.PaymentError(new Command(Op.Pay,0){Extras=selectedAssets.ToArray()});
    status.text=total+"M selected / "+due+"M due"+(total>due?" • "+(total-due)+"M overpayment":total<due&&error==null?" • All remaining assets":total<due?" • Select "+(due-total)+"M more":" • Ready to pay");
    confirm.SetEnabled(error==null);confirm.text=total==0?"Confirm • nothing to pay":"Pay "+total+"M";
    foreach(var pair in tiles)pair.Value.EnableInClassList("selected-payment",selectedAssets.Contains(pair.Key));
   };
   var assets=game.Assets(0).ToArray();
   foreach(bool bank in new[]{true,false})
   {
    var section=assets.Where(id=>game.Players[0].Bank.Contains(id)==bank).ToArray();if(section.Length==0)continue;
    var grid=paymentGrid;
    foreach(int id in section){int cid=id;Action toggle=()=>{if(!selectedAssets.Add(cid))selectedAssets.Remove(cid);refresh();};var tile=bank?Widgets.BankCard(game.Cards[id],toggle):TableCard(id,toggle);tile.Add(Widgets.Text("✓ SELECTED","selection-check"));tile.AddToClassList("payment-card");AddRecipientWarning(tile,id,game.Pending.Source);tiles[id]=tile;grid.Add(tile);}
   }
   var actions=Widgets.Box("row");w.Add(actions);
   Action suggest=()=>{selectedAssets.Clear();var suggestion=new Computer(Difficulty.Normal,0).Choose(game.Observe(0),game.Legal(0));foreach(int id in suggestion.Extras)selectedAssets.Add(id);refresh();};actions.Add(Widgets.Button("Suggest payment",suggest));
   actions.Add(Widgets.Button("Clear",()=>{selectedAssets.Clear();refresh();}));actions.Add(confirm);
   if(assets.Length==0)paymentGrid.Add(Widgets.Text("You have no assets with a payment value.","description"));suggest();
  }
 }
}
