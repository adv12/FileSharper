using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Conditions.Binary
{
    public class BinaryDataContainsTextParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public string Text { get; set; }
        [PropertyOrder(2, UsageContextEnum.Both)]
        public SearchTextEncodingType Encoding { get; set; } = SearchTextEncodingType.UTF8;
    }

    public class BinaryDataContainsTextCondition : BinaryDataSearchCondition
    {
        private BinaryDataContainsTextParameters m_Parameters = new BinaryDataContainsTextParameters();

        private byte[] m_bytes;

        protected override byte[] Bytes => m_bytes;

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { "Binary Data Contains \"" + m_Parameters.Text + "\"" };

        public override string Category => "Binary";

        public override string Name => "Binary Data Contains Text";

        public override string Description => "Tests whether a binary file's data contains the specified text in the specified encoding";

        public override object Parameters => m_Parameters;

        public override void LocalInit()
        {
            base.LocalInit();
            Encoding encoding = TextUtil.GetSearchTextEncoding(m_Parameters.Encoding);
            m_bytes = encoding.GetBytes(m_Parameters.Text);
        }
    }
}
