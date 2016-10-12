using System.Runtime.InteropServices;
using SampSharp.VisualStudio.PropertyPages;

namespace SampSharp.VisualStudio.ProgramProperties
{
    [Guid("82CE8435-2B18-44C8-9582-E87FC3627E01")]
    public class SampSharpPropertyPage : PropertyPage
    {
        public const string MonoDirectory = "MonoDirectory";
        public const string GameMode = "GameMode";

        protected override string HelpKeyword => string.Empty;
        public override string Title => "SampSharp";

        protected override IPageView GetNewPageView() => new SampSharpPropertiesView(this);
        protected override IPropertyStore GetNewPropertyStore() => new SampSharpPropertiesStore();
    }
}