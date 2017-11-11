// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FileSharperCore.Outputs;

namespace FileSharperCore
{
    public class OutputCatalog
    {
        private static OutputCatalog s_Instance;

        public static OutputCatalog Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new OutputCatalog();
                }
                return s_Instance;
            }
        }

        private IOutput[] m_Outputs = null;

        public IOutput[] Outputs
        {
            get
            {
                return m_Outputs;
            }
        }

        private OutputCatalog()
        {
            List<IOutput> outputs = new List<IOutput>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types = assembly.GetTypes();
                foreach (Type t in types)
                {
                    Type[] interfaces = t.GetInterfaces();
                    if (interfaces.Contains(typeof(IOutput)) && !t.IsAbstract &&
                        t != typeof(NothingOutput))
                    {
                        try
                        {
                            IOutput o = (IOutput)Activator.CreateInstance(t);
                            if (o != null)
                            {
                                outputs.Add(o);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
            }
            outputs = outputs.OrderBy(o => o.Category, StringComparer.OrdinalIgnoreCase)
                .ThenBy(o => o.Name, StringComparer.OrdinalIgnoreCase).ToList();
            outputs.Insert(0, new NothingOutput());
            m_Outputs = outputs.ToArray();
        }

        public IOutput CreateOutput(string typeName)
        {
            foreach (IOutput output in m_Outputs)
            {
                Type t = output.GetType();
                if (t.FullName == typeName)
                {
                    return (IOutput)Activator.CreateInstance(t);
                }
            }
            return null;
        }
    }
}
