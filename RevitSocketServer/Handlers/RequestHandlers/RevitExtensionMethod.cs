using ExtensionLib.Util;
using RevitSocketServer.Util;
using SocketServerEntities.Entity;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RevitSocketServer.Handlers.RequestHandlers
{
    internal class RevitExtensionMethod : IRequestTypeHandler
    {
        public RequestType RequestType => RequestType.ExecuteRevitExtension;

        private readonly IDocumentManager _manager;

        private readonly IPlugin _plugin = new AssemblyLoaderByAddinManager();
        private readonly DocumentDownloader _documentDownloader = new DocumentDownloader();

        public RevitExtensionMethod(IDocumentManager manager)
        {
            _manager = manager;
        }

        public async Task<Message> ExecuteAsync(Message message, CancellationToken token)
        {
            var (result, status) = await HandleMethodTypeAsync(message, token);
            return new Message { Body = result, MessageResult = status, Guid = message.Guid, ServiceKey = message.ServiceKey };
        }

        public async Task<(object result, MessageResult status)> HandleMethodTypeAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                var args = (Newtonsoft.Json.Linq.JObject)message.Body;
                string[] forAsm = ((string)args["path"]).Split(' ');
                if (forAsm.Length >= 2)
                {
                    var docpath = args.ContainsKey("serverPath")
                        ? (string)args["serverPath"]
                        : await _documentDownloader.DownloadAsync(
                            (string)args["downloadurn"],
                            (string)args["authorization"],
                            Path.Combine(AssemblyLoader.AssemblyPossibleDir, "files"
                          )
                    );                    
                    var needToLoadLinks = args.ContainsKey("needToLoadLinks") ? (bool)args["needToLoadLinks"] : false;
                    if (docpath != default)
                    {
                        var doc = await _manager.OpenDocumentAsync(docpath, needToLoadLinks);
                        if (!(doc is null))
                        {
                            var context = new ExtensionLib.Extension.ExtensionContext();
                            foreach (var i in args)
                            {
                                context.Args.Add(i.Key, i.Value.ToObject<object>());
                            }
                            context.Document = doc;
                            context.cancellationToken = cancellationToken;
                            var res = _plugin.LoadAndInvoke(forAsm[0], forAsm[1], context);
                            Logger.WriteInfo($"результат={res.Data}");
                            _plugin.Dispose();
                            return (res.Data, res.IsSuccess ? MessageResult.Success : MessageResult.Error);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return ("Задача отменена", MessageResult.Canceled);
            }
            catch (TargetInvocationException ex)
            {
                Logger.WriteError(ex.InnerException?.ToString());
                var inner = ex.InnerException;
                if (!(inner is null))
                {
                    if (inner is OperationCanceledException)
                    {
                        return ("Задача отменена", MessageResult.Canceled);
                    }
                    var error = $"Основное: {inner.Message}";
                    return (error, MessageResult.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.ToString());
                var error = $"Основное: {ex.Message}" + (ex.InnerException != default ? $"\nВнутреннее: {ex.InnerException.Message}" : "");
                return (error, MessageResult.Error);
            }

            return (default, MessageResult.NoResult);
        }
    }
}
