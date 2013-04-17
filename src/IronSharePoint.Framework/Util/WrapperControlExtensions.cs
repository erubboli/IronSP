using System;
using System.Web.UI;
using IronSharePoint.Diagnostics;
using IronSharePoint.Exceptions;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;

namespace IronSharePoint.Util
{
    internal static class WrapperControlExtensions
    {
        public static Control CreateDynamicControl(this IWrapperControl control)
        {
            IronRuntime runtime = IronRuntime.GetDefaultIronRuntime(SPContext.Current.Site);
            ScriptEngine engine = runtime.RubyEngine;
            return (Control) engine.CreateInstance(control.ControlName);
        }

        public static bool TryCreateDynamicControl(this IWrapperControl control, out Control dynamicControl)
        {
            try
            {
                dynamicControl = CreateDynamicControl(control);
                return true;
            }
            catch (Exception ex)
            {
                control.InstantiationException = ex;
                dynamicControl = null;
                return false;
            }
        }
    }
}