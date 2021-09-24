using System;
using System.Collections.Generic;
using System.Text;

namespace ExtensionLib.Extension
{
    public interface IExtension
    {
        ExtensionResult Execute(ExtensionContext context);
    }
}
