using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

// Loads the shipping EXE. Captures real desktop composition, not physical input.
internal static class EnglishLayoutRegression
{
    [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
    static Form pet; static Type type; static string output; static int count;
    static object Call(string name,params object[] args){return type.GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(pet,args);}
    static object Field(string name){return type.GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(pet);}
    static void Assert(bool condition,string message){if(!condition)throw new Exception(message);count++;Console.WriteLine("PASS "+message);}
    static void Capture(string name,Rectangle area)
    {
        using(Bitmap image=new Bitmap(area.Width,area.Height))
        {using(Graphics g=Graphics.FromImage(image))g.CopyFromScreen(area.Location,Point.Empty,area.Size);image.Save(Path.Combine(output,name+".png"),ImageFormat.Png);}
    }
    static void Inspect(ToolStripItemCollection items)
    {
        foreach(ToolStripItem item in items)
        {
            if(item is ToolStripSeparator || item.Name=="language")continue;
            Assert(!Regex.IsMatch(item.Text,@"\p{IsCJKUnifiedIdeographs}"),"English menu: "+item.Text);
            ToolStripMenuItem branch=item as ToolStripMenuItem;
            if(branch!=null && branch.DropDownItems.Count>0)Inspect(branch.DropDownItems);
        }
    }
    [STAThread] static void Main(string[] args)
    {
        SetProcessDPIAware();Application.EnableVisualStyles();output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
        Assembly assembly=Assembly.LoadFrom(Path.GetFullPath(args[0]));type=assembly.GetType("CodexPet.Pet");
        Type texts=assembly.GetType("CodexPet.PetText");texts.GetField("English").SetValue(null,true);
        using(pet=(Form)Activator.CreateInstance(type,new object[]{false,null}))
        using(Form backdrop=new Form{FormBorderStyle=FormBorderStyle.None,ShowInTaskbar=false,TopMost=true,BackColor=Color.FromArgb(28,34,46),StartPosition=FormStartPosition.Manual})
        using(Timer timer=new Timer{Interval=350})
        {
            int step=0;ContextMenuStrip menu=(ContextMenuStrip)Field("menu");
            Rectangle work=Screen.PrimaryScreen.WorkingArea;
            pet.Shown+=delegate{Call("ResizePet",160);pet.Location=new Point(work.Left+100,work.Top+100);backdrop.Bounds=new Rectangle(pet.Left-10,pet.Top-10,700,750);backdrop.Show();pet.BringToFront();timer.Start();};
            timer.Tick+=delegate
            {
                try
                {
                    if(step==0)
                    {
                        Assert((string)texts.GetProperty("Language").GetValue(null,null)=="en-US","English build selected");
                        Inspect(menu.Items);
                        using(Graphics g=Graphics.FromImage((Bitmap)Field("frame")))
                        using(Font font=new Font((string)texts.GetProperty("Font").GetValue(null,null),9))
                        using(StringFormat format=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})
                        {
                            foreach(string key in new[]{"Greeting","Recall","PatReply","HopReply","WakeReply","LinkError","TouchReply","Sleeping"})
                            {
                                string text=(string)texts.GetProperty(key).GetValue(null,null);int fitted,lines;
                                SizeF measured=g.MeasureString(text,font,new SizeF(pet.Width-4,36),format,out fitted,out lines);
                                Assert(fitted==text.Length && measured.Height<=36 && lines==1,"smallest speech bubble fits one line: "+key);
                            }
                        }
                        Call("Say",texts.GetProperty("TouchReply").GetValue(null,null));
                    }
                    else if(step==1){Capture("english-small-reply",pet.Bounds);Call("Sleep");}
                    else if(step==2){Capture("english-small-sleep",pet.Bounds);menu.Show(new Point(pet.Right+20,pet.Top));}
                    else if(step==3)
                    {
                        Capture("english-menu-sleep",menu.Bounds);
                        ToolStripMenuItem sleep=(ToolStripMenuItem)menu.Items[5];
                        Assert(sleep.Text=="Wake Up","sleep action updates to English wake action");
                        menu.Close();sleep.PerformClick();menu.Show(new Point(pet.Right+20,pet.Top));
                    }
                    else if(step==4)
                    {
                        Capture("english-menu-awake",menu.Bounds);
                        Assert(menu.Items[5].Text=="Take a Nap","waking restores English sleep action");
                        menu.Close();((ToolStripMenuItem)menu.Items[0]).PerformClick();
                        Assert((bool)Field("companion"),"English companion menu activates mode");
                    }
                    else if(step==5)
                    {
                        Capture("english-companion",pet.Bounds);
                        menu.Show(new Point(pet.Right+20,pet.Top));
                        ((ToolStripMenuItem)menu.Items[6]).ShowDropDown();
                    }
                    else if(step==6)
                    {
                        ToolStripDropDown submenu=((ToolStripMenuItem)menu.Items[6]).DropDown;
                        Capture("english-outfits",submenu.Bounds);menu.Close();
                        menu.Show(new Point(pet.Right+20,pet.Top));((ToolStripMenuItem)menu.Items["language"]).ShowDropDown();
                    }
                    else if(step==7)
                    {
                        Capture("language-picker",((ToolStripMenuItem)menu.Items["language"]).DropDown.Bounds);
                        menu.Close();Call("SetLanguage",false,false);menu.Show(new Point(pet.Right+20,pet.Top));
                    }
                    else if(step==8)
                    {
                        Capture("chinese-menu",menu.Bounds);
                        Assert(menu.Items[0].Text=="游戏搭子 · 经典制服","same instance switches companion label to Chinese");
                        Assert(menu.Items[menu.Items.Count-1].Text=="退出桌宠","same instance switches exit label to Chinese");
                        menu.Close();
                        Console.WriteLine("TOTAL "+count+"; native screenshots and method-driven menu actions, not physical input.");
                        pet.Close();
                    }
                    step++;
                }
                catch(Exception error){Console.WriteLine(error);Environment.ExitCode=1;pet.Close();}
            };
            Application.Run(pet);
        }
    }
}
