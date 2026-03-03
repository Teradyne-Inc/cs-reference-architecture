using System;
using System.IO;
using System.Runtime.CompilerServices;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using Csra.Interfaces;

namespace Csra.Services {

    /// <summary>
    /// AlertService - centralized info / warning / error alerting.
    /// </summary>
    [Serializable]
    public class Alert : IAlertService {

        private static IAlertService _instance = null;
        private AlertOutputTarget _logTarget;
        private AlertOutputTarget _infoTarget;
        private AlertOutputTarget _warningTarget;
        private AlertOutputTarget _errorTarget;
        
        protected Alert() {
            Reset();
        }

        public static IAlertService Instance => _instance ??= new Alert();

        public AlertOutputTarget LogTarget {
            get => _logTarget;
            set => _logTarget = value | AlertOutputTarget.OutputWindow; // minimum - quietly ignore if that would be turned off
        }

        public AlertOutputTarget InfoTarget {
            get => _infoTarget;
            set => _infoTarget = value | AlertOutputTarget.OutputWindow; // minimum - quietly ignore if that would be turned off
        }

        public AlertOutputTarget WarningTarget {
            get => _warningTarget;
            set => _warningTarget = value | AlertOutputTarget.OutputWindow | AlertOutputTarget.Datalog; // minimum - quietly ignore if that would be turned off
        }

        public AlertOutputTarget ErrorTarget {
            get => _errorTarget;
            set => _errorTarget = value; // IG-XL will log exceptions to the output window and datalog anyway, so not turning this on by default
        }

        public string OutputFile { get; set; }

        public bool TimeStamp { get; set; }

        public void Reset() {
            LogTarget = 0;
            InfoTarget = 0;
            WarningTarget = 0;
            ErrorTarget = 0;
            OutputFile = string.Empty;
            TimeStamp = false;
        }

        public void Log(string message, ColorConstants color = ColorConstants.Black, bool bold = false) {
            LogMessage(_logTarget, message, color, bold);
        }

        public void Log(string message, byte red, byte green, byte blue, bool bold = false) {
            LogMessage(_logTarget, message, (ColorConstants)((blue << 16) | (green << 8) | red), bold);
        }

        public void Info(string info, [CallerMemberName] string doNotSpecify = "") {
            LogMessage(_infoTarget, GetMessage("INFO", info, doNotSpecify), (ColorConstants)0x7f0000, false);
        }

        public void Warning(string warning, [CallerMemberName] string doNotSpecify = "") {
            LogMessage(_warningTarget, GetMessage("WARNING", warning, doNotSpecify), (ColorConstants)0x004088, false);
        }

        public void Error(string error, int validationArgumentIndex = 0, [CallerMemberName] string doNotSpecify = "") =>
            Error<Exception>(error, validationArgumentIndex, doNotSpecify);

        public void Error<TException>(string error, int validationArgumentIndex = 0, [CallerMemberName] string doNotSpecify = "") where TException :
            Exception {
            if (TheExec.Flow.IsValidating) TheExec.DataManager.WriteTemplateArgumentError(validationArgumentIndex, 0, error, "C#RA");
            else {
                LogMessage(_errorTarget, GetMessage("ERROR", error, doNotSpecify), ColorConstants.Red, true);
                throw (TException)Activator.CreateInstance(typeof(TException), error);
            }
        }

        private string GetMessage(string alertLabel, string text, string memberName) =>
            $"{(TimeStamp ? $"{DateTime.Now:HH:mm:ss.fff}   " : string.Empty)}{alertLabel}: {text} [{memberName}]";

        private void LogMessage(AlertOutputTarget target, string message, ColorConstants color, bool bold) {
            if (target.HasFlag(AlertOutputTarget.OutputWindow)) TheExec.AddOutput(message, color, bold);
            if (target.HasFlag(AlertOutputTarget.Datalog)) TheExec.Datalog.WriteComment(message);
            if (target.HasFlag(AlertOutputTarget.File)) File.AppendAllText(OutputFile, $"{message}\r\n"); // no need for try / catch, will throw an exception
        }
    }
}
