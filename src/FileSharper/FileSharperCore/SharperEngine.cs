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

        public IFileSource FileSource => RunInfo.FileSource;

        public ICondition Condition => RunInfo.Condition;

        public IOutput[] Outputs => RunInfo.Outputs;

        public IProcessor[] FoundProcessors => RunInfo.FoundProcessors;

        public IProcessor[] MatchedProcessors => RunInfo.MatchedProcessors;

        public SharperEngine(IFileSource fileSource, ICondition condition, IOutput[] outputs,
            IProcessor[] foundProcessors, IProcessor[] matchedProcessors)
        {
            RunInfo = new RunInfo(fileSource, condition, outputs, foundProcessors, matchedProcessors);
        }

        public void Run(CancellationToken token, IProgress<FileProgressInfo> foundProgress,
            IProgress<FileProgressInfo> matchedProgress, IProgress<ExceptionInfo> exceptionProgress,
            IProgress<bool> completeProgress)
        {
            try
            {
                Thread.MemoryBarrier();
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
                foreach (IProcessor processor in FoundProcessors)
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
                throw ex;
            }
            catch(Exception ex)
            {
                exceptionProgress?.Report(new ExceptionInfo(ex));
                completeProgress?.Report(false);
                return;
            }
            finally
            {
                Cleanup(exceptionProgress);
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
                        else
                        {
                            GetFileCaches(Condition, file, caches, exceptionProgress);
                        }
                        try
                        {
                            result = Condition.Matches(file, caches, token);
                            if (result.Values != null)
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
                        finally
                        {
                            DisposeFileCaches(caches, exceptionProgress);
                        }

                        if (result != null)
                        {
                            foundProgress?.Report(new FileProgressInfo(file, result.Type, result.Values));
                        }

                        RunProcessors(FoundProcessors, file, result.Values, exceptionProgress, token);
                        
                        if (result != null && result.Type == MatchResultType.Yes)
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

                            matchedProgress?.Report(new FileProgressInfo(file, result.Type, allValues));

                            RunProcessors(MatchedProcessors, file, result.Values, exceptionProgress, token);
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

        private void RunProcessors(IProcessor[] processors, FileInfo file, string[] values,
            IProgress<ExceptionInfo> exceptionProgress, CancellationToken token)
        {
            FileInfo[] lastOutputs = new FileInfo[0];
            foreach (IProcessor processor in processors)
            {
                token.ThrowIfCancellationRequested();
                if (processor == null)
                {
                    continue;
                }
                try
                {
                    lastOutputs = processor?.Process(file, values, lastOutputs, token);
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

        private void GetFileCaches(ICondition condition, FileInfo file, Dictionary<Type, IFileCache> cacheLookup,
            IProgress<ExceptionInfo> exceptionProgress)
        {
            Type[] cacheTypes = condition.CacheTypes;
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
            foreach (IProcessor processor in FoundProcessors)
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

    }
}
