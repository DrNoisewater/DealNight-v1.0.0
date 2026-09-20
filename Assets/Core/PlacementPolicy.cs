using System.Collections.Generic;
using System.Linq;
namespace DealNight.Core
{
 public static class PlacementPolicy
 {
  // -2 means meaningful alternatives remain; let the human choose.
  public static int AutomaticDestination(IEnumerable<PropertySet> sets,IEnumerable<int> legalIds)
  {
   var ids=legalIds.Distinct().Where(id=>id>=0).ToArray();if(ids.Length==1)return ids[0];
   var finish=sets.Where(s=>ids.Contains(s.Id)&&s.Cards.Count==Catalog.Sizes[(int)s.Color]-1).ToArray();
   return finish.Length==1?finish[0].Id:-2;
  }
  // Receiving a transferred card must be atomic. Keep its color, prefer a
  // near-complete group, then dormant buildings; never dismantle a full set.
  public static PropertySet ReceivingGroup(IEnumerable<PropertySet> sets,Group color)
  {
   return sets.Where(s=>s.Color==color&&s.Cards.Count<Catalog.Sizes[(int)color]).OrderByDescending(s=>s.Cards.Count).ThenByDescending(s=>(s.House>=0?1:0)+(s.Hotel>=0?1:0)).ThenBy(s=>s.Id).FirstOrDefault();
  }
 }
}
