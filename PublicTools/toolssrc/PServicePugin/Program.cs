using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PServicePugin
{

    public struct RPCCall
    {
        public string API;
        public string Request;
        public string Response;
        public string Url;

        public override string ToString()
        {
            return $"{API}:{Request}->{Response}";
        }
    }


    public class ServiceRpc
    {
        public List<RPCCall> call = new List<RPCCall>();
        public string name;
    }
   

    class Program
    {
 
        static Regex regex;
        static Program()
        {
            var str = @"rpc[ ]+([^ \(]+)[ \(]+([^\)]+)\)[ ]*returns[ \(]+([^\)]+)\)";
            regex = new Regex(str); //1 api name , 2 request 3 response
        }
        private static readonly HashSet<string> types = new HashSet<string>();

        static void Main(string[] args)
        {
            var root = string.Empty;
            var file = string.Empty;
            var fileSave = string.Empty;
            var indexFileName = "MessageIndex.cs";
            var version = "0.0.0";
            var onlyTypes = true;
            foreach (var i in args)
            {
                if (i.StartsWith("dir:", StringComparison.CurrentCultureIgnoreCase))
                {
                    root = i.Replace("dir:", "");
                }

                if (i.StartsWith("file:", StringComparison.CurrentCultureIgnoreCase))
                {
                    file = i.Replace("file:", "");
                }

                if (i.StartsWith("saveto:", StringComparison.CurrentCultureIgnoreCase))
                {
                    fileSave = i.Replace("saveto:", "");
                }

                if (i.StartsWith("index:", StringComparison.CurrentCultureIgnoreCase))
                {
                    indexFileName = i.Replace("index:", "");
                }

                if (i.StartsWith("version:", StringComparison.CurrentCultureIgnoreCase))
                {
                    version = i.Replace("version:", "");
                }
            }
            Console.WriteLine($"dir:{root} file:{file} saveto:{fileSave} index:{indexFileName} version:{version}");
            StringBuilder sb = null;
            var paths = Directory.GetFiles(root, file);
            string serives = string.Empty;
            string nameSpace = string.Empty;
            Stack<RPCCall> calls =null;
            List<ServiceRpc> callList = new List<ServiceRpc>();
            ServiceRpc ser_current =null;
            foreach (var path in paths)
            {
                if (!onlyTypes)
                {
                    using (var reader = new StreamReader(path))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            line = line.Trim();
                            //message class1 class2
                            var strs = line.Split("\t/ ".ToArray());
                            if (strs.Length == 0) continue;
                            //Console.WriteLine(line);
                            switch (strs[0])
                            {
                                case "package":
                                    nameSpace = strs[1].Replace(";", "");
                                    //[NAMESPACE]
                                    Console.WriteLine("nameSpace:" + nameSpace);
                                    break;
                                case "service":
                                    serives = strs[1];
                                    sb = new StringBuilder();
                                    Console.WriteLine("service:" + serives);
                                    calls = new Stack<RPCCall>();
                                    ser_current = new ServiceRpc() { name = serives };
                                    callList.Add(ser_current);
                                    break;
                                case "}":
                                    if (sb != null)
                                    {

                                        var callInvokes = new StringBuilder();
                                        var c_callInvokes = new StringBuilder();

                                        while (calls.Count > 0)
                                        {
                                            var c = calls.Pop();
                                            //@"        [API([URL])][Response] [API]([Request] req);"
                                            callInvokes.AppendLine(IRPCCall
                                                .Replace("[Response]", c.Response)
                                                .Replace("[Request]", c.Request)
                                                .Replace("[API]", c.API)
                                                .Replace("[URL]", c.Url));
                                            c_callInvokes.AppendLine(RPCCall
                                                .Replace("[Response]", c.Response)
                                                .Replace("[Request]", c.Request)
                                                .Replace("[API]", c.API)
                                                .Replace("[URL]", c.Url));

                                        }

                                        var icall = IRPCService.Replace("[SERVICE]", serives)
                                            .Replace("[RPC]", callInvokes.ToString());

                                        sb.AppendLine(icall);

                                        var call = RPCServeric.Replace("[SERVICE]", serives)
                                            .Replace("[RPC]", c_callInvokes.ToString());
                                        sb.Append(call);


                                        var result = FileTemplate.Replace("[CLASSES]", sb.ToString()).Replace("[SERVICE]", serives)
                                            .Replace("[NAMESPACE]", nameSpace);
                                        File.WriteAllText(Path.Combine(root, $"{fileSave}/{serives}.cs"), result);
                                        sb = null;
                                        serives = string.Empty;
                                    }
                                    break;
                                case "rpc":
                                    if (sb == null) throw new Exception("Rpc must in services");
                                    //regex 
                                    //1 api name , 2 request 3 response api url t[1]
                                    //Index_OF_API++;
                                    ProcessRPC(line, out string api, out string re, out string res, out string url);
                                    //url = $"{Index_OF_API}";
                                    Console.WriteLine($"{url}->rpc {api} ( {re} )return( {res} )");
                                    var code = MessageTemplate.Replace("[Name]", api)
                                        .Replace("[Request]", re).Replace("[Response]", res)
                                        .Replace("[NOTE]", url)
                                        .Replace("[API]", url);
                                    sb.AppendLine(code);
                                    var cl = new RPCCall { API = api, Request = re, Response = res, Url = url };
                                    calls?.Push(cl);
                                    if (ser_current != null) ser_current.call.Add(cl);
                                    break;
                            }
                        }
                    }
                }


            }
            var index_sb = new StringBuilder();
            Dictionary<string, int> types = new Dictionary<string, int>();
            var orderCall = callList.OrderBy(t => t.name).ToArray();
            var index = 1000;
            foreach (var i in orderCall)
            {
                int startIndex =  index*1000;
                foreach (var c in i.call)
                {
                    startIndex++;
                    if (!types.ContainsKey(c.Request))
                    {
                        types.Add(c.Request, startIndex);
                    }
                    startIndex++;
                    if (!types.ContainsKey(c.Response))
                    {
                        types.Add(c.Response, startIndex);
                    }

                    Console.WriteLine(c.ToString());
                }
                index++;
            }

            foreach (var i in types)
            {
                var str = $"    [Index({i.Value},typeof({i.Key}))]";
                index_sb.AppendLine(str);
            }

            var index_cs = Temple_Index
            .Replace("[VERSION]", version.Replace(".",","))
                .Replace("[ATTRIBUTES]",index_sb.ToString());
            File.WriteAllText(Path.Combine(root, $"{fileSave}/{indexFileName}"), index_cs);
        }

        private static bool ProcessRPC(string line, out string api, out string request, out string response, out string apiurl)
        {
            line = line.Trim();
            var match = regex.Match(line);
            if (match.Groups.Count >= 4)
            {
                api = match.Groups[1].Value;
                request = match.Groups[2].Value;
                response = match.Groups[3].Value;

                var google = "google.protobuf.";
                request = request.Replace(google, "").Trim();
                response = response.Replace(google, "").Trim();
            }
            else throw new Exception($"error:{line}");
            var temp = line.Replace(@"//", "\n");
            var t = temp.Split('\n');
            apiurl = t[1];
            return match.Success;
        }

        public static string FileTemplate = @"
