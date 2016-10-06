using System;

namespace SampSharp.VisualStudio.DebugEngine
{
    public static class Guids
    {
        public const string EngineIdGuidString = "4C46E2E6-9222-44F3-ABA0-E1A15BF03CD0";

        /// <summary>
        ///     The attach command group unique identifier string
        /// </summary>
        public const string AttachCommandGroupGuidString = "727E4A66-FA6B-44AE-8639-9489FC65347E";

        /// <summary>
        ///     The attach command identifier.
        /// </summary>
        public const uint AttachCommandId = 0x100;

        /// <summary>
        ///     The DE identifier GUID.
        /// </summary>
        public static readonly Guid EngineIdGuid = new Guid(EngineIdGuidString);

        /// <summary>
        ///     The attach command group GUID.
        /// </summary>
        public static readonly Guid AttachCommandGroupGuid = new Guid(AttachCommandGroupGuidString);

        /// <summary>
        ///     The c sharp language service GUID.
        /// </summary>
        public static readonly Guid CSharpLanguageService = new Guid("{694DD9B6-B865-4C5B-AD85-86356E9C88DC}");

        /// <summary>
        ///     The filter locals GUID.
        /// </summary>
        public static readonly Guid FilterLocals = new Guid("b200f725-e725-4c53-b36a-1ec27aef12ef");

        /// <summary>
        ///     The filter all locals GUID.
        /// </summary>
        public static Guid FilterAllLocals = new Guid("196db21f-5f22-45a9-b5a3-32cddb30db06");

        /// <summary>
        ///     The filter arguments GUID.
        /// </summary>
        public static Guid FilterArgs = new Guid("804bccea-0475-4ae7-8a46-1862688ab863");

        /// <summary>
        ///     The filter locals and arguments GUID.
        /// </summary>
        public static Guid FilterLocalsPlusArgs = new Guid("e74721bb-10c0-40f5-807f-920d37f95419");

        /// <summary>
        ///     The filter all locals and arguments GUID.
        /// </summary>
        public static Guid FilterAllLocalsPlusArgs = new Guid("939729a8-4cb0-4647-9831-7ff465240d5f");
    }
}