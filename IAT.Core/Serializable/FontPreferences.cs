using java.util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace IAT.Core.Serializable
{
    internal class FontPreferences
    {
            public static readonly FontPreferences Stimulus = new FontPreference(1, "Stimulus", DIText.UsedAs.Stimulus, 32);
            public static readonly FontPreferences ContinueInstructions = new FontPreference(2, "ContinueInstructions", DIText.UsedAs.ContinueInstructions, 16);
            public static readonly FontPreferences ResponseKey = new FontPreference(3, "ResponseKey", DIText.UsedAs.ResponseKey, 22);
            public static readonly FontPreferences Conjunction = new FontPreference(4, "Conjunction", DIText.UsedAs.Conjunction, 12);
            public static readonly FontPreferences IatBlockInstructions = new FontPreference(5, "IatBlockInstructions", DIText.UsedAs.IatBlockInstructions, 14);
            public static readonly FontPreferences TextInstructionsScreen = new FontPreference(6, "TextInstructionsScreen", DIText.UsedAs.TextInstructionsScreen, 18);

            private FontPreference(int value, String name, DIText.UsedAs usedAs, float fontSize) : base(value, name)
            {
                UsedAs = usedAs;
                FontSize = fontSize;
                FontColor = Color.White;
                FontFamily = System.Drawing.SystemFonts.DefaultFont.FontFamily.Name;
                LineSpacing = 1F;
                Justification = TextJustification.Center;
            }

            public static readonly IEnumerable<FontPreference> All = new FontPreference[] { Stimulus, ContinueInstructions, ResponseKey, IatBlockInstructions,
                    TextInstructionsScreen };
            public static FontPreference FromString(String name)
            {
                return All.Where(fp => fp.Name == name).FirstOrDefault();
            }
            public float FontSize { get; set; }
            public Color FontColor { get; set; }
            public String FontFamily { get; set; }
            public float LineSpacing { get; set; }
            public TextJustification Justification { get; set; }
            public DIText.UsedAs UsedAs { get; private set; }

            public void Save(XElement elem)
            {
                elem.Add(new XElement(Name, new XAttribute("for", UsedAs.Name), new XElement("FontSize", FontSize.ToString()),
                    new XElement("FontColor", FontColor.Name), new XElement("FontFamily", FontFamily), new XElement("LineSpacing", LineSpacing.ToString()),
                    new XElement("Justification", Justification.ToString())));
            }

            public void Load(XElement elem)
            {
                FontSize = Convert.ToSingle(elem.Element("FontSize").Value);
                FontColor = Color.FromName(elem.Element("FontColor").Value);
                FontFamily = elem.Element("FontFamily").Value;
                LineSpacing = Convert.ToSingle(elem.Element("LineSpacing").Value);
                Justification = TextJustification.FromString(elem.Element("Justification").Value);
                UsedAs = DIText.UsedAs.FromString(elem.Attribute("for").Value);
            }
        }
}
