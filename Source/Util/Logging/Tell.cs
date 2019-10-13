using System.IO;

namespace HumanStoryteller.Util.Logging {
    public static class Tell {
        public static T AssertNotNull<T>(T obj, string name, string className) {
            if (obj == null) {
                Err($"nullpointer: {name} in {className}, trying to continue..");
            }

            return obj;
        }

        public static void Log(string message, object obj1 = null, object obj2 = null) {
            if (!HumanStoryteller.CreatorTools) return;
            Print(message, Severity.Low, obj1, obj2);
        }

        public static void Warn(string message, object obj1 = null, object obj2 = null) {
            Print(message, Severity.Medium, obj1, obj2);
        }
        
        public static void Debug(string message, object obj1 = null, object obj2 = null) {
            if (!HumanStoryteller.DEBUG) return;
            Verse.Log.ResetMessageCount();
            Print(" _DEBUG_" + message, Severity.Medium, obj1, obj2);
        }

        public static void Err(string message, object obj1 = null, object obj2 = null) {
            Print(message, Severity.High, obj1, obj2);
        }

        public static void Print(string message, Severity severity, object obj1 = null, object obj2 = null) {
            if (obj1 != null) {
                StringWriter stringWriter = new StringWriter();
                ObjectDumper.Write(obj1, 0, stringWriter);
                message += " 1_" + obj1.GetType() + "_ " + stringWriter;
            }
            if (obj2 != null) {
                StringWriter stringWriter = new StringWriter();
                ObjectDumper.Write(obj2, 0, stringWriter);
                message += " 2_" + obj2.GetType() + "_ " + stringWriter;
            }
            
            message = $"_HS_ {message}";
            switch (severity) {
                case Severity.Low:
                    Verse.Log.Message(message);
                    break;

                case Severity.Medium:
                    Verse.Log.Warning(message);
                    break;

                case Severity.High:
                    Verse.Log.Error(message);
                    break;
            }
        }

        public enum Severity {
            Low,
            Medium,
            High
        }
    }
}