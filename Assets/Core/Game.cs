using System;
using System.Collections.Generic;
using System.Linq;
namespace DealNight.Core {
 public sealed class Game {
  public readonly List<Card> Cards=Catalog.Create(); public readonly List<Player> Players=new List<Player>();
  public readonly List<int> Deck=new List<int>(),Discard=new List<int>(); public readonly List<string> History=new List<string>();
  readonly Queue<Effect> effects=new Queue<Effect>(); readonly Random rng; int nextSet;
  public int Current {get;private set;} public int Remaining {get;private set;} public int Turn {get;private set;}
  public int Winner {get;private set;}=-1; public Phase Phase {get;private set;} public Effect Pending {get;private set;}
  public event Action Changed;
  public Game(int players,int seed,string humanName="Player") {
   if(players<2||players>5)throw new ArgumentException("Use 2–5 players."); rng=new Random(seed);
   var names=PresentationOptions.OpponentNames(players-1,seed,PresentationMath.PlayerName(humanName));
   for(int i=0;i<players;i++)Players.Add(new Player{Name=i==0?(PresentationMath.PlayerName(humanName)==""?"Player":PresentationMath.PlayerName(humanName)):names[i-1]});
   Deck.AddRange(Cards.Select(c=>c.Id)); Shuffle(Deck); foreach(var p in Players)Draw(p,5); Begin();
  }
  void Shuffle(List<int> list) {for(int i=list.Count-1;i>0;i--){int j=rng.Next(i+1),x=list[i];list[i]=list[j];list[j]=x;}}
  void Draw(Player p,int n) {for(int i=0;i<n;i++){if(Deck.Count==0){Deck.AddRange(Discard);Discard.Clear();Shuffle(Deck);}if(Deck.Count==0)break;int id=Deck[Deck.Count-1];Deck.RemoveAt(Deck.Count-1);p.Hand.Add(id);p.Drawn++;}}
  void Log(string s){History.Add(s);}
  void Begin(){Phase=Phase.Drawing;Turn++;Remaining=3;Draw(Players[Current],Players[Current].Hand.Count==0?5:2);Phase=Phase.Playing;Log(Players[Current].Name+" started turn "+Turn+".");CheckWin();}
  void Next(){Current=(Current+1)%Players.Count;Begin();}
  public bool Complete(PropertySet s){return s.Cards.Count>=Catalog.Sizes[(int)s.Color]&&s.Cards.Any(id=>Cards[id].Value>0);}
  public int CompleteCount(int p){return Players[p].Sets.Where(Complete).Select(s=>s.Color).Distinct().Count();}
  public int Rent(PropertySet s){if(s.Cards.Count==0||!s.Cards.Any(id=>Cards[id].Value>0))return 0;int r=Catalog.Rents[(int)s.Color][Math.Min(s.Cards.Count,Catalog.Sizes[(int)s.Color])-1];if(Complete(s)){if(s.House>=0)r+=3;if(s.Hotel>=0&&s.House>=0)r+=4;}return r;}
  public int BankTotal(int p){return Players[p].Bank.Sum(id=>Cards[id].Value);}
  public IEnumerable<int> Assets(int p){return Players[p].Bank.Concat(Players[p].Sets.SelectMany(s=>s.Cards.Concat(s.House>=0?new[]{s.House}:new int[0]).Concat(s.Hotel>=0?new[]{s.Hotel}:new int[0]))).Where(id=>Cards[id].Value>0);}
  public int DecisionPlayer {get {return Pending!=null?(Phase==Phase.Reaction?Pending.Responder:Pending.Target):Current;}}
  void CheckWin(){if(Phase!=Phase.Playing||Pending!=null||effects.Count>0)return;if(CompleteCount(Current)>=3){Winner=Current;Phase=Phase.GameOver;Log(Players[Winner].Name+" won with three colors!");}}
  public Observation Observe(int actor){return new Observation{Actor=actor,Current=Current,Remaining=Remaining,Phase=Phase,Hand=Players[actor].Hand.Select(id=>Cards[id]).ToArray(),Pending=Pending==null?null:new Effect{Card=Pending.Card,AllOpponents=Pending.AllOpponents,Source=Pending.Source,Target=Pending.Target,Amount=Pending.Amount,Kind=Pending.Kind,Responder=Pending.Responder,Cancelled=Pending.Cancelled},Players=Players.Select((p,i)=>new PublicPlayer{Bank=p.Bank.ToArray(),BankTotal=BankTotal(i),HandCount=p.Hand.Count,Complete=CompleteCount(i),Sets=p.Sets.Select(s=>new PropertySet{Id=s.Id,Color=s.Color,Cards=new List<int>(s.Cards),House=s.House,Hotel=s.Hotel}).ToArray()}).ToArray()};}
  IEnumerable<int> Destinations(int p,Group color){yield return -1;foreach(var s in Players[p].Sets.Where(s=>s.Color==color&&!Complete(s)&&s.Cards.Count<Catalog.Sizes[(int)color]))yield return s.Id;}
  public List<Command> Legal(int actor) {
   var l=new List<Command>();if(Phase==Phase.GameOver||actor!=DecisionPlayer)return l;var p=Players[actor];
   if(Phase==Phase.Reaction){l.Add(new Command(Op.Pass,actor));foreach(int id in p.Hand.Where(id=>Cards[id].Kind==Kind.JustSayNo))l.Add(new Command(Op.No,actor,id));return l;}
   if(Phase==Phase.Payment)return l;
   if(Phase==Phase.Discarding){foreach(int id in p.Hand)l.Add(new Command(Op.Discard,actor,id));return l;}
   l.Add(new Command(Op.End,actor));
   foreach(var s in p.Sets)foreach(int id in s.Cards.Where(id=>Cards[id].Property))foreach(var color in Cards[id].Colors)foreach(int dest in Destinations(actor,color))if(dest!=s.Id&&(color!=s.Color||dest>=0)&&(color==s.Color||Remaining>0))l.Add(new Command(Op.Move,actor,id){Color=color,Set=dest});
   // Buildings can be rearranged between eligible sets during your turn, without a card play.
   foreach(var s in p.Sets)foreach(int id in new[]{s.House,s.Hotel}.Where(id=>id>=0))foreach(var dst in p.Sets.Where(x=>x.Id!=s.Id&&BuildingAllowed(x,Cards[id].Kind)))l.Add(new Command(Op.Move,actor,id){Set=dst.Id,Color=dst.Color});
   if(Remaining<=0)return l;
   foreach(int id in p.Hand){Card c=Cards[id];if(!c.Property||(c.Kind==Kind.Wild&&c.Value>0))l.Add(new Command(Op.Bank,actor,id));
    if(c.Property){foreach(var color in c.Colors)foreach(int dest in Destinations(actor,color))l.Add(new Command(Op.Play,actor,id){Color=color,Set=dest});continue;}
    if(c.Kind==Kind.PassGo||c.Kind==Kind.Birthday)l.Add(new Command(Op.Play,actor,id));
    if(c.Kind==Kind.House||c.Kind==Kind.Hotel)foreach(var s in p.Sets.Where(s=>BuildingAllowed(s,c.Kind)))l.Add(new Command(Op.Play,actor,id){Set=s.Id,Color=s.Color});
    if(c.Kind==Kind.Rent)foreach(var s in p.Sets.Where(s=>c.Colors.Contains(s.Color)&&Rent(s)>0)){
     int[] doubles=p.Hand.Where(x=>Cards[x].Kind==Kind.DoubleRent).ToArray();
     for(int n=0;n<=Math.Min(doubles.Length,Remaining-1);n++){
      if(c.Colors.Length==10){for(int t=0;t<Players.Count;t++)if(t!=actor)l.Add(new Command(Op.Play,actor,id){Set=s.Id,Target=t,Extras=doubles.Take(n).ToArray()});}
      else l.Add(new Command(Op.Play,actor,id){Set=s.Id,Extras=doubles.Take(n).ToArray()});
     }
    }
    for(int t=0;t<Players.Count;t++){if(t==actor)continue;
     if(c.Kind==Kind.DebtCollector)l.Add(new Command(Op.Play,actor,id){Target=t});
     if(c.Kind==Kind.DealBreaker)foreach(var s in Players[t].Sets.Where(Complete))l.Add(new Command(Op.Play,actor,id){Target=t,Set=s.Id});
     if(c.Kind==Kind.SlyDeal||c.Kind==Kind.ForcedDeal)foreach(var s in Players[t].Sets.Where(s=>!Complete(s)))foreach(int take in s.Cards){
      if(c.Kind==Kind.SlyDeal)l.Add(new Command(Op.Play,actor,id){Target=t,Take=take});
      else foreach(int give in p.Sets.SelectMany(x=>x.Cards))l.Add(new Command(Op.Play,actor,id){Target=t,Take=take,Give=give});
     }
    }
   }return l;
  }
  bool BuildingAllowed(PropertySet s,Kind kind){return (int)s.Color<8&&Complete(s)&&(kind==Kind.House?s.House<0:s.House>=0&&s.Hotel<0);}
  static bool Same(Command a,Command b){return a.Op==b.Op&&a.Actor==b.Actor&&a.Card==b.Card&&a.Target==b.Target&&a.Take==b.Take&&a.Give==b.Give&&a.Set==b.Set&&a.Color==b.Color&&a.Extras.SequenceEqual(b.Extras);}
  public string PaymentError(Command c){if(Phase!=Phase.Payment||Pending==null||c.Actor!=Pending.Target||c.Op!=Op.Pay)return "It is not your payment.";var assets=Assets(c.Actor).ToArray();if(c.Extras.Distinct().Count()!=c.Extras.Length||c.Extras.Any(id=>!assets.Contains(id)))return "Choose only your bank, valued properties, or buildings, once each.";int total=c.Extras.Sum(id=>Cards[id].Value),available=assets.Sum(id=>Cards[id].Value);if(total<Math.Min(Pending.Amount,available))return "Pay the amount due, or all valued assets if you cannot afford it.";return null;}
  public bool Execute(Command c,out string error){error=null;if(c==null){error="No command.";return false;}if(c.Op==Op.Pay){error=PaymentError(c);if(error!=null)return false;}else if(!Legal(c.Actor).Any(x=>Same(x,c))){error="That action is unavailable in the current state.";return false;}
   var p=Players[c.Actor];
   switch(c.Op){
    case Op.End:if(p.Hand.Count>7){Phase=Phase.Discarding;Log("Discard to seven cards.");}else Next();break;
    case Op.Discard:p.Discarded++;p.Hand.Remove(c.Card);Discard.Add(c.Card);if(p.Hand.Count<=7)Next();break;
    case Op.Bank:p.Banked+=Cards[c.Card].Value;p.Hand.Remove(c.Card);p.Bank.Add(c.Card);Spend(c.Actor,1);Log(p.Name+" banked "+Cards[c.Card].Value+"M.");break;
    case Op.Move:bool rotate=Cards[c.Card].Kind==Kind.Wild&&p.Sets.First(s=>s.Cards.Contains(c.Card)).Color!=c.Color;Move(c);if(rotate)Spend(c.Actor,1);Log(p.Name+" moved "+Cards[c.Card].Name+".");break;
    case Op.Play:if(!Cards[c.Card].Property)p.Actions++;Play(c);break;
    case Op.Pass:if(Pending.Cancelled){Log((Pending.Card>=0?Cards[Pending.Card].Name:"Action")+" rejected: "+Players[Pending.Target].Name+" is protected.");AdvanceEffect();}else ResolveEffect();break;
    case Op.No:p.Nos++;p.Hand.Remove(c.Card);Discard.Add(c.Card);Pending.Cancelled=!Pending.Cancelled;Pending.Responder=c.Actor==Pending.Source?Pending.Target:Pending.Source;Log(p.Name+" played Just Say No.");break;
    case Op.Pay:Players[Pending.Source].Received+=c.Extras.Sum(id=>Cards[id].Value);int over=Math.Max(0,c.Extras.Sum(id=>Cards[id].Value)-Pending.Amount);p.Overpaid+=over;p.LargestOverpayment=Math.Max(p.LargestOverpayment,over);foreach(int id in c.Extras)TransferAsset(c.Actor,Pending.Source,id);p.Payments+=c.Extras.Sum(id=>Cards[id].Value);Log(p.Name+" paid "+c.Extras.Sum(id=>Cards[id].Value)+"M to "+Players[Pending.Source].Name+".");AdvanceEffect();break;
   }
   CheckWin();AssertConservation();if(Changed!=null)Changed();return true;
  }
  void Spend(int p,int n){Remaining-=n;Players[p].Plays+=n;}
  void Place(int player,int id,Group color,int set=-1){var p=Players[player];var s=p.Sets.FirstOrDefault(x=>x.Id==set);if(s==null){s=new PropertySet{Id=nextSet++,Color=color};p.Sets.Add(s);}s.Cards.Add(id);}
  void AutoPlace(int player,int id,Group color){var dst=PlacementPolicy.ReceivingGroup(Players[player].Sets,color);Place(player,id,color,dst==null?-1:dst.Id);}
  void Clean(int p){Players[p].Sets.RemoveAll(s=>s.Cards.Count==0&&s.House<0&&s.Hotel<0);}
  void Move(Command c){var p=Players[c.Actor];var s=p.Sets.First(x=>x.Cards.Contains(c.Card)||x.House==c.Card||x.Hotel==c.Card);if(Cards[c.Card].Property){s.Cards.Remove(c.Card);Place(c.Actor,c.Card,c.Color,c.Set);}else{if(s.House==c.Card)s.House=-1;else s.Hotel=-1;var d=p.Sets.First(x=>x.Id==c.Set);if(Cards[c.Card].Kind==Kind.House)d.House=c.Card;else d.Hotel=c.Card;}Clean(c.Actor);}
  void Play(Command c){var p=Players[c.Actor];var card=Cards[c.Card];p.Hand.Remove(c.Card);Spend(c.Actor,1+c.Extras.Length);Log(p.Name+" played "+card.Name+".");
   if(card.Property){Place(c.Actor,c.Card,c.Color,c.Set);return;}
   if(card.Kind==Kind.House||card.Kind==Kind.Hotel){var s=p.Sets.First(x=>x.Id==c.Set);if(card.Kind==Kind.House)s.House=c.Card;else s.Hotel=c.Card;return;}
   Discard.Add(c.Card);foreach(int id in c.Extras){p.Hand.Remove(id);Discard.Add(id);}
   if(card.Kind==Kind.PassGo){Draw(p,2);return;}
   int amount=card.Kind==Kind.DebtCollector?5:card.Kind==Kind.Birthday?2:card.Kind==Kind.Rent?Rent(p.Sets.First(x=>x.Id==c.Set))*(1<<c.Extras.Length):0;
   if(card.Kind==Kind.Birthday||(card.Kind==Kind.Rent&&card.Colors.Length==2)){for(int offset=1;offset<Players.Count;offset++)Queue(c,card.Kind,(c.Actor+offset)%Players.Count,amount);}
   else Queue(c,card.Kind,c.Target,amount);AdvanceEffect();
  }
  void Queue(Command c,Kind k,int target,int amount){effects.Enqueue(new Effect{Kind=k,Card=c.Card,AllOpponents=k==Kind.Birthday||(k==Kind.Rent&&Cards[c.Card].Colors.Length==2),Source=c.Actor,Target=target,Amount=amount,Take=c.Take,Give=c.Give,Set=c.Set,Responder=target});}
  void AdvanceEffect(){Pending=null;if(effects.Count==0){Phase=Phase.Playing;return;}Pending=effects.Dequeue();Phase=Phase.Reaction;}
  void ResolveEffect(){var e=Pending;if(e.Amount>0){Phase=Phase.Payment;return;}
   if(e.Kind==Kind.DealBreaker){var s=Players[e.Target].Sets.First(x=>x.Id==e.Set);Players[e.Target].Sets.Remove(s);Players[e.Source].Sets.Add(s);Players[e.Source].Stolen+=s.Cards.Count;}
   else if(e.Kind==Kind.SlyDeal){TransferAsset(e.Target,e.Source,e.Take);Players[e.Source].Stolen++;}
   else if(e.Kind==Kind.ForcedDeal){TransferAsset(e.Target,e.Source,e.Take);TransferAsset(e.Source,e.Target,e.Give);Players[e.Source].Stolen++;}
   Log("Resolved "+e.Kind+" against "+Players[e.Target].Name+".");AdvanceEffect();
  }
  void TransferAsset(int from,int to,int id){var p=Players[from];if(p.Bank.Remove(id)){Players[to].Bank.Add(id);return;}var s=p.Sets.First(x=>x.Cards.Contains(id)||x.House==id||x.Hotel==id);if(s.Cards.Remove(id))AutoPlace(to,id,s.Color);else{if(s.House==id)s.House=-1;else s.Hotel=-1;Players[to].Bank.Add(id);}Clean(from);}
  public string Describe(Command c){if(c.Op==Op.End)return "End turn";if(c.Op==Op.Pass)return "Pass / allow response";if(c.Op==Op.No)return "Just Say No";if(c.Op==Op.Bank)return "Bank for "+Cards[c.Card].Value+"M";if(c.Op==Op.Discard)return "Discard "+Cards[c.Card].Name;if(c.Op==Op.Move)return "Move to "+Catalog.Labels[(int)c.Color]+(c.Set<0?" (new group)":" #"+c.Set);
   string s="Play "+Cards[c.Card].Name;if(Cards[c.Card].Property)s+=" as "+Catalog.Labels[(int)c.Color]+(c.Set<0?" (new group)":" #"+c.Set);else if(c.Set>=0)s+=" on set #"+c.Set;if(c.Target>=0)s+=" → "+Players[c.Target].Name;if(c.Take>=0)s+=" • take "+Cards[c.Take].Name;if(c.Give>=0)s+=" • give "+Cards[c.Give].Name;if(c.Extras.Length>0)s+=" • "+(1<<c.Extras.Length)+"× rent ("+(1+c.Extras.Length)+" plays)";return s;
  }
  public void AssertConservation(){var ids=Deck.Concat(Discard).Concat(Players.SelectMany(p=>p.Hand.Concat(p.Bank).Concat(p.Sets.SelectMany(s=>s.Cards.Concat(s.House>=0?new[]{s.House}:new int[0]).Concat(s.Hotel>=0?new[]{s.Hotel}:new int[0]))))).ToArray();if(ids.Length!=106||ids.Distinct().Count()!=106||ids.Any(id=>id<0||id>=106))throw new InvalidOperationException("Card conservation failed.");foreach(var p in Players)foreach(var s in p.Sets){if(s.Cards.Count>Catalog.Sizes[(int)s.Color]||s.Cards.Any(id=>!Cards[id].Colors.Contains(s.Color)))throw new InvalidOperationException("Illegal property group.");}if(Remaining<0||Remaining>3)throw new InvalidOperationException("Play budget invalid.");}
 }
}
