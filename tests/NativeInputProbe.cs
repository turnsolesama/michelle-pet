// Optional manual/UI-automation acceptance harness, never included in the release.
// Loads the exact packaged EXE. No generated keystrokes; send input to the blank test window.
using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
internal static class NativeInputProbe
{
    [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
    [STAThread] static void Main(string[] args)
    {
        SetProcessDPIAware();Application.EnableVisualStyles();
        Assembly assembly=Assembly.LoadFrom(System.IO.Path.GetFullPath(args[0]));Type type=assembly.GetType("CodexPet.Pet");
        assembly.GetType("CodexPet.LayeredForm").GetField("Inspectable",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);
        using(Form pet=(Form)Activator.CreateInstance(type,new object[]{false,null}))
        using(Form target=new Form{Text="Michelle input acceptance - blank target",Width=600,Height=250,StartPosition=FormStartPosition.Manual,Location=new Point(150,150)})
        using(Timer timer=new Timer{Interval=15})
        {
            Label label=new Label{Dock=DockStyle.Fill,Font=new Font("Segoe UI",12),Text="Focus here. Test W/A/S/D, Q/E/R/F, Shift/Ctrl/Space and both mouse buttons."};target.Controls.Add(label);
            object input=type.GetField("companionInput",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(pet);Type it=input.GetType();
            bool[] observed=new bool[15];bool initialized=false;Point original=Point.Empty;
            Button toggle=new Button{Text="Stop input and reset observations",Dock=DockStyle.Bottom,Height=36};target.Controls.Add(toggle);
            toggle.Click+=delegate{type.GetMethod("SetCompanion",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(pet,new object[]{false});Array.Clear(observed,0,observed.Length);};
            pet.ShowInTaskbar=true;pet.Show();type.GetMethod("SetCompanion",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(pet,new object[]{true});original=pet.Location;
            timer.Tick+=delegate
            {
                if(!initialized){initialized=true;target.Show();target.WindowState=FormWindowState.Normal;target.BringToFront();original=pet.Location;Array.Clear(observed,0,observed.Length);}
                for(int i=0;i<11;i++)if((float)it.GetMethod("Amount",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(input,new object[]{i})>.15)observed[i]=true;
                if((float)it.GetField("LeftPulse",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(input)>.15)observed[11]=true;
                if((float)it.GetField("RightPulse",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(input)>.15)observed[12]=true;
                if(Math.Abs((float)it.GetField("MouseX",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(input))>.1 || Math.Abs((float)it.GetField("MouseY",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(input))>.1)observed[13]=true;
                if((float)it.GetField("TypingPulse",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(input)>.15)observed[14]=true;
                string[] names={"W","A","S","D","Q","E","R","F","Shift","Ctrl","Space","Mouse L","Mouse R","Mouse move","Typing"};string result="";
                for(int i=0;i<names.Length;i++)result+=names[i]+"="+(observed[i]?"YES":"--")+"  "+(i==6||i==10?"\n":"");
                bool active=(bool)type.GetField("companion",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(pet);
                toggle.Enabled=active;
                string status="Mode: "+(active?"ON":"OFF")+" / registration: "+it.GetProperty("Enabled",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(input,null)+"\n"+result+"\nPet location stable: "+(!active || pet.Location==original);
                label.Text=status;
                if(args.Length>1)System.IO.File.WriteAllText(args[1],status);
            };
            timer.Start();target.FormClosed+=delegate{pet.Close();};Application.Run(target);
        }
    }
}
