using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
class CompanionExpressionRegression
{
    [DllImport("user32.dll")]static extern bool SetProcessDPIAware();
    static int count;
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);count++;Console.WriteLine("PASS "+message);}
    static object Call(object o,string n,params object[] args){return o.GetType().GetMethod(n,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(o,args);}
    static object Get(object o,string n){return o.GetType().GetField(n,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(o);}
    static void Set(object o,string n,object v){o.GetType().GetField(n,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(o,v);}
    static Bitmap Render(object r,object input,int expression,bool blink)
    {
        Bitmap b=new Bitmap(800,600);using(Graphics g=Graphics.FromImage(b)){g.SmoothingMode=SmoothingMode.AntiAlias;g.InterpolationMode=InterpolationMode.HighQualityBicubic;Call(r,"DrawAnimated",g,new RectangleF(0,0,800,600),input,expression,blink,0,0f);}return b;
    }
    [STAThread]static void Main(string[] args)
    {
        try{
            SetProcessDPIAware();Application.EnableVisualStyles();string output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);Assembly a=Assembly.LoadFrom(Path.GetFullPath(args[0]));
            using(IDisposable input=(IDisposable)Activator.CreateInstance(a.GetType("CodexPet.CompanionInput"),true))
            for(int style=0;style<3;style++)
            using(IDisposable renderer=(IDisposable)Activator.CreateInstance(a.GetType("CodexPet.CompanionRenderer"),BindingFlags.Instance|BindingFlags.NonPublic,null,new object[]{style>0,style==2},null))
            using(Bitmap neutral=Render(renderer,input,0,false))
            {
                neutral.Save(Path.Combine(output,"style-"+style+"-neutral.png"));
                foreach(int expression in new[]{1,2,3})using(Bitmap face=Render(renderer,input,expression==3?0:expression,expression==3))
                {
                    face.Save(Path.Combine(output,"style-"+style+"-face-"+expression+".png"));int changes=0;bool faceOnly=true;
                    for(int y=0;y<600;y++)for(int x=0;x<800;x++)if(neutral.GetPixel(x,y)!=face.GetPixel(x,y)){changes++;if(x<280||x>510||y<165||y>320)faceOnly=false;}
                    Check(changes>100,"style "+style+" expression "+expression+" visibly changes");Check(faceOnly,"style "+style+" expression "+expression+" preserves hair, body, desk and hands");
                }
                using(Bitmap reset=Render(renderer,input,0,false)){bool same=true;for(int y=0;y<600;y++)for(int x=0;x<800;x++)if(reset.GetPixel(x,y)!=neutral.GetPixel(x,y))same=false;Check(same,"style "+style+" neutral restores exact pixels");}
            }
            using(Form pet=(Form)Activator.CreateInstance(a.GetType("CodexPet.Pet"),new object[]{false,output}))
            {
                pet.Show();Application.DoEvents();((Timer)Get(pet,"timer")).Stop();Call(pet,"SetSkin","magic");Call(pet,"SetCompanion",true);pet.Location=new Point(180,160);Point position=pet.Location;Size size=pet.Size;
                ContextMenuStrip menu=(ContextMenuStrip)Get(pet,"menu");ToolStripMenuItem styles=(ToolStripMenuItem)menu.Items["companion-outfit"],interactions=(ToolStripMenuItem)menu.Items["buddy-interactions"];
                Check(styles.DropDownItems.Count==3,"all three companion styles are selectable");
                for(int style=0;style<3;style++){
                    Set(pet,"paused",true);Call(pet,"SetCompanionStyle",style);Check((bool)Get(pet,"companionDorm")== (style>0)&&(bool)Get(pet,"companionRefined")== (style==2),"style selection "+style);
                    Check(pet.Location==position&&pet.Size==size&&(bool)Get(pet,"paused"),"style "+style+" preserves placement, size and pause");
                    for(int i=0;i<3;i++)Check(((ToolStripMenuItem)styles.DropDownItems[i]).Checked==(i==style),"exclusive style check "+style+"/"+i);
                    foreach(int reaction in new[]{1,2,3}){Call(pet,"BuddyReact",reaction);Check((int)Get(pet,"companionExpression")== (reaction==2?2:1),"reaction expression "+style+"/"+reaction);Check(pet.Location==position,"reaction leaves window stationary "+style+"/"+reaction);}
                    Call(pet,"BuddyReact",0);Check((int)Get(pet,"companionExpression")==0&&(double)Get(pet,"companionEmotionUntil")==0,"neutral clears transient reaction "+style);
                }
                Call(pet,"SetLanguage",true,false);Check(interactions.Text=="Interactions & Expressions","interaction menu translates");Call(pet,"BuddyReact",2);Check(((string)Get(pet,"words")).Contains("cheeks"),"reaction bubble translates");Call(pet,"SetLanguage",false,false);Check(!((string)Get(pet,"words")).Contains("cheeks"),"active reaction translates without reset");
                Call(pet,"SetLanguage",true,false);
                foreach(int height in new[]{160,240,360}){Call(pet,"ResizePet",height);foreach(int reaction in new[]{1,2,3}){Call(pet,"BuddyReact",reaction);Call(pet,"CaptureEvidence","reaction-"+height+"-"+reaction);}}
                Call(pet,"BuddyReact",1);double deadline=(double)Get(pet,"companionEmotionUntil");Set(pet,"time",deadline+.1);Call(pet,"CaptureEvidence","expired-reaction");Check((double)Get(pet,"time")>(double)Get(pet,"companionEmotionUntil"),"reaction expires after its deadline");
                ((ToolStripMenuItem)interactions.DropDownItems[4]).PerformClick();Check(!(bool)Get(pet,"companionBlink"),"automatic blink can be disabled");
                Call(pet,"BuddyReact",0);int pad=(int)a.GetType("CodexPet.Pet").GetField("Pad",BindingFlags.Static|BindingFlags.NonPublic).GetRawConstantValue();
                Point touch=new Point(pet.Left+pet.Width/2,pet.Top+pad+30);Call(pet,"BeginDrag",touch);Call(pet,"EndDrag",touch,pad+30);Check((int)Get(pet,"companionExpression")==1,"stationary head click pets companion");
                Point end=new Point(touch.X+30,touch.Y+20);Call(pet,"BeginDrag",touch);Call(pet,"MoveDrag",end);Call(pet,"EndDrag",end,pad+50);Check(Get(pet,"motion").ToString()=="Idle","drag still releases without falling");
                Call(pet,"SetCompanion",false);Check((string)Get(pet,"skin")=="magic"&&(int)Get(pet,"companionExpression")==0,"exit restores prior skin and clears mood");
            }
            Console.WriteLine("TOTAL "+count+"; method-driven expression and interaction checks.");
        }catch(Exception e){Console.WriteLine(e);Environment.ExitCode=1;}
    }
}
