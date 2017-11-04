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

        public IOutput[] Outputs { get; set; }

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

        public SharperEngine(IFileSource fileSource, ICondition condition, IOutput[] outputs,
            IProcessor[] testedProcessors, IProcessor[] matchedProcessors, int maxToMatch)
        {
            FileSource = fileSource;
            Condition = condition;
            Outputs = outputs;
            TestedProcessors = testedProcessors;
            MatchedProcessors = matchedProcessors;
            MaxToMatch = maxToMatch;
        }

        public void Run(CancellationToken token, IProgress<FileProgressInfo> testedProgress,
            IProgress<FileProgressInfo> matchedProgress, IProgress<ExceptionInfo> exceptionProgress,
            IProgress<bool> completeProgress)
        {
            int numMatched = 0;
            try
            {
                if (testedProgress == null)
                {
                    testedProgress = new Progress<FileProgressInfo>(x => { });
                }
                if (matchedProgress == null)
                {
                    matchedProgress = new Progress<FileProgressInfo>(x => { });
                }
                if (exceptionProgress == null)
                {
                    exceptionProgress = new Progress<ExceptionInfo>(x => { });
                }
                if (completeProgress == null)
                {
                    completeProgress = new Progress<bool>(x => { });
                }
                lock (m_Mutex)
                {
                    RunInfo = new RunInfo(FileSource, Condition, Outputs, TestedProcessors,
                        MatchedProcessors, MaxToMatch, token, testedProgress, matchedProgress,
                        exceptionProgress, completeProgress);
                }
                token.ThrowIfCancellationRequested();
                FileSource.Init(RunInfo, exceptionProgress);
                token.ThrowIfCancellationRequested();
                Condition.Init(RunInfo, exceptionProgress);
                token.ThrowIfCancellationRequested();
                foreach (IOutput output in Outputs)
                {
                    token.ThrowIfCancellationRequested();
                    output.Init(RunInfo, exceptionProgress);
                }
                foreach (IProcessor processor in TestedProcessors)
                {
                    token.ThrowIfCancellationRequested();
                    processor.Init(RunInfo, exceptionProgress);
                }
                foreach (IProcessor processor in MatchedProcessors)
                {
                    token.ThrowIfCancellationRequested();
                    processor.Init(RunInfo, exceptionProgress);
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
                exceptionProgress?.Report(new ExceptionInfo(ex));
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

                        if (!file.Exists)
                        {
                            continue;
                        }
                        try
                        {
                            GetFileCaches(Condition, Outputs, file, caches, exceptionProgress);
                            try
                            {
                                result = Condition.Matches(file, caches, token);
                                if (result?.Values != null)
                                {
                                    values.AddRange(result.Values);
                                }
                            }
                            catch (OperationCanceledException ex)
                            {
                                throw ex;
                            }
                            catch (Exception ex)
                            {
                                exceptionProgress?.Report(new ExceptionInfo(ex, file));
                            }
                            if (result != null)
                            {
                                foreach (IOutput output in Outputs)
                                {
                                    if (output != null)
                                    {
                                        try
                                        {
                                            string[] vals = output.GetValues(file, caches, token);
                                            if (vals != null)
                                            {
                                                values.AddRange(vals);
                                            }
                                        }
                                        catch (OperationCanceledException ex)
                                        {
                                            throw ex;
                                        }
                                        catch (Exception ex)
                                        {
                                            exceptionProgress?.Report(new ExceptionInfo(ex, file));
                                        }
                                    }
                                }

                                string[] allValues = values.ToArray();

                                testedProgress.Report(new FileProgressInfo(file, result.Type, allValues));
                                RunProcessors(TestedProcessors, file, allValues);

                                if (result.Type == MatchResultType.Yes)
                                {
                                    matchedProgress?.Report(new FileProgressInfo(file, result.Type, allValues));
                                    RunProcessors(MatchedProcessors, file, allValues);
                                    numMatched++;
                                }
                            }
                        }
                        finally
                        {
                            DisposeFileCaches(caches, exceptionProgress);
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        exceptionProgress?.Report(new ExceptionInfo(ex, file));
                    }
                    if (RunInfo.StopRequested || numMatched == MaxToMatch)
                    {
                        break;
                    }
                }
                AggregateProcessors(TestedProcessors);
                AggregateProcessors(MatchedProcessors);
            }
            catch (OperationCanceledException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                exceptionProgress?.Report(new ExceptionInfo(ex));
            }
            finally
            {
                Cleanup(exceptionProgress);
            }
            completeProgress?.Report(false);
        }

        private void RunProcessors(IProcessor[] processors, FileInfo file, string[] values)
        {
            FileInfo[] lastOutputs = new FileInfo[0];
            foreach (IProcessor processor in processors)
            {
                RunInfo.CancellationToken.ThrowIfCancellationRequested();
                try
                {
                    ProcessInput whatToProcess = (processor.InputFileSource == InputFileSource.OriginalFile ?
                        ProcessInput.OriginalFile : ProcessInput.GeneratedFiles);
                    ProcessingResult result = processor?.Process(file, values,
                        lastOutputs ?? new FileInfo[0], whatToProcess, RunInfo.ExceptionProgress, RunInfo.CancellationToken);
                    FileInfo[] outputFiles = result == null ? new FileInfo[0] : result.OutputFiles;
                    lastOutputs = outputFiles == null ? new FileInfo[0] : outputFiles;
                }
                catch (OperationCanceledException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    lastOutputs = new FileInfo[0];
                    RunInfo.ExceptionProgress?.Report(new ExceptionInfo(ex, file));
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
                    processor?.ProcessAggregated(RunInfo.ExceptionProgress, RunInfo.CancellationToken);
                }
                catch (OperationCanceledException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    RunInfo.ExceptionProgress?.Report(new ExceptionInfo(ex, null));
                }
            }
        }

        private void GetFileCaches(ICondition condition, IOutput[] outputs, FileInfo file,
            Dictionary<Type, IFileCache> cacheLookup, IProgress<ExceptionInfo> exceptionProgress)
        {
            List<Type> cacheTypes = new List<Type>();
            cacheTypes.AddRange(condition.CacheTypes);
            foreach (IOutput output in outputs)
            {
                cacheTypes.AddRange(output.CacheTypes);
            }
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
                catch (OperationCanceledException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    exceptionProgress.Report(new ExceptionInfo(ex, file));
                }
            }
        }

        private void DisposeFileCaches(Dictionary<Type, IFileCache> cacheLookup, IProgress<ExceptionInfo> exceptionProgress)
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
                    exceptionProgress.Report(new ExceptionInfo(ex));
                }
            }
            if (canceledException != null)
            {
                throw canceledException;
            }
        }

        private void Cleanup(IProgress<ExceptionInfo> exceptionProgress)
        {
            try
            {
                FileSource?.Cleanup(exceptionProgress);
            }
            catch (Exception ex)
            {
                exceptionProgress?.Report(new ExceptionInfo(ex));
            }
            try
            {
                Condition?.Cleanup(exceptionProgress);
            }
            catch (Exception ex)
            {
                exceptionProgress?.Report(new ExceptionInfo(ex));
            }
            foreach (IOutput output in Outputs)
            {
                try
                {
                    output?.Cleanup(exceptionProgress);
                }
                catch (Exception ex)
                {
                    exceptionProgress?.Report(new ExceptionInfo(ex));
                }
            }
            foreach (IProcessor processor in TestedProcessors)
            {
                try
                {
                    processor?.Cleanup(exceptionProgress);
                }
                catch (Exception ex)
                {
                    exceptionProgress?.Report(new ExceptionInfo(ex));
                }
            }
            foreach (IProcessor processor in MatchedProcessors)
            {
                try
                {
                    processor?.Cleanup(exceptionProgress);
                }
                catch (Exception ex)
                {
                    exceptionProgress?.Report(new ExceptionInfo(ex));
                }
            }
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
