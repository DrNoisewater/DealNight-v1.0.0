using System;
using System.Collections.Generic;
using System.Linq;
namespace DealNight.Core {
 public enum Phase { Drawing, Playing, Reaction, Payment, Discarding, GameOver }
 public enum Op { Bank, Play, Move, End, Discard, Pay, No, Pass }
 public sealed class Command {
  public Op Op; public int Actor,Card=-1,Target=-1,Take=-1,Give=-1,Set=-1; public Group Color; public int[] Extras=new int[0];
  public Command(Op op,int actor,int card=-1) {Op=op;Actor=actor;Card=card;}
 }
 public sealed class PropertySet {
  public int Id; public Group Color; public List<int> Cards=new List<int>(); public int House=-1,Hotel=-1;
 }
 public sealed class Player {
  public string Name; public List<int> Hand=new List<int>(),Bank=new List<int>(); public List<PropertySet> Sets=new List<PropertySet>();
  public int Piece,Plays,Payments,Stolen,Overpaid,LargestOverpayment,Received,Banked,Drawn,Discarded,Actions,Nos;
 }
 public sealed class Effect {
  public Kind Kind; public int Card=-1; public bool AllOpponents; public int Source,Target,Amount,Take,Give,Set; public bool Cancelled; public int Responder;
 }
 // Only public table state and the observer's own hand cross the AI boundary.
 public sealed class Observation {
  public int Actor,Current,Remaining; public Phase Phase; public Card[] Hand; public PublicPlayer[] Players; public Effect Pending;
 }
 public sealed class PublicPlayer {
  public int BankTotal,HandCount,Complete; public int[] Bank; public PropertySet[] Sets;
 }
}
