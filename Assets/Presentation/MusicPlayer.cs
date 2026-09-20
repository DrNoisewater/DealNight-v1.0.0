using System;
using UnityEngine;
namespace DealNight.Presentation {
 // Lives on the UI host, independently of the menu/table VisualElement hierarchy.
 public sealed class MusicPlayer:MonoBehaviour {
  public static readonly string[] Titles={"Lobby at Dusk","Penny Arcade Promenade","The Velvet Exchange","Sidewalk Serenade","Last Tram Home"};
  static readonly string[] Files={"01_Lobby_at_Dusk","02_Penny_Arcade_Promenade","03_The_Velvet_Exchange","04_Sidewalk_Serenade","05_Last_Tram_Home"};
  readonly AudioSource[] sources=new AudioSource[2];readonly AudioClip[] clips=new AudioClip[5];
  readonly System.Random random=new System.Random();int active,index=-1;float fade=1,gain;bool focus=true,suspended,halted;
  const float Transition=2f;
  public bool MusicEnabled {get;private set;}
  public float MusicVolume {get;private set;}
  public int Mode {get;private set;}
  public bool DuckResponses {get;private set;}
  public bool ResponseActive {get;set;}
  public int TrackIndex {get{return index;}}
  public string TrackTitle {get{return index<0?"No track available":Titles[index];}}
  void Awake(){MusicEnabled=PlayerPrefs.GetInt("musicEnabled",1)==1;MusicVolume=Mathf.Clamp01(PlayerPrefs.GetFloat("musicVolume",.15f));Mode=Mathf.Clamp(PlayerPrefs.GetInt("musicMode",0),0,2);DuckResponses=PlayerPrefs.GetInt("musicDuck",1)==1;
   for(int i=0;i<2;i++){sources[i]=gameObject.AddComponent<AudioSource>();sources[i].playOnAwake=false;sources[i].loop=false;sources[i].spatialBlend=0;sources[i].volume=0;sources[i].priority=200;}
   for(int i=0;i<Files.Length;i++)clips[i]=Resources.Load<AudioClip>("Music/"+Files[i]);
   int saved=Mathf.Clamp(PlayerPrefs.GetInt("musicTrack",0),0,4);Select(Mode==0?random.Next(Titles.Length):saved);}
  public void SetEnabled(bool value){MusicEnabled=value;PlayerPrefs.SetInt("musicEnabled",value?1:0);PlayerPrefs.Save();}
  public void SetVolume(float value){MusicVolume=Mathf.Clamp01(value);PlayerPrefs.SetFloat("musicVolume",MusicVolume);PlayerPrefs.Save();}
  public void SetMode(int value){Mode=Mathf.Clamp(value,0,2);PlayerPrefs.SetInt("musicMode",Mode);PlayerPrefs.Save();}
  public void SetDuck(bool value){DuckResponses=value;PlayerPrefs.SetInt("musicDuck",value?1:0);PlayerPrefs.Save();}
  public void Select(int track){track=Mathf.Clamp(track,0,4);if(clips[track]==null){Debug.LogWarning("Missing music: "+Files[track]);return;}int next=1-active;sources[next].Stop();sources[next].clip=clips[track];sources[next].volume=0;sources[next].Play();active=next;index=track;fade=0;PlayerPrefs.SetInt("musicTrack",index);PlayerPrefs.Save();if(!MusicEnabled||!focus||suspended){PauseSources();}else if(halted){foreach(var source in sources)source.UnPause();halted=false;}}
  public void Next(){int next;if(Mode==2)next=Mathf.Max(0,index);else if(Mode==1)next=(index+1)%Titles.Length;else{next=random.Next(Titles.Length-1);if(next>=index)next++;}Select(next);}
  void PauseSources(){foreach(var source in sources)source.Pause();halted=true;}
  void Update(){if(index<0)return;if(!focus||suspended){if(!halted)PauseSources();return;}float target=MusicEnabled?MusicVolume*(DuckResponses&&ResponseActive?.45f:1f):0;gain=Mathf.MoveTowards(gain,target,Time.unscaledDeltaTime*.65f);
   if(MusicEnabled&&halted){foreach(var source in sources)source.UnPause();halted=false;}if(!MusicEnabled&&gain<=.0001f){if(!halted)PauseSources();return;}if(halted)return;
   fade=Mathf.Min(1,fade+Time.unscaledDeltaTime/Transition);sources[active].volume=gain*Mathf.Sin(fade*Mathf.PI*.5f);sources[1-active].volume=gain*Mathf.Cos(fade*Mathf.PI*.5f);
   if(fade>=1){sources[1-active].Stop();var current=sources[active];if(MusicEnabled&&(!current.isPlaying||current.time>=current.clip.length-Transition))Next();}}
  void OnApplicationFocus(bool value){focus=value;if(!value)PauseSources();}
  void OnApplicationPause(bool value){suspended=value;if(value)PauseSources();}
  void OnDisable(){if(sources[0]!=null){foreach(var source in sources)source.Stop();}}
 }
}
