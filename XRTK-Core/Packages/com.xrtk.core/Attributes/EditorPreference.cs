using System;

namespace XRTK.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class KeyPreferenceAttribute : Attribute
    {
        public string Key { get; set; } = "";
        public object DefaultValue { get; set; } = new object();
        public bool ApplicationPrefix { get; set; } = false;
    }

    public sealed class EditorPreferenceAttribute : KeyPreferenceAttribute { }
    public sealed class SessionPreferenceAttribute : KeyPreferenceAttribute { }
}