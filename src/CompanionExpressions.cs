using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CodexPet
{
    // Generated expressions are composited only over facial features. Hair, body and hands stay fixed.
    internal sealed class CompanionExpressions : IDisposable
    {
        readonly Bitmap happy,shy,blink;
        readonly Rectangle crop;
        readonly int sourceWidth,sourceHeight;
        internal CompanionExpressions(bool dorm)
        {
            sourceWidth=dorm?1254:1055;sourceHeight=dorm?1254:1024;
            crop=dorm?new Rectangle(405,391,407,280):new Rectangle(326,326,371,239);
            RectangleF[] eyes=dorm?new[]{new RectangleF(420,446,172,166),new RectangleF(612,405,190,168)}:
                new[]{new RectangleF(336,362,163,158),new RectangleF(518,329,174,156)};
            RectangleF mouth=dorm?new RectangleF(548,570,159,93):new RectangleF(454,475,143,77);
            using(Bitmap source=Load(dorm,"Happy")){happy=MakePatch(source,eyes,mouth,false);blink=MakePatch(source,eyes,mouth,true);}
            using(Bitmap source=Load(dorm,"Shy"))shy=MakePatch(source,eyes,mouth,false);
        }
        static Bitmap Load(bool dorm,string expression)
        {
            using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("Expression."+(dorm?"Dorm":"Classic")+"."+expression))
            using(Bitmap source=new Bitmap(stream))return source.Clone(new Rectangle(0,0,source.Width,source.Height),PixelFormat.Format32bppArgb);
        }
        Bitmap MakePatch(Bitmap source,RectangleF[] eyes,RectangleF mouth,bool eyesOnly)
        {
            if(source.Height!=sourceHeight || source.Width<sourceWidth)throw new InvalidOperationException("Expression alignment does not match its body sprite.");
            using(Bitmap region=source.Clone(crop,PixelFormat.Format32bppArgb))
            {
                Bitmap result=new Bitmap(crop.Width,crop.Height,PixelFormat.Format32bppArgb);
                int[] colors=new int[crop.Width*crop.Height],pixels=new int[colors.Length];
                BitmapData data=region.LockBits(new Rectangle(0,0,region.Width,region.Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
                try{Marshal.Copy(data.Scan0,colors,0,colors.Length);}finally{region.UnlockBits(data);}
                foreach(RectangleF feature in eyes)Mask(colors,pixels,feature);
                if(!eyesOnly)Mask(colors,pixels,mouth);
                data=result.LockBits(new Rectangle(0,0,result.Width,result.Height),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);
                try{Marshal.Copy(pixels,0,data.Scan0,pixels.Length);}finally{result.UnlockBits(data);}
                return result;
            }
        }
        void Mask(int[] colors,int[] pixels,RectangleF feature)
        {
            float cx=feature.X+feature.Width/2,cy=feature.Y+feature.Height/2,rx=feature.Width/2,ry=feature.Height/2;
            int left=Math.Max(0,(int)feature.Left-crop.Left),right=Math.Min(crop.Width,(int)Math.Ceiling(feature.Right)-crop.Left);
            int top=Math.Max(0,(int)feature.Top-crop.Top),bottom=Math.Min(crop.Height,(int)Math.Ceiling(feature.Bottom)-crop.Top);
            for(int y=top;y<bottom;y++)for(int x=left;x<right;x++)
            {
                double dx=(x+crop.X-cx)/rx,dy=(y+crop.Y-cy)/ry,d=Math.Sqrt(dx*dx+dy*dy);
                if(d>=1)continue;
                double coverage=Math.Min(1,(1-d)/.18);coverage=coverage*coverage*(3-2*coverage);
                int index=y*crop.Width+x,alpha=(int)(((uint)colors[index]>>24)*coverage);
                if(alpha>((uint)pixels[index]>>24))pixels[index]=(alpha<<24)|(colors[index]&0xFFFFFF);
            }
        }
        internal void Draw(Graphics graphics,RectangleF body,int expression,bool blinking)
        {
            Bitmap image=expression==1?happy:expression==2?shy:blinking?blink:null;
            if(image==null)return;
            graphics.DrawImage(image,new RectangleF(body.X+crop.X*body.Width/sourceWidth,body.Y+crop.Y*body.Height/sourceHeight,crop.Width*body.Width/sourceWidth,crop.Height*body.Height/sourceHeight));
        }
        public void Dispose(){happy.Dispose();shy.Dispose();blink.Dispose();}
    }
}
