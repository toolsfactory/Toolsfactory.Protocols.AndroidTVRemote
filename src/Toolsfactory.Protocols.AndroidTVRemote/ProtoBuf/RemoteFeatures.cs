using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolsfactory.Protocols.AndroidTVRemote.ProtoBuf
{
    [Flags]
    public enum RemoteFeatures : Int32
    {
        Ping =     0b_0000_0000_0001,
        Key =      0b_0000_0000_0010,
        IME =      0b_0000_0000_0100, // Input Method Editor
        Unknown1 = 0b_0000_0000_1000,
        Unknown2 = 0b_0000_0001_0000,
        Power =    0b_0000_0010_0000,
        Volume =   0b_0000_0100_0000,
        Unknown3 = 0b_0000_1000_0000,
        Unknown4 = 0b_0001_0000_0000,
        AppLink =  0b_0010_0000_0000
    }
}
