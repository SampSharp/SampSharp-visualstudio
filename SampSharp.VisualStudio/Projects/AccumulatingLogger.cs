using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.Shell.Interop;
using SampSharp.VisualStudio.Utils;

namespace SampSharp.VisualStudio.Projects
{
    public class AccumulatingLogger : ILogger
    {
        private readonly string _solutionPath;

        private readonly Queue<Entry> _buffer = new Queue<Entry>();
        private string _currentProject;

        public AccumulatingLogger(string solutionPath)
        {
            _solutionPath = solutionPath;
        }

        public void Flush(IVsOutputWindowPane outputPane)
        {
            while (_buffer.Any())
            {
                var entry = _buffer.Dequeue();
                
                outputPane.Log(entry.Severity, entry.Project, entry.File, entry.LogMessage, entry.ItemMessage, entry.Line, entry.Column, entry.ErrorCode);
            }
        }

        private class Entry
        {
            public Entry(VsLogSeverity severity, string itemMessage, string project, string file, int line, int column,
                string logMessage, string errorCode)
            {
                Column = column;
                ItemMessage = itemMessage;
                File = file;
                LogMessage = logMessage;
                Line = line;
                Project = project;
                Severity = severity;
                ErrorCode = errorCode;
            }

            public int Column { get; }
            public string ItemMessage { get; }
            public string File { get; }
            public string LogMessage { get; }
            public int Line { get; }
            public string Project { get; }
            public string ErrorCode { get; }
            public VsLogSeverity Severity { get; }
        }

        #region Implementation of ILogger

        public LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Normal;

        public string Parameters { get; set; }

        private string GetProjectIdentifier()
        {
            var id = _currentProject;
            if (_currentProject != null && _currentProject.StartsWith(_solutionPath))
                id = _currentProject.Substring(_solutionPath.Length);

            return id;
        }

        public void Initialize(IEventSource eventSource)
        {
            eventSource.ErrorRaised += (sender, args) =>
            {
                var filePath = Path.Combine(Path.GetDirectoryName(args.ProjectFile) ?? string.Empty, args.File);
                var position = $"{args.LineNumber},{args.ColumnNumber},{args.LineNumber},{args.ColumnNumber}";

                _buffer.Enqueue(new Entry(VsLogSeverity.Error, args.Message, GetProjectIdentifier(), filePath,
                    args.LineNumber - 1, args.ColumnNumber - 1,
                    $"{filePath}({position}): error {args.Code}: {args.Message}", args.Code));
            };
            eventSource.WarningRaised += (sender, args) =>
            {
                var filePath = Path.Combine(Path.GetDirectoryName(args.ProjectFile) ?? string.Empty, args.File);
                var position = $"{args.LineNumber},{args.ColumnNumber},{args.LineNumber},{args.ColumnNumber}";

                _buffer.Enqueue(new Entry(VsLogSeverity.Warning, args.Message, GetProjectIdentifier(), filePath,
                    args.LineNumber - 1, args.ColumnNumber - 1,
                    $"{filePath}({position}): warning {args.Code}: {args.Message}", args.Code));
            };
            eventSource.ProjectStarted += (sender, args) => { _currentProject = args.ProjectFile; };
            eventSource.ProjectFinished += (sender, args) => { _currentProject = null; };
        }

        public void Shutdown()
        {
        }

        #endregion
    }
}