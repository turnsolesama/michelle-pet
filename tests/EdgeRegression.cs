using System;
using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
internal static class EdgeRegression
{
    [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
    static Form pet; static Type type; static int failures;
    static object Call(string name,params object[] args){return type.GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(pet,args);}
    static object Field(string name){return type.GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(pet);}
    static Rectangle Area {get{return (Rectangle)type.GetProperty("Work",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(pet,null);}}
    static Point Grab {get{return new Point(pet.Left+pet.Width/2,pet.Top+100);}}
    static void MoveTo(int x){Point point=Grab;Call("BeginDrag",point);Point end=new Point(point.X+x-pet.Left,point.Y);Call("MoveDrag",end);Call("EndDrag",end,100);}
    static void Expect(bool value,string name){Console.WriteLine((value?"PASS ":"FAIL ")+name+" / state="+Field("motion"));if(!value)failures++;}
    static bool Docked(int side){return Field("motion").ToString()=="Docked" && (int)Field("side")==side;}
    [STAThread] static void Main(string[] args)
    {
        SetProcessDPIAware();Application.EnableVisualStyles();
        Assembly assembly=Assembly.LoadFrom(System.IO.Path.GetFullPath(args[0]));type=assembly.GetType("CodexPet.Pet");
        using(pet=(Form)Activator.CreateInstance(type,new object[]{false,null}))
        {
            pet.Shown+=delegate{pet.BeginInvoke(new Action(delegate
            {
                try
                {
                    foreach(int size in new int[]{160,240,360})
                    {
                        Call("Home");Call("ResizePet",size);Rectangle area=Area;
                        MoveTo(area.Right-pet.Width+20);Expect(Docked(1),size+" home directly to right");
                        Call("Expand");MoveTo(area.Right-pet.Width+20);Expect(Docked(1),size+" fresh drag back to right after expand");
                        Call("Home");MoveTo(area.Left-20);Expect(Docked(-1),size+" directly to left");
                        Call("Expand");MoveTo(area.Left-20);Expect(Docked(-1),size+" fresh drag back to left after expand");
                        Call("DockAt",-1,area,area.Top+300);Call("Expand");
                        Point start=Grab;Call("BeginDrag",start);
                        Call("MoveDrag",new Point(area.Left+area.Width/2,start.Y));
                        Point end=new Point(area.Right-2,start.Y);Call("MoveDrag",end);Call("EndDrag",end,100);
                        Expect(Docked(1),size+" left via middle to right");
                        foreach(int edge in new int[]{-1,1})
                        {
                            Call("DockAt",edge,area,area.Top+300);Point p=Grab;Call("BeginDrag",p);
                            Point inner=new Point(p.X-edge*60,p.Y);Call("MoveDrag",inner);
                            // No intermediate mouse-move through the middle: opposite edge must still work.
                            Point opposite=new Point(edge<0?area.Right-2:area.Left+2,p.Y);
                            Call("MoveDrag",opposite);Call("EndDrag",opposite,100);
                            Expect(Docked(-edge),size+" direct cross from "+edge+" to opposite edge");
                            Call("DockAt",edge,area,area.Top+300);p=Grab;Call("BeginDrag",p);
                            inner=new Point(p.X-edge*60,p.Y);Call("MoveDrag",inner);Call("EndDrag",inner,100);
                            Expect(Field("motion").ToString()!="Docked",size+" unfolding does not immediately redock "+edge);
                        }
                    }
                }
                catch(Exception ex){Console.WriteLine(ex);failures++;}
                finally{pet.Close();}
            }));};
            Application.Run(pet);
        }
        Environment.ExitCode=failures==0?0:1;
    }
}
