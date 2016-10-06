using System;
using Microsoft.VisualStudio.Debugger.Interop;
using static Microsoft.VisualStudio.VSConstants;

namespace SampSharp.VisualStudio.DebugEngine
{
    public class SampSharpDebugProvider : IDebugProgramProvider2
    {
        /// <summary>
        ///     Obtains information about programs running, filtered in a variety of ways.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="port">The port.</param>
        /// <param name="processId">The process identifier.</param>
        /// <param name="engineFilter">The engine filter.</param>
        /// <param name="process">The process.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetProviderProcessData(enum_PROVIDER_FLAGS flags, IDebugDefaultPort2 port, AD_PROCESS_ID processId,
            CONST_GUID_ARRAY engineFilter, PROVIDER_PROCESS_DATA[] process)
        {
            return S_FALSE;
        }

        /// <summary>
        ///     Gets a program node, given a specific process ID.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="port">The port.</param>
        /// <param name="processId">The process identifier.</param>
        /// <param name="guidEngine">The unique identifier engine.</param>
        /// <param name="programId">The program identifier.</param>
        /// <param name="programNode">The program node.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int GetProviderProgramNode(enum_PROVIDER_FLAGS flags, IDebugDefaultPort2 port, AD_PROCESS_ID processId,
            ref Guid guidEngine, ulong programId, out IDebugProgramNode2 programNode)
        {
            programNode = null;
            return S_FALSE;
        }

        /// <summary>
        ///     Establishes a callback to watch for provider events associated with specific kinds of processes.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="port">The port.</param>
        /// <param name="processId">The process identifier.</param>
        /// <param name="engineFilter">The engine filter.</param>
        /// <param name="guidLaunchingEngine">The unique identifier launching engine.</param>
        /// <param name="eventCallback">The event callback.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int WatchForProviderEvents(enum_PROVIDER_FLAGS flags, IDebugDefaultPort2 port, AD_PROCESS_ID processId,
            CONST_GUID_ARRAY engineFilter, ref Guid guidLaunchingEngine, IDebugPortNotify2 eventCallback)
        {
            return S_OK;
        }

        /// <summary>
        ///     Establishes a locale for any language-specific resources needed by the DE.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        public int SetLocale(ushort locale)
        {
            return S_OK;
        }
    }
}