using System;
using System.Linq;
namespace DealNight.Core
{
 public static class PresentationOptions
 {
  public static readonly float[] Durations={.25f,.8125f,1.375f,1.9375f,2.5f};
  public static readonly string[] SpeedNames={"Quick","Normal","Relaxed","Slow","Slowest"};
  public static int MigrateSpeed(float previous){for(int i=0;i<Durations.Length;i++)if(Durations[i]>=previous)return i;return 4;}
  public static string[] OpponentNames(int count,int seed,string human)
  {
   string[] pool={"Morgan","Riley","Avery","Casey","Quinn","Parker","Jordan","Blake","Harper","Rowan","Finley","Reese","Cameron","Skyler","Drew","Taylor","Alex","Jamie","Charlie","Sage","Remy","Ellis","Emerson","Dakota","Hayden","River","Robin","Jules","Logan","Arden","Lane","Shay","Amelia","Benny","Cleo","Dexter","Eliza","Felix","Georgia","Hugo","Iris","Jasper","Kira","Leo","Mabel","Nico","Olive","Percy","Ruby","Silas","Tessa","Victor","Willa","Zane","Ada","Beau","Clara","Dante","Esme","Flynn","Greta","Hazel","Idris","Juniper","Kai","Luna","Miles","Nora","Oscar","Piper","Rafael","Stella","Theo","Vera","Wesley","Yara","Zoe","Alfie","Bianca","Cody","Delia","Elliot","Freya","Gus","Holly","Imogen","Jonah","Keiko","Luca","Maeve","Nell","Otis","Priya","Rory","Sasha","Toby","Uma","Violet","Winnie","Xander","Yuki","Zelda","Archie","Bonnie","Carmen","Daisy","Evan","Frankie","Gabe","Heidi","Ivy","Juno","Kit","Lola","Milo","Noel","Opal","Phoebe","Rhea","Sam","Tristan","Val","Wyatt","Xena","Yasmin","Ziggy"};
   var names=pool.Where(x=>!string.Equals(x,human,StringComparison.OrdinalIgnoreCase)).ToArray();
   var random=new Random(unchecked(seed^0x51F238));for(int i=names.Length-1;i>0;i--){int j=random.Next(i+1);var n=names[i];names[i]=names[j];names[j]=n;}
   return names.Take(count).ToArray();
  }
 }
}
