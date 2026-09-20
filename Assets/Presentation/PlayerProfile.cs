using DealNight.Core;
using UnityEngine;
using UnityEngine.UIElements;
namespace DealNight.Presentation
{
 public sealed partial class TableApp
 {
  const string NameKey="playerName";
  string playerName="Player";
  void SaveName(string raw)
  {
   playerName=PresentationMath.PlayerName(raw);PlayerPrefs.SetString(NameKey,playerName);PlayerPrefs.Save();
   if(game!=null)game.Players[0].Name=playerName;
  }
  void Welcome()
  {
   var w=Modal("Welcome to Deal Night",false);
   w.Add(Widgets.Text("What should we call you?","description"));
   var input=new TextField("Player name"){maxLength=24};w.Add(input);
   PiecePicker(w);var help=Widgets.Text("You can change this in Settings.","muted");w.Add(help);
   var enter=Widgets.Button("Join the table",()=>{SaveName(input.value);Menu();OfferTutorial();},"primary");enter.SetEnabled(false);w.Add(enter);
   input.RegisterValueChangedCallback(e=>enter.SetEnabled(PresentationMath.PlayerName(e.newValue).Length>0));
   input.schedule.Execute(()=>input.Focus()).StartingIn(30);
  }
  TextField NameField(VisualElement window)
  {
   var field=new TextField("Player name"){value=playerName,maxLength=24};window.Add(field);
   window.Add(Widgets.Text("1–24 characters. Changes apply to your current game.","muted"));return field;
  }
 }
}
