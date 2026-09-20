using System;
using System.Linq;
namespace DealNight.Core
{
 public struct FittedGrid
 {
  public int Columns,Rows; public float Scale,CellWidth,CellHeight,OffsetX,OffsetY;
 }
 public static class PresentationMath
 {
  // Find the largest uniform scale that keeps every tile within its own region.
  public static FittedGrid Fit(int count,float width,float height,float tileWidth,float tileHeight,float maxScale=1)
  {
   if(count<=0||width<=0||height<=0||float.IsNaN(width)||float.IsNaN(height)||float.IsInfinity(width)||float.IsInfinity(height))return new FittedGrid();
   FittedGrid best=new FittedGrid();
   for(int cols=1;cols<=count;cols++)
   {
    int rows=(count+cols-1)/cols;float cw=width/cols,ch=height/rows;
    float scale=Math.Min(maxScale,Math.Min(cw/(tileWidth+8),ch/(tileHeight+8)));
    if(scale<=best.Scale)continue;
    best=new FittedGrid{Columns=cols,Rows=rows,Scale=scale,CellWidth=cw,CellHeight=ch,OffsetX=(cw-tileWidth*scale)/2,OffsetY=(ch-tileHeight*scale)/2};
   }
   return best;
  }
  public static string PlayerName(string value)
  {
   if(string.IsNullOrWhiteSpace(value))return "";
   string name=string.Join(" ",value.Split((char[])null,StringSplitOptions.RemoveEmptyEntries));
   name=new string(name.Where(c=>!char.IsControl(c)).ToArray());
   return name.Length>24?name.Substring(0,24).Trim():name;
  }
 }
}