using [NAMESPACE];
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace [NAMESPACE].X.[SERVICE]
{
[CLASSES]
}";

        public static string IRPCService = @"
    public interface I[SERVICE]
    {
[RPC]
    }
   ";
        public static string IRPCCall = @"        [API([URL])] Task<[Response]> [API]([Request] req);";

        public static string RPCServeric = @"
    public abstract class [SERVICE]
    {
[RPC]
    }
";
        public static string RPCCall = @"        [API([URL])]public abstract Task<[Response]> [API]([Request] request);";

        public static string MessageTemplate = @"
    /// <summary>
    /// [NOTE]
    /// </summary>    
    [API([API])]
    public class [Name]:APIBase<[Request], [Response]> 
    {
        private [Name]() : base() { }
        public  static [Name] CreateQuery(){ return new [Name]();}
    }
    ";

        private static string Temple_Index = @"using System;
using System.Collections.Generic;

namespace Proto
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =true)]
    public class IndexAttribute:Attribute
    {
         public IndexAttribute(int index,Type tOm) 
         {
            this.Index = index;
            this.TypeOfMessage = tOm;
         }
        public int Index { set; get; }
        public Type TypeOfMessage { set; get; }
    }

    [AttributeUsage(AttributeTargets.Class,AllowMultiple =false)]
    public class ApiVersionAttribute : Attribute
    {
        public ApiVersionAttribute(int m, int dev, int bate)
        {
            if (m > 99 || dev > 99 || bate > 99) throw new Exception(""must less then 100"");
            v = m* 10000 + dev* 100 + bate;
        }
        private readonly int v = 0;
        public int Version { get { return v; } }
    }


[ATTRIBUTES]
    [ApiVersion([VERSION])]
    public static class MessageTypeIndexs
    {
        private static readonly Dictionary<int, Type> types = new Dictionary<int, Type>();

        private static readonly Dictionary<Type, int> indexs = new Dictionary<Type, int>();
        
        static MessageTypeIndexs()
        {
            var tys = typeof(MessageTypeIndexs).GetCustomAttributes(typeof(IndexAttribute), false) as IndexAttribute[];

            foreach(var t in tys)
            {
                types.Add(t.Index, t.TypeOfMessage);
                indexs.Add(t.TypeOfMessage, t.Index);
            }
            var ver = typeof(MessageTypeIndexs).GetCustomAttributes(typeof(ApiVersionAttribute), false) as ApiVersionAttribute[];
            if (ver != null && ver.Length > 0)
                Version = ver[0].Version;
        }
        public static int Version { get; private set; }

        /// <summary>
        /// Tries the index of the get.
        /// </summary>
        /// <returns><c>true</c>, if get index was tryed, <c>false</c> otherwise.</returns>
        /// <param name=""type"">Type.</param>
        /// <param name=""index"">Index.</param>
        public static bool TryGetIndex(Type type,out int index)
        {
            return indexs.TryGetValue(type, out index);
        }
        /// <summary>
        /// Tries the type of the get.
        /// </summary>
        /// <returns><c>true</c>, if get type was tryed, <c>false</c> otherwise.</returns>
        /// <param name=""index"">Index.</param>
        /// <param name=""type"">Type.</param>
        public static bool TryGetType(int index,out Type type)
        {
            return types.TryGetValue(index, out type);
        }
    }
}
";

    }

    
}
