using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Helper
{
    public static class CommonConverter
    {
        public static T ConvertToDerived<T>(object baseObj) where T : new()
        {
            var derivedObj = new T();
            var members = baseObj.GetType().GetMembers();
            foreach (var member in members)
            {
                object val = null;
                if (member.MemberType == MemberTypes.Field)
                {
                    val = ((FieldInfo)member).GetValue(baseObj);
                    ((FieldInfo)member).SetValue(derivedObj, val);
                }
                else if (member.MemberType == MemberTypes.Property)
                {
                    val = ((PropertyInfo)member).GetValue(baseObj);
                    if (val is IList && val.GetType().IsGenericType)
                    {
                        var listType = val.GetType().GetGenericArguments().Single();
                        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listType));
                        foreach (var item in (IList)val)
                        {
                            list.Add(item);
                        }
                        ((PropertyInfo)member).SetValue(baseObj, list, null);
                    }
                    if (((PropertyInfo)member).CanWrite)
                        ((PropertyInfo)member).SetValue(derivedObj, val);
                }
            }
            return derivedObj;
        }
    }
}
