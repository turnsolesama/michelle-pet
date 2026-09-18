using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
internal static class LanguageRegression
{
    [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
    static Type type,texts;static Form pet;static int count;
    static object Call(string method,params object[] args){return type.GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(pet,args);}
    static object Field(string name){return type.GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(pet);}
    static void Assert(bool value,string label){if(!value)throw new Exception(label);count++;Console.WriteLine("PASS "+label);}
    static void Toggle(bool english)
    {
        Point position=pet.Location;Size size=pet.Size;object motion=Field("motion"),skin=Field("skin"),companion=Field("companion"),paused=Field("paused"),linked=Field("linked");
        ContextMenuStrip menu=(ContextMenuStrip)Field("menu");
        ToolStripMenuItem language=(ToolStripMenuItem)menu.Items["language"];
        ((ToolStripMenuItem)language.DropDownItems[english?1:0]).PerformClick();
        Assert(pet.Location==position && pet.Size==size,"switch preserves native geometry");
        Assert(motion.Equals(Field("motion")) && skin.Equals(Field("skin")) && companion.Equals(Field("companion")) && paused.Equals(Field("paused")) && linked.Equals(Field("linked")),"switch preserves motion, outfit, companion, pause and link");
        Assert(pet.Text==(english?"Michele Desktop Pet":"米雪儿桌宠"),"window title changes");
        Assert(menu.Items[menu.Items.Count-1].Text==(english?"Exit":"退出桌宠"),"translated exit remains last");
        Assert(((ToolStripMenuItem)language.DropDownItems[english?1:0]).Checked && !((ToolStripMenuItem)language.DropDownItems[english?0:1]).Checked,"language selection is exclusive");
        Assert(((NotifyIcon)Field("tray")).Text.StartsWith(english?"Michele":"米雪儿"),"tray title changes");
    }
    [STAThread] static void Main(string[] args)
    {
        SetProcessDPIAware();Application.EnableVisualStyles();Assembly assembly=Assembly.LoadFrom(Path.GetFullPath(args[0]));
        type=assembly.GetType("CodexPet.Pet");texts=assembly.GetType("CodexPet.PetText");
        // Diagnostic flag prevents menu clicks from writing the user's language preference.
        using(pet=(Form)Activator.CreateInstance(type,new object[]{true,null}))
        {
            Call("Home");Call("Say","嘿嘿，今天也一起加油！");Toggle(true);
            Assert((string)Field("words")=="You got this!","active bubble translates without resetting state");Toggle(false);
            Call("SetSkin","heart");Call("Sleep");Toggle(true);Toggle(false);
            Call("DockAt",1,Screen.PrimaryScreen.WorkingArea,240);Toggle(true);Toggle(false);
            Call("Home");Call("SetCompanion",true);pet.Location=new Point(120,130);
            type.GetField("paused",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(pet,true);
            type.GetField("linked",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(pet,false);
            Call("UpdateInput");Toggle(true);Toggle(false);
        }
        string directory=Path.GetFullPath(args[1]);Directory.CreateDirectory(directory);string preference=Path.Combine(directory,"language.txt");
        texts.GetField("English").SetValue(null,true);
        Assert((bool)texts.GetMethod("SavePreference").Invoke(null,new object[]{preference}),"language preference saves to isolated test path");
        texts.GetField("English").SetValue(null,false);texts.GetMethod("LoadPreference").Invoke(null,new object[]{preference});
        Assert((bool)texts.GetField("English").GetValue(null),"next launch restores English preference");
        texts.GetField("English").SetValue(null,false);texts.GetMethod("SavePreference").Invoke(null,new object[]{preference});
        texts.GetField("English").SetValue(null,true);texts.GetMethod("LoadPreference").Invoke(null,new object[]{preference});
        Assert(!(bool)texts.GetField("English").GetValue(null),"next launch restores Chinese preference");
        File.WriteAllText(preference,"unknown");texts.GetMethod("LoadPreference").Invoke(null,new object[]{preference});
        Assert(!(bool)texts.GetField("English").GetValue(null),"invalid preference falls back to Chinese");
        Assert(!(bool)texts.GetMethod("SavePreference").Invoke(null,new object[]{directory}),"unwritable preference returns failure without crashing");
        Console.WriteLine("TOTAL "+count+"; method-driven switching and isolated persistence tests.");
    }
}
