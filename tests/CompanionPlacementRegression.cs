using System;
using System.Reflection;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
internal static class CompanionPlacementRegression
{
    [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
    static Form pet;static Type type;static int count,index;static bool waiting;static Point expected;static Stopwatch elapsed=new Stopwatch();
    static object Call(string name,params object[] args){return type.GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(pet,args);}
    static object Field(string name){return type.GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(pet);}
    static void Assert(bool value,string name){if(!value)throw new Exception(name);count++;Console.WriteLine("PASS "+name);}
    [STAThread] static void Main(string[] args)
    {
        SetProcessDPIAware();Application.EnableVisualStyles();Assembly assembly=Assembly.LoadFrom(System.IO.Path.GetFullPath(args[0]));type=assembly.GetType("CodexPet.Pet");
        using(pet=(Form)Activator.CreateInstance(type,new object[]{false,null}))using(Timer timer=new Timer{Interval=40})
        {
            pet.Shown+=delegate{timer.Start();};
            timer.Tick+=delegate
            {
                try
                {
                    if(waiting)
                    {
                        if(elapsed.ElapsedMilliseconds<650)return;
                        Assert(pet.Location==expected && Field("motion").ToString()=="Idle","free placement remains fixed after gravity interval "+index);
                        Call("Say","位置保持");Assert(pet.Location==expected,"click response preserves placement "+index);
                        index++;waiting=false;
                        if(index==9)
                        {
                            Call("SetCompanion",false);Assert(Field("motion").ToString()=="Fall","normal pet resumes gravity after leaving companion");
                            Console.WriteLine("TOTAL "+count+"; message-loop placement regression, not physical drag.");pet.Close();return;
                        }
                    }
                    int size=new int[]{160,240,360}[index/3],position=index%3;
                    Call("Home");Call("ResizePet",size);Call("SetCompanion",true);
                    Rectangle area=(Rectangle)type.GetProperty("Work",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(pet,null);
                    expected=new Point(position==0?area.Left+(area.Width-pet.Width)/2:position==1?area.Left:area.Right-pet.Width,area.Top+100);
                    Point start=new Point(pet.Left+pet.Width/2,pet.Top+100),end=new Point(start.X+expected.X-pet.Left,start.Y+expected.Y-pet.Top);
                    Call("BeginDrag",start);Call("MoveDrag",end);Call("EndDrag",end,100);
                    Assert(pet.Location==expected && Field("motion").ToString()=="Idle","release does not fall or dock "+size+" position "+position);
                    elapsed.Restart();waiting=true;
                }
                catch(Exception ex){Console.WriteLine(ex);Environment.ExitCode=1;pet.Close();}
            };
            Application.Run(pet);
        }
    }
}
