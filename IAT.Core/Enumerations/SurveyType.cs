using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// Represents a type of survey, characterized by a name and description, used to distinguish when a survey is
    /// administered relative to a specific event or action.
    /// </summary>
    /// <remarks>Use the predefined instances <see cref="Before"/> and <see cref="After"/> to represent common
    /// survey types administered before or after an event. Additional survey types can be defined by extending this
    /// record if needed.</remarks>
    /// <param name="Name">The name that identifies the survey type. This value is used to distinguish between different survey types, such
    /// as "Before" or "After".</param>
    /// <param name="Description">A description that explains the purpose or context of the survey type.</param>
    public abstract record SurveyType(string Name, string Description)
    {
        /// <summary>
        /// Represents a survey type that occurs before a specified event or action.
        /// </summary>
        public static readonly SurveyType Before = new _Before();

        /// <summary>
        /// Represents a survey type that is conducted after a specified event or action.
        /// </summary>
        public static readonly SurveyType After = new _After();

        /// <summary>
        /// Given the name of a survey type, returns the corresponding SurveyType instance.
        /// </summary>
        /// <param name="name">The name of the survey type</param>
        /// <returns>The object that represents the survey type.</returns>
        /// <exception cref="ArgumentException">Thrown if the name is not valid</exception>
        static public SurveyType FromName(String name) =>
            name.ToLowerInvariant() switch
            {
                "before" => Before,
                "after" => After,
                _ => throw new ArgumentException($"Unknown SurveyType name: {name}")
            };

        /// <summary>
        /// Represents a survey type that is administered before the IAT to collect baseline information or
        /// demographics.
        /// </summary>
        private sealed record _Before() : SurveyType("Before", "A survey administered before the IAT to collect baseline information or demographics.");

        /// <summary>
        /// Represents a survey type that is administered after the IAT to collect feedback, debriefing information, or
        /// additional responses.
        /// </summary>
        private sealed record _After() : SurveyType("After", "A survey administered after the IAT to collect feedback, debriefing information, or additional responses.");
    }
}
