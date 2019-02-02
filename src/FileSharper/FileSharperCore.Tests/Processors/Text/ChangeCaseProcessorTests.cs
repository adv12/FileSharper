// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Globalization;
using FileSharperCore.Processors.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileSharperCore.Tests.Processors.Text
{
    [TestClass]
    public class ChangeCaseProcessorTests : TestBase
    {
        [TestMethod]
        public void ExposesParameters()
        {
            // The use of these properties is tested in LineProcessorTests.
            // All we need to do here is verify that the parameters are
            // being exposed properly.

            ChangeCaseProcessor processor = new ChangeCaseProcessor();

            processor.SetParameter("MoveOriginalToRecycleBin", true);
            Assert.AreEqual(true, processor.MoveOriginalToRecycleBin);

            processor.SetParameter("LineEndings", LineEndings.ClassicMacOS);
            Assert.AreEqual(LineEndings.ClassicMacOS, processor.LineEndings);

            processor.SetParameter("OutputEncoding", OutputEncodingType.UTF16_LE);
            Assert.AreEqual(OutputEncodingType.UTF16_LE, processor.OutputEncodingType);

            processor.SetParameter("FileName", @"C:\foo.txt");
            Assert.AreEqual(@"C:\foo.txt", processor.FileName);

            processor.SetParameter("OverwriteExistingFile", true);
            Assert.AreEqual(true, processor.OverwriteExistingFile);
        }

        [TestMethod]
        public void TransformLineSuccess()
        {
            CultureInfo savedCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-us");
            ChangeCaseProcessor processor = new ChangeCaseProcessor();
            processor.SetParameter("Case", TextCase.Uppercase);

            processor.Init(RunInfo);

            string upperString = processor.TransformLine("ΑαΒβΓγΔδΕεΖζΗηΘθΙιΚκΛλΜμΝνΞξΟοΠπΡρΣσ/ςΤτΥυΦφΧχΨψΩω");
            Assert.AreEqual("ΑΑΒΒΓΓΔΔΕΕΖΖΗΗΘΘΙΙΚΚΛΛΜΜΝΝΞΞΟΟΠΠΡΡΣΣ/ΣΤΤΥΥΦΦΧΧΨΨΩΩ", upperString);

            upperString = processor.TransformLine("AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz");
            Assert.AreEqual("AABBCCDDEEFFGGHHIIJJKKLLMMNNOOPPQQRRSSTTUUVVWWXXYYZZ", upperString);

            processor.Cleanup();
            CultureInfo.CurrentCulture = savedCulture;
        }
    }
}
