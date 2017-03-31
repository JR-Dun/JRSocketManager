using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JRSocketManager
{
    class FFMPEGExtendLib
    {
        [DllImport("FFMPEGExtendLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void startCatchVideo(string courseId);

        [DllImport("FFMPEGExtendLibrary.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void stopCatchVideo(string courseId);

    }
}
