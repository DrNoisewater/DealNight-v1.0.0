using System;
using System.Linq;
namespace DealNight.Core {
 // Public assets only: cannot promise a particular property, see hands, or predict a No.
 public static class ChargeAdvice {
  public static int Amount(Game g,Command c){var k=g.Cards[c.Card].Kind;return k==Kind.Birthday?2:k==Kind.DebtCollector?5:k==Kind.Rent?g.Rent(g.Players[c.Actor].Sets.First(s=>s.Id==c.Set))*(1<<c.Extras.Length):0;}
  public static double Score(Game g,Command c){int due=Amount(g,c);if(due==0)return 0;var targets=c.Target>=0?new[]{c.Target}:Enumerable.Range(0,g.Players.Count).Where(i=>i!=c.Actor).ToArray();double score=0;
   foreach(int t in targets){int assets=g.Assets(t).Sum(i=>g.Cards[i].Value),bank=g.BankTotal(t);int shortfall=Math.Max(0,due-bank);int propertyValue=Math.Max(0,assets-bank);
    score+=Math.Min(due,assets)+Math.Min(shortfall,propertyValue)*.8;
    // Mild preference for potentially useful colors, never a guaranteed transfer.
    if(shortfall>0)score+=g.Players[t].Sets.Count(s=>s.Cards.Any(i=>g.Cards[i].Value>0)&&g.Players[c.Actor].Sets.Any(m=>m.Color==s.Color&&!g.Complete(m)))*.15;
   }
   return score-c.Extras.Length*.75;
  }
  public static string Outlook(Game g,int target,int due){int bank=g.BankTotal(target);int assets=g.Assets(target).Sum(i=>g.Cards[i].Value);if(assets==0)return "No payable assets";if(due>bank&&assets>bank)return "Bank short by "+(due-bank)+"M • property/building payment needed if allowed";if(due>assets)return "Only "+assets+"M payable • cannot cover bill";return "Bank can cover bill • payer chooses cards";}
 }
}
