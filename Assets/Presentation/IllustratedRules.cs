using System.Linq;
using DealNight.Core;
using UnityEngine.UIElements;
namespace DealNight.Presentation {
 public sealed partial class TableApp {
  void IllustratedRules(){
   var w=Modal("The table guide",false);w.Add(Widgets.Text("Three complete sets. Three plays per turn. Start with the basics, then explore the cards.","muted"));
   var tabs=Widgets.Box("rules-tabs");w.Add(tabs);var scroll=new ScrollView();scroll.AddToClassList("illustrated-rules");w.Add(scroll);
   System.Action<int> show=page=>{
    scroll.contentContainer.Clear();var catalog=Catalog.Create();
    System.Action<string,string,Card> section=(title,text,card)=>{
     var row=Widgets.Box("rule-entry");scroll.Add(row);if(card!=null){var frame=Widgets.Box("rule-card-frame");var visual=Widgets.Card(card,()=>{});visual.AddToClassList("rule-card");IgnorePointer(visual);frame.Add(visual);row.Add(frame);}
     var copy=Widgets.Box("rule-copy");row.Add(copy);copy.Add(Widgets.Text(title,"rule-title"));copy.Add(Widgets.Text(text,"rule-body"));
    };
    System.Func<Kind,Card> sample=k=>catalog.First(c=>c.Kind==k);
    if(page==0){
     section("Win with THREE different colors","Complete three property sets on your turn after outstanding actions resolve. Brown / dark blue need 2 cards; pink, orange, red, yellow, green and light blue need 3; railroads need 4; utilities need 2. Money does not count as a set.",catalog.First(c=>c.Name=="Park Place"));
     section("Draw → play up to three → end","Everyone starts with five cards. At the beginning of your turn, draw TWO cards, or FIVE if your hand is empty. Then take up to three plays. Banking a card, playing a card and rotating a laid wildcard each cost a play. You can end early.",sample(Kind.PassGo));
     section("Seven-card hand limit","You can hold more than seven during your turn. Press End turn and discard until seven remain; you cannot finish the turn before discarding. Running out of cards does not trigger an immediate refill.",null);
     section("Use the board","Tap or click a card to inspect it. Tap a group to view its properties and rearrangement choices. The play counter sits above your hand. Hold the board button on phone, or the right mouse button on desktop, to reveal the board behind a menu.",null);
    }else if(page==1){
     section("Bank before you need it","Money, eligible actions and valued wildcards may be banked. They then act ONLY as money, permanently. Natural properties are played as properties. Cards in your hand cannot pay a bill.",sample(Kind.Money));
     section("Pay from the table","Pay using your bank, laid properties or buildings. Suggested payment is editable. No change is given: a 10M card pays a 2M bill with 8M lost. If you cannot cover a bill, give all assets with payment value; no ongoing debt remains.",sample(Kind.DebtCollector));
     section("What the receiver gets","Money and previously banked cards join the receiver’s bank. Paid properties join their table, preserving wildcard colors. Paid houses/hotels become bank money. A 0M universal wildcard cannot pay debts. A red warning marks properties whose color the receiver already owns.",sample(Kind.Wild));
    }else if(page==2){
     foreach(var card in catalog.Where(c=>c.Kind==Kind.Rent).GroupBy(c=>string.Join(",",c.Colors)).Select(g=>g.First()))section(card.Colors.Length==10?"ANY COLOR → ONE opponent":string.Join(" / ",card.Colors.Select(c=>Catalog.Labels[(int)c]))+" → ALL opponents",Catalog.Describe(card),card);
     section("Double the Rent",Catalog.Describe(sample(Kind.DoubleRent))+" Rent and one double cost two plays; rent and two doubles cost three. Doubling is optional.",sample(Kind.DoubleRent));
     section("Who should you charge?","Suggested targets consider the amount collectible and public bank balances. A bank shortfall may require property/building payment, but the payer chooses the assets and may reject the action. Advice cannot predict their hidden hand.",null);
    }else if(page==3){
     foreach(var kind in new[]{Kind.PassGo,Kind.DebtCollector,Kind.Birthday,Kind.SlyDeal,Kind.ForcedDeal,Kind.DealBreaker,Kind.JustSayNo})section(sample(kind).Name,Catalog.Describe(sample(kind)),sample(kind));
     section("Use your No where it matters","A small bill you can pay exactly may be worth allowing. Protect a complete set or stop an opponent winning when possible. A No can be countered by another No, restoring the original action. Responses do not spend turn plays. Against an all-opponents charge, a No protects only that responder.",sample(Kind.JustSayNo));
    }else{
     section("Two-color property wildcards","Choose one printed color when playing. During your turn, you may explicitly rotate a laid wildcard to its other legal color for ONE play. Same-color regrouping is free. Received or stolen wildcards retain their existing color; receiving adds no extra play cost.",catalog.First(c=>c.Kind==Kind.Wild&&c.Colors.Length==2));
     section("Universal property wildcard","Choose any color. This 0M card cannot pay debts or be banked. It needs a valued property in its group to collect rent or form a valid complete set.",catalog.First(c=>c.Kind==Kind.Wild&&c.Value==0));
     section("House",Catalog.Describe(sample(Kind.House)),sample(Kind.House));section("Hotel",Catalog.Describe(sample(Kind.Hotel)),sample(Kind.Hotel));
     section("If a built set breaks","Buildings give no bonus while their set is incomplete. They can be moved to another eligible complete street set on your turn. Deal Breaker takes the complete set AND its attached buildings.",null);
    }
   };
   string[] labels={"Basics","Bank & payment","Rent","Actions & defense","Wilds & buildings"};for(int i=0;i<labels.Length;i++){int page=i;tabs.Add(Widgets.Button(labels[i],()=>show(page)));}show(0);
   w.Add(Widgets.Button("Back",()=>{if(game!=null&&paused)Pause();else Close();}));
  }
 }
}
