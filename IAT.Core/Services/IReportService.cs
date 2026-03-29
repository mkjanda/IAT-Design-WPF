using System;
using System.Collections.Generic;
using System.Text;

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
}
