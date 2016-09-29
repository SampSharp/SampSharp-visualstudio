namespace SampSharp.VisualStudio.PropertyPages
{
	public interface IPageViewSite
	{
		void PropertyChanged(string propertyName, string propertyValue);
		string GetValueForProperty(string propertyName);
	}
}