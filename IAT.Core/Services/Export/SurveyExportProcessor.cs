using System.Text;
using System.Windows.Media;
using IAT.Core.ConfigFile;
using IAT.Core.Domain;
using System.IO;
using IAT.Core.Serializable;
using IAT.Core.Enumerations;

namespace IAT.Core.Services.Export;

/// <summary>
/// Maps designer surveys onto ConfigFile <c>Survey</c> elements the server XSLT can administer.
/// </summary>
public interface ISurveyExportProcessor
{
    /// <summary>
    /// Converts every survey on <paramref name="test"/> into ConfigFile surveys and stores them
    /// on <paramref name="exportContext"/>. Also sets before/after counts used by the config root.
    /// </summary>
    void ProcessSurveys(IatTest test, ExportContext exportContext);
}

/// <summary>
/// Builds caption, item, and image XML for each questionnaire. Element names follow
/// SurveyPage.xslt (Boolean, Likert, MultipleResponse, MultiBoolean, BoundedLength, …),
/// not the designer type names.
/// </summary>
public sealed class SurveyExportProcessor : ISurveyExportProcessor
{
    private readonly IProjectPackageService _packageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SurveyExportProcessor"/> class.
    /// </summary>
    /// <param name="packageService">The project package service.</param>
    /// <exception cref="ArgumentNullException">Thrown is packageService is null</exception>
    public SurveyExportProcessor(IProjectPackageService packageService)
    {
        _packageService = packageService ?? throw new ArgumentNullException(nameof(packageService));
    }

    /// <summary>
    /// Processes the surveys in the given <paramref name="test"/> and adds them to the <paramref name="exportContext"/>.
    /// </summary>
    /// <param name="test">The IAT test containing the surveys to process.</param>
    /// <param name="exportContext">The export context to which the processed surveys will be added.</param>
    public void ProcessSurveys(IatTest test, ExportContext exportContext)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(exportContext);

