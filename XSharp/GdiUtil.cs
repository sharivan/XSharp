using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XSharp
{
    public static class GdiUtil
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct GdiplusStartupInput
        {
            // internalted to silent compiler
            internal uint GdiplusVersion;
            internal IntPtr DebugEventCallback;
            internal int SuppressBackgroundThread;
            internal int SuppressExternalCodecs;

            internal static GdiplusStartupInput MakeGdiplusStartupInput()
            {
                var result = new GdiplusStartupInput
                {
                    GdiplusVersion = 1,
                    DebugEventCallback = IntPtr.Zero,
                    SuppressBackgroundThread = 0,
                    SuppressExternalCodecs = 0
                };
                return result;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GdiplusStartupOutput
        {
            internal IntPtr NotificationHook;
            internal IntPtr NotificationUnhook;

            internal static GdiplusStartupOutput MakeGdiplusStartupOutput()
            {
                var result = new GdiplusStartupOutput();
                result.NotificationHook = result.NotificationUnhook = IntPtr.Zero;
                return result;
            }
        }

        public enum Status
        {
            Ok = 0,
            GenericError = 1,
            InvalidParameter = 2,
            OutOfMemory = 3,
            ObjectBusy = 4,
            InsufficientBuffer = 5,
            NotImplemented = 6,
            Win32Error = 7,
            WrongState = 8,
            Aborted = 9,
            FileNotFound = 10,
            ValueOverflow = 11,
            AccessDenied = 12,
            UnknownImageFormat = 13,
            FontFamilyNotFound = 14,
            FontStyleNotFound = 15,
            NotTrueTypeFont = 16,
            UnsupportedGdiplusVersion = 17,
            GdiplusNotInitialized = 18,
            PropertyNotFound = 19,
            PropertyNotSupported = 20
        };

        [DllImport("GdiPlus.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern Status GdiplusStartup(out IntPtr token, ref GdiplusStartupInput input, out GdiplusStartupOutput output);

        [DllImport("GdiPlus.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern Status GdiplusShutdown(IntPtr token);

        [DllImport("GdiPlus.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern Status GdipCreateBitmapFromGdiDib(IntPtr pBIH, IntPtr pPix, out IntPtr pBitmap);

        public static Bitmap BitmapFromDIB(IntPtr pDIB, IntPtr pPix)
        {
            MethodInfo mi = typeof(Bitmap).GetMethod("FromGDIplus", BindingFlags.Static | BindingFlags.NonPublic);

            if (mi == null)
                return null; // (permission problem) 

            IntPtr pBmp = IntPtr.Zero;
            Status status = GdipCreateBitmapFromGdiDib(pDIB, pPix, out pBmp);
            if (status == Status.Ok && pBmp != IntPtr.Zero) // success 
                return (Bitmap) mi.Invoke(null, new object[] { pBmp });

            return null; // failure 
        }
    }
}
