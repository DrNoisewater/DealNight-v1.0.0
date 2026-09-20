using System;
using System.Collections.Generic;
using System.Linq;
namespace DealNight.Core {
 public sealed class TutorialStep {
  public string Title,Instruction,Result;public Func<Game,Command> Action;
 }
 // Deliberately staged practice scenes. All actions use the normal engine validator.
 // No random AI decisions: one coach, fixed cards, fixed responses, repeatable lessons.
 public sealed class TutorialSession {
  public const int LessonCount=15;
  public Game World {get;private set;}public int Lesson {get;private set;}public int StepIndex {get;private set;}
  public readonly List<TutorialStep> Steps=new List<TutorialStep>();readonly string name;
  public TutorialStep Current {get{return Steps[StepIndex];}}
  public TutorialSession(string playerName){name=playerName;Load(0);}
  public bool Next(){StepIndex++;if(StepIndex<Steps.Count)return true;if(Lesson+1>=LessonCount)return false;Load(Lesson+1);return true;}
  public Command Expected(){return Current.Action(World);}
  public bool Matches(Command c){var e=Expected();if(c.Op==e.Op&&c.Actor==e.Actor&&(c.Op==Op.No||c.Op==Op.Discard)&&c.Card>=0&&World.Cards[c.Card].Kind==World.Cards[e.Card].Kind&&World.Cards[c.Card].Value==World.Cards[e.Card].Value)return true;return c.Op==e.Op&&c.Actor==e.Actor&&c.Card==e.Card&&c.Target==e.Target&&c.Set==e.Set&&c.Take==e.Take&&c.Give==e.Give&&c.Color==e.Color&&c.Extras.OrderBy(x=>x).SequenceEqual(e.Extras.OrderBy(x=>x));}
  void Add(string title,string instruction,string result,Func<Game,Command> action){Steps.Add(new TutorialStep{Title=title,Instruction=instruction,Result=result,Action=action});}
  int Take(Func<Card,bool> match){int id=World.Deck.First(i=>match(World.Cards[i]));World.Deck.Remove(id);return id;}
  int Hand(int p,Kind kind,int value=-1){int id=Take(c=>c.Kind==kind&&(value<0||c.Value==value));World.Players[p].Hand.Add(id);return id;}
  int Property(int p,Group color){int id=Take(c=>c.Kind==Kind.Property&&c.Colors[0]==color);World.Players[p].Hand.Add(id);return id;}
  int Wild(int p,Group a,Group b){int id=Take(c=>c.Kind==Kind.Wild&&c.Colors.Length==2&&c.Colors.Contains(a)&&c.Colors.Contains(b));World.Players[p].Hand.Add(id);return id;}
  int RentCard(int p,Group color,bool any=false){int id=Take(c=>c.Kind==Kind.Rent&&c.Colors.Contains(color)&&c.Colors.Length==(any?10:2));World.Players[p].Hand.Add(id);return id;}
  int Cash(int p,int value){int id=Take(c=>c.Kind==Kind.Money&&c.Value==value);World.Players[p].Bank.Add(id);return id;}
  PropertySet Set(int p,Group color,int count){var s=new PropertySet{Id=100+World.Players.Sum(x=>x.Sets.Count),Color=color};World.Players[p].Sets.Add(s);for(int i=0;i<count;i++)s.Cards.Add(Take(c=>c.Kind==Kind.Property&&c.Colors[0]==color));return s;}
  static Command Play(Game g,int actor,int id){return g.Legal(actor).First(c=>c.Op==Op.Play&&c.Card==id);}
  static Command Pay(Game g){return new Computer(Difficulty.Normal,1).Choose(g.Observe(g.DecisionPlayer),g.Legal(g.DecisionPlayer));}
  void CoachTurn(){string error;if(!World.Execute(new Command(Op.End,0),out error))throw new InvalidOperationException(error);}
  void Load(int lesson){
   Lesson=lesson;StepIndex=0;Steps.Clear();World=new Game(2,19092026,name);World.Players[1].Name="Dr. Noisewater";
   foreach(var p in World.Players){p.Hand.Clear();p.Bank.Clear();p.Sets.Clear();}World.Deck.Clear();World.Discard.Clear();World.Deck.AddRange(World.Cards.Select(c=>c.Id));World.History.Clear();World.History.Add("Practice scene "+(lesson+1)+" • cards are arranged for this lesson.");
   switch(lesson){
    case 0:{
     int cash=Hand(0,Kind.Money,2),a=Property(0,Group.Brown),b=Property(0,Group.Brown);
     Add("Build your safety net","Win by completing THREE sets of DIFFERENT colors. First tap your 2M card and choose Bank. Money in your hand cannot pay a bill.","Your bank now holds 2M. Banking used one of your three plays.",g=>new Command(Op.Bank,0,cash));
     Add("Start a property set","Tap Mediterranean Avenue, then Play card. Properties on the table build toward your winning sets.","One brown property is down. Brown needs two cards to complete.",g=>Play(g,0,a));
     Add("Complete your first set","Play Baltic Avenue into your brown group. A matching card joins automatically when there is one clear destination.","Brown is complete: 1 of 3 winning colors. All three plays are used; your hand area dims. You can end early on other turns.",g=>g.Legal(0).First(c=>c.Op==Op.Play&&c.Card==b&&c.Set==g.Players[0].Sets[0].Id));
     Add("End your turn","Press End turn. You do not have to use all three plays, but you cannot play a fourth card.","Coach takes a turn. The next scene demonstrates drawing with an empty hand.",g=>new Command(Op.End,0));break;
    }
    case 1:{
     Add("An empty hand refills next turn","This practice scene starts with your hand empty. End your turn; you do NOT draw immediately when you run out.","Your empty hand stays empty during the coach’s turn.",g=>new Command(Op.End,0));
     Add("Watch the next-turn draw","Watch Coach end their turn. A normal turn draws TWO cards; an EMPTY hand draws FIVE instead. Your opening turn normally starts with five plus two drawn.","You now have five cards and three new plays. Emptying your hand is sometimes useful.",g=>new Command(Op.End,1));break;
    }
    case 2:{
     for(int i=0;i<7;i++)Hand(0,Kind.Money);int pass=Hand(0,Kind.PassGo);
     Add("Pass Go draws two","Play Pass Go. It costs one play and draws two cards. This scene deliberately starts with eight cards so you can practice the hand limit.","Pass Go left your hand and two cards arrived. You now have nine cards.",g=>Play(g,0,pass));
     Add("Finish with at most seven","End your turn now. You may keep more than seven DURING your turn, but must discard down to seven before it finishes.","Choose two cards to discard. Discards cost no plays and go to the shared discard pile.",g=>new Command(Op.End,0));
     Add("Discard one card","Discard the first 1M money card for this practice step.","Eight cards remain; one more must go.",g=>g.Legal(0).First(c=>c.Op==Op.Discard&&g.Cards[c.Card].Kind==Kind.Money&&g.Cards[c.Card].Value==1));
     Add("Discard to seven","Discard another 1M money card. In a real game, keep cards that help complete sets or defend them.","Seven cards remain. The turn advances automatically.",g=>g.Legal(0).First(c=>c.Op==Op.Discard&&g.Cards[c.Card].Kind==Kind.Money&&g.Cards[c.Card].Value==1));break;
    }
    case 3:{
     var rail=Set(0,Group.Railroad,3);int wild=Wild(0,Group.Railroad,Group.Utility);
     Add("Choose a wildcard’s color","Play the Railroad / Utility wildcard as Utility for this demonstration. Its printed colors are the only legal choices. Playing it uses one play.","The wildcard is currently a Utility. Its other color does not count yet.",g=>g.Legal(0).First(c=>c.Op==Op.Play&&c.Card==wild&&c.Color==Group.Utility));
     Add("Rotate a laid-down wildcard","Tap your Utility group, then the wildcard, then Rotate / regroup. Choose Railroad and add it to your three railroad cards.","Railroad is complete. Explicitly rotating to another color used ONE additional play. Same-color regrouping is free on your turn.",g=>g.Legal(0).First(c=>c.Op==Op.Move&&c.Card==wild&&c.Color==Group.Railroad&&c.Set==rail.Id));break;
    }
    case 4:{
     Set(0,Group.Brown,2);Cash(1,2);int rent=RentCard(0,Group.Brown);
     Add("Paired-color rent: everyone pays","Play Brown / Light blue Rent on your brown set. You own brown, so it charges 2M to EACH opponent. There is only one coach here; with more players, everyone is charged separately.","Coach has received a 2M request. Rent comes from your chosen group, not the money value printed on the rent card.",g=>Play(g,0,rent));
     Add("Watch Coach allow rent","Coach will allow the action. Each affected player responds separately.","Now Coach must choose payment from their bank or laid-down assets.",g=>new Command(Op.Pass,1));
     Add("Receive the payment","Watch Coach pay 2M from the bank. No change is returned, so exact payments are useful.","Your bank received 2M. Properties received as payment would join your table instead.",Pay);break;
    }
    case 5:{
     Set(0,Group.DarkBlue,1);Cash(1,1);Set(1,Group.Orange,1);int rent=RentCard(0,Group.DarkBlue,true);
     Add("Any-color rent: choose ONE opponent","Play Any-color rent, choose Coach, and use dark blue for 3M. This rainbow card works with ANY group you own, but charges only ONE opponent.","Coach has only 1M banked. If this action proceeds, banked money alone cannot cover the bill.",g=>Play(g,0,rent));
     Add("Predict a property payment","Watch Coach allow the rent. Public bank balances help choose targets; you cannot choose exactly which asset a payer gives.","The payer must cover the bill using bank and/or table assets. Cards in hand are never payment.",g=>new Command(Op.Pass,1));
     Add("Watch a mixed payment","Coach pays the 1M bank card and a 2M orange property. When short of all assets, a player gives all valued assets and the remaining debt disappears.","You received 1M in your bank and an orange property on your table. A 0M universal wildcard cannot pay debts.",Pay);break;
    }
    case 6:{
     Set(0,Group.Brown,2);int h=Hand(0,Kind.House),hotel=Hand(0,Kind.Hotel);
     Add("Build a house","Play House onto your complete brown set. Only complete STREET sets qualify: never Utility or Railroad. Each set may have one house.","Brown rent rose from 2M to 5M. A house adds 3M.",g=>Play(g,0,h));
     Add("Then build a hotel","Play Hotel on the same set. A hotel requires an existing house and adds 4M MORE rent.","Brown now rents for 9M: 2 base + 3 house + 4 hotel. Buildings stop adding rent if the set becomes incomplete.",g=>Play(g,0,hotel));break;
    }
    case 7:{
     Set(0,Group.Brown,2);Cash(1,10);int rent=RentCard(0,Group.Brown),d=Hand(0,Kind.DoubleRent);
     Add("Double rent uses another play","Play Brown / Light blue Rent and select 2× rent. Rent plus one Double the Rent uses TWO plays total.","Your 2M rent became 4M. Two doubles would make 8M and use all three plays.",g=>g.Legal(0).First(c=>c.Card==rent&&c.Op==Op.Play&&c.Extras.Contains(d)));
     Add("Watch Coach allow the charge","Coach allows this 4M bill.","Coach’s only asset is a 10M bank card. They cannot make change.",g=>new Command(Op.Pass,1));
     Add("No change is given","Watch the 10M payment for a 4M bill. You receive the whole card; Coach loses the extra 6M.","Keeping small bank cards can avoid expensive overpayments.",Pay);break;
    }
    case 8:{
     int cash=Cash(0,2);Hand(0,Kind.JustSayNo);int birthday=Hand(1,Kind.Birthday);CoachTurn();
     Add("Save your defense for a bigger threat","Watch Coach play It’s My Birthday. Every opponent owes 2M separately. You have an exact 2M bank card and a Just Say No.","This is a small, affordable bill. In this exercise, keep your Just Say No for a threatened set.",g=>Play(g,1,birthday));
     Add("Allow a small bill","Choose Allow action. You CAN say no to money requests, but you do not need to use it every time.","The payment screen suggests a starting selection; you can adjust it before paying.",g=>new Command(Op.Pass,0));
     Add("Pay from your bank","Pay the selected 2M bank card. Never pay using a card still in your hand.","Your Just Say No is still in hand for a stronger threat. Normal turn plays were not used while responding.",g=>new Command(Op.Pay,0){Extras=new[]{cash}});break;
    }
    case 9:{
     Set(0,Group.Brown,2);Hand(0,Kind.JustSayNo);int counter=Hand(1,Kind.JustSayNo);Hand(0,Kind.JustSayNo);int breaker=Hand(1,Kind.DealBreaker);CoachTurn();
     Add("Protect a complete set","Watch Coach play Deal Breaker against your brown set. It steals the WHOLE complete set, including any house and hotel.","Losing a full set moves you farther from winning. This is a good time for Just Say No.",g=>Play(g,1,breaker));
     Add("Just Say No","Choose Just Say No to reject this Deal Breaker. Responding costs the card but no normal turn play.","The action is rejected. The original attacker may counter using another Just Say No; you could counter again.",g=>g.Legal(0).First(c=>c.Op==Op.No));
     Add("A No can be countered","Watch Dr. Noisewater counter your Just Say No. This restores the ORIGINAL Deal Breaker; it is not a new steal.","Deal Breaker is active again. You have one last Just Say No in this practice hand.",g=>new Command(Op.No,1,counter));
     Add("Protect your set again","Use your second Just Say No. Each response consumes its card, but does not spend a normal play.","Deal Breaker is rejected again. Dr. Noisewater has no more counters in this scene.",g=>g.Legal(0).First(c=>c.Op==Op.No));
     Add("The rejection stands","Watch Coach continue without countering. On an all-opponents charge, your No would protect only YOU, not the other opponents.","You kept your brown set. No property changed hands.",g=>new Command(Op.Pass,1));break;
    }
    case 10:{
     Set(1,Group.Orange,1);int wild=Wild(1,Group.Railroad,Group.Utility);World.Players[1].Hand.Remove(wild);var util=new PropertySet{Id=150,Color=Group.Utility,Cards=new List<int>{wild}};World.Players[1].Sets.Add(util);int sly=Hand(0,Kind.SlyDeal);
     Add("Take an unprotected property","Play Sly Deal against Coach and choose their Utility wildcard. Sly Deal cannot take any card from a complete set.","Coach can respond before the property transfers.",g=>g.Legal(0).First(c=>c.Card==sly&&c.Op==Op.Play&&c.Take==wild));
     Add("A stolen wild keeps its color","Watch Coach allow the take. You receive this wildcard as UTILITY, exactly as Coach had it. Receiving it costs no additional play.","You may rotate it to Railroad on your own turn for one play. The original Sly Deal already cost one play.",g=>new Command(Op.Pass,1));break;
    }
    case 11:{
     Set(0,Group.Red,1);Set(1,Group.Green,1);int trade=Hand(0,Kind.ForcedDeal);
     Add("Trade with Forced Deal","Play Forced Deal: choose Coach, their green property, then your red property to give. Their targeted property must be outside a complete set.","Review both mini cards: what you receive and what you give. The trade awaits Coach’s response.",g=>Play(g,0,trade));
     Add("Watch the exchange","Coach allows the trade. Unlike Sly Deal, you must give one property back. Wildcards keep their assigned colors in both directions.","You now own green and Coach owns red. Set completion, not bank balance, decides the winner.",g=>new Command(Op.Pass,1));break;
    }
    case 12:{
     int house=Hand(0,Kind.House),cash=Cash(0,2),debt=Hand(1,Kind.DebtCollector);
     Add("Actions can become bank money","Bank this House for 3M. Many action cards and valued wildcards can be banked. Once banked, they permanently act as money.","That House can no longer build on a set. Your bank contains 5M total.",g=>new Command(Op.Bank,0,house));
     Add("Prepare for a fixed bill","End your turn so Dr. Noisewater can demonstrate Debt Collector.","Debt Collector charges ONE chosen opponent a fixed 5M, regardless of their properties.",g=>new Command(Op.End,0));
     Add("Debt Collector","Watch Dr. Noisewater choose you for a 5M bill.","You can cover this bill with your 2M money card plus the banked 3M House.",g=>Play(g,1,debt));
     Add("Allow the bill","Allow Debt Collector. Only the targeted player owes money; this is not a rent card.","Choose banked cards to pay 5M. The banked House is money only.",g=>new Command(Op.Pass,0));
     Add("Pay with a banked action","Pay your 2M and banked House together.","The House stays money in Dr. Noisewater’s bank. Banked cards never become actions again.",g=>new Command(Op.Pay,0){Extras=new[]{cash,house}});break;
    }
    case 13:{
     int wild=Hand(0,Kind.Wild,0),pink=Property(0,Group.Pink);
     Add("Universal property wildcards","Play the 0M universal wildcard as Pink. It may use ANY property color, but has no payment value and cannot be banked.","A universal wildcard alone charges no rent. It needs a valued property alongside it.",g=>g.Legal(0).First(c=>c.Card==wild&&c.Op==Op.Play&&c.Color==Group.Pink));
     Add("Give the wildcard a partner","Play your pink property into the wildcard’s group.","The group now has two of three pink properties and can collect rent. A wildcard counts toward completion, but still cannot pay a debt if its value is 0M.",g=>g.Legal(0).First(c=>c.Card==pink&&c.Op==Op.Play&&c.Set>=0));break;
    }
    case 14:{
     Set(0,Group.Brown,2);Set(0,Group.Utility,2);Set(0,Group.DarkBlue,1);int final=Property(0,Group.DarkBlue);
     Add("Complete three DIFFERENT colors","You have brown and utility complete, plus one dark blue. Play the remaining dark blue into that group to win.","You win with three different complete sets. Money, houses and extra sets of the same color do not replace the three-color goal. You are ready for a normal game!",g=>g.Legal(0).First(c=>c.Card==final&&c.Op==Op.Play&&c.Set>=0));break;
    }
   }
   World.AssertConservation();
  }
 }
}
