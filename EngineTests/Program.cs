using System;
using System.Linq;
using DealNight.Core;
using DealNight.Tests;
class Program {
 static int Main(string[] args){try{RuleChecks.RunAll();int matches=args.Length>0?int.Parse(args[0]):300,max=0,total=0;for(int seed=0;seed<matches;seed++){var g=new Game(2+seed%4,seed);var ai=Enumerable.Range(0,g.Players.Count).Select(i=>new Computer((Difficulty)(seed%3),seed*7+i)).ToArray();int steps=0;while(g.Phase!=Phase.GameOver&&steps++<15000){int actor=g.DecisionPlayer;string error;var c=ai[actor].Choose(g.Observe(actor),g.Legal(actor));if(!g.Execute(c,out error))throw new Exception("Seed "+seed+": "+error);g.AssertConservation();}if(g.Phase!=Phase.GameOver)throw new Exception("STALLED seed="+seed+" turn="+g.Turn+" phase="+g.Phase);max=Math.Max(max,steps);total+=steps;}Console.WriteLine("PASS "+matches+" seeded AI matches; max commands="+max+"; total commands="+total);return 0;}catch(Exception e){Console.WriteLine(e);return 1;}}
}
