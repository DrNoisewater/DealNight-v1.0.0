"""Original period-lounge score: deterministic notes and synthesized instruments.
No reference recording is read by this generator. Requires numpy, scipy, ffmpeg.
Run: python Tools/compose_soundtrack.py (optional argument: track number 1..5).
"""
import sys,json,tempfile,subprocess
from pathlib import Path
import numpy as np
from scipy.signal import butter,sosfilt
from scipy.io import wavfile
SR=32000
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Assets/Resources/Music'
TRACKS=[('01_Lobby_at_Dusk','Lobby at Dusk',120,65,'brass'),('02_Penny_Arcade_Promenade','Penny Arcade Promenade',116,70,'reed'),('03_The_Velvet_Exchange','The Velvet Exchange',100,63,'piano'),('04_Sidewalk_Serenade','Sidewalk Serenade',112,67,'reed'),('05_Last_Tram_Home','Last Tram Home',96,60,'brass')]
QUAL={'6':[0,4,7,9],'m7':[0,3,7,10],'7':[0,4,7,10],'dim':[0,3,6,9]}
A=[(0,'6'),(9,'m7'),(2,'m7'),(7,'7'),(0,'6'),(0,'7'),(5,'6'),(6,'dim'),(0,'6'),(9,'7'),(2,'m7'),(7,'7'),(4,'m7'),(9,'7'),(2,'m7'),(7,'7')]
B=[(5,'6'),(5,'m7'),(0,'6'),(9,'7'),(2,'7'),(2,'7'),(7,'7'),(7,'7')]
def instrument(note,seconds,kind):
 t=np.arange(int(seconds*SR))/SR;f=440*2**((note-69)/12);phase=2*np.pi*f*t
 release=np.minimum(1,np.maximum(0,(seconds-t)/.12))
 if kind in ['reed','brass']:
  attack=1-np.exp(-t/(.045 if kind=='brass' else .03));vibrato=.018*np.sin(2*np.pi*4.7*t)*np.minimum(1,t/.35);p=phase+vibrato;sig=np.zeros_like(t)
  for h in range(1,15):
   weight=((1 if h%2 else .16)/h**1.35) if kind=='reed' else ((.45+1.2*np.exp(-((h*f-950)/650)**2))/h**1.25)
   sig+=weight*np.sin(h*p)*np.exp(-t*.18*h/5)
  env=attack*release*(.86+.14*np.exp(-t*5));sig*=env*.52
 elif kind=='piano':
  sig=np.zeros_like(t)
  for h in range(1,10):
   sig+=(1/h**1.55)*np.sin(phase*h*np.sqrt(1+.00016*h*h))*np.exp(-t*(1.1+.65*h))
  sig*= (1-np.exp(-t/.003))*release
 elif kind=='bass':
  sig=sum(np.sin(phase*h)/h**1.7*np.exp(-t*(1.6+h*.6)) for h in range(1,9));sig*=(1-np.exp(-t/.009))*release
 elif kind=='guitar':
  sig=sum(np.sin(phase*h+.08*h)/h**1.6*np.exp(-t*(2.1+h*1.1)) for h in range(1,10));sig*=(1-np.exp(-t/.004))*release
 else:
  sig=(np.sin(phase)+.1*np.sin(phase*4)*np.exp(-t*8))*np.exp(-t*2.3)*(1+.06*np.sin(2*np.pi*5*t));sig*=(1-np.exp(-t/.01))*release
 return sig.astype(np.float32)
