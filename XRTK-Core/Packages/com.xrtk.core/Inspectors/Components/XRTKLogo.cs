using UnityEngine.UIElements;
using XRTK.Extensions;

namespace XRTK.Inspectors.Components
{
    public class XRTKLogo : VisualElement
    {
        public static readonly string ussClassName = "xrtk-logo";
        public static readonly string lightVariant = ussClassName + "--light";
        public static readonly string darkVariant = ussClassName + "--dark";

        public bool isPro = false;
        public XRTKLogo()
        {
            this.LoadStyleSheet();
            AddToClassList(ussClassName);
            AddToClassList(isPro ? lightVariant : darkVariant);
        }
    }
}