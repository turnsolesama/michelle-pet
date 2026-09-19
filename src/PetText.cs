using System;
using System.IO;
namespace CodexPet
{
    internal static class PetText
    {
        public static bool English;
        public static string CompanionOutfit {get{return English?"Gaming Buddy Outfit":"搭子造型";}}
        public static string DormOutfit {get{return English?"Feline Energy - Dorm":"喵萌元气 · 宿舍";}}
        public static string DormCompanion {get{return English?"Gaming Buddy - Dorm":"游戏搭子 · 宿舍";}}
        public static string SettingsPath {get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"MichelePet","language.txt");}}
        public static void LoadPreference(string path)
        {
            English=false;
            try { if(File.Exists(path))English=File.ReadAllText(path).Trim()=="en-US"; }
            catch(IOException) {} catch(UnauthorizedAccessException) {} catch(System.Security.SecurityException) {}
        }
        public static bool SavePreference(string path)
        {
            try { Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,Language);return true; }
            catch(IOException) {return false;} catch(UnauthorizedAccessException) {return false;} catch(System.Security.SecurityException) {return false;}
        }
        public static string SaveError {get{return English?"Language changed for this session; unable to save preference.":"语言已切换，但未能保存偏好，下次启动可能恢复默认。";}}
        static class En
        {

        public const string AppName="Michele Desktop Pet", StartupError="Michele Desktop Pet could not start";
        public const string Greeting="Michele here!", Recall="Here, meow!", Tray="Michele Desktop Pet - double-click to bring back";
        public const string Classic="Classic Uniform", Dessert="Sweet Dreams", Heart="Heart Guardian", Magic="Starry Dreamweaver";
        public const string Companion="Gaming Buddy - Classic", Link="Keyboard / Mouse Input", Pat="Pat Her Head", PatReply="You got this!";
        public const string Hop="Hop", HopReply="Let's go, meow!", Nap="Take a Nap", Wake="Wake Up", WakeReply="Ready to go!";
        public const string Outfit="Outfit", Size="Size", Small="Small - 160", Medium="Medium - 240", Large="Large - 360";
        public const string Edge="Peek from Screen Edge", Left="Left Edge", Right="Right Edge", Expand="Show Full Character";
        public const string Pause="Pause Animation", Top="Always on Top", Home="Move to Bottom Right", Exit="Exit";
        public const string LinkError="Input link failed", LinkErrorTitle="Keyboard / mouse input unavailable";
        public const string TouchReply="Thanks, meow!", Sleeping="Zzz... Resting", MissingAsset="Missing asset: ";
        public const string Font="Segoe UI", Language="en-US";
        }
        static class Zh
        {

        public const string AppName="米雪儿桌宠", StartupError="米雪儿桌宠启动失败";
        public const string Greeting="米雪儿，报到！", Recall="我在这里喵！", Tray="米雪儿桌宠 · 双击唤回";
        public const string Classic="经典制服", Dessert="甜点美梦", Heart="桃心卫士", Magic="绮星梦使";
        public const string Companion="游戏搭子 · 经典制服", Link="键鼠联动", Pat="摸摸头", PatReply="嘿嘿，今天也一起加油！";
        public const string Hop="轻轻跳一下", HopReply="喵，出发！", Nap="睡一会儿", Wake="醒来啦", WakeReply="睡醒啦，一起出发！";
        public const string Outfit="换皮肤", Size="大小", Small="小 · 160", Medium="中 · 240", Large="大 · 360";
        public const string Edge="收纳到边缘", Left="左侧贴墙探头", Right="右侧贴墙探头", Expand="展开全身";
        public const string Pause="暂停动画", Top="保持置顶", Home="回到右下角", Exit="退出桌宠";
        public const string LinkError="键鼠联动未能启动，请重新开启", LinkErrorTitle="键鼠联动未启动";
        public const string TouchReply="嘿嘿，摸摸头就有精神啦！", Sleeping="Zzz… 休息一会儿", MissingAsset="缺少素材：";
        public const string Font="Microsoft YaHei UI", Language="zh-CN";
        }
        public static string AppName {get{return English?En.AppName:Zh.AppName;}}
        public static string StartupError {get{return English?En.StartupError:Zh.StartupError;}}
        public static string Greeting {get{return English?En.Greeting:Zh.Greeting;}}
        public static string Recall {get{return English?En.Recall:Zh.Recall;}}
        public static string Tray {get{return English?En.Tray:Zh.Tray;}}
        public static string Classic {get{return English?En.Classic:Zh.Classic;}}
        public static string Dessert {get{return English?En.Dessert:Zh.Dessert;}}
        public static string Heart {get{return English?En.Heart:Zh.Heart;}}
        public static string Magic {get{return English?En.Magic:Zh.Magic;}}
        public static string Companion {get{return English?En.Companion:Zh.Companion;}}
        public static string Link {get{return English?En.Link:Zh.Link;}}
        public static string Pat {get{return English?En.Pat:Zh.Pat;}}
        public static string PatReply {get{return English?En.PatReply:Zh.PatReply;}}
        public static string Hop {get{return English?En.Hop:Zh.Hop;}}
        public static string HopReply {get{return English?En.HopReply:Zh.HopReply;}}
        public static string Nap {get{return English?En.Nap:Zh.Nap;}}
        public static string Wake {get{return English?En.Wake:Zh.Wake;}}
        public static string WakeReply {get{return English?En.WakeReply:Zh.WakeReply;}}
        public static string Outfit {get{return English?En.Outfit:Zh.Outfit;}}
        public static string Size {get{return English?En.Size:Zh.Size;}}
        public static string Small {get{return English?En.Small:Zh.Small;}}
        public static string Medium {get{return English?En.Medium:Zh.Medium;}}
        public static string Large {get{return English?En.Large:Zh.Large;}}
        public static string Edge {get{return English?En.Edge:Zh.Edge;}}
        public static string Left {get{return English?En.Left:Zh.Left;}}
        public static string Right {get{return English?En.Right:Zh.Right;}}
        public static string Expand {get{return English?En.Expand:Zh.Expand;}}
        public static string Pause {get{return English?En.Pause:Zh.Pause;}}
        public static string Top {get{return English?En.Top:Zh.Top;}}
        public static string Home {get{return English?En.Home:Zh.Home;}}
        public static string Exit {get{return English?En.Exit:Zh.Exit;}}
        public static string LinkError {get{return English?En.LinkError:Zh.LinkError;}}
        public static string LinkErrorTitle {get{return English?En.LinkErrorTitle:Zh.LinkErrorTitle;}}
        public static string TouchReply {get{return English?En.TouchReply:Zh.TouchReply;}}
        public static string Sleeping {get{return English?En.Sleeping:Zh.Sleeping;}}
        public static string MissingAsset {get{return English?En.MissingAsset:Zh.MissingAsset;}}
        public static string Font {get{return English?En.Font:Zh.Font;}}
        public static string Language {get{return English?En.Language:Zh.Language;}}
        public static string TranslateTo(string text,bool english)
        {
            string[] from=English?new string[]{En.AppName,En.StartupError,En.Greeting,En.Recall,En.Tray,En.Classic,En.Dessert,En.Heart,En.Magic,En.Companion,En.Link,En.Pat,En.PatReply,En.Hop,En.HopReply,En.Nap,En.Wake,En.WakeReply,En.Outfit,En.Size,En.Small,En.Medium,En.Large,En.Edge,En.Left,En.Right,En.Expand,En.Pause,En.Top,En.Home,En.Exit,En.LinkError,En.LinkErrorTitle,En.TouchReply,En.Sleeping,En.MissingAsset,En.Font,En.Language}:new string[]{Zh.AppName,Zh.StartupError,Zh.Greeting,Zh.Recall,Zh.Tray,Zh.Classic,Zh.Dessert,Zh.Heart,Zh.Magic,Zh.Companion,Zh.Link,Zh.Pat,Zh.PatReply,Zh.Hop,Zh.HopReply,Zh.Nap,Zh.Wake,Zh.WakeReply,Zh.Outfit,Zh.Size,Zh.Small,Zh.Medium,Zh.Large,Zh.Edge,Zh.Left,Zh.Right,Zh.Expand,Zh.Pause,Zh.Top,Zh.Home,Zh.Exit,Zh.LinkError,Zh.LinkErrorTitle,Zh.TouchReply,Zh.Sleeping,Zh.MissingAsset,Zh.Font,Zh.Language};
            string[] to=english?new string[]{En.AppName,En.StartupError,En.Greeting,En.Recall,En.Tray,En.Classic,En.Dessert,En.Heart,En.Magic,En.Companion,En.Link,En.Pat,En.PatReply,En.Hop,En.HopReply,En.Nap,En.Wake,En.WakeReply,En.Outfit,En.Size,En.Small,En.Medium,En.Large,En.Edge,En.Left,En.Right,En.Expand,En.Pause,En.Top,En.Home,En.Exit,En.LinkError,En.LinkErrorTitle,En.TouchReply,En.Sleeping,En.MissingAsset,En.Font,En.Language}:new string[]{Zh.AppName,Zh.StartupError,Zh.Greeting,Zh.Recall,Zh.Tray,Zh.Classic,Zh.Dessert,Zh.Heart,Zh.Magic,Zh.Companion,Zh.Link,Zh.Pat,Zh.PatReply,Zh.Hop,Zh.HopReply,Zh.Nap,Zh.Wake,Zh.WakeReply,Zh.Outfit,Zh.Size,Zh.Small,Zh.Medium,Zh.Large,Zh.Edge,Zh.Left,Zh.Right,Zh.Expand,Zh.Pause,Zh.Top,Zh.Home,Zh.Exit,Zh.LinkError,Zh.LinkErrorTitle,Zh.TouchReply,Zh.Sleeping,Zh.MissingAsset,Zh.Font,Zh.Language};
            for(int i=0;i<from.Length;i++)if(from[i]==text)return to[i];
            return text;
        }
    }
}
