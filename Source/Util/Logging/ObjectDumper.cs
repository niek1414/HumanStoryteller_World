using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace HumanStoryteller.Util.Logging {
    public class ObjectDumper {
        private readonly int depth;

        private int level;

        private int pos;

        private TextWriter writer;

        private ObjectDumper(int depth) {
            this.depth = depth;
        }

        public static void Write(object element) {
            Write(element, 0);
        }

        public static void Write(object element, int depth) {
            Write(element, depth, System.Console.Out);
        }

        public static void Write(object element, int depth, TextWriter log) {
            ObjectDumper objectDumper = new ObjectDumper(depth);
            objectDumper.writer = log;
            objectDumper.WriteObject(null, element);
        }

        private void Write(string s) {
            if (s != null) {
                writer.Write(s);
                pos += s.Length;
            }
        }

        private void WriteIndent() {
            for (int i = 0; i < level; i++) {
                writer.Write("  ");
            }
        }

        private void WriteLine() {
            writer.WriteLine();
            pos = 0;
        }

        private void WriteTab() {
            Write("  ");
            while (pos % 8 != 0) {
                Write(" ");
            }
        }

        private void WriteObject(string prefix, object element) {
            if (element == null || element is ValueType || element is string) {
                WriteIndent();
                Write(prefix);
                WriteValue(element);
                WriteLine();
            } else {
                IEnumerable enumerable = element as IEnumerable;
                if (enumerable != null) {
                    foreach (object item in enumerable) {
                        if (item is IEnumerable && !(item is string)) {
                            WriteIndent();
                            Write(prefix);
                            Write("...");
                            WriteLine();
                            if (level < depth) {
                                level++;
                                WriteObject(prefix, item);
                                level--;
                            }
                        } else {
                            WriteObject(prefix, item);
                        }
                    }
                } else {
                    MemberInfo[] members = element.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public);
                    WriteIndent();
                    Write(prefix);
                    bool flag = false;
                    MemberInfo[] array = members;
                    foreach (MemberInfo memberInfo in array) {
                        FieldInfo fieldInfo = memberInfo as FieldInfo;
                        PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                        if (fieldInfo != null || propertyInfo != null) {
                            if (flag) {
                                WriteTab();
                            } else {
                                flag = true;
                            }

                            Write(memberInfo.Name);
                            Write("=");
                            Type type = (fieldInfo != null) ? fieldInfo.FieldType : propertyInfo.PropertyType;
                            if (type.IsValueType || type == typeof(string)) {
                                WriteValue((fieldInfo != null) ? fieldInfo.GetValue(element) : propertyInfo.GetValue(element, null));
                            } else if (typeof(IEnumerable).IsAssignableFrom(type)) {
                                Write("...");
                            } else {
                                Write("{ }");
                            }
                        }
                    }

                    if (flag) {
                        WriteLine();
                    }

                    if (level < depth) {
                        array = members;
                        foreach (MemberInfo memberInfo2 in array) {
                            FieldInfo fieldInfo2 = memberInfo2 as FieldInfo;
                            PropertyInfo propertyInfo2 = memberInfo2 as PropertyInfo;
                            if (fieldInfo2 != null || propertyInfo2 != null) {
                                Type type2 = (fieldInfo2 != null) ? fieldInfo2.FieldType : propertyInfo2.PropertyType;
                                if (!type2.IsValueType && type2 != typeof(string)) {
                                    object obj = (fieldInfo2 != null) ? fieldInfo2.GetValue(element) : propertyInfo2.GetValue(element, null);
                                    if (obj != null) {
                                        level++;
                                        WriteObject(memberInfo2.Name + ": ", obj);
                                        level--;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void WriteValue(object o) {
            if (o == null) {
                Write("null");
            } else if (o is DateTime) {
                Write(((DateTime) o).ToShortDateString());
            } else if (o is ValueType || o is string) {
                Write(o.ToString());
            } else if (o is IEnumerable) {
                Write("...");
            } else {
                Write("{ }");
            }
        }
    }
}