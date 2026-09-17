using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
internal static class UiProbe
{
    [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
    [STAThread] static void Main(string[] args)
    {
        SetProcessDPIAware();Application.EnableVisualStyles();
        Assembly package=Assembly.LoadFrom(Path.GetFullPath(args[0]));
        package.GetType("CodexPet.LayeredForm").GetField("Inspectable",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,true);
        using(Form pet=(Form)Activator.CreateInstance(package.GetType("CodexPet.Pet"),new object[]{false,null}))
        {pet.ShowInTaskbar=true;Application.Run(pet);}
    }
}
