using Cronos.SDK.Data;
using Cronos.SDK.Enum;
using Cronos.SDK.Helper;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cronos.SDK.Entity
{
    /// <summary>
    /// Tag entity
    /// </summary>
    public sealed class TagEntity
    {
        #region Private Properties
        private ESLType tagType;    // Tag type

        /// <summary>
        /// PData
        /// </summary>
        internal List<byte> PData { get; set; }

        /// <summary>
        /// Tag data: [Length:X4][TagID:X6][Pattern:X2][ItemData]
        /// </summary>
        public byte[] Data
        {
            get
            {
                if (PData is null || PData.Count == 0) Prepare();
                return PData.ToArray();
            }
        }

        /// <summary>
        /// Patten code
        /// </summary>
        internal byte ParttenCode
        {
            get
            {
                switch (Pattern)
                {
                    case Pattern.Query: return 0x56;
                    case Pattern.Display:
                        return PageIndex switch
                        {
                            PageIndex.P0 => 0x35,
                            PageIndex.P1 => 0x38,
                            PageIndex.P2 => 0x3B,
                            PageIndex.P3 => 0x3E,
                            PageIndex.P4 => 0x48, 
                            PageIndex.P5 => 0x49, 
                            PageIndex.P6 => 0x4A, 
                            PageIndex.P7=>0x4B,
                            _ => 0x35,
                        };
                    case Pattern.DisplayInfor: return 0x31;
                    case Pattern.Update:
                        return PageIndex switch
                        {
                            PageIndex.P0 => 0x34,
                            PageIndex.P1 => 0x37,
                            PageIndex.P2 => 0x3A,
                            PageIndex.P3 => 0x3D,
                            PageIndex.P4 => 0x44,
                            PageIndex.P5 => 0x45,
                            PageIndex.P6 => 0x46,
                            PageIndex.P7 => 0x47,
                            _ => 0x34,
                        };
                    case Pattern.UpdateDisplay:
                        return PageIndex switch
                        {
                            PageIndex.P0 => 0x33,
                            PageIndex.P1 => 0x36,
                            PageIndex.P2 => 0x39,
                            PageIndex.P3 => 0x3C,
                            PageIndex.P4 => 0x40,
                            PageIndex.P5 => 0x41,
                            PageIndex.P6 => 0x42,
                            PageIndex.P7 => 0x43,
                            _ => 0x34,
                        };
                    case Pattern.Check: return 0x70;
                    case Pattern.UpdatePart: return 0x31;
                    case Pattern.LED: return 0x32;
                    default: return 0x20;
                }
            }
        }

        #region LED Data
        /// <summary>
        /// LED
        /// </summary>
        internal byte[] LED
        {
            get
            {
                byte rgb = (byte)(R ? 1 : 0);
                rgb <<= 1;
                rgb += (byte)(G ? 1 : 0);
                rgb <<= 1;
                rgb += (byte)(B ? 1 : 0);
                var token = PB.B2(Token);
                var times = Times == -1 ? new List<byte> { 0x7F, 0xFE } : PB.B2(Times);
                return new byte[] { 0x07, rgb, token[0], token[1], 0x00, 0xED, times[0], times[1] };
            }
        }
        #endregion
        #endregion

        #region Public Properties
        /// <summary>
        /// Tag ID
        /// </summary>
        public string TagID { get; set; } = "";
        /// <summary>
        /// Tag type
        /// </summary>
        public ESLType TagType
        {
            get
            {
                if (TagID.Length != 12) return tagType;
                return EnumHelper.GetESLType(TagID[..2]);
            }
            set => tagType = value;
        }
        /// <summary>
        /// Pattern, default is Update + Display
        /// </summary>
        public Pattern Pattern { get; set; } = Pattern.UpdateDisplay;
        /// <summary>
        /// Page index, default is the 1st page data cache
        /// </summary>
        public PageIndex PageIndex { get; set; } = PageIndex.P0;
        /// <summary>
        /// Tag status, default is Unknow;
        /// </summary>
        public TagResult Status { get; set; } = TagResult.Unknown;
        /// <summary>
        /// Tag data compress mode: null is auto.
        /// </summary>
        public bool? Compress { get; set; } = null;
        /// <summary>
        /// Red LED light
        /// </summary>
        public bool R { get; set; } = false;
        /// <summary>
        /// Green LED light
        /// </summary>
        public bool G { get; set; } = false;
        /// <summary>
        /// Blue LED light
        /// </summary>
        public bool B { get; set; } = false;
        /// <summary>
        /// LED light flashing times
        /// </summary>
        public int Times { get; set; } = 0;
        /// <summary>
        /// LED lighting before screen refresh
        /// </summary>
        public bool Before { get; set; } = false;
        /// <summary>
        /// Service code, 0~65535(0XFFFF)
        /// </summary>
        public int Token { get; set; } = 0;
        /// <summary>
        /// Group code
        /// </summary>
        public int Group { get; set; } = -1;
        /// <summary>
        /// Index value
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Send count
        /// </summary>
        public int XCount = 0;
        /// <summary>
        /// Send time
        /// </summary>
        public DateTime XTime;
        /// <summary>
        /// Data item list
        /// </summary>
        public SKBitmap Image { get; set; }
        /// <summary>
        /// The new key
        /// </summary>
        public string NewKey { get; set; }
        #endregion

        #region Public methods
        /// <summary>
        /// Prepare data
        /// </summary>
        internal void Prepare()
        {
            List<byte> lst = new() { ParttenCode };

            if (Pattern == Pattern.Query)
            {
                lst.AddRange(new List<byte> { 0x08, 0x12, 0x34, 0x56, 0x00, 0xEC });
                lst.AddRange(PB.B2(Group));
                lst.Add((byte)Index);
                lst.AddRange(LED);
            }
            else
            {
                // Part 0: LED
                lst.AddRange(Server.KEY);
                lst.AddRange(LED);
                if (Group != -1)
                {
                    var group = PB.B2(Group);
                    lst.AddRange(new byte[] { 0x09, 0x12, 0x34, 0x56, 0x00, 0xEC, group[0], group[1], (byte)(Index % 8), (byte)(Index / 8) });
                }
                // Part 1: Pixel
                if (Image != null) lst.AddRange(PB.I(0, 0, Image, TagType));
            }

            List<byte> header = PB.B2((lst.Count + 6) * 2, 3);
            header.AddRange(PB.HB(TagID));

            PData = header.Concat(lst).ToList();
        }
        #endregion
    }

    /// <summary>
    /// Tag entity, X
    /// </summary>
    public sealed class TagEntityX
    {
        /// <summary>
        /// Tag ID
        /// </summary>
        public string TagID { get; private set; }
        /// <summary>
        /// Tag data
        /// </summary>
        public byte[] Data { get; private set; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tagID">Tag ID</param>
        /// <param name="data">Tag data</param>
        public TagEntityX(string tagID, byte[] data)
        {
            TagID = tagID;
            Data = data;
        }
    }
}
