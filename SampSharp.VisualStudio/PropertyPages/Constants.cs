namespace SampSharp.VisualStudio.PropertyPages
{
    public static class Constants
    {
        public const int SwShow = 5;
        public const int SwShownormal = 1;
        public const int SwHide = 0;

        /// <summary>
        ///     The values in the pages have changed, so the state of the
        ///     Apply button should be updated.
        /// </summary>
        public const int ProppagestatusDirty = 0x1;

        /// <summary>
        ///     Now is an appropriate time to apply changes.
        /// </summary>
        public const int ProppagestatusValidate = 0x2;
    }
}