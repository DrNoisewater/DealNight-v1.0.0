using System;
using System.Linq;
using DealNight.Core;
using UnityEngine;
using UnityEngine.UIElements;
namespace DealNight.Presentation {
 public sealed partial class TableApp {
  TutorialSession tutorial;string tutorialResult;bool tutorialResultShown;int tutorialLastCard=-1;
  const string TutorialPromptKey="tutorialOfferedRelease1";
  void RequestNewGame(){if(PlayerPrefs.GetInt(TutorialPromptKey,0)==0)OfferTutorial();else StartGame();}
  void OfferTutorial(){
   var w=Modal("Learn at the table?",false);w.Add(Widgets.Text("Practice against Dr. Noisewater with fixed cards and guided actions. Learn sets, payments, wildcards, rent, buildings and when to say no. You can replay it from the main menu.","description"));
   Action remember=()=>{PlayerPrefs.SetInt(TutorialPromptKey,1);PlayerPrefs.Save();};
   w.Add(Widgets.Button("Start guided tutorial",()=>{remember();StartTutorial();},"primary"));w.Add(Widgets.Button("Skip • play a regular game",()=>{remember();StartGame();}));
  }
  void StartTutorial(){PlayerPrefs.SetInt(TutorialPromptKey,1);PlayerPrefs.Save();tutorial=new TutorialSession(playerName);tutorialResult=null;BindTutorialScene();TutorialInstruction();}
  void BindTutorialScene(){
   foreach(var e in root.Children().Where(e=>e.ClassListContains("motion-card")||e.ClassListContains("bank-gain")).ToArray())e.RemoveFromHierarchy();
   game=tutorial.World;game.Players[0].Piece=pieceChoice;game.Players[1].Piece=(pieceChoice+1)%PieceNames.Length;
   arrivals.Clear();mobilePages.Clear();history=false;paused=false;selected=-1;noAnimation=false;feedback="Practice scene "+(tutorial.Lesson+1)+" • Dr. Noisewater";Render();
  }
  bool TutorialAllows(Command c){
   if(tutorialResult!=null)return false;
   if(tutorial.Matches(c))return true;
   var w=Modal("Let’s practice this step",false);w.Add(Widgets.Text(tutorial.Current.Instruction,"description"));w.Add(Widgets.Button("Back to this step",()=>{Render();TutorialInstruction();},"primary"));return false;
  }
  void TutorialTick(){
   if(paused||noAnimation||tutorialResult==null||tutorialResultShown||Time.unscaledTime<nextAI)return;
   tutorialResultShown=true;var w=Modal("Step complete",false);w.Add(Widgets.Text(tutorialResult,"description"));
   if(tutorialLastCard>=0)MiniCards(w,new[]{tutorialLastCard});
   w.Add(Widgets.Button("Continue",()=>{
    tutorialResult=null;tutorialResultShown=false;
    if(!tutorial.Next()){PlayerPrefs.SetInt("tutorialCompletedRelease1",1);PlayerPrefs.Save();tutorial=null;Menu();var done=Modal("Ready for Deal Night",false);done.Add(Widgets.Text("Complete three different colors. Keep a useful bank, protect your sets, and review the amount and targets before confirming an action.","description"));done.Add(Widgets.Button("Start a regular game",StartGame,"primary"));done.Add(Widgets.Button("Main menu",Menu));return;}
    if(game!=tutorial.World)BindTutorialScene();else Render();TutorialInstruction();
   },"primary"));w.Add(Widgets.Button("Leave tutorial",ExitTutorial));
  }
  void TutorialInstruction(){
   var step=tutorial.Current;var c=tutorial.Expected();var w=Modal("Practice "+(tutorial.Lesson+1)+" / "+TutorialSession.LessonCount+" • "+step.Title,false);
   w.Add(Widgets.Text(step.Instruction,"description"));if(c.Card>=0)MiniCards(w,new[]{c.Card});
   w.Add(Widgets.Text("Fixed practice scene • one opponent: Dr. Noisewater. Your normal game is separate.","muted"));
   w.Add(Widgets.Button(c.Actor==1?"Watch Dr. Noisewater":"Try it on the board",()=>{if(c.Actor==1)Send(c);else{Render();}},"primary"));w.Add(Widgets.Button("Leave tutorial",ExitTutorial));
  }
  void ExitTutorial(){var w=Modal("Leave the tutorial?",false);w.Add(Widgets.Text("You can restart the guided practice from the main menu.","description"));w.Add(Widgets.Button("Keep learning",()=>{Render();if(tutorialResult!=null){tutorialResultShown=false;TutorialTick();}else TutorialInstruction();},"primary"));w.Add(Widgets.Button("Main menu",Menu));}
  void AddTutorialBar(){
   if(tutorial==null||tutorialResult!=null)return;
   var bar=Widgets.Box("tutorial-bar");body.Insert(1,bar);bar.Add(Widgets.Text("Practice "+(tutorial.Lesson+1)+" / "+TutorialSession.LessonCount+" • "+tutorial.Current.Title,"tutorial-objective"));
   var c=tutorial.Expected();bar.Add(Widgets.Button(c.Actor==1?"Watch Dr. Noisewater":"Show this step",()=>{
    if(c.Actor==1){TutorialInstruction();return;}
    if(c.Op==Op.Move){SetInspect(0,game.Players[0].Sets.First(s=>s.Cards.Contains(c.Card)));return;}
    if(c.Op==Op.Bank||c.Op==Op.Play){Inspect(c.Card);return;}
    if(c.Op==Op.Pay)Payment();else if(c.Op==Op.Pass||c.Op==Op.No)Reaction();else if(c.Op==Op.Discard)DiscardUI();else TutorialInstruction();
   }));bar.Add(Widgets.Button("Help",TutorialInstruction));
   // Find the desired card and end-turn control without replacing normal interaction.
   if(c.Card>=0){var card=root.Q("handcard-"+c.Card);if(card!=null)card.AddToClassList("tutorial-target");}
  }
 }
}
