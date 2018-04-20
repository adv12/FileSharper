// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FileSharperCore.Processors;

namespace FileSharperCore
{
    public class ProcessorCatalog
    {
        private static ProcessorCatalog s_Instance;

        public static ProcessorCatalog Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new ProcessorCatalog();
                }
                return s_Instance;
            }
        }

        private IProcessor[] m_Processors = null;

        public IProcessor[] Processors
        {
            get
            {
                return m_Processors;
            }
        }

        private ProcessorCatalog()
        {
            List<IProcessor> processors = new List<IProcessor>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types = assembly.GetTypes();
                foreach (Type t in types)
                {
                    Type[] interfaces = t.GetInterfaces();
                    if (interfaces.Contains(typeof(IProcessor)) && !t.IsAbstract &&
                        t != typeof(DoNothingProcessor))
                    {
                        try
                        {
                            IProcessor p = (IProcessor)Activator.CreateInstance(t);
                            if (p != null)
                            {
                                processors.Add(p);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
            }
            processors = processors.OrderBy(p => p.Category, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
            processors.Insert(0, new DoNothingProcessor());
            m_Processors = processors.ToArray();
        }

        public IProcessor CreateProcessor(string typeName)
        {
            foreach (IProcessor processor in m_Processors)
            {
                Type t = processor.GetType();
                if (t.FullName == typeName)
                {
                    return (IProcessor)Activator.CreateInstance(t);
                }
            }
            return null;
        }

        public Type GetProcessorType(string typeName)
        {
            foreach (IProcessor processor in m_Processors)
            {
                Type t = processor.GetType();
                if (t.FullName == typeName)
                {
                    return t;
                }
            }
            return null;
        }

        public Type FindProcessorTypeWithSameName(string typeName)
        {
            if (typeName == null)
            {
                return null;
            }
            string name = typeName;
            int idx = typeName.LastIndexOf(".");
            if (idx > -1)
            {
                name = typeName.Substring(idx + 1);
            }
            foreach (IProcessor processor in m_Processors)
            {
                Type t = processor.GetType();
                if (t.Name == name)
                {
                    return t;
                }
            }
            return null;
        }

    }
}
