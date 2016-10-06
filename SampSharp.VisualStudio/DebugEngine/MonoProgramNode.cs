using System;
using Microsoft.VisualStudio.Debugger.Interop;
using static Microsoft.VisualStudio.VSConstants;

namespace SampSharp.VisualStudio.DebugEngine
{
    public class MonoProgramNode : IDebugProgramNode2
    {
        private readonly AD_PROCESS_ID _processId;

        public MonoProgramNode(AD_PROCESS_ID processId)
        {
            _processId = processId;
        }

        #region Implementation of IDebugProgramNode2

        /// <summary>
        ///     Gets the name of a program.
        /// </summary>
        /// <param name="programName">Name of the program.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetProgramName(out string programName)
        {
            programName = null;
            return S_OK;
        }

        /// <summary>
        ///     Gets the name of the process hosting a program.
        /// </summary>
        /// <param name="hostNameType">Type of the host name.</param>
        /// <param name="hostName">Name of the host.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetHostName(enum_GETHOSTNAME_TYPE hostNameType, out string hostName)
        {
            hostName = null;
            return S_OK;
        }

        /// <summary>
        ///     Gets the system process identifier for the process hosting a program.
        /// </summary>
        /// <param name="hostProcessIds">The host process id.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetHostPid(AD_PROCESS_ID[] hostProcessIds)
        {
            hostProcessIds[0] = _processId;
            return S_OK;
        }

        /// <summary>
        ///     DEPRECATED. DO NOT USE.
        /// </summary>
        /// <param name="hostMachineName"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetHostMachineName_V7(out string hostMachineName)
        {
            hostMachineName = null;
            return E_NOTIMPL;
        }

        /// <summary>
        ///     DEPRECATED. DO NOT USE. See the IDebugProgramNodeAttach2 interface for an alternative approach.
        /// </summary>
        /// <param name="pMdmProgram"></param>
        /// <param name="pCallback"></param>
        /// <param name="dwReason"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int Attach_V7(IDebugProgram2 pMdmProgram, IDebugEventCallback2 pCallback, uint dwReason)
        {
            return E_NOTIMPL;
        }

        /// <summary>
        ///     Gets the name and identifier of the DE running this program.
        /// </summary>
        /// <param name="engineName">Name of the engine.</param>
        /// <param name="engineGuid">The engine GUID.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetEngineInfo(out string engineName, out Guid engineGuid)
        {
            engineName = "Mono Debug Engine";
            engineGuid = Guids.EngineIdGuid;
            return S_OK;
        }

        /// <summary>
        ///     DEPRECATED. DO NOT USE.
        /// </summary>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int DetachDebugger_V7()
        {
            return E_NOTIMPL;
        }

        #endregion
    }
}