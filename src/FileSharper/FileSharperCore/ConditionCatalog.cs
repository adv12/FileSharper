// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FileSharperCore.Conditions;

namespace FileSharperCore
{
    public class ConditionCatalog
    {
        private static ConditionCatalog s_Instance;

        public static ConditionCatalog Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new ConditionCatalog();
                }
                return s_Instance;
            }
        }

        private ICondition[] m_Conditions = null;

        public ICondition[] Conditions => m_Conditions;

        private ConditionCatalog()
        {
            List<ICondition> conditions = new List<ICondition>();
            
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types = assembly.GetTypes();
                foreach (Type t in types)
                {
                    Type[] interfaces = t.GetInterfaces();
                    if (interfaces.Contains(typeof(ICondition)) &&
                        !t.IsSubclassOf(typeof(CompoundCondition)) &&
                        t != typeof(NotCondition) && t != typeof(MatchEverythingCondition) &&
                        !t.IsAbstract)
                    {
                        try
                        {
                            ICondition c = (ICondition)Activator.CreateInstance(t);
                            if (c != null)
                            {
                                conditions.Add(c);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
            }
            conditions = conditions.OrderBy(c => c.Category, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase).ToList();
            conditions.Insert(0, new MatchEverythingCondition());
            conditions.Insert(1, new AllCondition());
            conditions.Insert(2, new AnyCondition());
            m_Conditions = conditions.ToArray();
        }

        public ICondition CreateCondition(string typeName)
        {
            foreach (ICondition condition in m_Conditions)
            {
                Type t = condition.GetType();
                if (t.FullName == typeName)
                {
                    return (ICondition)Activator.CreateInstance(t);
                }
            }
            return null;
        }
    }
}
