using Cronos.SDK.Entity;
using Cronos.SDK.Enum;
using Cronos.SDK.Helper;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cronos.SDK.Data
{
    /// <summary>
    /// Protocol base
    /// </summary>
    internal static class PB
    {
        internal static ILogger lg;
        internal static object k = new object();

        #region H
        /// <summary>
        /// Response when station register
        /// </summary>
        internal static string D_R => string.Format("{0} \"{1}\"", "ok", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        /// <summary>
        /// Response when recieve done
        /// </summary>
        internal static byte[] D_E => Encoding.ASCII.GetBytes("$");
        #endregion

        #region P
        internal static byte[] D(List<TagEntity> t, int id = -1)
        {
            lock (k)
            {
                var w = new List<byte>();
                var d = new List<byte>();

                w.AddRange(B2(t.Count));
                w.Add(0x0F);
                w.Add(0x1B);

                if (id == -1)
                {
                    t.ForEach(x =>
                    {
                        if (x != null) { d.AddRange(x.Data); }
                    });
                }
                else
                {
                    int i = 0, j = 0;
                    byte b = 0;
                    var g = new List<byte>();

                    g.AddRange(B2(id));
                    t.Sort((x, y) => x.Index.CompareTo(y.Index));

                    for (; j < 216; j++)
                    {
                        b <<= 1;
                        if (t.Count > 0 && t[0].Index == j)
                        {
                            b += 1;
                            d.AddRange(t[0].Data);
                            t.RemoveAt(0);
                        }
                        if (i++ == 7) { i = 0; g.Add(b); b = 0; }
                    }

                    w.Add((byte)g.Count);
                    w.AddRange(g);
                }

                var b0 = new List<byte> { };
                b0.AddRange(w);
                b0.AddRange(d);
                b0.AddRange(C3(b0));

                var h = Encoding.Default.GetBytes($"E{b0.Count + 6:X6}{Q(t, id)}");
                var r = new byte[h.Length + b0.Count];
                Buffer.BlockCopy(h, 0, r, 0, h.Length);
                Buffer.BlockCopy(b0.ToArray(), 0, r, h.Length, b0.Count);
                return r;
            }
        }

        internal static byte[] DX(List<TagEntityX> t)
        {
            lock (k)
            {
                var b = new List<byte>();
                var d = new List<byte>();

                b.AddRange(B2(t.Count));
                b.Add(0x0F);
                b.Add(0x1B);
                t.ForEach(x => d.AddRange(x.Data));

                b.AddRange(d);
                b.AddRange(C3(b));

                var h = Encoding.Default.GetBytes($"E{b.Count + 6:X6}FFFFFE");
                var r = new byte[h.Length + b.Count];
                Buffer.BlockCopy(h, 0, r, 0, h.Length);
                Buffer.BlockCopy(b.ToArray(), 0, r, h.Length, b.Count);
                return r;
            }
        }

        internal static byte[] B(BC b, int t, int f = 0)
        {
            lock (k)
            {
                try
                {
                    var h = new List<byte> { 0x00, 0x01, 0x0F, 0x33 };

                    byte f0 = 0x00, f1 = 0x00;
                    switch (b)
                    {
                        case BC.S: if (f == 0) { f0 = 0xC8; f1 = 0x03; } break;
                        case BC.B:
                            f0 = 0xD8; f1 =
                                0x12; break;
                        case BC.P:
                            switch (f)
                            {
                                case 0: f0 = 0x01; f1 = 0x12; t = 1; break;
                                case 1: f0 = 0x01; f1 = 0x22; t = 2; break;
                                case 2: f0 = 0x01; f1 = 0x32; t = 3; break;
                                case 3: f0 = 0x01; f1 = 0x42; t = 4; break;
                                case 4: f0 = 0x01; f1 = 0x52; t = 5; break;
                                case 5: f0 = 0x01; f1 = 0x62; t = 6; break;
                                case 6: f0 = 0x01; f1 = 0x72; t = 7; break;
                                case 7: f0 = 0x01; f1 = 0x82; t = 8; break;
                                default: f0 = 0x01; f1 = 0x11; t = 9; break;
                            }
                            break;
                    }

                    var tb = GB((uint)t);
                    var p = new byte[] {
                    (byte)b,tb[0], tb[1], 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,0xFF, 0xFF, 0xFF,
                    0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, f0, f1};
                    var c0 = C3(p);

                    h.AddRange(p);
                    h.AddRange(c0);
                    var c1 = C3(h);
                    h.AddRange(c1);

                    h.InsertRange(0, new byte[] { 0x45, 0x30, 0x30, 0x30, 0x30, 0x32, 0x37, 0x0046, 0x0046, 0x0046, 0x0046, 0x0046, 0x0044 });
                    return h.ToArray();
                }
                catch (Exception ex)
                {
                    lg.LogError(ex, "[SDK]PB.B Error");
                    return new byte[] { };
                }
            }
        }
        #endregion

        #region M
        internal static List<byte> I(int t, int l, SKBitmap i1, ESLType t0)
        {
            try
            {
                if (i1 == null) { lg.LogError("[SDK]IMG_IS_NUL"); return new List<byte>(); }
                var i0 = i1.Copy(SKColorType.Rgb888x);
                var s = EnumHelper.GetESLSize(t0);
                int b = i0.Height + t, r = i0.Width + l;
                int H = Math.Min(s.Item2, b) - t, W = Math.Min(s.Item1, r) - l;
                if (W % 8 > 0) W = ((W / 8) + 1) * 8;
                b = Math.Min(b, H); r = Math.Min(r, W);

                bool[] B9 = new bool[H * W];
                bool[] R9 = new bool[H * W];

                var Scan0 = i0.GetPixels();
                var offset = (i0.Width - W) * 4;
                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    for (int h = 0; h < H; ++h)
                    {
                        for (int w = 0; w < W; ++w)
                        {
                            if ((p[0] > 200) && (p[1] < 70) && (p[2] < 70))
                            {
                                R9[h * W + w] = true;
                            }
                            else
                            {
                                B9[h * W + w] = ((byte)((p[0] * 19595 + p[1] * 38469 + p[2] * 7472) >> 16)) <= 125;
                            }
                            p += 4;
                        }
                        p += offset;
                    }
                }


                byte[] L9 = l1(t, l, b - 1, r - 1);
                var D9 = new List<byte>();
                byte BF = 0xFE;
                byte RF = 0x03;
                byte[] BA0 = Array.Empty<byte>(), RA0 = Array.Empty<byte>();

                var c0 = false;
                var BA1 = C(B9);
                var RA1 = C(R9);

                if ((BA1.Item2 + RA1.Item2) < (H * W * 2))
                {
                    c0 = true;
                    BA0 = new byte[BA1.Item2];
                    RA0 = new byte[RA1.Item2];
                    Array.Copy(BA1.Item1, BA0, BA1.Item2);
                    Array.Copy(RA1.Item1, RA0, RA1.Item2);
                    BF = 0xFC;
                    RF = 0xFC;
                    goto R0;
                }

                BA0 = new byte[H * W / 8]; RA0 = new byte[H * W / 8];
                byte byteB = 0, byteR = 0;
                int j = 1;
                for (int i = 0; i < B9.Length; i++)
                {
                    byteB <<= 1; byteR <<= 1;
                    if (B9[i]) byteB += 1;
                    if (R9[i]) byteR += 1;
                    if (i % 8 == 0 && i != 0)
                    {
                        BA0[j] = byteB; RA0[j] = byteR;
                        byteB = 0; byteR = 0;
                        j++;
                    }
                }

            R0:
                D9 = m1(D9, BF, L9, BA0, c0);
                if (c0) { L9[0] |= 0x80; L9[4] |= 0x80; }
                D9 = m1(D9, RF, L9, RA0, c0);
                return D9;
            }
            catch (Exception ex)
            {
                lg.LogError(ex, "[SDK]IMG_PRO_ERR:");
                return new List<byte>();
            }
            // Get location
            byte[] l1(int t2, int l2, int h2, int w2) =>
                new byte[]
                {
                    B2(t2, 2)[0], B2(t2)[1],
                    B2(l2)[0], B2(l2)[1],
                    B2(h2)[0], B2(h2)[1],
                    B2(w2)[0], B2(w2)[1]
                };
            // Merge to datas
            List<byte> m1(List<byte> b1, byte f1, byte[] l1, byte[] d1, bool c0)
            {
                b1.Add(f1);
                b1.AddRange(l1);
                if (c0) b1.AddRange(B2(d1.Length, 4));
                b1.AddRange(d1);
                return b1;
            }
        }

        internal static List<byte> B2(int b, int x = 2)
        {
            var bt = BitConverter.GetBytes(b);
            var lst = new List<byte>();
            for (int i = 0; i < x; i++) { lst.Add((i < bt.Length) ? bt[i] : (byte)0x00); }
            lst.Reverse();
            return lst;
        }

        internal static List<byte> GB(uint b, int x = 2)
        {
            var bt = BitConverter.GetBytes(b);
            var lst = new List<byte>();
            for (int i = 0; i < x; i++) { lst.Add((i < bt.Length) ? bt[i] : (byte)0x00); }
            lst.Reverse();
            return lst;
        }

        internal static string IDX(this int i) => (((i % 8) << 5) + (i / 8)).ToString("X2");

        /// <summary>
        /// Compress
        /// </summary>
        /// <param name="input">Input bits</param>
        /// <returns>Compress bytes</returns>
        public static (byte[], int) C(bool[] input)
        {
            int pos = 0, i = 0, j = 0, length = input.Length;
            byte[] output = new byte[length];
            bool prev, next;
            for (; i < length; i++)
            {
                prev = input[i];//起始符
                for (j = 0; i + j < length;)
                {
                    next = input[i + j];//起始符
                    if (prev == next)
                    {
                        if (j >= 65535) break;
                        j++; continue;
                    }
                    break;
                }

                byte bit;
                byte count;
                byte temp;
                if (j < 7)//没有找到或者长度不够不压缩
                {
                    count = 0;
                    bit = 0;
                    for (j = 0; j < 7 && (i + j < length); j++)//7位组合
                    {
                        prev = input[i + j];//起始符
                        temp = (byte)(prev ? 1 : 0);
                        if (j == 0)
                            bit = temp;//第六位
                        else
                            count |= (byte)(temp << (byte)(6 - j));//其它位
                    }
                    output[pos++] = (byte)(0x80 + (bit << 6) + (count << 0));//生成一个字节数据
                    i += 6;
                }
                else if (j <= 31)//找到短压缩 
                {
                    temp = (byte)(prev ? 1 : 0);
                    bit = temp;//第六位，数值
                    count = (byte)j;//个数
                    output[pos++] = (byte)((bit << 6) + (count << 0));
                    i += j - 1;
                }
                else if (j <= 255)//找到长压缩，需要两个字节表示数据长度
                {
                    temp = (byte)(prev ? 1 : 0);
                    bit = temp;//第六位
                    count = 1;//常压缩的位置变为0
                    output[pos++] = (byte)((bit << 6) + (count << 0));
                    output[pos++] = (byte)(j & 0xff);
                    i += j - 1;
                }
                else//常压缩，需要两个字节表示数据长度
                {
                    temp = (byte)(prev ? 1 : 0);
                    bit = temp;//第六位
                    count = 0;//常压缩的位置变为0
                    output[pos++] = (byte)((bit << 6) + (count << 0));
                    output[pos++] = (byte)(j & 0xff);
                    output[pos++] = (byte)((j >> 0x08) & 0xff);
                    i += j - 1;
                }
            }

            return (output, pos);
        }

        /// <summary>
        /// Get qualitiy code
        /// </summary>
        /// <param name="lst">Tag entities</param>
        /// <param name="g">Group ID</param>
        /// <returns>Qualitity code</returns>
        static string Q(List<TagEntity> lst, int g)
        {
            if (g != -1) return "FFFFFC";
            if (lst.Count < 2) return "FFFFFF";

            float l = 0, m = 0, h = 0;
            var query = from x in lst
                        orderby new string(x.TagID.ToCharArray().Reverse().ToArray())
                        select x.TagID;
            var data = query.ToList();

            for (int i = 0; i < data.Count - 1; i++)
            {
                if (data[i][6..] == data[i + 1][6..]) h++;
                if (data[i][4..] == data[i + 1][4..]) m++;
                if (data[i][2..] == data[i + 1][2..]) l++;
            }

            // TODO: 10% not sure.
            float a = l / lst.Count, b = m / lst.Count, c = h / lst.Count;
            //if (a < 10F) return "FFFFFD";
            if (b < 10F) return "FFFFFE";
            return "FFFFFF";
        }
        #endregion

        #region CVT
        /// <summary>
        /// HB
        /// </summary>
        /// <param name="h">Hex string</param>
        /// <returns>Bytes array</returns>
        internal static byte[] HB(string h)
        {
            byte[] b = new byte[h.Length / 2];
            for (int i = 0; i < h.Length; i += 2)
                b[i / 2] = Convert.ToByte(h.Substring(i, 2), 16);
            return b;
        }

        /// <summary>
        /// Bytes array to hex string
        /// </summary>
        /// <param name="bytes">Bytes array</param>
        /// <returns>Hex string</returns>
        internal static string BH(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var b in bytes) builder.AppendFormat(b.ToString("X2"));
            return builder.ToString();
        }
        #endregion

        #region C
        /// <summary>
        /// C Table
        /// </summary>
        static uint[] CT = new uint[]
        {
            0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419,
            0x706af48f, 0xe963a535, 0x9e6495a3, 0x0edb8832, 0x79dcb8a4,
            0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07,
            0x90bf1d91, 0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de,
            0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7, 0x136c9856,
            0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9,
            0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4,
            0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
            0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3,
            0x45df5c75, 0xdcd60dcf, 0xabd13d59, 0x26d930ac, 0x51de003a,
            0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599,
            0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
            0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190,
            0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f,
            0x9fbfe4a5, 0xe8b8d433, 0x7807c9a2, 0x0f00f934, 0x9609a88e,
            0xe10e9818, 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
            0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed,
            0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950,
            0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3,
            0xfbd44c65, 0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2,
            0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a,
            0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5,
            0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa, 0xbe0b1010,
            0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
            0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17,
            0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6,
            0x03b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x04db2615,
            0x73dc1683, 0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8,
            0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1, 0xf00f9344,
            0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb,
            0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a,
            0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
            0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1,
            0xa6bc5767, 0x3fb506dd, 0x48b2364b, 0xd80d2bda, 0xaf0a1b4c,
            0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef,
            0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
            0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe,
            0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31,
            0x2cd99e8b, 0x5bdeae1d, 0x9b64c2b0, 0xec63f226, 0x756aa39c,
            0x026d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
            0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38, 0x92d28e9b,
            0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
            0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1,
            0x18b74777, 0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c,
            0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45, 0xa00ae278,
            0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7,
            0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc, 0x40df0b66,
            0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
            0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605,
            0xcdd70693, 0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8,
            0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b,
            0x2d02ef8d
        };

        /// <summary>
        /// C
        /// </summary>
        /// <param name="b">Bytes list to C</param>
        /// <returns>C value</returns>
        internal static List<byte> C3(List<byte> b)
        {
            uint c = 0xFFFFFFFF;
            for (int i = 0; i < b.Count; i++)
            {
                c = CT[(c ^ b[i]) & 0xFF] ^ ((c >> 8) & 0x00FFFFFF);
            }

            return GB(c ^ 0xFFFFFFFF, 4);
        }

        /// <summary>
        /// C
        /// </summary>
        /// <param name="b">Bytes array to C</param>
        /// <returns>C value</returns>
        internal static List<byte> C3(byte[] b)
        {
            uint c = 0xFFFFFFFF;
            for (int i = 0; i < b.Length; i++)
            {
                c = CT[(c ^ b[i]) & 0xFF] ^ ((c >> 8) & 0x00FFFFFF);
            }

            return GB(c ^ 0xFFFFFFFF, 4);
        }
        #endregion

        #region O
        /// <summary>
        /// OTA
        /// </summary>
        /// <param name="l">Tag ID</param>
        /// <param name="f"></param>
        /// <returns></returns>
        internal static byte[] O(List<string> l, string f)
        {
            //var h = F(f);
            var b = new StringBuilder();
            b.AppendFormat("{0:X6}", f.Length / 2);
            b.AppendFormat("{0:X4}", l.Count);
            b.Append("0F00");
            l.ForEach(x => { b.Append(x); });
            b.Append(f);
            var b0 = Encoding.ASCII.GetBytes(b.ToString());
            var c0 = C3(b0);
            b.Append(BH(c0.ToArray()));
            var d = $"T{b.Length:X6}{b}";
            return Encoding.ASCII.GetBytes(d);
        }

        /// <summary>
        /// Firmware
        /// </summary>
        /// <param name="f">File name</param>
        /// <returns>Firmware bytes</returns>
        internal static string F(string f)
        {
            try
            {
                var fs = new FileStream(f, FileMode.Open, FileAccess.Read);
                var br = new BinaryReader(fs);
                var bs = br.ReadBytes((int)fs.Length);
                return BH(bs);
            }
            catch (Exception ex)
            {
                lg.LogError(ex, "OTA Load Hex Error:" + f);
                return string.Empty;
            }
        }
        #endregion
    }
}
