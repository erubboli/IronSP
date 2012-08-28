using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronRuby;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using System.IO;
using IronSharePoint.Diagnostics;
using Microsoft.Scripting.Runtime;
using System.Web;
using Microsoft.SharePoint.Administration;
using Microsoft.Scripting;
using System.Dynamic;

namespace IronSharePoint
{
    public class IronEngine
    {
        private static Dictionary<String, ScriptEngine> _scriptEnginesCache = new Dictionary<string, ScriptEngine>();

        private SPFolder _scriptRootFolder;

        public SPFolder ScriptRootFolder
        {
            get { return _scriptRootFolder; }
        }

        private SPFolder _initialScriptFolder;

        public SPFolder InitialScriptFolder
        {
            get { return _initialScriptFolder; }
        }

        private SPListItem _initialScriptListItem;

        public SPListItem InitialScriptListItem
        {
            get { return _initialScriptListItem; }
        }


        private List<String> loadedScripts = new List<String>();

        public ScriptRuntime ScriptRuntime
        {
            get { return _scriptEngine.Runtime; }
        }

        private ScriptEngine _scriptEngine;

        public ScriptEngine ScriptEngine
        {
            get { return _scriptEngine; }
        }

        private ScriptScope _scriptScope;

        public ScriptScope ScriptScope
        {
            get { return _scriptScope; }
            set { _scriptScope = value; }
        }

        private SPWeb _web;

        public SPWeb Web
        {
            get { return _web; }
        }

        private SPSite _site;

        public SPSite Site
        {
            get { return _site; }
        }


        internal static IronEngine GetEngine(string extension, SPWeb web)
        {
            IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Core], TraceSeverity.Verbose, String.Format("Engine for extension {0} requested.", extension));
            var ironEngine = new IronEngine();

