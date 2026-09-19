using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
[assembly: AssemblyTitle("Michele Desktop Pet / 米雪儿桌宠")]
[assembly: AssemblyVersion("0.3.7.0")]
namespace CodexPet
{
    internal static class Program
    {
        [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
        [STAThread] static void Main(string[] args)
        {
            SetProcessDPIAware(); Application.EnableVisualStyles();
            bool check=args.Length>0 && args[0]=="--check", created;
            using(Mutex mutex=new Mutex(true,check?"MichellePet.Check":"MichellePet.Local.v1",out created))
            {
                if(!created)return;
                if(!check)PetText.LoadPreference(PetText.SettingsPath);
                if(Array.IndexOf(args,"--lang=en")>=0)PetText.English=true;
                if(Array.IndexOf(args,"--lang=zh")>=0)PetText.English=false;
                try
                {
                    using(Pet pet=new Pet(check,args.Length>1?args[1]:null))
                    {
                        if(!check && Array.IndexOf(args,"--companion")>=0)pet.Shown+=delegate{pet.SetCompanion(true);};
                        Application.Run(pet);
                    }
                }
                catch(Exception error)
                {
                    if(check) { if(args.Length>1){Directory.CreateDirectory(args[1]);File.WriteAllText(Path.Combine(args[1],"FAILED.txt"),error.ToString());} Environment.ExitCode=1; }
                    else MessageBox.Show(error.ToString(),PetText.StartupError);
                }
                finally { mutex.ReleaseMutex(); }
            }
        }
    }
    internal sealed class Pet : LayeredForm
    {
        enum Motion { Idle, Sleep, Drag, Fall, Docked }
        readonly SpriteBank bank=new SpriteBank();
        readonly System.Windows.Forms.Timer timer=new System.Windows.Forms.Timer();
        readonly NotifyIcon tray=new NotifyIcon();
        readonly ContextMenuStrip menu=new ContextMenuStrip();
        readonly Stopwatch elapsed=Stopwatch.StartNew();
        readonly Queue<Action> checks=new Queue<Action>();
        readonly CompanionInput companionInput=new CompanionInput();
        CompanionRenderer companionRenderer;
        bool companion,linked=true,companionDorm;
        string previousSkin="classic";
        readonly bool diagnostic;
        readonly string evidence;
        Bitmap frame; Icon petIcon,trayIcon;
        Motion motion=Motion.Idle;
        string skin="classic", words=PetText.Greeting;
        int petHeight=240, side, suppressedSide;
        const int Pad=16, BubbleHeight=50;
        bool held, moved, paused, dockDrag, suppressDock;
        Point press, origin, stable;
        Rectangle dockArea, suppressedArea;
        double velocity, preciseY, time, responseUntil=3, jumpUntil, lastTick, nextCheck, dockStarted;
        int assertions;
        static readonly string[] Skins={"classic","dessert","heart","magic"};
        static string[] Names {get{return new string[]{PetText.Classic,PetText.Dessert,PetText.Heart,PetText.Magic};}}
        Action refreshLanguage;
        protected override bool ShowWithoutActivation {get{return true;}}
        public Pet(bool check,string output)
        {
            diagnostic=check;evidence=output;Text=PetText.AppName;
            FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=false;TopMost=true;
            StartPosition=FormStartPosition.Manual;AutoScaleMode=AutoScaleMode.None;
            using(Stream stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("App.Icon"))
            {
                petIcon=new Icon(stream,new Size(32,32));
                stream.Position=0;trayIcon=new Icon(stream,SystemInformation.SmallIconSize);
            }
            Icon=petIcon;tray.Icon=trayIcon;tray.Text=PetText.Tray;tray.Visible=!check;
            tray.ContextMenuStrip=menu;tray.DoubleClick+=delegate{Home();Say(PetText.Recall);};
            ToolStripMenuItem game=new ToolStripMenuItem(PetText.Companion);game.Click+=delegate{SetCompanion(!companion);};menu.Items.Add(game);
            ToolStripMenuItem link=new ToolStripMenuItem(PetText.Link);link.Click+=delegate{linked=!linked;UpdateInput();Render();};menu.Items.Add(link);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(PetText.Pat,null,delegate{Say(PetText.PatReply);});
            menu.Items.Add(PetText.Hop,null,delegate{Jump();});
            ToolStripMenuItem sleep=new ToolStripMenuItem(PetText.Nap);sleep.Click+=delegate{if(motion==Motion.Sleep)Say(PetText.WakeReply);else Sleep();};menu.Items.Add(sleep);
            ToolStripMenuItem wardrobe=new ToolStripMenuItem(PetText.Outfit);
            for(int i=0;i<Skins.Length;i++){string selected=Skins[i];ToolStripMenuItem item=new ToolStripMenuItem(Names[i]);item.Tag=selected;item.Click+=delegate{SetSkin(selected);};wardrobe.DropDownItems.Add(item);}menu.Items.Add(wardrobe);
            ToolStripMenuItem sizes=new ToolStripMenuItem(PetText.Size);
            foreach(int value in new int[]{160,240,360}){int selected=value;ToolStripMenuItem item=new ToolStripMenuItem(value==160?PetText.Small:value==240?PetText.Medium:PetText.Large);item.Tag=value;item.Click+=delegate{ResizePet(selected);};sizes.DropDownItems.Add(item);}menu.Items.Add(sizes);
            ToolStripMenuItem edges=new ToolStripMenuItem(PetText.Edge);
            edges.DropDownItems.Add(PetText.Left,null,delegate{DockAt(-1,Work,Top+Height/2);});
            edges.DropDownItems.Add(PetText.Right,null,delegate{DockAt(1,Work,Top+Height/2);});menu.Items.Add(edges);
            ToolStripMenuItem expand=new ToolStripMenuItem(PetText.Expand);expand.Click+=delegate{Expand();};menu.Items.Add(expand);
            ToolStripMenuItem pause=new ToolStripMenuItem(PetText.Pause){CheckOnClick=true};pause.Click+=delegate{paused=pause.Checked;UpdateInput();};menu.Items.Add(pause);
            ToolStripMenuItem top=new ToolStripMenuItem(PetText.Top){Checked=true,CheckOnClick=true};top.Click+=delegate{TopMost=top.Checked;};menu.Items.Add(top);
            ToolStripItem home=menu.Items.Add(PetText.Home,null,delegate{Home();});
            ToolStripMenuItem language=new ToolStripMenuItem("语言 / Language"){Name="language"};
            ToolStripMenuItem chinese=new ToolStripMenuItem("简体中文"),english=new ToolStripMenuItem("English");
            chinese.Click+=delegate{SetLanguage(false,true);};english.Click+=delegate{SetLanguage(true,true);};
            language.DropDownItems.Add(chinese);language.DropDownItems.Add(english);menu.Items.Add(language);
            ToolStripMenuItem looks=new ToolStripMenuItem(PetText.CompanionOutfit){Name="companion-outfit"};
            ToolStripMenuItem classicLook=new ToolStripMenuItem(PetText.Classic),dormLook=new ToolStripMenuItem(PetText.DormOutfit);
            classicLook.Click+=delegate{SetCompanionOutfit(false);};dormLook.Click+=delegate{SetCompanionOutfit(true);};
            looks.DropDownItems.Add(classicLook);looks.DropDownItems.Add(dormLook);menu.Items.Add(looks);
            menu.Items.Add(new ToolStripSeparator());ToolStripItem exit=menu.Items.Add(PetText.Exit,null,delegate{Close();});
            refreshLanguage=delegate
            {
                Text=PetText.AppName;tray.Text=PetText.Tray;
                game.Text=companionDorm?PetText.DormCompanion:PetText.Companion;link.Text=PetText.Link;
                looks.Text=PetText.CompanionOutfit;classicLook.Text=PetText.Classic;dormLook.Text=PetText.DormOutfit;
                classicLook.Checked=!companionDorm;dormLook.Checked=companionDorm;
                menu.Items[3].Text=PetText.Pat;menu.Items[4].Text=PetText.Hop;
                sleep.Text=motion==Motion.Sleep?PetText.Wake:PetText.Nap;
                wardrobe.Text=PetText.Outfit;for(int i=0;i<Names.Length;i++)wardrobe.DropDownItems[i].Text=Names[i];
                sizes.Text=PetText.Size;sizes.DropDownItems[0].Text=PetText.Small;sizes.DropDownItems[1].Text=PetText.Medium;sizes.DropDownItems[2].Text=PetText.Large;
                edges.Text=PetText.Edge;edges.DropDownItems[0].Text=PetText.Left;edges.DropDownItems[1].Text=PetText.Right;
                expand.Text=PetText.Expand;pause.Text=PetText.Pause;top.Text=PetText.Top;home.Text=PetText.Home;exit.Text=PetText.Exit;
                chinese.Checked=!PetText.English;english.Checked=PetText.English;
            };
            refreshLanguage();
            menu.Opening+=delegate
            {
                refreshLanguage();pause.Checked=paused;expand.Visible=motion==Motion.Docked;
                game.Checked=companion;link.Checked=linked;link.Enabled=companion;
                companionInput.Disable();
                foreach(ToolStripMenuItem i in wardrobe.DropDownItems)i.Checked=(string)i.Tag==skin;
                foreach(ToolStripMenuItem i in sizes.DropDownItems)i.Checked=(int)i.Tag==petHeight;
            };
            menu.Closed+=delegate{UpdateInput();};
            ContextMenuStrip=menu;timer.Interval=33;timer.Tick+=Tick;
            Shown+=delegate{Home();if(diagnostic)SetupChecks();lastTick=elapsed.Elapsed.TotalSeconds;timer.Start();};
        }
        internal void SetLanguage(bool english,bool save)
        {
            words=PetText.TranslateTo(words,english);PetText.English=english;
            refreshLanguage();Render();
            if(save && !diagnostic && !PetText.SavePreference(PetText.SettingsPath))
                tray.ShowBalloonTip(4000,"语言 / Language",PetText.SaveError,ToolTipIcon.Warning);
        }
        int FullWidth {get{return (int)Math.Round(petHeight*(companion?4.0/3:.86))+2*Pad;}}
        int FullHeight {get{return petHeight+2*Pad+BubbleHeight;}}
        Rectangle Work {get{return motion==Motion.Docked?dockArea:Screen.FromPoint(new Point(Left+Width/2,Top+Height/2)).WorkingArea;}}
        int Floor {get{return Work.Bottom-FullHeight+Pad;}}
        internal void SetCompanion(bool enabled)
        {
            if(companion==enabled)return;
            if(motion==Motion.Docked)Expand();
            Rectangle area=Work;int center=Left+Width/2,bottom=Top+Height-Pad;
            ClearMovement();motion=Motion.Idle;paused=false;
            if(enabled){previousSkin=skin;skin="classic";if(companionRenderer==null)companionRenderer=new CompanionRenderer(companionDorm);}
            else skin=previousSkin;
            companion=enabled;Render();
            Location=new Point(Clamp(center-Width/2,area.Left,area.Right-Width),Clamp(bottom-Height+Pad,area.Top,area.Bottom-Height+Pad));
            preciseY=Top;motion=!companion && Top<Floor?Motion.Fall:Motion.Idle;UpdateInput();
        }
        internal void SetCompanionOutfit(bool dorm)
        {
            if(companionDorm!=dorm)
            {
                CompanionRenderer next=new CompanionRenderer(dorm),previous=companionRenderer;
                companionRenderer=next;companionDorm=dorm;if(previous!=null)previous.Dispose();
            }
            if(!companion)SetCompanion(true);else Render();
            refreshLanguage();
        }
        void UpdateInput()
        {
            bool active=companion && linked && !paused && !held && !menu.Visible;
            if(active && !companionInput.Enabled)
            {
                try{companionInput.Enable(Handle);}
                catch(System.ComponentModel.Win32Exception error)
                {
                    linked=false;words=PetText.LinkError;responseUntil=time+4;
                    if(diagnostic)throw;
                    tray.ShowBalloonTip(4000,PetText.LinkErrorTitle,error.Message,ToolTipIcon.Warning);
                }
            }
            else if(!active)companionInput.Disable();
        }
        protected override void WndProc(ref Message message)
        {
            if(message.Msg==0xFF && companionInput!=null)companionInput.Read(message.LParam);
            base.WndProc(ref message); // DefWindowProc releases foreground WM_INPUT resources.
        }
        void ClearMovement(){held=moved=dockDrag=false;Capture=false;velocity=0;jumpUntil=responseUntil=0;}
        void Home()
        {
            ClearMovement();motion=Motion.Idle;paused=false;suppressDock=false;
            Rectangle area=Screen.FromPoint(Cursor.Position).WorkingArea;
            Location=new Point(area.Right-FullWidth-40,area.Bottom-FullHeight+Pad);preciseY=Top;Render();
            UpdateInput();
        }
        void SetSkin(string value)
        {
            if(Array.IndexOf(Skins,value)<0)throw new ArgumentException("Unknown skin");
            if(companion)SetCompanion(false);
            int center=Top+Height/2;skin=value;Render();if(motion==Motion.Docked)PlaceDock(center);
        }
        void ResizePet(int size)
        {
            Rectangle area=Work;int bottom=Top+Height-Pad,centerX=Left+Width/2,centerY=Top+Height/2;
            petHeight=size;Render();
            if(motion==Motion.Docked){PlaceDock(centerY);return;}
            Location=new Point(Clamp(centerX-Width/2,area.Left,area.Right-Width),Clamp(bottom-Height+Pad,area.Top,area.Bottom-Height+Pad));
            preciseY=Top;velocity=0;if(motion!=Motion.Sleep)motion=!companion && Top<Floor?Motion.Fall:Motion.Idle;
        }
        static int Clamp(int n,int low,int high){return Math.Max(low,Math.Min(Math.Max(low,high),n));}
        void Ground()
        {
            if(motion==Motion.Docked)Expand();
            ClearMovement();motion=Motion.Idle;if(!companion)Top=Floor;preciseY=Top;paused=false;
            UpdateInput();
        }
        void Say(string line){Ground();words=line;responseUntil=time+2.8;Render();}
        void Jump(){Say(PetText.HopReply);jumpUntil=time+.65;}
        void Sleep(){if(companion)SetCompanion(false);Ground();motion=Motion.Sleep;Render();}
        void DockAt(int which,Rectangle area,int center)
        {
            if(companion)SetCompanion(false);
            ClearMovement();motion=Motion.Docked;side=which;dockArea=area;dockStarted=time;paused=false;Render();PlaceDock(center);
        }
        void PlaceDock(int center)
        {Location=new Point(side<0?dockArea.Left:dockArea.Right-Width,Clamp(center-Height/2,dockArea.Top,dockArea.Bottom-Height));preciseY=Top;}
        void Expand()
        {
            if(motion!=Motion.Docked)return;
            Rectangle area=dockArea;int center=Top+Height/2,previousSide=side;
            ClearMovement();motion=Motion.Idle;side=0;suppressDock=true;suppressedSide=previousSide;suppressedArea=area;paused=false;Render();
            Location=new Point(previousSide<0?area.Left+36:area.Right-Width-36,Clamp(center-Height/2,area.Top,area.Bottom-Height+Pad));
            preciseY=Top;velocity=0;motion=Top<Floor?Motion.Fall:Motion.Idle;
        }
        void BeginDrag(Point p)
        {
            press=p;origin=Location;held=true;moved=false;dockDrag=motion==Motion.Docked;paused=false;velocity=0;
            UpdateInput();
            // An intentional new drag must be able to return to either edge without a detour.
            if(!dockDrag)suppressDock=false;
            if(motion==Motion.Sleep)motion=Motion.Idle;
            if(motion==Motion.Fall)motion=Motion.Idle;
        }
        void MoveDrag(Point p)
        {
            if(!held)return;int dx=p.X-press.X,dy=p.Y-press.Y;
            if(!moved && dx*dx+dy*dy>=36){moved=true;responseUntil=jumpUntil=0;}
            if(!moved)return;
            if(dockDrag)
            {
                int inward=side<0?dx:-dx;
                if(inward>Math.Max(28,petHeight*.14))
                {
                    Expand();held=moved=true;dockDrag=false;motion=Motion.Drag;
                    Location=new Point(p.X-Width/2,p.Y-BubbleHeight-petHeight/4);press=p;origin=Location;Capture=true;
                }
                else Top=Clamp(origin.Y+dy,dockArea.Top,dockArea.Bottom-Height);
            }
            else
            {
                motion=Motion.Drag;Location=new Point(origin.X+dx,origin.Y+dy);
                Rectangle area=Screen.FromPoint(p).WorkingArea;
                if(suppressDock && (area!=suppressedArea || (suppressedSide<0?Left>area.Left+64:Right<area.Right-64)))suppressDock=false;
            }
            preciseY=Top;Render();
        }
        void EndDrag(Point p,int localY)
        {
            if(!held)return;bool wasMoved=moved,wasDock=dockDrag;held=moved=dockDrag=false;Capture=false;
            if(wasDock){if(!wasMoved)Expand();return;}
            if(wasMoved)ReleasePet(p);
            else if(localY>BubbleHeight+petHeight*.8)Jump();else Say(PetText.TouchReply);
            UpdateInput();
        }
        protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button==MouseButtons.Left){BeginDrag(Cursor.Position);Capture=true;}}
        protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);MoveDrag(Cursor.Position);}
        protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);if(e.Button==MouseButtons.Left)EndDrag(Cursor.Position,e.Y);}
        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);
            if(!Capture && held){bool dock=motion==Motion.Docked;held=moved=dockDrag=false;if(!dock)ReleasePet(Cursor.Position);UpdateInput();}
        }
        void ReleasePet(Point p)
        {
            Rectangle area=Screen.FromPoint(p).WorkingArea;
            if(companion)
            {
                // The desk is an overlay: release exactly where placed, with no gravity or edge snap.
                Location=new Point(Clamp(Left,area.Left,area.Right-Width),Clamp(Top,area.Top,area.Bottom-Height+Pad));
                preciseY=Top;velocity=0;motion=Motion.Idle;Render();return;
            }
            int distance=Math.Max(20,(int)(petHeight*.12));
            int candidate=Left<=area.Left+distance?-1:Right>=area.Right-distance?1:0;
            bool blocked=suppressDock && candidate==suppressedSide && area==suppressedArea;
            if(candidate!=0 && !blocked)
            {DockAt(candidate,area,Top+Height/2);return;}
            Location=new Point(Clamp(Left,area.Left,area.Right-Width),Clamp(Top,area.Top,area.Bottom-FullHeight+Pad));
            preciseY=Top;velocity=0;motion=Top<Floor?Motion.Fall:Motion.Idle;
        }
        void Tick(object sender,EventArgs e)
        {
            try
            {
                double now=elapsed.Elapsed.TotalSeconds,dt=Math.Min(.05,Math.Max(0,now-lastTick));lastTick=now;
                if(!paused)
                {
                    time+=dt;
                    if(companion)companionInput.Advance(dt,!diagnostic);
                    if(motion==Motion.Fall && !held && !companion)
                    {velocity+=1650*dt;preciseY+=velocity*dt;if(preciseY>=Floor){preciseY=Floor;velocity=0;motion=Motion.Idle;}Top=(int)Math.Round(preciseY);}
                    Render();
                }
                if(diagnostic && now>=nextCheck && checks.Count>0){checks.Dequeue()();nextCheck=now+.18;}
            }
            catch(Exception error)
            {
                if(!diagnostic)throw;File.WriteAllText(Path.Combine(evidence,"FAILED.txt"),error.ToString());Environment.ExitCode=1;Close();
            }
        }
        void Render()
        {
            bool dock=motion==Motion.Docked,sleep=motion==Motion.Sleep;
            Bitmap sprite=bank.Skin(skin,dock?(side<0?"wall-left":"wall-right"):sleep?"sleep":time<responseUntil?"happy":"idle");
            int width=FullWidth,height=FullHeight;
            if(dock){height=(int)Math.Round(petHeight*.78);width=(int)Math.Round(height*sprite.Width/(double)sprite.Height);}
            Bitmap next=new Bitmap(width,height,PixelFormat.Format32bppArgb);
            using(Graphics g=Graphics.FromImage(next))
            {
                g.SmoothingMode=SmoothingMode.AntiAlias;g.InterpolationMode=InterpolationMode.HighQualityBicubic;
                if(dock)
                {
                    // One short arrival reveal. After settling, hands and native window stay fixed.
                    double arrive=Math.Max(0,Math.Min(1,(time-dockStarted)/.28));
                    int inset=(int)Math.Round((1-arrive)*(1-arrive)*width*.16);
                    g.DrawImage(sprite,new Rectangle(side<0?-inset:inset,0,width,height));
                }
                else
                {
                    double lift=time<jumpUntil?Math.Sin((.65-(jumpUntil-time))/.65*Math.PI)*25:0;
                    if(companion)companionRenderer.Draw(g,new RectangleF(Pad,height-Pad-petHeight,width-2*Pad,petHeight),companionInput);
                    else g.DrawImage(sprite,BodyRectangle(sprite,width,height,sleep,lift));
                    if((time<responseUntil||sleep) && !held)
                    {
                        Rectangle bubble=new Rectangle(2,4,width-4,36);
                        using(Brush bg=new SolidBrush(Color.FromArgb(242,243,249,255)))g.FillRectangle(bg,bubble);
                        using(Pen border=new Pen(Color.FromArgb(125,177,225)))g.DrawRectangle(border,bubble);
                        using(Font font=new Font(PetText.Font,9))using(Brush ink=new SolidBrush(Color.FromArgb(35,64,99)))
                        using(StringFormat sf=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})g.DrawString(sleep?PetText.Sleeping:words,font,ink,bubble,sf);
                    }
                }
            }
            Present(next);if(frame!=null)frame.Dispose();frame=next;
        }
        RectangleF BodyRectangle(Bitmap sprite,int width,int height,bool sleep,double lift)
        {
            float h=petHeight*(sleep?.74f:1),w=h*sprite.Width/sprite.Height;
            if(w>width-2*Pad){h*=(width-2*Pad)/w;w=width-2*Pad;}
            // Keep horizontal scale/center constant. Very slow subpixel vertical breathing only.
            double breath=motion==Motion.Drag||motion==Motion.Fall?0:Math.Sin(time*(sleep?1.1:1.6))*.35;
            h-=(float)breath;
            return new RectangleF((width-w)/2,(float)(height-Pad-h-lift),w,h);
        }
        void Assert(bool value,string message){if(!value)throw new Exception(message);assertions++;File.AppendAllText(Path.Combine(evidence,"checks.txt"),"PASS "+message+Environment.NewLine);}
        void SetupChecks()
        {
            if(String.IsNullOrEmpty(evidence))throw new ArgumentException("--check requires an output directory");
            Directory.CreateDirectory(evidence);File.WriteAllText(Path.Combine(evidence,"checks.txt"),PetText.AppName+" 0.3.7 / "+PetText.Language+" / "+Environment.OSVersion+Environment.NewLine);
            checks.Enqueue(delegate{bank.VerifyMasks(Assert);});
            foreach(string value in Skins)
            {
                string s=value;
                checks.Enqueue(delegate
                {
                    Home();SetSkin(s);responseUntil=0;Render();
                    double saved=time;Bitmap sprite=bank.Skin(s,"idle");
                    RectangleF first=BodyRectangle(sprite,FullWidth,FullHeight,false,0);
                    bool horizontal=true,feet=true;
                    for(int i=0;i<24;i++){time=saved+i*.25;RectangleF next=BodyRectangle(sprite,FullWidth,FullHeight,false,0);horizontal&=first.X==next.X && first.Width==next.Width;feet&=Math.Abs(first.Bottom-next.Bottom)<.001;}
                    time=saved;
                    Assert(horizontal,s+" idle horizontal position and width fixed across breathing cycle");
                    Assert(feet,s+" idle feet stay planted across breathing cycle");
                });
                checks.Enqueue(delegate{CaptureEvidence(s+"-idle");Assert(frame.GetPixel(0,0).A==0,s+" transparent corner");Say(PetText.PatReply);});
                checks.Enqueue(delegate{CaptureEvidence(s+"-happy");Sleep();Assert(motion==Motion.Sleep,s+" sleeps");});
                checks.Enqueue(delegate{CaptureEvidence(s+"-sleep");SetSkin(Skins[(Array.IndexOf(Skins,s)+1)%Skins.Length]);Assert(motion==Motion.Sleep,"skin change retains sleep");SetSkin(s);Say(PetText.Wake);Assert(motion==Motion.Idle,"sleep wake clears state");DockAt(-1,Work,Work.Top+220);});
                checks.Enqueue(delegate
                {
                    dockStarted=time-1;Render();CaptureEvidence(s+"-left");Assert(Left==dockArea.Left,"left contact anchored");stable=Location;
                    SetSkin(Skins[(Array.IndexOf(Skins,s)+1)%Skins.Length]);Assert(motion==Motion.Docked && Left==dockArea.Left,"skin change retains left edge");SetSkin(s);
                    foreach(int size in new int[]{160,360,240}){ResizePet(size);Assert(Left==dockArea.Left && Top>=dockArea.Top && Bottom<=dockArea.Bottom,"dock size "+size);}
                    Point point=new Point(Left+Width/2,Top+Height/2);BeginDrag(point);MoveDrag(new Point(point.X,point.Y+50));EndDrag(new Point(point.X,point.Y+50),30);
                    Assert(motion==Motion.Docked,"vertical drag remains docked");DockAt(1,dockArea,Top+Height/2);
                });
                checks.Enqueue(delegate{dockStarted=time-1;Render();CaptureEvidence(s+"-right");Assert(Right==dockArea.Right,"right contact anchored");Point p=new Point(Left+Width/2,Top+Height/2);BeginDrag(p);MoveDrag(new Point(p.X-70,p.Y));Assert(motion==Motion.Drag,"inward drag unfolds");EndDrag(new Point(p.X-70,p.Y),50);Assert(motion!=Motion.Docked,"release does not immediately redock");Sleep();});
                checks.Enqueue(delegate{paused=true;Say(PetText.Wake);Assert(!paused && motion==Motion.Idle,"explicit action clears pause and sleep");});
            }
            checks.Enqueue(delegate{Home();Top-=100;preciseY=Top;velocity=0;motion=Motion.Fall;});
            for(int i=0;i<5;i++)checks.Enqueue(delegate{});
            checks.Enqueue(delegate{Assert(motion==Motion.Idle && velocity==0 && Top==Floor,"fall settles at floor");stable=Location;});
            for(int i=0;i<4;i++)checks.Enqueue(delegate{});
            checks.Enqueue(delegate
            {
                Home();SetSkin("magic");SetCompanion(true);responseUntil=0;
                Assert(companion && skin=="classic" && companionInput.Enabled,"companion enables classic pose and background input");
                Render();CaptureEvidence("companion-idle");stable=Location;
                foreach(int key in new int[]{16,17,32}){companionInput.Reset();companionInput.SetKey(key,true);Render();CaptureEvidence("companion-key-"+key);}companionInput.Reset();
                companionInput.SetKey(0x50,true);Render();CaptureEvidence("companion-typing");companionInput.Reset();
                companionInput.SetKey(0x57,true);companionInput.SetKey(0x10,true);companionInput.SetMouse(180,-60,1);Render();
                Assert(companionInput.IsDown(0) && companionInput.IsDown(8) && companionInput.LeftButton,"W Shift mouse together");
                CaptureEvidence("companion-combo");Assert(Location==stable,"companion input keeps native window fixed");
                companionInput.SetKey(0x57,false);Assert(!companionInput.IsDown(0) && companionInput.IsDown(8),"releasing W preserves Shift");
                linked=false;UpdateInput();Assert(!companionInput.Enabled && !companionInput.IsDown(8) && !companionInput.LeftButton,"disabling link clears and unregisters input");
                linked=true;UpdateInput();SetCompanion(false);Assert(skin=="magic" && !companionInput.Enabled,"leaving companion restores prior outfit and unregisters input");
                SetCompanion(true);Sleep();Assert(!companion && motion==Motion.Sleep && !companionInput.Enabled,"sleep exits companion");
                SetCompanion(true);DockAt(1,Work,Top+Height/2);Assert(!companion && motion==Motion.Docked && !companionInput.Enabled,"dock exits companion");
                Home();SetCompanion(true);paused=true;UpdateInput();Assert(!companionInput.Enabled,"pause suspends input");paused=false;UpdateInput();
                foreach(int size in new int[]{160,360,240}){ResizePet(size);Assert(Top+Height-Pad==Work.Bottom,"companion floor anchor "+size);Render();CaptureEvidence("companion-size-"+size);}
                SetCompanion(false);Home();stable=Location;
            });
            checks.Enqueue(delegate
            {
                Assert(Location==stable,"landed native position stable");foreach(int size in new int[]{160,360,240}){ResizePet(size);Assert(Top+Height-Pad==Work.Bottom,"full-size ground anchor "+size);}
                Assert(menu.Items[menu.Items.Count-1].Text==PetText.Exit,"exit last menu item");
                File.AppendAllText(Path.Combine(evidence,"checks.txt"),"TOTAL "+assertions+"\nPhysical mouse, mixed DPI and long-run tests not covered.\n");Close();
            });
        }
        void CaptureEvidence(string name)
        {
            Point beforeCapture=Location;
            // Keep full-body evidence away from desktop toolbars at the lower-right corner.
            if(motion!=Motion.Docked){Rectangle area=Work;Location=new Point(area.Left+(area.Width-Width)/2,area.Top+100);}
            try
            {
            frame.Save(Path.Combine(evidence,name+"-memory.png"));Rectangle bounds=GetNativeBounds();
            using(Bitmap screen=new Bitmap(bounds.Width,bounds.Height)){using(Graphics g=Graphics.FromImage(screen))g.CopyFromScreen(bounds.Location,Point.Empty,bounds.Size,CopyPixelOperation.SourceCopy);screen.Save(Path.Combine(evidence,name+"-desktop.png"));}
            // Native compositor evidence over controlled backgrounds, so enclosed white gaps are visible.
            using(Form background=new EvidenceBackground())
            {
                background.Bounds=bounds;background.Show();
                foreach(bool dark in new bool[]{true,false})
                {
                    background.BackColor=dark?Color.FromArgb(28,34,46):Color.FromArgb(223,233,242);
                    background.Refresh();BringToFront();Render();Thread.Sleep(70);
                    using(Bitmap screen=new Bitmap(bounds.Width,bounds.Height))
                    {
                        using(Graphics g=Graphics.FromImage(screen))g.CopyFromScreen(bounds.Location,Point.Empty,bounds.Size,CopyPixelOperation.SourceCopy);
                        screen.Save(Path.Combine(evidence,name+(dark?"-native-dark.png":"-native-light.png")));
                    }
                }
            }
            }
            finally{Location=beforeCapture;}
        }
        sealed class EvidenceBackground:Form
        {
            public EvidenceBackground(){FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=false;TopMost=true;StartPosition=FormStartPosition.Manual;AutoScaleMode=AutoScaleMode.None;}
            protected override bool ShowWithoutActivation {get{return true;}}
        }
        protected override void Dispose(bool disposing)
        {if(disposing){timer.Stop();timer.Dispose();companionInput.Dispose();if(companionRenderer!=null)companionRenderer.Dispose();tray.Visible=false;tray.Dispose();menu.Dispose();if(frame!=null)frame.Dispose();bank.Dispose();if(petIcon!=null)petIcon.Dispose();if(trayIcon!=null)trayIcon.Dispose();}base.Dispose(disposing);}
    }
}

