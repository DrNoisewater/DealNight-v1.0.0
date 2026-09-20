using System;
using System.Collections.Generic;
using System.Linq;
namespace DealNight.Core {
 public enum Kind { Money, Property, Wild, Rent, PassGo, DebtCollector, Birthday, SlyDeal, ForcedDeal, DealBreaker, JustSayNo, House, Hotel, DoubleRent, Reference }
 public enum Group { Brown, LightBlue, Pink, Orange, Red, Yellow, Green, DarkBlue, Railroad, Utility }
 public sealed class Card {
  public int Id; public Kind Kind; public string Name; public int Value; public Group[] Colors;
  public bool Property { get { return Kind==Kind.Property || Kind==Kind.Wild; } }
  public Card(int id,Kind kind,string name,int value,params Group[] colors) { Id=id;Kind=kind;Name=name;Value=value;Colors=colors; }
 }
 public static class Catalog {
  public static readonly int[] Sizes={2,3,3,3,3,3,3,2,4,2};
  public static readonly int[] Values={1,1,2,2,3,3,4,4,2,2};
  public static readonly int[][] Rents={new[]{1,2},new[]{1,2,3},new[]{1,2,4},new[]{1,3,5},new[]{2,3,6},new[]{2,4,6},new[]{2,4,7},new[]{3,8},new[]{1,2,3,4},new[]{1,2}};
  public static readonly string[] Labels={"Brown","Light blue","Pink","Orange","Red","Yellow","Green","Dark blue","Railroad","Utility"};
  public static List<Card> Create(bool references=false) {
   var d=new List<Card>(); Action<Kind,string,int,int,Group[]> add=(k,n,v,count,c)=>{for(int i=0;i<count;i++)d.Add(new Card(d.Count,k,n,v,c));};
   string[][] names={new[]{"Mediterranean Avenue","Baltic Avenue"},new[]{"Oriental Avenue","Vermont Avenue","Connecticut Avenue"},new[]{"St. Charles Place","States Avenue","Virginia Avenue"},new[]{"St. James Place","Tennessee Avenue","New York Avenue"},new[]{"Kentucky Avenue","Indiana Avenue","Illinois Avenue"},new[]{"Atlantic Avenue","Ventnor Avenue","Marvin Gardens"},new[]{"Pacific Avenue","North Carolina Avenue","Pennsylvania Avenue"},new[]{"Park Place","Boardwalk"},new[]{"Reading Railroad","Pennsylvania Railroad","B. & O. Railroad","Short Line"},new[]{"Electric Company","Water Works"}};
   for(int g=0;g<10;g++)foreach(var n in names[g])add(Kind.Property,n,Values[g],1,new[]{(Group)g});
   Group[][] pairs={new[]{Group.Brown,Group.LightBlue},new[]{Group.LightBlue,Group.Railroad},new[]{Group.Pink,Group.Orange},new[]{Group.Red,Group.Yellow},new[]{Group.DarkBlue,Group.Green},new[]{Group.Green,Group.Railroad},new[]{Group.Railroad,Group.Utility}};
   int[] counts={1,1,2,2,1,1,1},vals={1,4,2,3,4,4,2};
   for(int i=0;i<pairs.Length;i++)add(Kind.Wild,"Property wildcard",vals[i],counts[i],pairs[i]);
   var all=Enumerable.Range(0,10).Select(x=>(Group)x).ToArray(); add(Kind.Wild,"Universal wildcard",0,2,all);
   foreach(int i in new[]{0,2,3,4,6})add(Kind.Rent,"Rent",1,2,pairs[i]); add(Kind.Rent,"Any-color rent",3,3,all);
   Kind[] acts={Kind.PassGo,Kind.DebtCollector,Kind.Birthday,Kind.SlyDeal,Kind.ForcedDeal,Kind.DealBreaker,Kind.JustSayNo,Kind.House,Kind.Hotel,Kind.DoubleRent};
   string[] an={"Pass Go","Debt Collector","It's My Birthday","Sly Deal","Forced Deal","Deal Breaker","Just Say No","House","Hotel","Double the Rent"};
   int[] ac={10,3,3,3,3,2,3,3,2,2},av={1,3,2,3,3,5,4,3,4,1};
   for(int i=0;i<acts.Length;i++)add(acts[i],an[i],av[i],ac[i],new Group[0]);
   int[] money={1,2,3,4,5,10},mc={6,5,3,3,2,1}; for(int i=0;i<money.Length;i++)add(Kind.Money,money[i]+"M money",money[i],mc[i],new Group[0]);
   if(references)add(Kind.Reference,"Quick reference",0,4,new Group[0]); return d;
  }
  public static string Describe(Card c) {
   if(c.Property)return string.Join(" / ",c.Colors.Select(x=>Labels[(int)x]).ToArray())+"\n"+(c.Value==0?"Cannot pay debts. Needs another property to collect rent.":"Property asset • "+c.Value+"M");
   switch(c.Kind) {
    case Kind.Rent:return (c.Colors.Length==10?"ANY COLOR • Charge ONE opponent rent for any one group you own.":string.Join(" / ",c.Colors.Select(x=>Labels[(int)x]).ToArray())+" • ALL opponents each pay rent for one of your groups of either listed color.")+" The chosen group determines the amount. Each Double the Rent costs another play; two doubles make 4× rent.";
    case Kind.PassGo:return "Draw two cards. Costs one play.";
    case Kind.DebtCollector:return "One opponent pays you 5M.";
    case Kind.Birthday:return "Each opponent pays you 2M separately.";
    case Kind.SlyDeal:return "Take one laid-down property from ONE opponent. You cannot take a card from their complete set. Received wildcards keep their assigned color.";
    case Kind.ForcedDeal:return "Exchange one of your properties for an opponent's unprotected property.";
    case Kind.DealBreaker:return "Take a complete property set, including its buildings.";
    case Kind.JustSayNo:return "Cancel an action against you, or counter a Just Say No. Use in the reaction window.";
    case Kind.House:return "Add 3M rent to a complete street set. One house per set. No buildings on Utility or Railroad.";
    case Kind.Hotel:return "Add 4M MORE rent to a complete street set that already has a house (+7M together). One hotel per set. No Utility or Railroad.";
    case Kind.DoubleRent:return "Attach to rent. Each double uses one card play. Two doubles make rent four times.";
    default:return "Add to your bank to pay future debts. No change is given.";
   }
  }
 }
}
