using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
internal static class CompanionRegression
{
    static Type inputType;static object input;static int checks;
    static object Call(string name,params object[] args){return inputType.GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(input,args);}
    static object Field(string name){return inputType.GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(input);}
    static bool Down(int i){return (bool)Call("IsDown",i);}
    static void Assert(bool value,string name){if(!value)throw new Exception(name);checks++;Console.WriteLine("PASS "+name);}
    static void Packet(int header,int type,int key,int flags,int x,int y,int buttons)
    {
        byte[] bytes=new byte[header+24];IntPtr data=Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes,0,data,bytes.Length);Marshal.WriteInt32(data,0,type);Marshal.WriteInt32(data,4,bytes.Length);
            if(type==1){Marshal.WriteInt16(data,header,0x2A);Marshal.WriteInt16(data,header+2,(short)flags);Marshal.WriteInt16(data,header+6,(short)key);}
            else {Marshal.WriteInt16(data,header,(short)flags);Marshal.WriteInt16(data,header+4,(short)buttons);Marshal.WriteInt32(data,header+12,x);Marshal.WriteInt32(data,header+16,y);}
            Call("Decode",data,bytes.Length,header);
        }
        finally{Marshal.FreeHGlobal(data);}
    }
    static void Main(string[] args)
    {
        try
        {
            Assembly assembly=Assembly.LoadFrom(System.IO.Path.GetFullPath(args[0]));inputType=assembly.GetType("CodexPet.CompanionInput");input=Activator.CreateInstance(inputType,true);
            int[] keys={0x57,0x41,0x53,0x44,0x51,0x45,0x52,0x46,0x10,0x11,0x20};
            for(int i=0;i<keys.Length;i++)Call("SetKey",keys[i],true);
            for(int i=0;i<keys.Length;i++)Assert(Down(i),"simultaneous "+keys[i]);
            Call("SetKey",0x57,false);Assert(!Down(0) && Down(1) && Down(8),"independent release");
            Call("Reset");Call("SetKey",0x50,true);Assert((float)Call("Amount",0)==0,"typing P does not pretend W was pressed");
            Call("SetKey",0x57,true);Call("SetKey",0x57,false);Assert((float)Call("Amount",0)>.9,"quick tap survives a rendering interval");
            Call("Advance",.5,false);Assert((float)Call("Amount",0)<.01,"released tap settles");
            Call("SetKey",0x57,true);Call("Advance",.5,false);Call("SetKey",0x57,true);Assert((float)Call("Amount",0)<.8,"OS repeat does not trigger new taps");
            Call("Reset");Call("SetKey",0xA0,true);Call("SetKey",0xA1,true);Call("SetKey",0xA0,false);Assert(Down(8),"right Shift survives left Shift release");
            Call("SetKey",0xA1,false);Assert(!Down(8),"both Shifts released");
            foreach(int header in new int[]{16,24})
            {
                Call("Reset");Packet(header,1,0x57,0,0,0,0);Packet(header,1,0x44,0,0,0,0);Packet(header,1,0x57,1,0,0,0);
                Assert(!Down(0) && Down(3),header+" byte keyboard header ABI");
                Packet(header,0,0,0,200,-100,5);Assert((bool)Field("LeftButton") && (bool)Field("RightButton") && (float)Field("MouseX")>0 && (float)Field("MouseY")<0,header+" mouse buttons and signed deltas ABI");
                Packet(header,0,0,0,0,0,2);Assert(!(bool)Field("LeftButton") && (bool)Field("RightButton"),header+" independent mouse release");
                Call("Reset");Packet(header,0,0,1,65535,65535,1);Assert((float)Field("MouseX")==0 && (float)Field("MouseY")==0 && (bool)Field("LeftButton"),header+" tablet absolute positions do not become relative jumps");
            }
            Call("Reset");Call("SetMouse",Int32.MaxValue,Int32.MinValue,5);Assert(Math.Abs((float)Field("MouseX"))<=9 && Math.Abs((float)Field("MouseY"))<=5,"high DPI mouse stays within reach");
            Call("Disable");Assert(!(bool)Field("LeftButton") && !(bool)Field("RightButton") && (float)Field("MouseX")==0,"off resets all state");
            Type rendererType=assembly.GetType("CodexPet.CompanionRenderer");using(IDisposable renderer=(IDisposable)Activator.CreateInstance(rendererType,true))
            using(Bitmap idle=new Bitmap(400,300))using(Bitmap pressed=new Bitmap(400,300))
            {
                MethodInfo draw=rendererType.GetMethod("Draw",BindingFlags.Instance|BindingFlags.NonPublic);
                using(Graphics g=Graphics.FromImage(idle))draw.Invoke(renderer,new object[]{g,new RectangleF(0,0,400,300),input});
                Call("SetKey",0x57,true);Call("SetMouse",150,-50,1);
                using(Graphics g=Graphics.FromImage(pressed))draw.Invoke(renderer,new object[]{g,new RectangleF(0,0,400,300),input});
                bool head=true;int changed=0;
                for(int y=0;y<300;y++)for(int x=0;x<400;x++)if(idle.GetPixel(x,y)!=pressed.GetPixel(x,y)){if(y<180)head=false;changed++;}
                Assert(head,"face and upper body pixels fixed during concurrent input");Assert(changed>50,"hands and controls visibly react");
                Assert(idle.GetPixel(0,0).A==0 && idle.GetPixel(380,240).A==0,"transparent scene margins");
                if(args.Length>1){idle.Save(System.IO.Path.Combine(args[1],"companion-idle-large.png"));pressed.Save(System.IO.Path.Combine(args[1],"companion-pressed-large.png"));}
                MethodInfo contact=rendererType.GetMethod("KeyboardContact",BindingFlags.Static|BindingFlags.NonPublic);
                RectangleF[] caps=(RectangleF[])rendererType.GetField("Caps",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
                for(int i=0;i<keys.Length;i++)
                {
                    Call("Reset");Call("SetKey",keys[i],true);
                    PointF point=(PointF)contact.Invoke(null,new object[]{input});
                    RectangleF visible=new RectangleF(488-caps[i].Right,520-caps[i].Bottom-1.5f,caps[i].Width,caps[i].Height);
                    Assert(visible.Contains(point),"finger contact inside visible cap "+keys[i]);
                    if(args.Length>1 && i>=8)using(Bitmap preview=new Bitmap(600,450))
                    {
                        using(Graphics g=Graphics.FromImage(preview)){g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;g.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;draw.Invoke(renderer,new object[]{g,new RectangleF(0,0,600,450),input});}
                        preview.Save(System.IO.Path.Combine(args[1],"edge-key-"+keys[i]+".png"));
                    }
                }
                Call("Reset");Call("SetKey",0x57,true);Call("SetKey",0x11,true);
                Assert((int)Call("TrackingKey")==9,"new Ctrl wins over held W without averaging");
                Call("SetKey",0x57,true);Assert((int)Call("TrackingKey")==9,"auto-repeat W cannot steal Ctrl reach");
                Call("SetKey",0x11,false);Call("Advance",.5,false);Assert((int)Call("TrackingKey")==0,"release returns reach to still-held W");
                Call("SetKey",0xA1,true);Assert((int)Call("TrackingKey")==8,"right Shift reaches the Shift cap");
                Call("Reset");Assert((int)Call("TrackingKey")==-1,"off clears hand tracking priority");
                foreach(int key in new int[]{0x50,0x5A,0x31,0x67,0xBA,0xBF,8,9,13,0xE5})
                {
                    Call("Reset");Call("SetKey",key,true);Assert((int)Call("TrackingKey")==-2 && (float)Field("TypingPulse")>.9,"anonymous typing feedback "+key);
                }
                Call("Reset");Call("SetKey",0x70,true);Assert((int)Call("TrackingKey")==-1,"function key does not masquerade as typing");
                Call("SetKey",0x57,true);Call("SetKey",0x50,true);Assert((int)Call("TrackingKey")==-2,"typing reacts while game key is held");
                Call("Advance",.5,false);Assert((int)Call("TrackingKey")==0,"typing settles back to held game key");
                Call("SetKey",0x50,true);int first=(int)Field("TypingLane");Call("SetKey",0x4C,true);Assert((int)Field("TypingLane")!=first,"successive typing alternates tap positions");
                if(args.Length>1)using(Bitmap preview=new Bitmap(600,450))
                {using(Graphics g=Graphics.FromImage(preview))draw.Invoke(renderer,new object[]{g,new RectangleF(0,0,600,450),input});preview.Save(System.IO.Path.Combine(args[1],"generic-typing.png"));}
                Call("Disable");Assert((float)Field("TypingPulse")==0 && (int)Call("TrackingKey")==-1,"off clears anonymous typing state");
            }
            ((IDisposable)input).Dispose();Console.WriteLine("TOTAL "+checks+"; decoder/state/render tests, not physical input.");
        }
        catch(Exception ex){Console.WriteLine(ex);Environment.ExitCode=1;}
    }
}
