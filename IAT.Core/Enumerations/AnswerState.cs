using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// Represents the state of an answer in a question-and-answer workflow, such as whether an answer has been
    /// provided, is missing, or was force submitted.
    /// </summary>
    /// <remarks>Use the provided static instances, such as <see cref="Answered"/>, <see cref="Unanswered"/>,
    /// and <see cref="ForceSubmitted"/>, to represent common answer states. This type is intended to be used as a base
    /// for specific answer state records.</remarks>
    /// <param name="Name">The display name of the answer state. This value identifies the state in a human-readable format.</param>
    /// <param name="Value">The internal value associated with the answer state. This value is used for programmatic identification or
    /// serialization.</param>
    public abstract record AnswerState(string Name, string Value) 
    { 
        /// <summary>
        /// Represents the state indicating that an answer has been provided.
        /// </summary>
        public static readonly AnswerState Answered = new _Answered();

        /// <summary>
        /// Represents the state of an answer that has not yet been provided.
        /// </summary>
        public static readonly AnswerState Unanswered = new _Unanswered();

        /// <summary>
        /// Represents an answer state indicating that the answer was forcefully submitted, regardless of its completion
        /// status.
        /// </summary>
        /// <remarks>Use this value to identify answers that were submitted automatically, such as when a
        /// time limit expires or a submission is required by the system.</remarks>
        public static readonly AnswerState ForceSubmitted = new _ForceSubmitted();

        /// <summary>
        /// Represents the state indicating that an answer has been provided.
        /// </summary>
        /// <remarks>This type is used to identify when an answer has been recorded or acknowledged. It is
        /// a sealed record and cannot be inherited.</remarks>
        private sealed record _Answered() : AnswerState("Answered", "__Answered__") { }

        /// <summary>
        /// Represents an answer state indicating that no answer has been provided.
        /// </summary>
        /// <remarks>This type is used to distinguish unanswered states from other answer states within
        /// the system. It is a sealed record and cannot be inherited.</remarks>
        private sealed record _Unanswered() : AnswerState("Unanswered", "__Unanswered__") { }

        /// <summary>
        /// Represents an answer state indicating that the answer was forcefully submitted, regardless of user action.
        /// </summary>
        private sealed record _ForceSubmitted() : AnswerState("ForceSubmitted", "__ForceSubmitted__") { }
    }
}
