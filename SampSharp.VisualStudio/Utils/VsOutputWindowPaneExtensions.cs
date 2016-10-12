using System;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;

namespace SampSharp.VisualStudio.Utils
{
    public static class VsOutputWindowPaneExtensions
    {
        public static void Log(this IVsOutputWindowPane pane, string message)
        {
            pane.OutputString(message + Environment.NewLine);
        }

        public static void Log(this IVsOutputWindowPane pane, VsLogSeverity severity, string project, string file,
            string consoleMessage, int lineNumber = 0, int column = 0, string lookupKeyword = null)
        {
            pane.Log(severity, project, file, consoleMessage, consoleMessage, lineNumber, column, lookupKeyword);
        }

        public static void Log(this IVsOutputWindowPane pane, VsLogSeverity severity, string project, string file,
            string consoleMessage, string taskMessage, int lineNumber = 0, int column = 0, string lookupKeyword = null)
        {
            VSTASKPRIORITY priority;
            switch (severity)
            {
                case VsLogSeverity.Message:
                    priority = VSTASKPRIORITY.TP_LOW;
                    break;
                case VsLogSeverity.Warning:
                    priority = VSTASKPRIORITY.TP_NORMAL;
                    break;
                case VsLogSeverity.Error:
                    priority = VSTASKPRIORITY.TP_HIGH;
                    break;
                default:
                    throw new Exception();
            }
            
            var pane2 = (IVsOutputWindowPane2) pane;
            pane2.OutputTaskItemStringEx2(consoleMessage + "\r\n", priority, VSTASKCATEGORY.CAT_BUILDCOMPILE, "FOO",
                0, file, (uint) lineNumber, (uint) column, project, taskMessage, lookupKeyword);
        }
    }
}