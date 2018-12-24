using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Conditions.Binary
{
    public class BinaryDataContainsByteSequenceParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public string Bytes { get; set; }
        public BinaryInputFormat Format { get; set; } = BinaryInputFormat.Hexadecimal;
    }

    public class BinaryDataContainsByteSequenceCondition : BinaryDataSearchCondition
    {
        private BinaryDataContainsByteSequenceParameters m_Parameters = new BinaryDataContainsByteSequenceParameters();

        private byte[] m_Bytes;

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { "Binary Data Contains Byte Sequence \"" + m_Parameters.Bytes + "\"" };

        public override string Category => "Binary";

        public override string Name => "Binary Data Contains Byte Sequence";

        public override string Description => "Tests whether a binary file's data contains the specified byte sequence";

        public override object Parameters => m_Parameters;

        protected override byte[] Bytes => m_Bytes;

        public override void LocalInit()
        {
            base.LocalInit();
            List<byte> bytes = new List<byte>();
            switch (m_Parameters.Format)
            {
                case BinaryInputFormat.Hexadecimal:
                    string strval = Regex.Replace(m_Parameters.Bytes.ToUpperInvariant(), "[^ABCDEF\\d]", "");
                    for (int i = 0; i < strval.Length; i += 2)
                    {
                        string s = strval.Substring(i, 2);
                        bytes.Add(Convert.ToByte(s, 16));
                    }
                    break;
                case BinaryInputFormat.CommaDelimitedDecimal:
                    string[] vals = m_Parameters.Bytes.Split(',');

                    foreach (string val in vals)
                    {
                        string newval = Regex.Replace(val, "[^\\d]", "");
                        if (newval.Length > 0)
                        {
                            bytes.Add(byte.Parse(newval));
                        }
                    }
                    break;
            }
            m_Bytes = bytes.ToArray();
        }

    }
}
