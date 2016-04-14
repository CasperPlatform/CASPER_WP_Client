using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CasperWP
{
    class VideoFrame
    {
        public int imageNumber;
        public int numberOfPackets;
        public int imageSize;

        public byte[] image;

        public VideoFrame(int number, int packets, int size)
        {
            imageNumber = number;
            numberOfPackets = packets;
            imageSize = size;

            image = new byte[size];
        }
    }
}
