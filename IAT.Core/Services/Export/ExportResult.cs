using IAT.Core.ConfigFile;
using IAT.Core.Serializable;

namespace IAT.Core.Services.Export
{
    public class ExportResult
    {
        /// <summary>
        /// The configuration data for the exported test, represented as an IATConfigFile. This object contains all the necessary 
        /// information about the test's structure, stimuli, and settings that are required for the IAT software to execute the 
        /// test correctly. It is a key component of the export result, as it defines how the test will be presented and function 
        /// when imported into the IAT software.
        /// </summary>
        public IATConfigFile ConfigFile { get; init; }

        /// <summary>
        /// The file manifest for the exported test, represented as a Manifest object. This object contains information about all 
        /// the files included in the export, such as images, audio, and other resources. It ensures that all necessary files are 
        /// accounted for and can be correctly referenced by the IAT software during test execution.
        /// </summary>
        public Manifest FileManifest { get; init; }

        /// <summary>
        /// The file maniifest for the item slides, represented as a Manifest object. This manifest specifically tracks the slide 
        /// images that are generated for the test's stimuli.
        /// </summary>
        public Manifest SlideManifest { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportResult"/> class with the specified configuration file and
        /// file manifest.
        /// </summary>
        /// <param name="configFile">The configuration file for the export operation.</param>
        /// <param name="fileManifest">The manifest containing the exported files.</param>
        /// <param name="slideManifest">The manifest containing the exported slide images.</param>
        /// <exception cref="ArgumentNullException"><paramref name="configFile"/>, <paramref name="fileManifest"/>, or <paramref name="slideManifest    "/> is <see langword="null"/>.</exception>
        public ExportResult(IATConfigFile configFile, Manifest fileManifest, Manifest slideManifest)
        {
            ConfigFile = configFile ?? throw new ArgumentNullException(nameof(configFile));
            FileManifest = fileManifest ?? throw new ArgumentNullException(nameof(fileManifest));
            SlideManifest = slideManifest ?? throw new ArgumentNullException(nameof(slideManifest));
        }
    }
}
