﻿using System;
using System.Reflection;
using PacketDotNet;
using System.Drawing;

namespace CodeStrikeBot
{
    public static class Extensions
    {
        public static ushort PayloadChecksum(this PacketDotNet.Packet packet)
        {
            return CRC16.ComputeChecksum(packet.PayloadData);
        }

        public static ushort Checksum(this SuperBitmap bitmap)
        {
            //return Checksum(bitmap, 0, 0, bitmap.Bitmap.Width, bitmap.Bitmap.Height);

            return CRC16.ComputeChecksum((byte[])(new ImageConverter()).ConvertTo(bitmap.Bitmap, typeof(byte[])));
        }

        public static ushort Checksum(this Bitmap bitmap, int x, int y, int w, int h)
        {

            byte[] bytes;
            Bitmap bmp = new Bitmap(w, h);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                bool success = false;
                do
                {
                    try
                    {
                        g.DrawImage(bitmap, 0, 0, new Rectangle(x, y, w, h), GraphicsUnit.Pixel);
                        success = true;
                    }
                    catch (InvalidOperationException e)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                }
                while (!success);
            }

            //ret = icon.Checksum();
            bytes = (byte[])(new ImageConverter()).ConvertTo(bmp, typeof(byte[]));
            bmp.Dispose();
            /*bytes = new byte[w * h * 2];
            for (int r = 0; r < h; r++)
            {
                System.Buffer.BlockCopy(bitmap.Bits, (y + r) * bitmap.Bitmap.Width * 2 + x * 2, bytes, r * w * 2, w * 2);
            }*/
            return CRC16.ComputeChecksum(bytes);

            //return CRC16.ComputeChecksum((byte[])(new ImageConverter()).ConvertTo(bmp, typeof(byte[])));
        }

        public static ushort Checksum(this Bitmap bitmap)
        {
            byte[] bytes = (byte[])(new ImageConverter()).ConvertTo(bitmap, typeof(byte[]));

            return CRC16.ComputeChecksum(bytes);
        }

        public static byte[] ToByteArray(this Object obj)
        {
            byte[] ret;

            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (var ms = new System.IO.MemoryStream())
            {
                bf.Serialize(ms, obj);
                ret = ms.ToArray();
            }

            return ret;
        }
    }
}