            try
            {
                if (extension.ToLower() == ".rb".ToLower())
                {
                    if (_scriptEnginesCache.ContainsKey(extension))
                    {
                        ironEngine._scriptEngine = _scriptEnginesCache[extension];
                    }
                    else
                    {
                        var scriptRuntime = Ruby.CreateRuntime();
                        ironEngine._scriptEngine = scriptRuntime.GetEngineByFileExtension(extension);
                        
                        SPSecurity.RunWithElevatedPrivileges(() =>
                        {
                            var feature = web.Site.Features[new Guid(IronConstants.IronSiteFeatureId)];

                            if (feature == null)
                            {
                                throw new InvalidOperationException("The IronSP Feature is not activated on the current Site Collection!");
                            }

                            var featureRootFolder = new DirectoryInfo(feature.Definition.RootDirectory).Parent;
                            var ironRubyRootFolder = Path.Combine(featureRootFolder.FullName, "IronSP_IronRuby10\\");

                            Environment.SetEnvironmentVariable("IRONRUBY_10_20", ironRubyRootFolder);

                            ironEngine._scriptEngine.SetSearchPaths(new List<String>() {
                                Path.Combine(ironRubyRootFolder, @"Lib\IronRuby"),
                                Path.Combine(ironRubyRootFolder, @"Lib\ruby\site_ruby\1.8"),
                                Path.Combine(ironRubyRootFolder, @"Lib\ruby\site_ruby"),
                                Path.Combine(ironRubyRootFolder, @"Lib\ruby\1.8")
                            });

                        });
      
                        _scriptEnginesCache.Add(extension, ironEngine._scriptEngine);
                    }              

                    ironEngine._site = web.Site;
                    ironEngine._web = web;
                    ironEngine._scriptRootFolder = web.Site.RootWeb.GetFolder(IronConstants.IronHiveListPath);

                    ironEngine._scriptScope = ironEngine.ScriptEngine.CreateScope();
                    ironEngine._scriptScope.SetVariable("iron", ironEngine);
                    ironEngine._scriptScope.SetVariable("site", ironEngine._site);
                    ironEngine._scriptScope.SetVariable("web", ironEngine._web);
                    ironEngine._scriptScope.SetVariable("scriptRootFolder", ironEngine._scriptRootFolder);
                    ironEngine._scriptScope.SetVariable("ctx", SPContext.Current);

                }
                else
                {
                    Exception ex = new NotSupportedException(String.Format("Language for extenstion {0} is not supported.", extension));;
                    LogError(String.Format("Engine for extenstion {0} is not supported.", extension), ex);
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Core], TraceSeverity.Unexpected, String.Format("Error while loading engine for {0}. Error:{1}; Stack:{2}", extension, ex.Message, ex.StackTrace));
                throw;
            }
    
            return ironEngine;
        }

        public object InvokeDyamicMethodIfExists(string methodName, params object[] args)
        {
            try
            {
                object dynMethod = null;
                if (ScriptScope.TryGetVariable(methodName, out dynMethod))
                {
                    if (args.Length == 0)
                    {
                        return _scriptEngine.Operations.Invoke(dynMethod);
                    }
                    else
                    {
                        return _scriptEngine.Operations.Invoke(dynMethod, args);
                    }
                }

            }
            catch (Exception ex)
            {
                LogError(String.Format("Error invoking method {0}", methodName), ex);
                throw;
            }

            return null;
        }

        public object ExcecuteScriptFile(SPFile scriptFile)
        {
            object output = null;

            if (_initialScriptListItem == null)
            {
                _initialScriptListItem = scriptFile.Item;
                _initialScriptFolder = scriptFile.ParentFolder;
                
                _scriptScope.SetVariable("scriptFolder", _initialScriptFolder);
                _scriptScope.SetVariable("scriptItem", _initialScriptListItem);
            }

            if (!loadedScripts.Contains(scriptFile.ServerRelativeUrl))
            {
                string script = String.Empty;
                loadedScripts.Add(scriptFile.ServerRelativeUrl);

                var httpCtx = HttpContext.Current;
                IronScriptCache scriptCache = null;

                if (httpCtx != null)
                {
                    scriptCache = httpCtx.Cache[scriptFile.UniqueId.ToString()] as IronScriptCache;
                }

                if (scriptCache != null && scriptCache.Timestamp == scriptFile.TimeLastModified)
                {
                    output = scriptCache.CompiledCode.Execute(_scriptScope);
                    script = scriptCache.Script;
                    return output;
                }
              
                //var options = new IronRuby.Compiler.RubyCompilerOptions();
                // options.GetType().InvokeMember("FactoryKind", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty, null, options, new object[]{4});

                //var compiledCode = _scriptEngine.CreateScriptSourceFromString(script, Microsoft.Scripting.SourceCodeKind.File).Compile();

                script = scriptFile.Web.GetFileAsString(scriptFile.Url);

                if (httpCtx != null)
                {
                    var compiled = _scriptEngine.CreateScriptSourceFromString(script).Compile();
                    //httpCtx.Cache[scriptFile.UniqueId.ToString()] = new IronScriptCache() { Id = scriptFile.UniqueId, Script = script, Timestamp = scriptFile.TimeLastModified, CompiledCode = compiled };
                }


                output = _scriptEngine.CreateScriptSourceFromString(script, SourceCodeKind.File).Execute(_scriptEngine.Runtime.Globals);
            }

            return output;
        }

        public SPFile GetFile(string fileName)
        {
            var ctx = HttpContext.Current;

            var cache = ctx.Cache[fileName];

            if (cache != null)
                return ctx.Cache[fileName] as SPFile;

            SPFile file = null;
            var rootWeb = _site.RootWeb;

            if (_initialScriptFolder != null)
            {
                file = rootWeb.GetFile(String.Format("{0}/{1}", _initialScriptFolder.ServerRelativeUrl, fileName));
                if (file.Exists)
                {
                    ctx.Cache[fileName] = file;
                    return file;
                }
            }

            file = rootWeb.GetFile(String.Format("{0}/{1}", _scriptRootFolder.ServerRelativeUrl, fileName));
            if (!file.Exists)
            {
                throw new FileNotFoundException();
            }
            //ctx.Cache[fileName] = file;
            return file;
        }

        public SPFile get_file(string fileName)
        {
            return GetFile(fileName);
        }

        public object LoadScript(string scriptName)
        {     
            var scriptFile = GetFile(scriptName);

            return ExcecuteScriptFile(scriptFile);
        }

        public object load_script(string scriptName)
        {
            return LoadScript(scriptName);
        }

        public string LoadText(string fileName)
        {         
            var file = GetFile(fileName);
            var str = _web.Site.RootWeb.GetFileAsString(file.Url);

            return str;
        }
        
        public string load_text(string fileName)
        {
            return LoadText(fileName);
        }


        public static void LogError(string msg, Exception ex)
        {
            IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Controls], TraceSeverity.Unexpected, String.Format("{0}. Error:{1}; Stack:{2}", msg, ex.Message, ex.StackTrace));
        }

        public static void LogVerbose(string msg)
        {
            IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[IronCategoryDiagnosticsId.Controls], TraceSeverity.Verbose, String.Format("{0}.", msg));
        }
        
    }

    public class IronScope : DynamicObject
    {

    }
}
