using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Enumerations;


namespace IAT.Core.Models
{
    public interface ISurveyItemResponse
    {
        String Value { get; }
        bool IsAnswered { get; }
        bool IsBlank { get; }
        bool WasForceSubmitted { get; }
        void ReadXml(XmlReader reader);
        void WriteXml(XmlWriter writer);
    }
    internal class SurveyItemResponse
    {
        public String Value { get; set; }
        public AnswerState AnswerState { get; set; }
    }
}
