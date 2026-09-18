using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CodexPet
{
    internal sealed class SpriteBank : IDisposable
    {
        private readonly Dictionary<string, Bitmap> sprites = new Dictionary<string, Bitmap>();
        private readonly LinkedList<string> recentlyUsed = new LinkedList<string>();
        private const int CacheLimit = 10;
        private sealed class MaskPoint { public int X,Y; public bool Clear; }
        private readonly Dictionary<string,List<MaskPoint>> masks = new Dictionary<string,List<MaskPoint>>();
        public SpriteBank()
        {
            using(Stream stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("AlphaSeeds"))
            using(StreamReader reader=new StreamReader(stream))
            {
                string line;
                while((line=reader.ReadLine())!=null)
                {
                    if(line.StartsWith("#") || String.IsNullOrWhiteSpace(line))continue;
                    string[] p=line.Split(',');List<MaskPoint> points;
                    if(!masks.TryGetValue(p[0],out points)){points=new List<MaskPoint>();masks.Add(p[0],points);}
                    points.Add(new MaskPoint{X=Int32.Parse(p[1]),Y=Int32.Parse(p[2]),Clear=p[3]=="clear"});
                }
            }
        }
        public Bitmap Get(string variant, string expression) { return Skin("classic", expression); }
        public Bitmap Skin(string outfit, string pose) { return Load(outfit + "." + pose, "Skin." + outfit + "." + (pose.StartsWith("wall-")?"wall":pose)); }
        public Bitmap Get(string variant, string outfit, string expression)
        {
            string key = variant + "." + outfit + "." + expression;
            string resource = outfit == "maid" ? "Sprite." + variant + "." + expression : "Wardrobe." + key;
            return Load(key, resource);
        }
        public Bitmap GetPose(string pose) { return Load("pose." + pose, "Pose." + pose); }
        public Bitmap GetGrip(string variant, string outfit) { return Load("grip." + variant + "." + outfit, "Grip." + variant + "." + outfit); }
        private Bitmap Load(string key, string resource)
        {
            Bitmap bitmap;
            if (sprites.TryGetValue(key, out bitmap))
            {
                recentlyUsed.Remove(key);
                recentlyUsed.AddLast(key);
                return bitmap;
            }
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                if (stream == null) throw new InvalidDataException(PetText.MissingAsset + key);
                using (Bitmap original = new Bitmap(stream))
                if(key.Contains(".wall-"))
                {
                    // Two separately drawn upright poses. Preserve the generated RGBA, including white clothes.
                    int half=original.Width/2;
                    bool left=key.EndsWith("-left");
                    double contactRatio=key.StartsWith("dessert.")?.024:key.StartsWith("heart.")?.038:key.StartsWith("magic.")?.030:.035;
                    int contact=(int)Math.Round(original.Width*contactRatio);
                    Rectangle crop=left?new Rectangle(contact,0,half-contact,original.Height):new Rectangle(half,0,original.Width-half-contact,original.Height);
                    using(Bitmap pose=original.Clone(crop,PixelFormat.Format32bppArgb))bitmap=Trim(pose);
                }
                else
                using (Bitmap prepared = Prepare(original,key))
                {
                    if(key.EndsWith(".grip"))
                    {
                        // Contact lines are measured in original canvas coordinates, not alpha bounds.
                        double line = key.StartsWith("heart.") ? .874 : key.StartsWith("magic.") ? .867 : .860;
                        using(Bitmap contact = prepared.Clone(new Rectangle(0,0,prepared.Width,(int)(prepared.Height*line)),PixelFormat.Format32bppArgb)) bitmap=Trim(contact);
                    }
                    else bitmap = Trim(prepared);
                }
            }
            // Cache only the most recent frames so 50 embedded outfits/expressions do not remain decoded in RAM.
            while (sprites.Count >= CacheLimit)
            {
                string expired = recentlyUsed.First.Value;
                recentlyUsed.RemoveFirst();
                sprites[expired].Dispose();
                sprites.Remove(expired);
            }
            sprites.Add(key, bitmap);
            recentlyUsed.AddLast(key);
            return bitmap;
        }

        private static Bitmap Trim(Bitmap source)
        {
            int left=source.Width, top=source.Height, right=-1, bottom=-1;
            for(int y=0;y<source.Height;y++) for(int x=0;x<source.Width;x++)
                if(source.GetPixel(x,y).A>24) { left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);bottom=Math.Max(bottom,y); }
            if(right<left) throw new InvalidDataException("Empty sprite");
            return source.Clone(Rectangle.FromLTRB(left,top,right+1,bottom+1),PixelFormat.Format32bppArgb);
        }
        // Remove only near-white background connected to the border, preserving white costume details.
        // This is an in-memory rendering mask; original artwork is never overwritten.
        private Bitmap Prepare(Bitmap original,string key)
        {
            int width = original.Width, height = original.Height, count = width * height;
            Bitmap result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(result)) g.DrawImageUnscaled(original, 0, 0);
            BitmapData data = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                byte[] pixels = new byte[count * 4];
                for (int y = 0; y < height; y++) Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), pixels, y * width * 4, width * 4);
                bool[] background = new bool[count];
                int[] queue = new int[count];
                int head = 0, tail = 0;
                Action<int> enqueue = delegate(int index)
                {
                    if (background[index]) return;
                    int p = index * 4;
                    if (pixels[p + 3] == 0 || (pixels[p] >= 242 && pixels[p + 1] >= 242 && pixels[p + 2] >= 242))
                    { background[index] = true; queue[tail++] = index; }
                };
                for (int x = 0; x < width; x++) { enqueue(x); enqueue((height - 1) * width + x); }
                for (int y = 1; y < height - 1; y++) { enqueue(y * width); enqueue(y * width + width - 1); }
                List<MaskPoint> points;
                if(masks.TryGetValue(key,out points))foreach(MaskPoint seed in points)
                    if(seed.Clear)enqueue(seed.Y*width+seed.X);
                while (head < tail)
                {
                    int index = queue[head++], x = index % width, y = index / width;
                    if (x > 0) enqueue(index - 1);
                    if (x + 1 < width) enqueue(index + 1);
                    if (y > 0) enqueue(index - width);
                    if (y + 1 < height) enqueue(index + width);
                }
                for (int index = 0; index < count; index++)
                    if (background[index]) { int p = index * 4; pixels[p] = pixels[p + 1] = pixels[p + 2] = pixels[p + 3] = 0; }
                for (int y = 0; y < height; y++) Marshal.Copy(pixels, y * width * 4, IntPtr.Add(data.Scan0, y * data.Stride), width * 4);
            }
            finally { result.UnlockBits(data); }
            return result;
        }
        internal void VerifyMasks(Action<bool,string> assert)
        {
            foreach(var pair in masks)
            using(Stream stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("Skin."+pair.Key))
            using(Bitmap source=new Bitmap(stream))
            using(Bitmap prepared=Prepare(source,pair.Key))
            {
                foreach(MaskPoint point in pair.Value)
                {
                    Color before=source.GetPixel(point.X,point.Y),after=prepared.GetPixel(point.X,point.Y);
                    assert(point.Clear?after.A==0:after.ToArgb()==before.ToArgb(),pair.Key+" "+(point.Clear?"hair gap transparent":"white costume preserved")+" "+point.X+","+point.Y);
                }
            }
        }
        public void Dispose() { foreach (Bitmap sprite in sprites.Values) sprite.Dispose(); sprites.Clear(); recentlyUsed.Clear(); }
    }
}
