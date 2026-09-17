using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CodexPet
{
    // Only these game controls have state. No text conversion, history, files or hooks.
    internal sealed class CompanionInput : IDisposable
    {
        internal static readonly int[] Keys={0x57,0x41,0x53,0x44,0x51,0x45,0x52,0x46,0x10,0x11,0x20};
        readonly bool[] down=new bool[15];
        readonly float[] pulse=new float[11];
        readonly long[] priority=new long[11];
        long pressSerial;
        long typingPriority;
        internal float TypingPulse;
        internal int TypingLane;
        internal bool Enabled {get;private set;}
        internal bool LeftButton,RightButton;
        internal float MouseX,MouseY,LeftPulse,RightPulse;
        IntPtr buffer;
        const int Capacity=256;
        [StructLayout(LayoutKind.Sequential)] struct Device {public ushort Page,Usage;public uint Flags;public IntPtr Target;}
        [DllImport("user32.dll",SetLastError=true)] static extern bool RegisterRawInputDevices(Device[] devices,uint count,uint size);
        [DllImport("user32.dll")] static extern uint GetRawInputData(IntPtr raw,uint command,IntPtr data,ref uint size,uint header);
        [DllImport("user32.dll")] static extern short GetAsyncKeyState(int key);
        static uint HeaderSize {get{return (uint)(8+2*IntPtr.Size);}}
        internal void Enable(IntPtr window)
        {
            if(Enabled)return;
            Device[] devices={new Device{Page=1,Usage=6,Flags=0x100,Target=window},new Device{Page=1,Usage=2,Flags=0x100,Target=window}};
            if(!RegisterRawInputDevices(devices,2,(uint)Marshal.SizeOf(typeof(Device))))throw new Win32Exception(Marshal.GetLastWin32Error());
            Enabled=true;Reset();
            if(buffer==IntPtr.Zero)buffer=Marshal.AllocHGlobal(Capacity);
        }
        internal void Disable()
        {
            if(Enabled)
            {
                Device[] devices={new Device{Page=1,Usage=6,Flags=1},new Device{Page=1,Usage=2,Flags=1}};
                if(!RegisterRawInputDevices(devices,2,(uint)Marshal.SizeOf(typeof(Device))))throw new Win32Exception(Marshal.GetLastWin32Error());
                Enabled=false;
            }
            Reset();
        }
        internal void Read(IntPtr raw)
        {
            if(!Enabled)return;
            uint size=Capacity;
            uint read=GetRawInputData(raw,0x10000003,buffer,ref size,HeaderSize);
            if(read==UInt32.MaxValue || read<HeaderSize)return;
            Decode(buffer,(int)read,(int)HeaderSize);
        }
        // Separate decoder permits ABI regression tests without fabricating physical input.
        internal void Decode(IntPtr data,int count,int header)
        {
            if(count<header || header<16)return;
            int type=Marshal.ReadInt32(data),offset=header;
            if(type==1 && count>=offset+16)
            {
                int flags=(ushort)Marshal.ReadInt16(data,offset+2),key=(ushort)Marshal.ReadInt16(data,offset+6);
                if(key==0x10)key=Marshal.ReadInt16(data,offset)==0x36?0xA1:0xA0;
                if(key==0x11)key=(flags&2)!=0?0xA3:0xA2;
                SetKey(key,(flags&1)==0);
            }
            else if(type==0 && count>=offset+24)
            {
                int flags=(ushort)Marshal.ReadInt16(data,offset),buttons=(ushort)Marshal.ReadInt16(data,offset+4);
                // Absolute tablet coordinates are not deltas; only use their buttons.
                int x=(flags&1)==0?Marshal.ReadInt32(data,offset+12):0,y=(flags&1)==0?Marshal.ReadInt32(data,offset+16):0;
                SetMouse(x,y,buttons);
            }
        }
        internal void SetKey(int key,bool pressed)
        {
            int index=Array.IndexOf(Keys,key);
            if(key>=0xA0 && key<=0xA3)index=11+key-0xA0;
            if(index<0)
            {
                // Only an anonymous tap survives; never retain or translate the character.
                if(pressed && IsTypingKey(key)){TypingPulse=1;TypingLane=(TypingLane+1)%3;typingPriority=++pressSerial;}
                return;
            }
            if(pressed && !down[index])
            {
                int logical=index>=11?(index<=12?8:9):index;
                pulse[logical]=1;priority[logical]=++pressSerial;
            }
            down[index]=pressed;
        }
        internal bool IsDown(int index)
        {return down[index] || (index==8 && (down[11]||down[12])) || (index==9 && (down[13]||down[14]));}
        internal float Amount(int index){return Math.Max(IsDown(index)?.72f:0,pulse[index]);}
        static bool IsTypingKey(int key)
        {return (key>=0x30 && key<=0x5A) || (key>=0x60 && key<=0x6F) || (key>=0xBA && key<=0xC0) || (key>=0xDB && key<=0xDF) || key==8 || key==9 || key==13 || key==46 || key==0xE2 || key==0xE5 || key==0xE7;}
        internal int TrackingKey()
        {
            int selected=-1;long newest=0;
            for(int i=0;i<11;i++)if((IsDown(i)||pulse[i]>.15f) && priority[i]>newest){selected=i;newest=priority[i];}
            if(TypingPulse>.15f && typingPriority>newest)return -2;
            return selected;
        }
        internal void SetMouse(int x,int y,int buttons)
        {
            MouseX=Math.Max(-9,Math.Min(9,MouseX+x*.04f));MouseY=Math.Max(-5,Math.Min(5,MouseY+y*.04f));
            if((buttons&1)!=0){LeftButton=true;LeftPulse=1;}if((buttons&2)!=0)LeftButton=false;
            if((buttons&4)!=0){RightButton=true;RightPulse=1;}if((buttons&8)!=0)RightButton=false;
        }
        internal void Advance(double dt,bool reconcile)
        {
            float fade=(float)Math.Exp(-dt*12);
            for(int i=0;i<pulse.Length;i++)pulse[i]*=fade;
            TypingPulse*=fade;
            MouseX*=(float)Math.Exp(-dt*3);MouseY*=(float)Math.Exp(-dt*3);LeftPulse*=fade;RightPulse*=fade;
            if(!Enabled || !reconcile)return;
            // Recover releases lost during screen locking, device removal or a secure desktop.
            for(int i=0;i<down.Length;i++)if(down[i] && (GetAsyncKeyState(i<11?Keys[i]:0xA0+i-11)&0x8000)==0)down[i]=false;
            if((GetAsyncKeyState(1)&0x8000)==0)LeftButton=false;
            if((GetAsyncKeyState(2)&0x8000)==0)RightButton=false;
        }
        internal void Reset(){Array.Clear(down,0,down.Length);Array.Clear(pulse,0,pulse.Length);Array.Clear(priority,0,priority.Length);pressSerial=typingPriority=0;TypingPulse=0;TypingLane=0;MouseX=MouseY=LeftPulse=RightPulse=0;LeftButton=RightButton=false;}
        public void Dispose(){Disable();if(buffer!=IntPtr.Zero){Marshal.FreeHGlobal(buffer);buffer=IntPtr.Zero;}}
    }
}
