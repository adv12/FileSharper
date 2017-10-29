// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Threading;
using FileSharperCore.Util;

namespace FileSharperCore.Processors
{

    public class CommandLineParameters
    {
        public string CommandLine { get; set; } = "echo \"{FullName}\"";
    }

    public class CommandLineProcessor : SingleFileProcessorBase
    {
        private CommandLineParameters m_Parameters = new CommandLineParameters();

        public override string Name => "Execute command line";

        public override string Description => "Executes the specified command line";

        public override object Parameters => m_Parameters;

        public override ProcessingResult Process(FileInfo file, string[] values,
            IProgress<ExceptionInfo> exceptionProgress, CancellationToken token)
        {
            try
            {
                string comandLine = ReplaceUtil.Replace(m_Parameters.CommandLine, file);
                System.Diagnostics.Process.Start("cmd", "/c " + comandLine);
            }
            catch (Exception ex)
            {
                exceptionProgress.Report(new ExceptionInfo(ex, file));
                return new ProcessingResult(ProcessingResultType.Failure, ex.Message, null);
            }
            return new ProcessingResult(ProcessingResultType.Success, "Success", null);
        }
    }
}
