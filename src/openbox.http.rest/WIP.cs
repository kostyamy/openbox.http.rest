using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Shared.Rest
{
	public static class WIP
    {
        private const string MethodNotImplementedMessage = "Method {0}.{1}() is not implemented.";

        public static void NotSupported(params object[] args) => throw new NotSupportedException();
        public static T NotSupported<T>(params object[] args) => throw new NotSupportedException();

        public static readonly ConcurrentBag<string> Log = new ConcurrentBag<string>();

        public static void NotReady(params object[] args)
        {
            var method = new StackFrame(1).GetMethod();
            var message = string.Format(MethodNotImplementedMessage, method?.DeclaringType?.FullName, method?.Name);
            throw new NotImplementedException(LogMessage(message));
        }

        public static T NotReady<T>(params object[] args)
        {
            var method = new StackFrame(1)?.GetMethod();
            var message = string.Format(MethodNotImplementedMessage, method?.DeclaringType?.FullName, method?.Name);
            throw new NotImplementedException(LogMessage(message));
        }

        private static string LogMessage(string message)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Log(1, "WIP", $@"
=======> {message}
");
                Log.Add(message);
            }
            return message;
        }
    }
}
