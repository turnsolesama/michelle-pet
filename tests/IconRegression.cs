using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
internal static class IconRegression
{
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern uint PrivateExtractIcons(string file,int index,int width,int height,IntPtr[] icons,uint[] ids,uint count,uint flags);
    [DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr icon);
    static int count;
    static void Assert(bool value,string message){if(!value)throw new Exception(message);count++;Console.WriteLine("PASS "+message);}
    static bool Same(Bitmap a,Bitmap b)
    {
        if(a.Size!=b.Size)return false;
        for(int y=0;y<a.Height;y++)for(int x=0;x<a.Width;x++)
        {
            Color p=a.GetPixel(x,y),q=b.GetPixel(x,y);
            if(Math.Abs(p.A-q.A)>2)return false;
            if(p.A>240 && (Math.Abs(p.R-q.R)>3 || Math.Abs(p.G-q.G)>3 || Math.Abs(p.B-q.B)>3))return false;
        }
        return true;
    }
    [STAThread] static void Main(string[] args)
    {
        try{Run(args);}catch(Exception error){Console.WriteLine(error.GetType().FullName+": "+error.Message);Environment.ExitCode=1;}
    }
    static void Run(string[] args)
    {
        string exe=Path.GetFullPath(args[0]),ico=Path.GetFullPath(args[1]),output=Path.GetFullPath(args[2]);Directory.CreateDirectory(output);
        foreach(int size in new[]{16,20,24,32,40,48,64,128,256})
        {
            IntPtr[] handles=new IntPtr[1],sourceHandles=new IntPtr[1];uint[] ids=new uint[1];
            Assert(PrivateExtractIcons(exe,0,size,size,handles,ids,1,0)==1 && handles[0]!=IntPtr.Zero,"Windows extracts EXE icon at "+size);
            if(PrivateExtractIcons(ico,0,size,size,sourceHandles,ids,1,0)!=1)throw new Exception("Cannot extract source icon");
            try
            {
                using(Icon actual=Icon.FromHandle(handles[0]))using(Bitmap bitmap=actual.ToBitmap())
                using(Icon source=Icon.FromHandle(sourceHandles[0]))using(Bitmap expected=source.ToBitmap())
                {bitmap.Save(Path.Combine(output,"shell-"+size+".png"),ImageFormat.Png);expected.Save(Path.Combine(output,"expected-"+size+".png"),ImageFormat.Png);Assert(Same(bitmap,expected),"EXE matches portrait icon at "+size);}
            }
            finally{DestroyIcon(handles[0]);DestroyIcon(sourceHandles[0]);}
        }
        Assembly assembly=Assembly.LoadFrom(exe);
        using(Form pet=(Form)Activator.CreateInstance(assembly.GetType("CodexPet.Pet"),new object[]{false,null}))
        using(Icon source=new Icon(ico,new Size(32,32)))using(Bitmap expected=source.ToBitmap())using(Bitmap actual=pet.Icon.ToBitmap())
        {
            Assert(Same(actual,expected),"window uses the same portrait");
            NotifyIcon tray=(NotifyIcon)pet.GetType().GetField("tray",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(pet);
            using(Icon small=new Icon(ico,SystemInformation.SmallIconSize))using(Bitmap smallBitmap=small.ToBitmap())using(Bitmap trayBitmap=tray.Icon.ToBitmap())
                Assert(Same(smallBitmap,trayBitmap),"tray selects the system small-icon size");
        }
        Console.WriteLine("TOTAL "+count+"; Windows shell extraction and runtime icon resources verified.");
    }
}
