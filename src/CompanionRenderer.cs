using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;

namespace CodexPet
{
    internal sealed class CompanionRenderer : IDisposable
    {
        readonly Bitmap body,forearm,keyboardHand,mouseHand;
        readonly Font font=new Font("Segoe UI",7,FontStyle.Bold,GraphicsUnit.Pixel);
        internal static readonly string[] Labels={"W","A","S","D","Q","E","R","F","Shift","Ctrl","Space"};
        internal static readonly RectangleF[] Caps={
            new RectangleF(219,243,15,11),new RectangleF(202,256,15,11),new RectangleF(219,256,15,11),new RectangleF(236,256,15,11),
            new RectangleF(202,243,15,11),new RectangleF(236,243,15,11),new RectangleF(253,243,15,11),new RectangleF(253,256,15,11),
            new RectangleF(176,256,24,11),new RectangleF(176,269,24,11),new RectangleF(202,269,66,11)};
        internal CompanionRenderer()
        {
            using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("Companion.Classic"))
            using(Bitmap atlas=new Bitmap(stream))
            {
                body=atlas.Clone(new Rectangle(0,0,1055,1024),PixelFormat.Format32bppArgb);
            }
            using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("Companion.Hands"))
            using(Bitmap source=new Bitmap(stream))
            {
                forearm=source.Clone(new Rectangle(170,110,340,550),PixelFormat.Format32bppArgb);
                keyboardHand=source.Clone(new Rectangle(680,470,390,335),PixelFormat.Format32bppArgb);
                mouseHand=source.Clone(new Rectangle(1300,470,370,380),PixelFormat.Format32bppArgb);
            }
        }
        internal static PointF KeyboardContact(CompanionInput input)
        {
            int key=input.TrackingKey();
            if(key==-2)return new PointF(210,272-input.TypingLane*13-input.TypingPulse*1.5f);
            if(key<0)return new PointF(252,263);
            RectangleF cap=Caps[key];
            // Use the same 180-degree transform as the visible keys, in both axes.
            return new PointF(488-cap.X-cap.Width/2,520-cap.Y-cap.Height/2-input.Amount(key)*1.5f);
        }
        internal void Draw(Graphics g,RectangleF bounds,CompanionInput input)
        {
            GraphicsState saved=g.Save();g.TranslateTransform(bounds.X,bounds.Y);g.ScaleTransform(bounds.Width/400,bounds.Height/300);
            g.DrawImage(body,new RectangleF(52,0,290,281.5f));
            using(Brush shadow=new SolidBrush(Color.FromArgb(40,15,24,40)))g.FillEllipse(shadow,69,281,264,13);
            using(Brush edge=new SolidBrush(Color.FromArgb(34,44,68)))g.FillRectangle(edge,64,238,272,51);
            using(Brush surface=new SolidBrush(Color.FromArgb(55,69,99)))g.FillRectangle(surface,64,233,272,50);
            using(Pen trim=new Pen(Color.FromArgb(109,172,223),2))g.DrawLine(trim,66,284,334,284);
            using(Brush board=new SolidBrush(Color.FromArgb(24,32,52)))g.FillRectangle(board,172,239,144,43);
            GraphicsState boardState=g.Save();g.TranslateTransform(244,260);g.RotateTransform(180);g.TranslateTransform(-244,-260);
            for(int i=0;i<Caps.Length;i++)
            {
                float amount=input.Amount(i);
                RectangleF cap=Caps[i];cap.Y+=amount*1.5f;
                using(Brush fill=new SolidBrush(amount>.12?Color.FromArgb(113,215,248):Color.FromArgb(78,99,138)))g.FillRectangle(fill,cap);
                using(Brush ink=new SolidBrush(amount>.12?Color.FromArgb(24,43,63):Color.FromArgb(227,239,255)))
                using(StringFormat sf=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})g.DrawString(Labels[i],font,ink,cap,sf);
            }
            // Unlabelled keys provide anonymous typing feedback without displaying typed text.
            for(int x=273;x<312;x+=13)for(int row=0;row<3;row++)
            {
                float amount=x==273 && row==input.TypingLane?input.TypingPulse:0;
                using(Brush spare=new SolidBrush(amount>.15f?Color.FromArgb(171,194,255):Color.FromArgb(65,83,118)))g.FillRectangle(spare,x,243+row*13+amount*1.5f,10,10);
            }
            g.Restore(boardState);
            float click=Math.Max(Math.Max(input.LeftPulse,input.RightPulse),input.LeftButton||input.RightButton?.7f:0);
            float mx=input.MouseX,my=input.MouseY;
            using(Brush pad=new SolidBrush(Color.FromArgb(29,40,62)))g.FillEllipse(pad,111,239,70,41);
            using(Pen rim=new Pen(input.RightButton?Color.FromArgb(240,169,243):input.LeftButton?Color.FromArgb(126,231,250):Color.FromArgb(78,103,144),2))g.DrawEllipse(rim,111,239,70,41);
            DrawLimb(g,mouseHand,new PointF(180,10),new PointF(180,250),new PointF(132,214),new PointF(152+mx,260+my+click*2));
            DrawLimb(g,keyboardHand,new PointF(220,10),new PointF(134,274),new PointF(266,217),KeyboardContact(input));
            // Separate button lamps remain readable even when the hand covers the mouse.
            using(Brush lamp=new SolidBrush(input.LeftButton||input.LeftPulse>.2?Color.FromArgb(125,232,253):Color.FromArgb(67,93,126)))g.FillEllipse(lamp,109,277,5,3);
            using(Brush lamp=new SolidBrush(input.RightButton||input.RightPulse>.2?Color.FromArgb(245,165,236):Color.FromArgb(67,93,126)))g.FillEllipse(lamp,118,277,5,3);
            g.Restore(saved);
        }
        void DrawLimb(Graphics g,Bitmap hand,PointF wrist,PointF contact,PointF elbow,PointF target)
        {
            // Hands share a constant scale. Reaching changes only position and rotation.
            const float scale=.09f;
            double angle=Math.Atan2(target.Y-elbow.Y,target.X-elbow.X)-Math.Atan2(contact.Y-wrist.Y,contact.X-wrist.X);
            float a=(float)Math.Cos(angle)*scale,b=(float)Math.Sin(angle)*scale;
            float tx=target.X-a*contact.X+b*contact.Y,ty=target.Y-b*contact.X-a*contact.Y;
            PointF joint=new PointF(a*wrist.X-b*wrist.Y+tx,b*wrist.X+a*wrist.Y+ty);
            float dx=joint.X-elbow.X,dy=joint.Y-elbow.Y,len=(float)Math.Sqrt(dx*dx+dy*dy);
            if(len>.1f)
            {
                float sx=scale*dy/len,sy=-scale*dx/len,ex=dx/350,ey=dy/350;
                using(Matrix arm=new Matrix(sx,sy,ex,ey,elbow.X-sx*160-ex*170,elbow.Y-sy*160-ey*170))DrawTransformed(g,forearm,arm);
            }
            using(Matrix palm=new Matrix(a,b,-b,a,tx,ty))DrawTransformed(g,hand,palm);
        }
        static void DrawTransformed(Graphics g,Bitmap sprite,Matrix matrix)
        {PointF[] corners={new PointF(0,0),new PointF(sprite.Width,0),new PointF(0,sprite.Height)};matrix.TransformPoints(corners);g.DrawImage(sprite,corners);}
        public void Dispose(){body.Dispose();forearm.Dispose();keyboardHand.Dispose();mouseHand.Dispose();font.Dispose();}
    }
}
