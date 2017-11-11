// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FileSharperCore
{
    public class FileSourceCatalog
    {
        private static FileSourceCatalog s_Instance;

        public static FileSourceCatalog Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new FileSourceCatalog();
                }
                return s_Instance;
            }
        }

        private IFileSource[] m_FileSources = null;

        public IFileSource[] FileSources
        {
            get
            {
                return m_FileSources;
            }
        }

        private FileSourceCatalog()
        {
            List<IFileSource> fileSources = new List<IFileSource>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types = assembly.GetTypes();
                foreach (Type t in types)
                {
                    Type[] interfaces = t.GetInterfaces();
                    if (interfaces.Contains(typeof(IFileSource)) && !t.IsAbstract)
                    {
                        try
                        {
                            IFileSource fs = (IFileSource)Activator.CreateInstance(t);
                            if (fs != null)
                            {
                                fileSources.Add(fs);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
            }
            fileSources = fileSources.OrderBy(fs => fs.Category, StringComparer.OrdinalIgnoreCase)
                .ThenBy(fs => fs.Name, StringComparer.OrdinalIgnoreCase).ToList();
            m_FileSources = fileSources.ToArray();
        }

        public IFileSource CreateFileSource(string typeName)
        {
            foreach (IFileSource fileSource in m_FileSources)
            {
                Type t = fileSource.GetType();
                if (t.FullName == typeName)
                {
                    return (IFileSource)Activator.CreateInstance(t);
                }
            }
            return null;
        }
    }
}
