// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FileSharperCore.FieldSources;

namespace FileSharperCore
{
    public class FieldSourceCatalog
    {
        private static FieldSourceCatalog s_Instance;

        public static FieldSourceCatalog Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new FieldSourceCatalog();
                }
                return s_Instance;
            }
        }

        private IFieldSource[] m_FieldSources = null;

        public IFieldSource[] FieldSources
        {
            get
            {
                return m_FieldSources;
            }
        }

        private FieldSourceCatalog()
        {
            List<IFieldSource> fieldSources = new List<IFieldSource>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types = assembly.GetTypes();
                foreach (Type t in types)
                {
                    Type[] interfaces = t.GetInterfaces();
                    if (interfaces.Contains(typeof(IFieldSource)) && !t.IsAbstract &&
                        t != typeof(NothignFieldSource))
                    {
                        try
                        {
                            IFieldSource o = (IFieldSource)Activator.CreateInstance(t);
                            if (o != null)
                            {
                                fieldSources.Add(o);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
            }
            fieldSources = fieldSources.OrderBy(o => o.Category, StringComparer.OrdinalIgnoreCase)
                .ThenBy(o => o.Name, StringComparer.OrdinalIgnoreCase).ToList();
            fieldSources.Insert(0, new NothignFieldSource());
            m_FieldSources = fieldSources.ToArray();
        }

        public IFieldSource CreateFieldSource(string typeName)
        {
            foreach (IFieldSource fieldSource in m_FieldSources)
            {
                Type t = fieldSource.GetType();
                if (t.FullName == typeName)
                {
                    return (IFieldSource)Activator.CreateInstance(t);
                }
            }
            return null;
        }

        public Type GetFieldSourceType(string typeName)
        {
            foreach (IFieldSource fieldSource in m_FieldSources)
            {
                Type t = fieldSource.GetType();
                if (t.FullName == typeName)
                {
                    return t;
                }
            }
            return null;
        }

        public Type FindFieldSourceTypeWithSameName(string typeName)
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
            foreach (IFieldSource fieldSource in m_FieldSources)
            {
                Type t = fieldSource.GetType();
                if (t.Name == name)
                {
                    return t;
                }
            }
            return null;
        }
    }
}
