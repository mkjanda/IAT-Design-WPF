using System.Reflection;
using System.Xml;
using System.IO;
using net.sf.saxon.s9api;
using javax.xml.transform.stream;
using net.liberty_development.SaxonHE12s9apiExtensions;

namespace IAT.Core.Services
{
    public interface IXsltService
    {
        XsltTransformer GetTransform(string resourceName);  // cacheable
    }

    public class XsltService : IXsltService
    {
        private readonly Dictionary<string, XsltTransformer> _cache = new();

        public XsltTransformer GetTransform(string resourceName)
        {
            if (_cache.TryGetValue(resourceName, out var cached))
                return cached;
            
            var assembly = Assembly.GetExecutingAssembly();  // or typeof(SomeCoreType).Assembly
            var fullResourceName = $"IAT.Core.Transforms.{resourceName}";  // adjust namespace/folders

            using var stream = assembly.GetManifestResourceStream(fullResourceName)
                ?? throw new FileNotFoundException($"XSLT resource not found: {fullResourceName}");

            var processor = new Processor();
            var compiler = processor.newXsltCompiler();
            DotNetInputStream inStream = new DotNetInputStream(stream);
            StreamSource source = new StreamSource(inStream);
            XsltExecutable executable = compiler.compile(source);
            _cache["resourceName"] = executable.load();  // cache the compiled XSLT for future use
            return _cache["resourceName"];
        }
    }
}