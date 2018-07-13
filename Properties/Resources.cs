using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Properties
{
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [DebuggerNonUserCode]
    [CompilerGenerated]
    internal class Resources
    {
        private static ResourceManager _resourceMan;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                if (_resourceMan == null)
                    _resourceMan = new ResourceManager("XCI_Explorer.Properties.Resources", typeof(Resources).Assembly);
                return _resourceMan;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture { get; set; }
    }
}