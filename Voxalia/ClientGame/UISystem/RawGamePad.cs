using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security;

namespace Voxalia.ClientGame.UISystem
{
    public enum XInputErrorCode
    {
        Success = 0,
        DeviceNotConnected = 1
    }

    public enum XInputUserIndex
    {
        First = 0,
        Second = 1,
        Third = 2,
        Fourth = 3,
        Any = 0xff
    }

    public struct XInputVibration
    {
        public ushort LeftMotorSpeed;
        public ushort RightMotorSpeed;

        public XInputVibration(ushort left, ushort right)
        {
            LeftMotorSpeed = left;
            RightMotorSpeed = right;
        }
    }

    public class XInput : IDisposable
    {
        public IntPtr dll;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string dllName);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr handle);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr handle, string funcname);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr handle, IntPtr funcname);
        
        public XInput()
        {
            // Try to load the newest XInput***.dll installed on the system
            // The delegates below will be loaded dynamically from that dll
            dll = LoadLibrary("XINPUT1_4");
            if (dll == IntPtr.Zero)
            {
                dll = LoadLibrary("XINPUT1_3");
            }
            if (dll == IntPtr.Zero)
            {
                dll = LoadLibrary("XINPUT1_2");
            }
            if (dll == IntPtr.Zero)
            {
                dll = LoadLibrary("XINPUT1_1");
            }
            if (dll == IntPtr.Zero)
            {
                dll = LoadLibrary("XINPUT9_1_0");
            }
            if (dll == IntPtr.Zero)
            {
                throw new NotSupportedException("XInput was not found on this platform");
            }
            SetState = (XInputSetState)Load("XInputSetState", typeof(XInputSetState));
        }

        public Delegate Load(string name, Type type)
        {
            IntPtr pfunc = GetProcAddress(dll, name);
            if (pfunc != IntPtr.Zero)
            {
                return Marshal.GetDelegateForFunctionPointer(pfunc, type);
            }
            return null;
        }
        
        public XInputSetState SetState;
        
        [SuppressUnmanagedCodeSecurity]
        public delegate XInputErrorCode XInputSetState(XInputUserIndex dwUserIndex, ref XInputVibration pVibration);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool manual)
        {
            if (manual)
            {
                if (dll != IntPtr.Zero)
                {
                    FreeLibrary(dll);
                    dll = IntPtr.Zero;
                }
            }
        }

        public bool SetVibration(int index, float left, float right)
        {
            left = Math.Max(Math.Min(left, 1f), 0f);
            right = Math.Max(Math.Min(right, 1f), 0f);

            XInputVibration vibration = new XInputVibration(
                (ushort)(left * UInt16.MaxValue),
                (ushort)(right * UInt16.MaxValue));

            return SetState((XInputUserIndex)index, ref vibration) == XInputErrorCode.Success;
        }
    }
}
