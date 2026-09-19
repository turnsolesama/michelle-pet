using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
internal static class CompanionOutfitRegression
{
    [DllImport("user32.dll")]static extern bool SetProcessDPIAware();
    static int count;
    static void Assert(bool value,string message){if(!value)throw new Exception(message);count++;Console.WriteLine("PASS "+message);}
    static object Call(object value,string method,params object[] args){return value.GetType().GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(value,args);}
    static object Field(object value,string field){return value.GetType().GetField(field,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(value);}
    static Bitmap Render(object renderer,object input)
    {
        Bitmap image=new Bitmap(800,600);using(Graphics g=Graphics.FromImage(image))
        {g.SmoothingMode=SmoothingMode.AntiAlias;g.InterpolationMode=InterpolationMode.HighQualityBicubic;Call(renderer,"Draw",g,new RectangleF(0,0,800,600),input);}return image;
    }
    [STAThread]static void Main(string[] args)
    {
        try
        {
            SetProcessDPIAware();Application.EnableVisualStyles();string output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
            Assembly a=Assembly.LoadFrom(Path.GetFullPath(args[0]));Type r=a.GetType("CodexPet.CompanionRenderer");
            using(IDisposable input=(IDisposable)Activator.CreateInstance(a.GetType("CodexPet.CompanionInput"),true))
            {
                foreach(bool dorm in new[]{false,true})
                using(IDisposable renderer=(IDisposable)Activator.CreateInstance(r,BindingFlags.NonPublic|BindingFlags.Instance,null,new object[]{dorm},null))
                {
                    string theme=dorm?"dorm":"classic";Call(input,"Reset");
                    using(Bitmap idle=Render(renderer,input))
                    {
                        idle.Save(Path.Combine(output,theme+"-idle.png"));Assert(idle.GetPixel(0,0).A==0 && idle.GetPixel(790,500).A==0,theme+" outer margins stay transparent");
                        Call(input,"SetKey",0x11,true);Call(input,"SetMouse",150,-80,1);
                        using(Bitmap pressed=Render(renderer,input))
                        {
                            pressed.Save(Path.Combine(output,theme+"-ctrl-mouse.png"));bool head=true;int changes=0;
                            for(int y=0;y<600;y++)for(int x=0;x<800;x++)if(idle.GetPixel(x,y)!=pressed.GetPixel(x,y)){changes++;if(y<360)head=false;}
                            Assert(head,theme+" body and face remain fixed during input");Assert(changes>200,theme+" hands and keys visibly respond");
                        }
                    }
                    foreach(int key in new[]{16,17,32,0x50})
                    {Call(input,"Reset");Call(input,"SetKey",key,true);using(Bitmap image=Render(renderer,input))image.Save(Path.Combine(output,theme+"-key-"+key+".png"));}
                    Call(input,"Disable");Assert((float)Field(input,"TypingPulse")==0,theme+" input reset clears typing feedback");
                }
            }
            using(Form pet=(Form)Activator.CreateInstance(a.GetType("CodexPet.Pet"),new object[]{false,output}))
            {
                pet.Show();Application.DoEvents();((Timer)Field(pet,"timer")).Stop();
                Call(pet,"Home");Call(pet,"SetSkin","magic");Call(pet,"SetCompanion",true);pet.Location=new Point(160,150);
                Point position=pet.Location;Size size=pet.Size;Call(pet,"SetCompanionOutfit",true);
                Assert((bool)Field(pet,"companion") && (bool)Field(pet,"companionDorm"),"dorm option enters the requested mode");
                Assert(pet.Location==position && pet.Size==size,"switching outfit preserves free placement and size");
                Call(pet,"SetLanguage",true,false);
                Assert((bool)Field(pet,"companionDorm"),"language switch retains dorm outfit");
                ContextMenuStrip menu=(ContextMenuStrip)Field(pet,"menu");
                Assert(((ToolStripMenuItem)menu.Items["companion-outfit"]).DropDownItems[1].Text=="Feline Energy - Original Hair","dorm menu localizes to English");
                Call(pet,"SetLanguage",false,false);Call(pet,"CaptureEvidence","dorm-native");
                foreach(int height in new[]{160,240,360})
                {
                    Call(pet,"ResizePet",height);Point before=pet.Location;Size dimensions=pet.Size;
                    Call(pet,"SetCompanionOutfit",false);((ToolStripMenuItem)((ToolStripMenuItem)menu.Items["companion-outfit"]).DropDownItems[1]).PerformClick();
                    Assert(pet.Location==before && pet.Size==dimensions,"menu outfit switch keeps geometry at size "+height);
                    Point start=new Point(pet.Left+pet.Width/2,pet.Top+100),end=new Point(start.X+30,start.Y+20);
                    Call(pet,"BeginDrag",start);Call(pet,"MoveDrag",end);Call(pet,"EndDrag",end,100);
                    Assert(Field(pet,"motion").ToString()=="Idle" && (bool)Field(pet,"companionDorm"),"dorm drag releases without falling at size "+height);
                    Call(pet,"CaptureEvidence","dorm-size-"+height);
                }
                position=pet.Location;
                pet.GetType().GetField("paused",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(pet,true);Call(pet,"UpdateInput");Call(pet,"SetCompanionOutfit",false);
                Assert((bool)Field(pet,"paused") && pet.Location==position,"style switch preserves pause and location");
                Call(pet,"SetCompanion",false);Assert((string)Field(pet,"skin")=="magic","leaving either companion outfit restores prior normal skin");
                Call(pet,"SetCompanionOutfit",true);Call(pet,"Sleep");Assert(!(bool)Field(pet,"companion") && Field(pet,"motion").ToString()=="Sleep","sleep leaves dorm companion cleanly");
            }
            Console.WriteLine("TOTAL "+count+"; two-outfit render/state checks and native capture; not physical input.");
        }
        catch(Exception e){Console.WriteLine(e);Environment.ExitCode=1;}
    }
}
