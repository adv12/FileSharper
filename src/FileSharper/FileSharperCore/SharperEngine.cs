// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FileSharperCore
{
    public class SharperEngine
    {
        public RunInfo RunInfo { get; set; }

        public IFileSource FileSource { get; set; }

        public ICondition Condition { get; set; }

        public IFieldSource[] FieldSources { get; set; }

        public IProcessor[] TestedProcessors { get; set; }

        public IProcessor[] MatchedProcessors { get; set; }

        public int MaxToMatch { get; set; }

        public bool StopRequested
        {
            get
            {
                lock (m_Mutex)
                {
                    if (RunInfo == null)
                    {
                        return false;
                    }
                    return RunInfo.StopRequested;
                }
            }
        }

        private object m_Mutex = new object();

        public SharperEngine(IFileSource fileSource, ICondition condition, IFieldSource[] fieldSources,
            IProcessor[] testedProcessors, IProcessor[] matchedProcessors, int maxToMatch)
        {
            FileSource = fileSource;
            Condition = condition;
            FieldSources = fieldSources;
            TestedProcessors = testedProcessors;
            MatchedProcessors = matchedProcessors;
            MaxToMatch = maxToMatch;
        }

        public void Run(CancellationToken token, IProgress<string> fileSourceProgress,
            IProgress<IEnumerable<FileProgressInfo>> testedProgress,
            IProgress<IEnumerable<FileProgressInfo>> matchedProgress,
            IProgress<IEnumerable<ExceptionInfo>> exceptionProgress,
            IProgress<bool> completeProgress, TimeSpan reportInterval)
        {
            int numMatched = 0;
            DateTime lastExceptionReportTime = DateTime.Now;
            DateTime lastTestedReportTime = DateTime.Now;
            DateTime lastMatchedReportTime = DateTime.Now;
            List<FileProgressInfo> testedFileProgressInfos = new List<FileProgressInfo>();
            List<FileProgressInfo> matchedFileProgressInfos = new List<FileProgressInfo>();
            List<ExceptionInfo> exceptionInfos = new List<ExceptionInfo>();
            try
            {
                if (fileSourceProgress == null)
                {
                    fileSourceProgress = new Progress<string>(x => { });
                }
                if (testedProgress == null)
                {
                    testedProgress = new Progress<IEnumerable<FileProgressInfo>>(x => { });
                }
                if (matchedProgress == null)
                {
                    matchedProgress = new Progress<IEnumerable<FileProgressInfo>>(x => { });
                }
                if (exceptionProgress == null)
                {
                    exceptionProgress = new Progress<IEnumerable<ExceptionInfo>>(x => { });
                }
                if (completeProgress == null)
                {
                    completeProgress = new Progress<bool>(x => { });
                }
                lock (m_Mutex)
                {
                    RunInfo = new RunInfo(FileSource, Condition, FieldSources, TestedProcessors,
                        MatchedProcessors, MaxToMatch, token, fileSourceProgress, testedProgress,
                        matchedProgress, exceptionProgress, completeProgress);
                }
                token.ThrowIfCancellationRequested();
                FileSource.Init(RunInfo, exceptionInfos);
                token.ThrowIfCancellationRequested();
                Condition.Init(RunInfo, exceptionInfos);
                token.ThrowIfCancellationRequested();
                foreach (IFieldSource fieldSource in FieldSources)
                {
                    token.ThrowIfCancellationRequested();
                    fieldSource.Init(RunInfo, exceptionInfos);
                }
                foreach (IProcessor processor in TestedProcessors)
                {
                    token.ThrowIfCancellationRequested();
                    processor.Init(RunInfo, exceptionInfos);
                }
                foreach (IProcessor processor in MatchedProcessors)
                {
                    token.ThrowIfCancellationRequested();
                    processor.Init(RunInfo, exceptionInfos);
                }
            }
            catch (OperationCanceledException ex)
            {
                Cleanup(exceptionProgress);
                throw ex;
            }
            catch(Exception ex)
            {
                Cleanup(exceptionProgress);
                exceptionInfos.Add(new ExceptionInfo(ex));
                ReportFilesAndExceptions(testedFileProgressInfos, matchedFileProgressInfos, exceptionInfos);
                completeProgress?.Report(false);
                return;
            }
            try
            {
                foreach (FileInfo file in FileSource.Files)
                {
                    if (DateTime.Now - lastExceptionReportTime> reportInterval)
                    {
                        exceptionProgress.Report(exceptionInfos.ToArray());
                        exceptionInfos.Clear();
                        lastExceptionReportTime = DateTime.Now;
                    }
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        Dictionary<Type, IFileCache> caches = new Dictionary<Type, IFileCache>();
                        List<string> values = new List<string>();

                        MatchResult result = null;

                        MatchResultType matchResultType = MatchResultType.NotApplicable;

                        if (!file.Exists)
                        {
                            continue;
                        }
                        try
                        {
                            GetFileCaches(Condition, FieldSources, file, caches, exceptionProgress);
                            try
                            {
                                result = Condition.Matches(file, caches, token);
                                if (result?.Values != null)
                                {
                                    values.AddRange(result.Values);
                                }
                                matchResultType = result == null ? MatchResultType.NotApplicable : result.Type;
                            }
                            catch (Exception ex) when (!(ex is OperationCanceledException))
                            {
                                exceptionInfos.Add(new ExceptionInfo(ex, file));
                            }
                            if (result != null)
                            {
                                foreach (IFieldSource fieldSource in FieldSources)
                                {
                                    if (fieldSource != null)
                                    {
                                        try
                                        {
                                            string[] vals = fieldSource.GetValues(file, caches, token);
                                            if (vals != null)
                                            {
                                                values.AddRange(vals);
                                            }
                                        }
                                        catch (Exception ex) when (!(ex is OperationCanceledException))
                                        {
                                            exceptionInfos.Add(new ExceptionInfo(ex, file));
                                        }
                                    }
                                }

                                string[] allValues = values.ToArray();

                                testedFileProgressInfos.Add(new FileProgressInfo(file, result.Type, allValues));
                                if (DateTime.Now - lastTestedReportTime > reportInterval)
                                {
                                    testedProgress.Report(testedFileProgressInfos.ToArray());
                                    testedFileProgressInfos.Clear();
                                    lastTestedReportTime = DateTime.Now;
                                }
                                RunProcessors(TestedProcessors, file, matchResultType, allValues, exceptionInfos);

                                if (result.Type == MatchResultType.Yes)
                                {
                                    matchedFileProgressInfos.Add(new FileProgressInfo(file, result.Type, allValues));
                                    if (DateTime.Now - lastMatchedReportTime > reportInterval)
                                    {
                                        matchedProgress.Report(matchedFileProgressInfos.ToArray());
                                        matchedFileProgressInfos.Clear();
                                        lastMatchedReportTime = DateTime.Now;
                                    }
                                    RunProcessors(MatchedProcessors, file, matchResultType, allValues, exceptionInfos);
                                    numMatched++;
                                }
                            }
                        }
                        finally
                        {
                            DisposeFileCaches(caches, exceptionInfos);
                        }
                    }
                    catch (Exception ex) when (!(ex is OperationCanceledException))
                    {
                        exceptionInfos.Add(new ExceptionInfo(ex, file));
                    }
                    if (RunInfo.StopRequested || numMatched == MaxToMatch)
                    {
                        break;
                    }
                }
                AggregateProcessors(TestedProcessors, exceptionInfos);
                AggregateProcessors(MatchedProcessors, exceptionInfos);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                exceptionInfos.Add(new ExceptionInfo(ex));
            }
            finally
            {
                Cleanup(exceptionProgress);
            }
            ReportFilesAndExceptions(testedFileProgressInfos, matchedFileProgressInfos, exceptionInfos);
            completeProgress?.Report(false);
        }

        private void RunProcessors(IProcessor[] processors, FileInfo file,
            MatchResultType matchResultType, string[] values, List<ExceptionInfo> exceptionInfos)
        {
            FileInfo[] lastOutputs = new FileInfo[0];
            foreach (IProcessor processor in processors)
            {
                RunInfo.CancellationToken.ThrowIfCancellationRequested();
                try
                {
                    ProcessInput whatToProcess = (processor.InputFileSource == InputFileSource.OriginalFile ?
                        ProcessInput.OriginalFile : ProcessInput.GeneratedFiles);
                    ProcessingResult result = processor?.Process(file, matchResultType, values,
                        lastOutputs ?? new FileInfo[0], whatToProcess, exceptionInfos, RunInfo.CancellationToken);
                    FileInfo[] outputFiles = result == null ? new FileInfo[0] : result.OutputFiles;
                    lastOutputs = outputFiles == null ? new FileInfo[0] : outputFiles;
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    lastOutputs = new FileInfo[0];
                    exceptionInfos.Add(new ExceptionInfo(ex, file));
                }
            }
        }

        private void AggregateProcessors(IProcessor[] processors, List<ExceptionInfo> exceptionInfos)
        {
            foreach (IProcessor processor in processors)
            {
                RunInfo.CancellationToken.ThrowIfCancellationRequested();
                try
                {
                    processor?.ProcessAggregated(exceptionInfos, RunInfo.CancellationToken);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    exceptionInfos.Add(new ExceptionInfo(ex, null));
                }
            }
        }

        private void GetFileCaches(ICondition condition, IFieldSource[] fieldSources, FileInfo file,
            Dictionary<Type, IFileCache> cacheLookup, IProgress<IEnumerable<ExceptionInfo>> exceptionProgress)
        {
            List<Type> cacheTypes = new List<Type>();
            cacheTypes.AddRange(condition.CacheTypes);
            foreach (IFieldSource fieldSource in fieldSources)
            {
                cacheTypes.AddRange(fieldSource.CacheTypes);
            }
            List<ExceptionInfo> exinfos = new List<ExceptionInfo>();
            foreach (Type type in cacheTypes)
            {
                Type[] interfaces = type.GetInterfaces();
                if (type == null || !interfaces.Contains(typeof(IFileCache)))
                {
                    continue;
                }
                try
                {
                    if (!cacheLookup.ContainsKey(type))
                    {
                        IFileCache cache = (IFileCache)Activator.CreateInstance(type);
                        cache.Load(file);
                        cacheLookup[type] = cache;
                    }
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    exinfos.Add(new ExceptionInfo(ex, file));
                }
            }
            exceptionProgress.Report(exinfos.ToArray());
        }

        private void ReportSingleException(IProgress<IEnumerable<ExceptionInfo>> exceptionProgress, Exception ex)
        {
            exceptionProgress.Report(new ExceptionInfo[] { new ExceptionInfo(ex) });
        }

        private void ReportFilesAndExceptions(List<FileProgressInfo> testedFileProgressInfos,
                            List<FileProgressInfo> matchedFileProgressInfos,
                            List<ExceptionInfo> exceptionInfos)
        {
            RunInfo.TestedProgress.Report(testedFileProgressInfos.ToArray());
            RunInfo.MatchedProgress.Report(matchedFileProgressInfos.ToArray());
            RunInfo.ExceptionProgress.Report(exceptionInfos.ToArray());
        }

        private void DisposeFileCaches(Dictionary<Type, IFileCache> cacheLookup, List<ExceptionInfo> exceptionInfos)
        {
            OperationCanceledException canceledException = null;
            foreach (IFileCache cache in cacheLookup.Values)
            {
                try
                {
                    cache?.Dispose();
                }
                catch (OperationCanceledException ex)
                {
                    canceledException = ex;
                }
                catch (Exception ex)
                {
                    exceptionInfos.Add(new ExceptionInfo(ex));
                }
            }
            if (canceledException != null)
            {
                throw canceledException;
            }
        }

        private void Cleanup(IProgress<IEnumerable<ExceptionInfo>> exceptionProgress)
        {
            List<ExceptionInfo> exinfos = new List<ExceptionInfo>();
            try
            {
                FileSource?.Cleanup(exinfos);
            }
            catch (Exception ex)
            {
                exinfos.Add(new ExceptionInfo(ex));
            }
            try
            {
                Condition?.Cleanup(exinfos);
            }
            catch (Exception ex)
            {
                exinfos.Add(new ExceptionInfo(ex));
            }
            foreach (IFieldSource fieldSource in FieldSources)
            {
                try
                {
                    fieldSource?.Cleanup(exinfos);
                }
                catch (Exception ex)
                {
                    exinfos.Add(new ExceptionInfo(ex));
                }
            }
            foreach (IProcessor processor in TestedProcessors)
            {
                try
                {
                    processor?.Cleanup(exinfos);
                }
                catch (Exception ex)
                {
                    exinfos.Add(new ExceptionInfo(ex));
                }
            }
            foreach (IProcessor processor in MatchedProcessors)
            {
                try
                {
                    processor?.Cleanup(exinfos);
                }
                catch (Exception ex)
                {
                    exinfos.Add(new ExceptionInfo(ex));
                }
            }

            exceptionProgress?.Report(exinfos);
        }

        public void RequestStop()
        {
            lock (m_Mutex)
            {
                RunInfo runInfo = RunInfo;
                if (runInfo != null)
                {
                    runInfo.RequestStop();
                }
            }
        }

    }
}
