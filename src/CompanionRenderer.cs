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
        readonly bool dorm;
        readonly PointF armElbow,armWrist,keyWrist,keyContact,mouseWrist,mouseContact;
        readonly Font font=new Font("Segoe UI",7,FontStyle.Bold,GraphicsUnit.Pixel);
        internal static readonly string[] Labels={"W","A","S","D","Q","E","R","F","Shift","Ctrl","Space"};
        internal static readonly RectangleF[] Caps={
            new RectangleF(219,243,15,11),new RectangleF(202,256,15,11),new RectangleF(219,256,15,11),new RectangleF(236,256,15,11),
            new RectangleF(202,243,15,11),new RectangleF(236,243,15,11),new RectangleF(253,243,15,11),new RectangleF(253,256,15,11),
            new RectangleF(176,256,24,11),new RectangleF(176,269,24,11),new RectangleF(202,269,66,11)};
        internal CompanionRenderer():this(false) {}
        internal CompanionRenderer(bool dormOutfit)
        {
            dorm=dormOutfit;
            using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream(dorm?"Companion.DormBody":"Companion.Classic"))
            using(Bitmap atlas=new Bitmap(stream))
            {
                body=dorm?(Bitmap)atlas.Clone():atlas.Clone(new Rectangle(0,0,1055,1024),PixelFormat.Format32bppArgb);
            }
            using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream(dorm?"Companion.DormHands":"Companion.Hands"))
            using(Bitmap source=new Bitmap(stream))
            {
                forearm=source.Clone(dorm?new Rectangle(180,170,270,520):new Rectangle(170,110,340,550),PixelFormat.Format32bppArgb);
                keyboardHand=source.Clone(dorm?new Rectangle(650,340,420,470):new Rectangle(680,470,390,335),PixelFormat.Format32bppArgb);
                mouseHand=source.Clone(dorm?new Rectangle(1260,340,400,510):new Rectangle(1300,470,370,380),PixelFormat.Format32bppArgb);
            }
            armElbow=dorm?new PointF(135,85):new PointF(160,170);armWrist=dorm?new PointF(135,475):new PointF(160,520);
            keyWrist=dorm?new PointF(235,80):new PointF(220,10);keyContact=dorm?new PointF(135,420):new PointF(134,274);
            mouseWrist=dorm?new PointF(190,80):new PointF(180,10);mouseContact=dorm?new PointF(190,360):new PointF(180,250);
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
            FillRound(g,dorm?Color.FromArgb(218,163,104):Color.FromArgb(113,153,187),new RectangleF(64,237,272,52),9);
            FillRound(g,dorm?Color.FromArgb(255,237,211):Color.FromArgb(218,239,248),new RectangleF(64,232,272,51),9);
            using(Pen trim=new Pen(Color.FromArgb(250,254,255),1.5f))g.DrawLine(trim,75,235,325,235);
            Paw(g,dorm?Color.FromArgb(221,159,101):Color.FromArgb(150,187,210),86,263,1.1f);
            using(Pen stitch=new Pen(dorm?Color.FromArgb(224,191,151):Color.FromArgb(167,203,222),.7f)){stitch.DashPattern=new float[]{2,3};g.DrawLine(stitch,76,280,325,280);}
            FillRound(g,dorm?Color.FromArgb(203,156,111):Color.FromArgb(111,149,183),new RectangleF(171,240,146,44),4);
            FillRound(g,Color.FromArgb(247,247,231),new RectangleF(171,237,146,45),4);
            FillRound(g,dorm?Color.FromArgb(232,191,144):Color.FromArgb(167,201,218),new RectangleF(175,240,138,40),2);
            GraphicsState boardState=g.Save();g.TranslateTransform(244,260);g.RotateTransform(180);g.TranslateTransform(-244,-260);
            for(int i=0;i<Caps.Length;i++)
            {
                float amount=input.Amount(i);
                RectangleF cap=Caps[i];cap.Y+=amount*1.5f;
                FillRound(g,dorm?Color.FromArgb(210,166,124):Color.FromArgb(135,172,192),new RectangleF(cap.X,cap.Y+1,cap.Width,cap.Height),2);
                FillRound(g,amount>.12?(dorm?Color.FromArgb(255,175,161):Color.FromArgb(255,214,122)):i>=8?(dorm?Color.FromArgb(255,201,134):Color.FromArgb(160,210,237)):Color.FromArgb(253,253,245),cap,2);
                using(Brush ink=new SolidBrush(dorm?Color.FromArgb(112,78,49):Color.FromArgb(44,83,114)))
                using(StringFormat sf=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})g.DrawString(Labels[i],font,ink,cap,sf);
            }
            // Unlabelled keys provide anonymous typing feedback without displaying typed text.
            for(int x=273;x<312;x+=13)for(int row=0;row<3;row++)
            {
                float amount=x==273 && row==input.TypingLane?input.TypingPulse:0;
                RectangleF cap=new RectangleF(x,243+row*13+amount*1.5f,10,10);
                FillRound(g,dorm?Color.FromArgb(210,166,124):Color.FromArgb(135,172,192),new RectangleF(cap.X,cap.Y+1,10,10),2);
                FillRound(g,amount>.15f?Color.FromArgb(255,181,185):dorm?Color.FromArgb(255,244,224):Color.FromArgb(227,240,248),cap,2);
            }
            g.Restore(boardState);
            float click=Math.Max(Math.Max(input.LeftPulse,input.RightPulse),input.LeftButton||input.RightButton?.7f:0);
            float mx=input.MouseX,my=input.MouseY;
            using(Brush pad=new SolidBrush(dorm?Color.FromArgb(245,204,155):Color.FromArgb(160,201,228)))
            {
                g.FillPolygon(pad,new PointF[]{new PointF(113,249),new PointF(115,233),new PointF(130,241)});
                g.FillPolygon(pad,new PointF[]{new PointF(162,241),new PointF(178,233),new PointF(178,251)});
                g.FillEllipse(pad,111,239,70,41);
            }
            using(Pen rim=new Pen(input.RightButton?Color.FromArgb(243,163,191):input.LeftButton?Color.FromArgb(255,218,139):Color.FromArgb(241,250,255),1.3f))g.DrawEllipse(rim,113,241,66,37);
            DrawLimb(g,mouseHand,mouseWrist,mouseContact,new PointF(132,dorm?217:214),new PointF(152+mx,260+my+click*2));
            DrawLimb(g,keyboardHand,keyWrist,keyContact,new PointF(266,217),KeyboardContact(input));
            // Separate button lamps remain readable even when the hand covers the mouse.
            using(Brush lamp=new SolidBrush(input.LeftButton||input.LeftPulse>.2?Color.FromArgb(125,232,253):Color.FromArgb(67,93,126)))g.FillEllipse(lamp,109,277,5,3);
            using(Brush lamp=new SolidBrush(input.RightButton||input.RightPulse>.2?Color.FromArgb(245,165,236):Color.FromArgb(67,93,126)))g.FillEllipse(lamp,118,277,5,3);
            g.Restore(saved);
        }
        static void FillRound(Graphics g,Color color,RectangleF rectangle,float radius)
        {
            float d=radius*2;
            using(GraphicsPath path=new GraphicsPath())using(Brush brush=new SolidBrush(color))
            {
                path.AddArc(rectangle.X,rectangle.Y,d,d,180,90);path.AddArc(rectangle.Right-d,rectangle.Y,d,d,270,90);
                path.AddArc(rectangle.Right-d,rectangle.Bottom-d,d,d,0,90);path.AddArc(rectangle.X,rectangle.Bottom-d,d,d,90,90);
                path.CloseFigure();g.FillPath(brush,path);
            }
        }
        static void Paw(Graphics g,Color color,float x,float y,float scale)
        {
            using(Brush brush=new SolidBrush(color))
            {
                g.FillEllipse(brush,x-5*scale,y,10*scale,7*scale);
                g.FillEllipse(brush,x-8*scale,y-5*scale,4*scale,5*scale);g.FillEllipse(brush,x-3*scale,y-8*scale,4*scale,5*scale);
                g.FillEllipse(brush,x+2*scale,y-8*scale,4*scale,5*scale);g.FillEllipse(brush,x+7*scale,y-4*scale,4*scale,5*scale);
            }
        }
        void DrawLimb(Graphics g,Bitmap hand,PointF wrist,PointF contact,PointF elbow,PointF target)
        {
            // Hands share a constant scale. Reaching changes only position and rotation.
            float scale=dorm?.085f:.09f;
            double angle=Math.Atan2(target.Y-elbow.Y,target.X-elbow.X)-Math.Atan2(contact.Y-wrist.Y,contact.X-wrist.X);
            float a=(float)Math.Cos(angle)*scale,b=(float)Math.Sin(angle)*scale;
            float tx=target.X-a*contact.X+b*contact.Y,ty=target.Y-b*contact.X-a*contact.Y;
            PointF joint=new PointF(a*wrist.X-b*wrist.Y+tx,b*wrist.X+a*wrist.Y+ty);
            float dx=joint.X-elbow.X,dy=joint.Y-elbow.Y,len=(float)Math.Sqrt(dx*dx+dy*dy);
            if(len>.1f)
            {
                float sx=scale*dy/len,sy=-scale*dx/len,ex=dx/(armWrist.Y-armElbow.Y),ey=dy/(armWrist.Y-armElbow.Y);
                using(Matrix arm=new Matrix(sx,sy,ex,ey,elbow.X-sx*armElbow.X-ex*armElbow.Y,elbow.Y-sy*armElbow.X-ey*armElbow.Y))DrawTransformed(g,forearm,arm);
            }
            using(Matrix palm=new Matrix(a,b,-b,a,tx,ty))DrawTransformed(g,hand,palm);
        }
        static void DrawTransformed(Graphics g,Bitmap sprite,Matrix matrix)
        {PointF[] corners={new PointF(0,0),new PointF(sprite.Width,0),new PointF(0,sprite.Height)};matrix.TransformPoints(corners);g.DrawImage(sprite,corners);}
        public void Dispose(){body.Dispose();forearm.Dispose();keyboardHand.Dispose();mouseHand.Dispose();font.Dispose();}
    }
}
