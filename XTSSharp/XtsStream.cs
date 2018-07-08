using System.IO;

namespace XTSSharp {
    public class XtsStream : RandomAccessSectorStream {
        public XtsStream(Stream baseStream, Xts xts)
            : this(baseStream, xts, 512) {
        }

        public XtsStream(Stream baseStream, Xts xts, int sectorSize)
            : base(new XtsSectorStream(baseStream, xts, sectorSize), true) {
        }

        public XtsStream(Stream baseStream, Xts xts, int sectorSize, long offset)
            : base(new XtsSectorStream(baseStream, xts, sectorSize, offset), true) {
        }
    }
}
