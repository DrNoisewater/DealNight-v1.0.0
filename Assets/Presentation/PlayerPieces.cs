using System;
using System.Linq;
using DealNight.Core;
using UnityEngine;
using UnityEngine.UIElements;
namespace DealNight.Presentation {
 public sealed partial class TableApp {
  static readonly string[] PieceNames={"Toaster","Avocado","Cone","Controller","Duck","Cactus","Pizza","TP roll"};
  int pieceChoice;bool autoChoices=true;
  Image PieceImage(int piece){var image=new Image{image=Resources.Load<Texture2D>("Pieces/"+piece),scaleMode=ScaleMode.ScaleToFit};image.AddToClassList("piece-image");image.pickingMode=PickingMode.Ignore;return image;}
  VisualElement PlayerBadge(int index,string cls="",string suffix=""){var box=Widgets.Box("player-badge");if(cls.Length>0)box.AddToClassList(cls);box.Add(PieceImage(game.Players[index].Piece));box.Add(Widgets.Text(game.Players[index].Name+suffix));return box;}
  VisualElement PlayerText(string text,string cls){var row=Widgets.Box("player-text");row.AddToClassList(cls);int start=0;while(start<text.Length){int next=-1,player=-1;for(int i=0;i<game.Players.Count;i++){int at=text.IndexOf(game.Players[i].Name,start,StringComparison.Ordinal);if(at>=0&&(next<0||at<next)){next=at;player=i;}}if(next<0){row.Add(Widgets.Text(text.Substring(start)));break;}if(next>start)row.Add(Widgets.Text(text.Substring(start,next-start)));row.Add(PlayerBadge(player));start=next+game.Players[player].Name.Length;}return row;}
  void AssignPieces(){game.Players[0].Piece=pieceChoice;var pool=Enumerable.Range(0,PieceNames.Length).Where(i=>i!=pieceChoice).OrderBy(i=>UnityEngine.Random.value).ToArray();for(int i=1;i<game.Players.Count;i++)game.Players[i].Piece=pool[i-1];}
  void PiecePicker(VisualElement parent){parent.Add(Widgets.Text("Choose your piece","eyebrow"));var row=Widgets.Box("piece-picker");parent.Add(row);for(int i=0;i<PieceNames.Length;i++){int piece=i;var button=Widgets.Button("",()=>{pieceChoice=piece;PlayerPrefs.SetInt("piece",piece);PlayerPrefs.Save();if(game!=null)game.Players[0].Piece=piece;foreach(var child in row.Children())child.EnableInClassList("piece-selected",(int)child.userData==piece);},"piece-option");button.userData=piece;button.Add(PieceImage(piece));button.Add(Widgets.Text(PieceNames[piece],"piece-label"));button.EnableInClassList("piece-selected",piece==pieceChoice);row.Add(button);}}
  void AddRecipientWarning(VisualElement card,int id,int recipient){var color=AssignedColor(id);if(!color.HasValue||!game.Players[recipient].Sets.Any(s=>s.Color==color.Value))return;Widgets.Tint(card,new Color(.9f,.18f,.2f));card.Add(Widgets.Text("! Matches recipient","recipient-warning"));card.tooltip+="\nWarning: "+game.Players[recipient].Name+" already has this color.";}
 }
}
