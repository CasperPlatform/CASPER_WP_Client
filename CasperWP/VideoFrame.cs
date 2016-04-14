using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private int count;

        public VideoFrame(int number, int packets, int size)
        {
            imageNumber = number;
            numberOfPackets = packets;
            imageSize = size;

            image = new byte[size];
            count = 0;
        }
    
        public bool AddImagePart(byte[] imageData, int packetNumber)
        {
            try
            {
                Array.Copy(imageData, 6, image, 8000 * packetNumber, imageData.Length - 6);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            count++;            
            return count == numberOfPackets;
        }
    }
}
