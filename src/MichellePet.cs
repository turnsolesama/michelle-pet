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
[assembly: AssemblyTitle("米雪儿桌宠")]
[assembly: AssemblyVersion("0.2.2.0")]
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
                try { using(Pet pet=new Pet(check,args.Length>1?args[1]:null)) Application.Run(pet); }
                catch(Exception error)
                {
                    if(check) { if(args.Length>1){Directory.CreateDirectory(args[1]);File.WriteAllText(Path.Combine(args[1],"FAILED.txt"),error.ToString());} Environment.ExitCode=1; }
                    else MessageBox.Show(error.ToString(),"米雪儿桌宠启动失败");
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
        readonly bool diagnostic;
        readonly string evidence;
        Bitmap frame; Icon petIcon;
        Motion motion=Motion.Idle;
        string skin="classic", words="米雪儿，报到！";
        int petHeight=240, side, suppressedSide;
        const int Pad=16, BubbleHeight=50;
        bool held, moved, paused, dockDrag, suppressDock;
        Point press, origin, stable;
        Rectangle dockArea, suppressedArea;
        double velocity, preciseY, time, responseUntil=3, jumpUntil, lastTick, nextCheck, dockStarted;
        int assertions;
        static readonly string[] Skins={"classic","dessert","heart","magic"};
        static readonly string[] Names={"经典制服","甜点美梦","桃心卫士","绮星梦使"};
        protected override bool ShowWithoutActivation {get{return true;}}
        [DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr handle);
        public Pet(bool check,string output)
        {
            diagnostic=check;evidence=output;Text="米雪儿桌宠";
            FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=false;TopMost=true;
            StartPosition=FormStartPosition.Manual;AutoScaleMode=AutoScaleMode.None;
            using(Bitmap thumb=new Bitmap(32,32))
            {
                using(Graphics g=Graphics.FromImage(thumb))g.DrawImage(bank.Skin("classic","idle"),new Rectangle(0,0,32,32),new Rectangle(300,0,500,500),GraphicsUnit.Pixel);
                IntPtr h=thumb.GetHicon();petIcon=(Icon)Icon.FromHandle(h).Clone();DestroyIcon(h);
            }
            Icon=petIcon;tray.Icon=petIcon;tray.Text="米雪儿桌宠 · 双击唤回";tray.Visible=!check;
            tray.ContextMenuStrip=menu;tray.DoubleClick+=delegate{Home();Say("我在这里喵！");};
            menu.Items.Add("摸摸头",null,delegate{Say("嘿嘿，今天也一起加油！");});
            menu.Items.Add("轻轻跳一下",null,delegate{Jump();});
            ToolStripMenuItem sleep=new ToolStripMenuItem("睡一会儿");sleep.Click+=delegate{if(motion==Motion.Sleep)Say("睡醒啦，一起出发！");else Sleep();};menu.Items.Add(sleep);
            ToolStripMenuItem wardrobe=new ToolStripMenuItem("换皮肤");
            for(int i=0;i<Skins.Length;i++){string selected=Skins[i];ToolStripMenuItem item=new ToolStripMenuItem(Names[i]);item.Tag=selected;item.Click+=delegate{SetSkin(selected);};wardrobe.DropDownItems.Add(item);}menu.Items.Add(wardrobe);
            ToolStripMenuItem sizes=new ToolStripMenuItem("大小");
            foreach(int value in new int[]{160,240,360}){int selected=value;ToolStripMenuItem item=new ToolStripMenuItem(value==160?"小 · 160":value==240?"中 · 240":"大 · 360");item.Tag=value;item.Click+=delegate{ResizePet(selected);};sizes.DropDownItems.Add(item);}menu.Items.Add(sizes);
            ToolStripMenuItem edges=new ToolStripMenuItem("收纳到边缘");
            edges.DropDownItems.Add("左侧贴墙探头",null,delegate{DockAt(-1,Work,Top+Height/2);});
            edges.DropDownItems.Add("右侧贴墙探头",null,delegate{DockAt(1,Work,Top+Height/2);});menu.Items.Add(edges);
            ToolStripMenuItem expand=new ToolStripMenuItem("展开全身");expand.Click+=delegate{Expand();};menu.Items.Add(expand);
            ToolStripMenuItem pause=new ToolStripMenuItem("暂停动画"){CheckOnClick=true};pause.Click+=delegate{paused=pause.Checked;};menu.Items.Add(pause);
            ToolStripMenuItem top=new ToolStripMenuItem("保持置顶"){Checked=true,CheckOnClick=true};top.Click+=delegate{TopMost=top.Checked;};menu.Items.Add(top);
            menu.Items.Add("回到右下角",null,delegate{Home();});menu.Items.Add(new ToolStripSeparator());menu.Items.Add("退出桌宠",null,delegate{Close();});
            menu.Opening+=delegate
            {
                sleep.Text=motion==Motion.Sleep?"醒来啦":"睡一会儿";pause.Checked=paused;expand.Visible=motion==Motion.Docked;
                foreach(ToolStripMenuItem i in wardrobe.DropDownItems)i.Checked=(string)i.Tag==skin;
                foreach(ToolStripMenuItem i in sizes.DropDownItems)i.Checked=(int)i.Tag==petHeight;
            };
            ContextMenuStrip=menu;timer.Interval=33;timer.Tick+=Tick;
            Shown+=delegate{Home();if(diagnostic)SetupChecks();lastTick=elapsed.Elapsed.TotalSeconds;timer.Start();};
        }
        int FullWidth {get{return (int)Math.Round(petHeight*.86)+2*Pad;}}
        int FullHeight {get{return petHeight+2*Pad+BubbleHeight;}}
        Rectangle Work {get{return motion==Motion.Docked?dockArea:Screen.FromPoint(new Point(Left+Width/2,Top+Height/2)).WorkingArea;}}
        int Floor {get{return Work.Bottom-FullHeight+Pad;}}
        void ClearMovement(){held=moved=dockDrag=false;Capture=false;velocity=0;jumpUntil=responseUntil=0;}
        void Home()
        {
            ClearMovement();motion=Motion.Idle;paused=false;suppressDock=false;
            Rectangle area=Screen.FromPoint(Cursor.Position).WorkingArea;
            Location=new Point(area.Right-FullWidth-40,area.Bottom-FullHeight+Pad);preciseY=Top;Render();
        }
        void SetSkin(string value)
        {
            if(Array.IndexOf(Skins,value)<0)throw new ArgumentException("Unknown skin");
            int center=Top+Height/2;skin=value;Render();if(motion==Motion.Docked)PlaceDock(center);
        }
        void ResizePet(int size)
        {
            Rectangle area=Work;int bottom=Top+Height-Pad,centerX=Left+Width/2,centerY=Top+Height/2;
            petHeight=size;Render();
            if(motion==Motion.Docked){PlaceDock(centerY);return;}
            Location=new Point(Clamp(centerX-Width/2,area.Left,area.Right-Width),Clamp(bottom-Height+Pad,area.Top,area.Bottom-Height+Pad));
            preciseY=Top;velocity=0;if(motion!=Motion.Sleep)motion=Top<Floor?Motion.Fall:Motion.Idle;
        }
        static int Clamp(int n,int low,int high){return Math.Max(low,Math.Min(Math.Max(low,high),n));}
        void Ground()
        {
            if(motion==Motion.Docked)Expand();
            ClearMovement();motion=Motion.Idle;Top=Floor;preciseY=Top;paused=false;
        }
        void Say(string line){Ground();words=line;responseUntil=time+2.8;Render();}
        void Jump(){Say("喵，出发！");jumpUntil=time+.65;}
        void Sleep(){Ground();motion=Motion.Sleep;Render();}
        void DockAt(int which,Rectangle area,int center)
        {
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
            else if(localY>BubbleHeight+petHeight*.8)Jump();else Say("嘿嘿，摸摸头就有精神啦！");
        }
        protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button==MouseButtons.Left){BeginDrag(Cursor.Position);Capture=true;}}
        protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);MoveDrag(Cursor.Position);}
        protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);if(e.Button==MouseButtons.Left)EndDrag(Cursor.Position,e.Y);}
        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);
            if(!Capture && held){bool dock=motion==Motion.Docked;held=moved=dockDrag=false;if(!dock)ReleasePet(Cursor.Position);}
        }
        void ReleasePet(Point p)
        {
            Rectangle area=Screen.FromPoint(p).WorkingArea;
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
                    if(motion==Motion.Fall && !held)
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
                    g.DrawImage(sprite,BodyRectangle(sprite,width,height,sleep,lift));
                    if((time<responseUntil||sleep) && !held)
                    {
                        Rectangle bubble=new Rectangle(2,4,width-4,36);
                        using(Brush bg=new SolidBrush(Color.FromArgb(242,243,249,255)))g.FillRectangle(bg,bubble);
                        using(Pen border=new Pen(Color.FromArgb(125,177,225)))g.DrawRectangle(border,bubble);
                        using(Font font=new Font("Microsoft YaHei UI",9))using(Brush ink=new SolidBrush(Color.FromArgb(35,64,99)))
                        using(StringFormat sf=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})g.DrawString(sleep?"Zzz… 休息一会儿":words,font,ink,bubble,sf);
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
            Directory.CreateDirectory(evidence);File.WriteAllText(Path.Combine(evidence,"checks.txt"),"MichellePet 0.2.2 / "+Environment.OSVersion+Environment.NewLine);
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
                checks.Enqueue(delegate{CaptureEvidence(s+"-idle");Assert(frame.GetPixel(0,0).A==0,s+" transparent corner");Say("今天也一起加油！");});
                checks.Enqueue(delegate{CaptureEvidence(s+"-happy");Sleep();Assert(motion==Motion.Sleep,s+" sleeps");});
                checks.Enqueue(delegate{CaptureEvidence(s+"-sleep");SetSkin(Skins[(Array.IndexOf(Skins,s)+1)%Skins.Length]);Assert(motion==Motion.Sleep,"skin change retains sleep");SetSkin(s);Say("醒来啦");Assert(motion==Motion.Idle,"sleep wake clears state");DockAt(-1,Work,Work.Top+220);});
                checks.Enqueue(delegate
                {
                    dockStarted=time-1;Render();CaptureEvidence(s+"-left");Assert(Left==dockArea.Left,"left contact anchored");stable=Location;
                    SetSkin(Skins[(Array.IndexOf(Skins,s)+1)%Skins.Length]);Assert(motion==Motion.Docked && Left==dockArea.Left,"skin change retains left edge");SetSkin(s);
                    foreach(int size in new int[]{160,360,240}){ResizePet(size);Assert(Left==dockArea.Left && Top>=dockArea.Top && Bottom<=dockArea.Bottom,"dock size "+size);}
                    Point point=new Point(Left+Width/2,Top+Height/2);BeginDrag(point);MoveDrag(new Point(point.X,point.Y+50));EndDrag(new Point(point.X,point.Y+50),30);
                    Assert(motion==Motion.Docked,"vertical drag remains docked");DockAt(1,dockArea,Top+Height/2);
                });
                checks.Enqueue(delegate{dockStarted=time-1;Render();CaptureEvidence(s+"-right");Assert(Right==dockArea.Right,"right contact anchored");Point p=new Point(Left+Width/2,Top+Height/2);BeginDrag(p);MoveDrag(new Point(p.X-70,p.Y));Assert(motion==Motion.Drag,"inward drag unfolds");EndDrag(new Point(p.X-70,p.Y),50);Assert(motion!=Motion.Docked,"release does not immediately redock");Sleep();});
                checks.Enqueue(delegate{paused=true;Say("醒来啦");Assert(!paused && motion==Motion.Idle,"explicit action clears pause and sleep");});
            }
            checks.Enqueue(delegate{Home();Top-=100;preciseY=Top;velocity=0;motion=Motion.Fall;});
            for(int i=0;i<5;i++)checks.Enqueue(delegate{});
            checks.Enqueue(delegate{Assert(motion==Motion.Idle && velocity==0 && Top==Floor,"fall settles at floor");stable=Location;});
            for(int i=0;i<4;i++)checks.Enqueue(delegate{});
            checks.Enqueue(delegate
            {
                Assert(Location==stable,"landed native position stable");foreach(int size in new int[]{160,360,240}){ResizePet(size);Assert(Top+Height-Pad==Work.Bottom,"full-size ground anchor "+size);}
                Assert(menu.Items[menu.Items.Count-1].Text=="退出桌宠","exit last menu item");
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
        {if(disposing){timer.Stop();timer.Dispose();tray.Visible=false;tray.Dispose();menu.Dispose();if(frame!=null)frame.Dispose();bank.Dispose();if(petIcon!=null)petIcon.Dispose();}base.Dispose(disposing);}
    }
}

