using System;

namespace SampSharp.VisualStudio.Utils
{
	public class ComponentException : Exception
	{
		public ComponentException(int code)
		{
			Code = code;
		}

		public int Code { get; }
	}
}