using System;
using System.Drawing;
using System.Windows.Forms;

namespace SampSharp.VisualStudio.PropertyPages
{
    public interface IPageView : IDisposable
    {
        Size Size { get; }
        void HideView();
        void Initialize(Control parentControl, Rectangle rectangle);
        void MoveView(Rectangle rectangle);
        int ProcessAccelerator(ref Message message);
        void RefreshPropertyValues();
        void ShowView();
    }
}