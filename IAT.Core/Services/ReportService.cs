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
    /// <summary>
    /// Centeral service for all client-side XSLT transformations of reports. This is the only service that should be responsible for loading XSLT files, 
    /// and it should be the only service that performs transformations of XML data into HTML for display in the UI. This keeps all XSLT-related logic in 
    /// one place and makes it easier to maintain and update as needed.
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Generates an Excel report based on the provided data model.
        /// </summary>
        /// <remarks>The returned stream is positioned at the beginning. Ensure that the provided model
        /// contains all necessary data for the report; otherwise, the generated report may be incomplete or
        /// invalid.</remarks>
        /// <param name="model">An object containing the data to be included in the Excel report. The structure and required properties of
        /// the model depend on the report's format and content requirements.</param>
        /// <returns>A stream containing the generated Excel report. The caller is responsible for disposing of the stream when
        /// finished.</returns>
        public Task<Stream> GenerateExcelReportAsync(object model);

        /// <summary>
        /// Transforms the specified model using the given XSLT template and returns the result as a stream in the
        /// specified output format.
        /// </summary>
        /// <remarks>The caller is responsible for disposing the returned stream when it is no longer
        /// needed. The transformation behavior may vary depending on the provided template and output method.</remarks>
        /// <param name="model">The data model to be transformed. This object provides the input data for the XSLT transformation.</param>
        /// <param name="transformName">The name of the transformation to apply. Used to identify the specific transformation logic or
        /// configuration.</param>
        /// <param name="xsltTemplateName">The name of the XSLT template to use for the transformation. This template defines how the model is
        /// converted to the output format.</param>
        /// <param name="outputMethod">The output method to use for the transformation result. Supported values typically include "xml", "html", or
        /// "text". The default is "xml".</param>
        /// <returns>A stream containing the result of the transformation in the specified output format.</returns>
        public Task<Stream> TransformAsync(object model, String transformName, string outputMethod = "xml");
    }

    /// <summary>
    /// This service provides functionality for generating reports, including Excel reports and transforming data models into various output 
    /// formats using XSLT templates. It serves as a central point for all client-side XSLT transformations of reports, ensuring that all XSLT-related 
    /// logic is contained in one place for easier maintenance and updates. The service includes methods for generating Excel reports based on provided 
    /// data models and for transforming data models using specified XSLT templates, returning the results as streams in the desired output formats.
    /// </summary>
    public class ReportService : IReportService
    {
        /// <summary>
        /// Asynchronously generates an Excel report based on the provided data model.
        /// </summary>
        /// <param name="model">An object containing the data to be included in the report. The structure and required properties of the
        /// model depend on the specific report implementation.</param>
        /// <returns>A stream containing the generated Excel report. The caller is responsible for disposing the returned stream.</returns>
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
