using System;
using System.Collections.Generic;
using System.Linq;
namespace DealNight.Core {
 public enum Difficulty { Easy, Normal, Hard }
 public sealed class Computer {
  readonly Random rng; readonly Difficulty difficulty; readonly List<Card> catalog=Catalog.Create();
  public Computer(Difficulty difficulty,int seed){this.difficulty=difficulty;rng=new Random(seed);}
  public Command Choose(Observation o,List<Command> legal){
   if(o.Phase==Phase.Payment)return Payment(o);
   if(legal.Count==0)throw new InvalidOperationException("No legal decision.");
   return legal.OrderByDescending(c=>Score(o,c)+rng.NextDouble()*(difficulty==Difficulty.Easy?12:0.3)).First();
  }
  double Strength(PublicPlayer p){return p.Sets.Sum(s=>Math.Pow((double)s.Cards.Count/Catalog.Sizes[(int)s.Color],2)*8);}
  double PropertyGain(PublicPlayer p,int id){return catalog[id].Colors.Max(color=>{var set=p.Sets.Where(s=>s.Color==color&&s.Cards.Count<Catalog.Sizes[(int)color]).OrderByDescending(s=>s.Cards.Count).FirstOrDefault();return set==null?4:set.Cards.Count==Catalog.Sizes[(int)color]-1?22:8;});}
  double Score(Observation o,Command c){var me=o.Players[o.Actor];
   if(c.Op==Op.No){if(difficulty==Difficulty.Easy)return 5;if(o.Pending.Kind==Kind.DealBreaker||o.Pending.Kind==Kind.ForcedDeal||o.Pending.Kind==Kind.SlyDeal)return 35;return o.Pending.Amount>=3||me.BankTotal<o.Pending.Amount?15:-5;}
   if(c.Op==Op.Pass)return 0;
   if(c.Op==Op.End)return -20;
   if(c.Op==Op.Discard)return -Keep(o,c.Card);
   if(c.Op==Op.Bank)return catalog[c.Card].Kind==Kind.Money?12+catalog[c.Card].Value:(me.BankTotal<5?7:1)-Keep(o,c.Card)*0.25;
   if(c.Op==Op.Move){if(!catalog[c.Card].Property){var origin=me.Sets.First(s=>s.House==c.Card||s.Hotel==c.Card);return origin.Cards.Count<Catalog.Sizes[(int)origin.Color]?15:-100;}var src=me.Sets.First(s=>s.Cards.Contains(c.Card));var dst=me.Sets.FirstOrDefault(s=>s.Id==c.Set);int n=dst==null?0:dst.Cards.Count;double gain=(n==Catalog.Sizes[(int)c.Color]-1?24:n*3+2)-(src.Cards.Count==Catalog.Sizes[(int)src.Color]?30:(src.Cards.Count-1)*3+2);return gain>0?gain:-100;}
   Card card=catalog[c.Card];if(card.Property){var dst=me.Sets.FirstOrDefault(s=>s.Id==c.Set);int n=dst==null?0:dst.Cards.Count;return n==Catalog.Sizes[(int)c.Color]-1?50:n*6+14;}
   switch(card.Kind){case Kind.PassGo:return o.Hand.Length<5?24:10;
    case Kind.House:case Kind.Hotel:return 12;
    case Kind.DealBreaker:return 70+(difficulty==Difficulty.Hard?o.Players[c.Target].Complete*15:0);
    case Kind.SlyDeal:return 14+PropertyGain(me,c.Take)+(difficulty==Difficulty.Hard?Strength(o.Players[c.Target]):0);
    case Kind.ForcedDeal:return 12+PropertyGain(me,c.Take)-PropertyGain(me,c.Give)+(difficulty==Difficulty.Hard?Strength(o.Players[c.Target])*.4:0);
    case Kind.Rent:var s=me.Sets.First(x=>x.Id==c.Set);int rent=Catalog.Rents[(int)s.Color][s.Cards.Count-1];if(s.Cards.Count==Catalog.Sizes[(int)s.Color])rent+=(s.House>=0?3:0)+(s.Hotel>=0&&s.House>=0?4:0);return 8+rent*(1<<c.Extras.Length)/(double)(1+c.Extras.Length);
    case Kind.DebtCollector:return 10+Math.Min(5,o.Players[c.Target].BankTotal);
    case Kind.Birthday:return 8+o.Players.Length;default:return 1;
   }
  }
  double Keep(Observation o,int id){var c=catalog[id];if(c.Property)return 15+PropertyGain(o.Players[o.Actor],id);switch(c.Kind){case Kind.JustSayNo:return 32;case Kind.DealBreaker:return 35;case Kind.PassGo:return 20;case Kind.Money:return c.Value;default:return 10;}}
  Command Payment(Observation o){var p=o.Players[o.Actor];var ids=p.Bank.Concat(p.Sets.SelectMany(s=>s.Cards.Concat(s.House>=0?new[]{s.House}:new int[0]).Concat(s.Hotel>=0?new[]{s.Hotel}:new int[0]))).Where(id=>catalog[id].Value>0).ToArray();
   int total=ids.Sum(id=>catalog[id].Value),need=Math.Min(total,o.Pending.Amount);if(total<=need)return new Command(Op.Pay,o.Actor){Extras=ids};
   // Bounded knapsack minimizes property damage and overpayment without exponential search.
   var best=new Dictionary<int,Tuple<double,List<int>>>();best[0]=Tuple.Create(0.0,new List<int>());
   foreach(int id in ids){double cost=catalog[id].Value;if(!p.Bank.Contains(id)){var s=p.Sets.First(x=>x.Cards.Contains(id)||x.House==id||x.Hotel==id);if(catalog[id].Property){int size=Catalog.Sizes[(int)s.Color],n=s.Cards.Count;double progress=(double)n/size;double protect=n>=size?250:8+60*Math.Pow(progress,3)+(n>=2?55:0)+(size==2?12:0);if(s.Color==Group.Railroad&&n<size)protect*=.8;if(n==1&&p.Sets.Sum(x=>x.Cards.Count)>=8)protect*=.65;cost+=protect+(catalog[id].Kind==Kind.Wild?20:0);var receiver=o.Players[o.Pending.Source];if(receiver.Sets.Any(x=>x.Color==s.Color))cost+=30;if(receiver.Sets.Any(x=>x.Color==s.Color&&x.Cards.Count==size-1))cost+=receiver.Complete>=2?180:45;}else cost+=60;}if(difficulty==Difficulty.Easy)cost=catalog[id].Value;
    foreach(var entry in best.ToArray()){int sum=entry.Key+catalog[id].Value;double score=entry.Value.Item1+cost;if(!best.ContainsKey(sum)||best[sum].Item1>score){var chosen=new List<int>(entry.Value.Item2){id};best[sum]=Tuple.Create(score,chosen);}}
   }
   var result=best.Where(x=>x.Key>=need).OrderBy(x=>x.Value.Item1+(x.Key-need)*2).First();return new Command(Op.Pay,o.Actor){Extras=result.Value.Item2.ToArray()};
  }
 }
}
