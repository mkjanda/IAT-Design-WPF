using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using net.sf.saxon.s9api;
using javax.xml.transform.stream;
using net.liberty_development.SaxonHE12s9apiExtensions;
using IAT.Core.Extensions;
using System.IO;


namespace IAT.Core.Services
{
    public class ReportService : IReportService
    {
        public async Task<Stream> GenerateExcelReportAsync(object model)
        {
            // Placeholder implementation - replace with actual report generation logic
            await Task.Delay(100); // Simulate async work
            return new MemoryStream(); // Return an empty stream for now
        }

        /// <summary>
        /// Transforms the specified model using the given XSLT template and returns the result as a stream in the specified output format.
        /// </summary>
        /// <param name="model">The data model to be transformed. This object provides the input data for the XSLT transformation.</param>
        /// <param name="transformName">The name of the transformation to apply. Used to identify the specific transformation logic or configuration.</param>
        /// <param name="outputType">The output method to use for the transformation result. Supported values typically include "xml", "html", or "text". The default is "xml".</param>
        /// <returns></returns>
        public async Task<Stream> TransformAsync(object model, string transformName, string outputType = "xml")
        {
            // load the appropriate XSLT template based on the transformName parameter
            var resourceName = $"IAT.Core.Xslt.{transformName}.xslt";
            using Stream xsltStream = typeof(ReportService).Assembly.GetManifestResourceStream(resourceName) 
                ?? throw new InvalidOperationException($"Could not find the specified XSLT resource: {resourceName}");

            var xsltInputStream = new DotNetInputStream(xsltStream);
            var output = new MemoryStream();
            var processor = new Processor();
            var compiler = processor.newXsltCompiler();
            var executable = compiler.compile(new StreamSource(xsltInputStream));
            var transform = executable.load();
            var serializer = processor.newSerializer(new DotNetOutputStream(output));
            serializer.setOutputProperty(Serializer.Property.METHOD, outputType);
            transform.setDestination(serializer);
            var input = model.ToXml();
            transform.setSource(new StreamSource(new DotNetInputStream(new MemoryStream(Encoding.Unicode.GetBytes(input)))));
            transform.transform();
            return output;
        }

    }
}
