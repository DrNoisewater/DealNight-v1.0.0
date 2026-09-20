using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
namespace DealNight.Presentation {
 public sealed partial class TableApp {
  MusicPlayer music;
  void MusicSettings(){var window=Modal("Music & soundtrack",false);window.Add(Widgets.Text("Original small-ensemble swing tracks • plays across menus and matches","muted"));
   var enabled=new Toggle("Enable background music"){value=music.MusicEnabled};enabled.RegisterValueChangedCallback(e=>music.SetEnabled(e.newValue));window.Add(enabled);
   var volume=new Slider("Music volume",0,1){value=music.MusicVolume};var percent=Widgets.Text((int)(music.MusicVolume*100)+"%","muted");volume.RegisterValueChangedCallback(e=>{music.SetVolume(e.newValue);percent.text=(int)(e.newValue*100)+"%";});window.Add(volume);window.Add(percent);
   var mode=new DropdownField("Playback",new List<string>{"Shuffle playlist","Play in order","Repeat this track"},music.Mode);mode.RegisterValueChangedCallback(e=>music.SetMode(mode.index));window.Add(mode);
   var track=new DropdownField("Track",new List<string>(MusicPlayer.Titles),Math.Max(0,music.TrackIndex));track.RegisterValueChangedCallback(e=>music.Select(track.index));window.Add(track);
   var now=Widgets.Text("","music-now");window.Add(now);now.schedule.Execute(()=>{now.text=(music.MusicEnabled?"Now playing: ":"Music off • ")+music.TrackTitle;track.SetValueWithoutNotify(MusicPlayer.Titles[Math.Max(0,music.TrackIndex)]);}).Every(250);
   var duck=new Toggle("Lower music during action responses"){value=music.DuckResponses};duck.RegisterValueChangedCallback(e=>music.SetDuck(e.newValue));window.Add(duck);
   window.Add(Widgets.Button("Next track",()=>music.Next()));window.Add(Widgets.Button("Back to Settings",Settings,"primary"));}
 }
}