        var position = 0;
        foreach (var survey in test.Surveys)
        {
            ConfigFile.Survey s = MapSurvey(test, survey, position++);
            s.Contents.Where(si => si is SurveyImageItem).Cast<SurveyImageItem>().ToList().ForEach(si =>
            {
               
                var bytes = Convert.FromBase64String(si.ImageData);
                var mf = new ManifestFile()
                {
                    MimeType = MimeFromFileName(si.FileName),
                    Name = si.FileName,
                    Content = bytes,
                    ResourceType = ResourceType.SurveyImage,
                    ResourceId = exportContext.FileManifest.Files.Select(f => f.ResourceType != ResourceType.ErrorMark &&
                        f.ResourceType != ResourceType.KeyOutline).Count() + 1,
                    Size = bytes.Length
                };
                si.ResourceId = mf.ResourceId;
                exportContext.FileManifest.Files.Add(mf);
            });
            exportContext.Surveys.Add(s);
        }
    }

    /// <summary>
    /// Maps a single survey from the domain model to the configuration file model.
    /// </summary>
    /// <param name="test">The IAT test containing the survey.</param>
    /// <param name="source">The domain survey to map.</param>
    /// <param name="initialPosition">The initial position of the survey.</param>
    /// <returns>The mapped configuration file survey.</returns>
    private ConfigFile.Survey MapSurvey(IatTest test, Domain.Survey source, int initialPosition)
    {
        var fileBase = SanitizeFileName(source.Name);
        var mapped = new ConfigFile.Survey
        {
            TimeoutMillis = Math.Max(0, source.TimeoutSeconds) * 1000L,
            IATName = test.Name ?? string.Empty,
            ClientId = 0,
            SurveyName = string.IsNullOrWhiteSpace(source.Name) ? fileBase : source.Name,
            InitialPosition = initialPosition,
        };

        var itemNum = 1;
        var questionNum = 1;

        foreach (var item in source.Items)
        {
            switch (item)
            {
                case SurveyHeader header:
                    mapped.Contents.Add(MapCaption(header));
                    break;

                case SurveyImage image:
                    mapped.Contents.Add(MapImage(image, test));
                    itemNum++;
                    break;

                case SurveyInstruction instruction:
                    mapped.Contents.Add(MapInstruction(instruction, itemNum, source.AllQuestionsOptional));
                    itemNum++;
                    break;

                case SurveyQuestion question:
                    mapped.Contents.Add(MapQuestion(question, itemNum, questionNum, source.AllQuestionsOptional));
                    itemNum++;
                    questionNum++;
                    break;
            }
        }

        return mapped;
    }
    
    /// <summary>
    /// Maps a survey header to a survey caption.
    /// </summary>
    /// <param name="header">The survey header to map.</param>
    /// <returns>The mapped survey caption.</returns>
    private static SurveyCaption MapCaption(SurveyHeader header)
    {
        var style = header.Style ?? new SurveyHeaderStyle();
        var fontSize = style.FontSize > 0 ? style.FontSize : SurveyHeaderStyle.DefaultFontSize;
        var text = header.Text ?? string.Empty;
        var textWidth = (int)Math.Ceiling(Math.Max(text.Length, 1) * fontSize * 9.0 / 8.0);
        var lineHeight = (int)Math.Round(fontSize * 1.5);
        var separator = style.SeparatorWidth > 0 ? style.SeparatorWidth : SurveyHeaderStyle.DefaultSeparatorWidth;

        return new SurveyCaption
        {
            Text = text,
            TextWidth = textWidth.ToString(),
            BorderWidth = separator.ToString(),
            FontName = string.IsNullOrWhiteSpace(style.FontFamily) ? SurveyHeaderStyle.DefaultFontFamily : style.FontFamily,
            LineHeight = lineHeight.ToString(),
            FontSize = ((int)Math.Round(fontSize)).ToString(),
            FontColor = style.FontColor,
            BackColor = style.BackColor,
            BorderColor = style.SeparatorColor,
        };
    }

    /// <summary>
    /// Maps a survey image to a survey image item.
    /// </summary>
    /// <param name="image">The survey image to map.</param>
    /// <param name="test">The IAT test containing the image.</param>
    /// <returns>The mapped survey image item.</returns>
    private SurveyImageItem MapImage(SurveyImage image, IatTest test)
    {
        var bytes = image.ImageId == Guid.Empty ? [] : (_packageService.GetImageBytes(image.ImageId) ?? []);
        var stim = test.Stimuli.OfType<ImageStimulus>().FirstOrDefault(s => s.Id == image.ImageId);
        var fileName = stim?.FileName ?? string.Empty;

        return new SurveyImageItem
        {
            MimeType = MimeFromFileName(fileName),
            ImageData = bytes.Length == 0 ? string.Empty : Convert.ToBase64String(bytes),
            ResourceId = 0,
            Id = image.ImageId == Guid.Empty ? Guid.NewGuid().ToString("N") : image.ImageId.ToString("N")
        };
    }

    /// <summary>
    /// Maps a survey instruction to a survey item.
    /// </summary>
    /// <param name="instruction">The survey instruction to map.</param>
    /// <param name="itemNum">The item number of the survey item.</param>
    /// <param name="surveyOptional">A flag indicating whether the survey is optional.</param>
    /// <returns>The mapped survey item.</returns>
    private static ConfigFile.SurveyItem MapInstruction(SurveyInstruction instruction, int itemNum, bool surveyOptional)
    {
        var format = SurveyFormat.Default;
        return new ConfigFile.SurveyItem
        {
            Optional = true,
            ItemNum = itemNum,
            QuestionNum = 0,
            Text = instruction.Text ?? string.Empty,
            Response = new Instruction(),
            Format = format
        };
    }

    /// <summary>
    /// Maps a survey question to a survey item.
    /// </summary>
    /// <param name="question">The survey question to map.</param>
    /// <param name="itemNum">The item number of the survey item.</param>
    /// <param name="questionNum">The question number of the survey item.</param>
    /// <param name="surveyOptional">A flag indicating whether the survey is optional.</param>
    /// <returns>The mapped survey item.</returns>
    private static ConfigFile.SurveyItem MapQuestion(SurveyQuestion question, int itemNum, int questionNum, bool surveyOptional)
    {
        var optional = question.IsOptional ?? surveyOptional;
        var format = SurveyFormat.Default;
        var response = MapResponse(question.Response, format);

        return new ConfigFile.SurveyItem
        {
            Optional = optional,
            ItemNum = itemNum,
            QuestionNum = questionNum,
            Text = question.Text ?? string.Empty,
            Response = response,
            Format = format
        };
    }

    /// <summary>
    /// Maps a response definition to a configuration file response based on the survey format.
    /// </summary>
    /// <param name="definition">The response definition to map.</param>
    /// <param name="format">The survey format.</param>
    /// <returns>The mapped configuration file response.</returns>
    private static Response MapResponse(ResponseDefinition? definition, SurveyFormat format) => definition switch
    {
        TrueFalseResponse tf => new TrueFalse
        {
            Format = format,
            TrueStatement = string.IsNullOrWhiteSpace(tf.TrueStatement) ? "True" : tf.TrueStatement,
            FalseStatement = string.IsNullOrWhiteSpace(tf.FalseStatement) ? "False" : tf.FalseStatement
        },
        LikertResponse likert => MapLikert(likert, format),
        MultipleChoiceResponse mc => MapMultiChoice(mc, format),
        MultiSelectResponse ms => MapMultiSelect(ms, format),
        DateResponse date => MapDate(date, format),
        FixedDigitsResponse digits => new FixedDigit
        {
            Format = format,
            NumDigs = Math.Max(1, digits.DigitCount)
        },
        BoundedTextResponse text => new BoundedText
        {
            Format = format,
            MinLength = Math.Max(0, text.MinLength),
            MaxLength = Math.Max(text.MinLength, text.MaxLength)
        },
        BoundedNumberResponse number => new BoundedNumber
        {
            Format = format,
            MinValue = (decimal)(number.Min ?? 0),
            MaxValue = (decimal)(number.Max ?? 0)
        },
        RegexResponse regex => new RegEx
        {
            Format = format,
            RegularExpression = string.IsNullOrWhiteSpace(regex.Pattern) ? ".+" : regex.Pattern
        },
        _ => new Likert
        {
            Format = format,
            NumChoices = LikertResponse.DefaultLabels.Length,
            ReverseScored = false,
            Choices = LikertResponse.DefaultLabels.ToList()
        }
    };

    /// <summary>
    /// Maps a Likert response definition to a configuration file Likert response based on the survey format.
    /// </summary>
    /// <param name="source">The Likert response definition to map.</param>
    /// <param name="format">The survey format.</param>
    /// <returns>The mapped configuration file Likert response.</returns>
    private static Likert MapLikert(LikertResponse source, SurveyFormat format)
    {
        var labels = source.Labels.Count > 0
            ? source.Labels.ToList()
            : LikertResponse.DefaultLabels.ToList();

        var span = Math.Max(1, source.Max - source.Min + 1);
        if (labels.Count == 0)
        {
            for (var i = 0; i < span; i++)
                labels.Add((source.Min + i).ToString());
        }

        return new Likert
        {
            Format = format,
            NumChoices = labels.Count,
            ReverseScored = source.ReverseScored,
            Choices = labels
        };
    }

    /// <summary>
    /// Maps a multiple choice response definition to a configuration file multi-choice response based on the survey format.
    /// </summary>
    /// <param name="source">The multiple choice response definition to map.</param>
    /// <param name="format">The survey format.</param>
    /// <returns>The mapped configuration file multi-choice response.</returns>
    private static MultiChoice MapMultiChoice(MultipleChoiceResponse source, SurveyFormat format)
    {
        var choices = source.Choices.Count > 0 ? source.Choices.ToList() : new List<string> { "Option A", "Option B" };
        return new MultiChoice
        {
            Format = format,
            NumChoices = choices.Count,
            Choices = choices
        };
    }

    /// <summary>
    /// Maps a multi-select response definition to a configuration file multi-select response based on the survey format.
    /// </summary>
    /// <param name="source">The multi-select response definition to map.</param>
    /// <param name="format">The survey format.</param>
    /// <returns>The mapped configuration file multi-select response.</returns>
    private static MultiSelect MapMultiSelect(MultiSelectResponse source, SurveyFormat format)
    {
        var choices = source.Choices.Count > 0 ? source.Choices.ToList() : new List<string> { "Option A", "Option B" };
        var min = source.MinSelections ?? 0;
        var max = source.MaxSelections ?? choices.Count;
        return new MultiSelect
        {
            Format = format,
            NumValues = choices.Count,
            MinSelections = Math.Max(0, min),
            MaxSelections = Math.Max(min, max),
            Choices = choices
        };
    }

    /// <summary>
    /// Maps a date response definition to a configuration file date response based on the survey format.
    /// </summary>
    /// <param name="source">The date response definition to map.</param>
    /// <param name="format">The survey format.</param>
    /// <returns>The mapped configuration file date response.</returns>
    private static Date MapDate(DateResponse source, SurveyFormat format) => new()
    {
        Format = format,
        StartDate = source.MinDate ?? DateOnly.MinValue,
        EndDate = source.MaxDate ?? DateOnly.MaxValue
    };

    /// <summary>
    /// Determines the MIME type based on the file extension of the given file name.
    /// </summary>
    /// <param name="fileName">The name of the file.</param>
    /// <returns>The MIME type corresponding to the file extension.</returns>
    private static string MimeFromFileName(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "image/png"
        };
    }

    /// <summary>
    /// Sanitizes the given file name by removing invalid characters and replacing whitespace with underscores.
    /// </summary>
    /// <param name="name">The file name to sanitize.</param>
    /// <returns>The sanitized file name.</returns>
    private static string SanitizeFileName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Survey";

        var builder = new StringBuilder(name.Length);
        foreach (var ch in name.Trim())
        {
            if (char.IsLetterOrDigit(ch) || ch is '-' or '_')
                builder.Append(ch);
            else if (char.IsWhiteSpace(ch))
                builder.Append('_');
        }

        return builder.Length == 0 ? "Survey" : builder.ToString();
    }
}