def render(index):
 stem,title,bpm,key,lead=TRACKS[index];rng=np.random.default_rng(1981+index);beat=60/bpm;bars=48;duration=bars*4*beat+3;mix=np.zeros((int(duration*SR),2),np.float32)
 def add(sig,at,amp,pan):
  offset=max(0,int(at*SR));n=min(len(sig),len(mix)-offset)
  if n>0:
   mix[offset:offset+n,0]+=sig[:n]*amp*np.sqrt((1-pan)/2);mix[offset:offset+n,1]+=sig[:n]*amp*np.sqrt((1+pan)/2)
 # Explicit wrapper keeps instrument synthesis independent of reference audio.
 def play(n,at,length,kind,amp,pan=0):add(instrument(n,length*beat,kind),(at+rng.uniform(-.009,.009))*beat,amp*rng.uniform(.92,1.04),pan)
 t=np.arange(int(.22*SR))/SR;brush=sosfilt(butter(2,[600,4000],btype='bandpass',fs=SR,output='sos'),rng.normal(0,1,len(t)))*np.sin(np.linspace(0,np.pi,len(t)))**2
 th=np.arange(int(.12*SR))/SR;hat=sosfilt(butter(2,[1800,5000],btype='bandpass',fs=SR,output='sos'),rng.normal(0,1,len(th)))*np.exp(-th*38)
 kick=np.sin(2*np.pi*(46*t+18*.03*(1-np.exp(-t/.03))))*np.exp(-t*25)*(1-np.exp(-t*300))
 motifs=[[2,1,0,2,3,1],[0,1,2,1,3,2],[1,0,2,3,2,0],[3,2,1,0,2,1],[0,2,1,3,1,0]]
 for bar in range(bars):
  bridge=24<=bar<32;root,q=(B[(bar-24)%8] if bridge else A[bar%16]);
  if bar>=44:root,q=[(2,'m7'),(7,'7'),(0,'6'),(0,'6')][bar-44]
  chord=[key+root+x for x in QUAL[q]];at=bar*4;quiet=.65 if bar<4 or bar>=44 else .83 if bridge else 1
  bass=key+root-24
  while bass>48:bass-=12
  # Walking / two-beat bass, restrained so decisions remain foreground.
  bassline=[bass,bass+7,bass+9,bass+7] if q=='6' else [bass,bass+7,bass+10,bass+7]
  for k,n in enumerate(bassline):
   if bar<4 and k%2:continue
   play(n,at+k,.85,'bass',.18*quiet)
  # Sparse piano chord punches; mellow guitar on offbeats gives the period pulse.
  for pos in ([0,2.66] if index!=2 else [.66,2]):
   for j,n in enumerate(chord[1:]):
    while n>76:n-=12
    play(n,at+pos+j*.013,.95,'piano',.11*quiet,-.2)
  if bar>=4 and bar<44:
   for pos in [1,3]:
    for n in chord[1:3]:play(n-12 if n>76 else n,at+pos,.55,'guitar',.045*quiet,.18)
   for k in range(8):add(hat,(at+k//2+(0 if k%2==0 else .64))*beat,.019*quiet,.22)
   for pos in [1,3]:add(brush,(at+pos+.025)*beat,.026*quiet,.12)
   for pos in [0,2]:add(kick,(at+pos)*beat,.035*quiet,0)
  if 4<=bar<44 and bar%4!=2:
   motif=motifs[index];positions=([.66,1.66,3] if bar%4==0 else [0,1.33,2.66] if bar%4==1 else [.66,2]);lengths=[.65,.6,1.15]
   sound='piano' if bridge else ('vibes' if 16<=bar<24 and lead!='piano' else lead)
   for k,pos in enumerate(positions):
    degree=motif[(bar%4*2+k)%6];n=chord[degree]
    while n>80:n-=12
    while n<65:n+=12
    play(n,at+pos,lengths[k],sound,.105*quiet if sound!='piano' else .16*quiet,.1)
  # A small answering figure, not a constantly busy second melody.
  if bar%8==6 and bar<43:
   for k,n in enumerate([chord[2],chord[1],chord[0]]):
    while n>81:n-=12
    play(n,at+2+k*.5,.55,'piano',.09,-.15)
 dry=mix.copy()
 for delay,amp in [(.053,.12),(.091,.085),(.151,.045),(.247,.022)]:
  n=int(SR*delay);mix[n:]+=dry[:-n,::-1]*amp
 mix=sosfilt(butter(2,5600,fs=SR,output='sos'),mix,axis=0).astype(np.float32);mix-=mix.mean(axis=0);mix=np.tanh(mix*.8);mix*=.52/max(.001,float(abs(mix).max()))
 mix[:SR]*=np.linspace(0,1,SR)[:,None];mix[-SR*3:]*=np.linspace(1,0,SR*3)[:,None]
 OUT.mkdir(parents=True,exist_ok=True)
 with tempfile.TemporaryDirectory() as temp:
  wav=Path(temp)/'mix.wav';wavfile.write(wav,SR,(mix*32767).astype(np.int16));subprocess.run(['ffmpeg','-v','error','-y','-i',str(wav),'-c:a','libvorbis','-q:a','5',str(OUT/(stem+'.ogg'))],check=True)
 result={'file':stem+'.ogg','title':title,'bpm':bpm,'seconds':round(duration,2),'peak_dbfs':round(float(20*np.log10(abs(mix).max())),2),'rms_dbfs':round(float(20*np.log10(np.sqrt(np.mean(mix**2)))),2)};print(json.dumps(result),flush=True);return result
if __name__=='__main__':
 ids=[int(sys.argv[1])-1] if len(sys.argv)>1 else range(5);report=[render(i) for i in ids]
 if len(report)==5:(ROOT/'soundtrack-report.json').write_text(json.dumps(report,indent=2))
