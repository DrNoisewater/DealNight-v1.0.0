using System;
using DealNight.Core;
using UnityEngine;
using UnityEngine.UIElements;
namespace DealNight.Presentation
{
 public static class Widgets
 {
  public static readonly string[] Colors={"#955436","#73cbed","#d83b94","#f7941d","#ed3437","#f8df24","#16a65a","#2162bb","#40454c","#b5d837"};
  public static VisualElement Box(string cls){var e=new VisualElement();e.AddToClassList(cls);return e;}
  public static Label Text(string text,string cls=""){var l=new Label(text);l.enableRichText=false;if(cls!="")l.AddToClassList(cls);return l;}
  public static Button Button(string text,Action action,string cls=""){var b=new Button(action){text=text};if(cls!="")b.AddToClassList(cls);return b;}
  public static Color ColorFor(Card c){Color color;string hex=c.Colors.Length>0?Colors[(int)c.Colors[0]]:"#b9a3ce";if(c.Kind==Kind.Money){switch(c.Value){case 1:hex="#bd956f";break;case 2:hex="#ecd06a";break;case 3:hex="#83b67c";break;case 4:hex="#78abd1";break;case 5:hex="#c78fba";break;default:hex="#dfae46";break;}}else if(c.Kind==Kind.Rent&&c.Colors.Length==10)hex="#d9b86c";else if(c.Kind==Kind.House)hex="#16a65a";else if(c.Kind==Kind.Hotel||c.Kind==Kind.JustSayNo)hex="#df5554";else if(c.Kind==Kind.PassGo)hex="#78bad1";ColorUtility.TryParseHtmlString(hex,out color);return color;}
  public static VisualElement Card(Card c,Action click,bool compact=false)
  {
   var e=Box(compact?"mini-card":"card");e.tooltip=c.Name+"\n"+Catalog.Describe(c);e.focusable=true;e.tabIndex=0;
   e.Add(new CardBackground(c));Tint(e,ColorFor(c));var strip=Box("card-stripe");strip.style.backgroundColor=ColorFor(c);e.Add(strip);
   var top=Box("card-top");e.Add(top);top.Add(Text(c.Value==0?"WILD":c.Value+"M","card-value"));top.Add(Text(c.Kind==Kind.Wild?"WILD":c.Property?"PROPERTY":c.Kind==Kind.Money?"BANK":"ACTION","card-label"));
   e.Add(Text(c.Name,"card-title"));if(c.Kind==Kind.Rent)e.AddToClassList("rent-card");
   if(!compact)
   {
    e.Add(CardArt.Create(c));
    e.Add(Text(c.Property?(c.Colors.Length==10?"Any property color":string.Join(" / ",Array.ConvertAll(c.Colors,x=>Catalog.Labels[(int)x]))):ShortEffect(c),"card-kind"));
   }
   if(c.Kind==Kind.House||c.Kind==Kind.Hotel)e.Add(Text("No Utility/Railroad","building-limit"));
   e.RegisterCallback<ClickEvent>(evt=>{if(evt.button==0)click();});
   e.RegisterCallback<KeyDownEvent>(evt=>{if(evt.keyCode==KeyCode.Return||evt.keyCode==KeyCode.Space){click();evt.StopPropagation();}});
   return e;
  }
  static string ShortEffect(Card c)
  {
   switch(c.Kind){case Kind.Money:return "Add to your bank";case Kind.Rent:return (c.Colors.Length==10?"ANY COLOR • ONE opponent":string.Join(" / ",Array.ConvertAll(c.Colors,x=>Catalog.Labels[(int)x]))+" • ALL opponents");case Kind.PassGo:return "Draw two cards";case Kind.DebtCollector:return "Collect 5M from one player";case Kind.Birthday:return "Collect 2M from everyone";case Kind.SlyDeal:return "Take one loose property";case Kind.ForcedDeal:return "Trade one property each";case Kind.DealBreaker:return "Take a complete set";case Kind.JustSayNo:return "Cancel an action";case Kind.House:return "+3M to full set";case Kind.Hotel:return "+4M to full set • needs house";default:return "Attach to a Rent card";}
  }
  public static VisualElement BankCard(Card c,Action click)
  {
   var e=Card(c,click);e.Q<Label>(className:"card-label").text="BANKED";
   e.Q<Label>(className:"card-kind").text=c.Value+"M • Money only";return e;
  }
  public static Color GroupColor(Group group){Color c;ColorUtility.TryParseHtmlString(Colors[(int)group],out c);return c;}
  public static void Tint(VisualElement element,Color color){element.style.borderTopColor=color;element.style.borderBottomColor=color;element.style.borderLeftColor=color;element.style.borderRightColor=color;}
  public static VisualElement CardBack()
  {
   var e=Box("card-back");e.Add(Text("N / L","back-monogram"));e.Add(Text("DEAL\nNIGHT","back-title"));e.Add(Text("NOISEWATER LABS","back-footer"));return e;
  }
 }
}
