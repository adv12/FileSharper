// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Concurrent;
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

        public Timer Timer { get; set; }

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
                    TimeSpan interval = TimeSpan.FromMilliseconds(100);
                    Timer = new Timer(state => {
                        ReportFilesAndExceptions();
                        if (StopRequested)
                        {
                            Timer.Dispose();
                        }
                    }, null, interval, interval);
                }
                token.ThrowIfCancellationRequested();
                FileSource.Init(RunInfo);
                token.ThrowIfCancellationRequested();
                Condition.Init(RunInfo);
                token.ThrowIfCancellationRequested();
                foreach (IFieldSource fieldSource in FieldSources)
                {
                    token.ThrowIfCancellationRequested();
                    fieldSource.Init(RunInfo);
                }
                foreach (IProcessor processor in TestedProcessors)
                {
                    token.ThrowIfCancellationRequested();
                    processor.Init(RunInfo);
                }
                foreach (IProcessor processor in MatchedProcessors)
                {
                    token.ThrowIfCancellationRequested();
                    processor.Init(RunInfo);
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
                RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex));
                ReportFilesAndExceptions();
                completeProgress?.Report(false);
                return;
            }
            try
            {
                foreach (FileInfo file in FileSource.Files)
                {
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
                                RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, file));
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
                                            RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, file));
                                        }
                                    }
                                }

                                string[] allValues = values.ToArray();

                                RunInfo.TestedFileProgressInfos.Enqueue(new FileProgressInfo(file, result.Type, allValues));
                                RunProcessors(TestedProcessors, file, matchResultType, allValues);

                                if (result.Type == MatchResultType.Yes)
                                {
                                    RunInfo.MatchedFileProgressInfos.Enqueue(new FileProgressInfo(file, result.Type, allValues));
                                    RunProcessors(MatchedProcessors, file, matchResultType, allValues);
                                    numMatched++;
                                }
                            }
                        }
                        finally
                        {
                            DisposeFileCaches(caches, RunInfo.ExceptionInfos);
                        }
                    }
                    catch (Exception ex) when (!(ex is OperationCanceledException))
                    {
                        RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, file));
                    }
                    if (RunInfo.StopRequested || numMatched == MaxToMatch)
                    {
                        break;
                    }
                }
                AggregateProcessors(TestedProcessors);
                AggregateProcessors(MatchedProcessors);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex));
            }
            finally
            {
                Cleanup(exceptionProgress);
            }
            ReportFilesAndExceptions();
            completeProgress?.Report(false);
        }

        private void RunProcessors(IProcessor[] processors, FileInfo file,
            MatchResultType matchResultType, string[] values)
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
                        lastOutputs ?? new FileInfo[0], whatToProcess, RunInfo.CancellationToken);
                    FileInfo[] outputFiles = result == null ? new FileInfo[0] : result.OutputFiles;
                    lastOutputs = outputFiles == null ? new FileInfo[0] : outputFiles;
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    lastOutputs = new FileInfo[0];
                    RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, file));
                }
            }
        }

        private void AggregateProcessors(IProcessor[] processors)
        {
            foreach (IProcessor processor in processors)
            {
                RunInfo.CancellationToken.ThrowIfCancellationRequested();
                try
                {
                    processor?.ProcessAggregated(RunInfo.CancellationToken);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, null));
                }
            }
        }

        private T[] DequeueToArray<T>(ConcurrentQueue<T> queue)
        {
            List<T> tmp = new List<T>();
            T item;
            while (queue.TryDequeue(out item))
            {
                tmp.Add(item);
            }
            return tmp.ToArray();
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
                    RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, file));
                }
            }
        }

        private void ReportFilesAndExceptions()
        {
            lock (m_Mutex)
            {
                RunInfo.TestedProgress.Report(DequeueToArray(RunInfo.TestedFileProgressInfos));
                RunInfo.MatchedProgress.Report(DequeueToArray(RunInfo.MatchedFileProgressInfos));
                RunInfo.ExceptionProgress.Report(DequeueToArray(RunInfo.ExceptionInfos));
            }
        }

        private void DisposeFileCaches(Dictionary<Type, IFileCache> cacheLookup, ConcurrentQueue<ExceptionInfo> exceptionInfos)
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
                    exceptionInfos.Enqueue(new ExceptionInfo(ex));
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
                FileSource?.Cleanup();
            }
            catch (Exception ex)
            {
                RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex));
            }
            try
            {
                Condition?.Cleanup();
            }
            catch (Exception ex)
            {
                RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex));
            }
            foreach (IFieldSource fieldSource in FieldSources)
            {
                try
                {
                    fieldSource?.Cleanup();
                }
                catch (Exception ex)
                {
                    RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex));
                }
            }
            foreach (IProcessor processor in TestedProcessors)
            {
                try
                {
                    processor?.Cleanup();
                }
                catch (Exception ex)
                {
                    RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex));
                }
            }
            foreach (IProcessor processor in MatchedProcessors)
            {
                try
                {
                    processor?.Cleanup();
                }
                catch (Exception ex)
                {
                    RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex));
                }
            }
            ReportFilesAndExceptions();
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
