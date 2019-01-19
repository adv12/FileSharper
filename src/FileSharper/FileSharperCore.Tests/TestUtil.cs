// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;
using System.IO.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileSharperCore.Tests
{
    public static class TestUtil
    {
        public static void AssertZipEquality(FileInfo expected, FileInfo result,
            bool matchFileDates = false, bool matchExternalAttributes = false)
        {
            using (FileStream expectedStream = expected.OpenRead())
            using (FileStream resultStream = result.OpenRead())
            using (ZipArchive expectedArchive = new ZipArchive(expectedStream))
            using (ZipArchive resultArchive = new ZipArchive(resultStream))
            {
                var expectedEntries = expectedArchive.Entries;
                var resultEntries = resultArchive.Entries;
                for (int i = 0; i <  expectedEntries.Count; i++)
                {
                    AssertZipEntryEquality(expectedEntries[i], resultEntries[i],
                        matchFileDates, matchExternalAttributes);
                }
            }
        }

        public static void AssertZipEntryEquality(ZipArchiveEntry expectedEntry,
            ZipArchiveEntry resultEntry, bool matchFileDates = false,
            bool matchExternalAttributes = false)
        {
            Assert.AreEqual(expectedEntry.FullName, resultEntry.FullName);
            Assert.AreEqual(expectedEntry.Length, resultEntry.Length);
            using (Stream expectedStream = expectedEntry.Open())
            using (Stream resultStream = resultEntry.Open())
            {
                Assert.IsTrue(AreStreamsEqual(expectedStream, resultStream));
            }
            if (matchFileDates)
            {
                Assert.AreEqual(expectedEntry.LastWriteTime, resultEntry.LastWriteTime);
            }
            if (matchExternalAttributes)
            {
                Assert.AreEqual(expectedEntry.ExternalAttributes, resultEntry.ExternalAttributes);
            }
        }

        public static bool AreStreamsEqual(Stream expectedStream, Stream resultStream)
        {
            int expectedVal = 0;
            int resultVal = 0;
            while (expectedVal != -1)
            {
                expectedVal = expectedStream.ReadByte();
                resultVal = resultStream.ReadByte();
                if (expectedVal != resultVal)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
