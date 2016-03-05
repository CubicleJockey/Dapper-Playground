using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace Dapper.Basics.Playground.Helpers
{
    public class ObjectDumper
    {
        #region Constants

        private const string SPACE = " ";
        private const string EMPTY_CULRYBRACES = "{ }";
        private const string NULL = "null";
        private const string ELLIPSIS = "...";

        #endregion Constants

        #region Fields

        private TextWriter writer;
        private int pos;
        private int level;
        private readonly int depth;

        #endregion Fields

        public static void Write(object element)
        {
            Write(element, 0);
        }

        public static void Write(object element, int depth)
        {
            Write(element, depth, Console.Out);
        }

        public static void Write(object element, int depth, TextWriter log)
        {
            var dumper = new ObjectDumper(depth)
            {
                writer = log
            };
            dumper.WriteObject(null, element);
        }

        #region Helper Methods

        private ObjectDumper(int depth)
        {
            this.depth = depth;
        }

        private void Write(string s)
        {
            if(s != null)
            {
                writer.Write(s);
                pos += s.Length;
            }
        }

        private void WriteIndent()
        {
            for(var i = 0; i < level; i++)
            {
                writer.Write(SPACE);
            }
        }

        private void WriteLine()
        {
            writer.WriteLine();
            pos = 0;
        }

        private void WriteTab()
        {
            Write(SPACE);
            while(pos%8 != 0)
            {
                Write(SPACE);
            }
        }

        private void WriteObject(string prefix, object element)
        {
            if(element == null || element is ValueType || element is string)
            {
                WriteIndent();
                Write(prefix);
                WriteValue(element);
                WriteLine();
            }
            else
            {
                var enumerableElement = element as IEnumerable;
                if(enumerableElement != null)
                {
                    foreach(var item in enumerableElement)
                    {
                        if(item is IEnumerable && !(item is string))
                        {
                            WriteIndent();
                            Write(prefix);
                            Write(ELLIPSIS);
                            WriteLine();
                            if(level < depth)
                            {
                                level++;
                                WriteObject(prefix, item);
                                level--;
                            }
                        }
                        else
                        {
                            WriteObject(prefix, item);
                        }
                    }
                }
                else
                {
                    var members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                    WriteIndent();
                    Write(prefix);
                    var propWritten = false;
                    foreach(var member in members)
                    {
                        var fieldInfo = member as FieldInfo;
                        var propertyInfo = member as PropertyInfo;
                        if(fieldInfo != null || propertyInfo != null)
                        {
                            if(propWritten)
                            {
                                WriteTab();
                            }
                            else
                            {
                                propWritten = true;
                            }
                            Write(member.Name);
                            Write("=");
                            var type = fieldInfo?.FieldType ?? propertyInfo.PropertyType;
                            if(type.IsValueType || type == typeof(string))
                            {
                                WriteValue(fieldInfo != null ? fieldInfo.GetValue(element) : propertyInfo.GetValue(element, null));
                            }
                            else
                            {
                                Write(typeof(IEnumerable).IsAssignableFrom(type) ? ELLIPSIS : EMPTY_CULRYBRACES);
                            }
                        }
                    }
                    if(propWritten)
                    {
                        WriteLine();
                    }
                    if(level < depth)
                    {
                        foreach(var memberInfo in members)
                        {
                            var fieldInfo = memberInfo as FieldInfo;
                            var propertyInfo = memberInfo as PropertyInfo;
                            if(fieldInfo != null || propertyInfo != null)
                            {
                                var type = fieldInfo?.FieldType ?? propertyInfo.PropertyType;
                                if(!(type.IsValueType || type == typeof(string)))
                                {
                                    var value = fieldInfo != null ? fieldInfo.GetValue(element) : propertyInfo.GetValue(element, null);
                                    if(value != null)
                                    {
                                        level++;
                                        WriteObject($"{memberInfo.Name}:", value);
                                        level--;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void WriteValue(object o)
        {
            if(o == null)
            {
                Write(NULL);
            }
            else if(o is DateTime)
            {
                Write(((DateTime) o).ToShortDateString());
            }
            else if(o is ValueType || o is string)
            {
                Write(o.ToString());
            }
            else if(o is IEnumerable)
            {
                Write(ELLIPSIS);
            }
            else
            {
                Write(EMPTY_CULRYBRACES);
            }
        }

        #endregion Helper Methods
    }
}
