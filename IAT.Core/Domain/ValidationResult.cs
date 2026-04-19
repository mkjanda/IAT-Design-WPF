using com.sun.corba.se.impl.protocol.giopmsgheaders;

namespace IAT.Core.Domain
{
    /// <summary>
    /// The result of validating a domain entity, such as a stimulus, trial, or entire test. This class accumulates multiple error messages 
    /// during validation and provides a way to check overall validity. It supports aggregating results from sub-validations (e.g., validating 
    /// all stimuli in a test) and is designed for UI display, such as in error banners or dialogs.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Indicates whether the validated item is valid (no errors).
        /// </summary>
        public bool IsValid => Errors.Count == 0;

        /// <summary>
        /// The list of error messages. Empty if valid.
        /// </summary>
        public List<string> Errors { get; } = new();

        /// <summary>
        /// Gets a ValidationResult that indicates a successful validation with no errors.
        /// </summary>
        public static ValidationResult Success => new();

        /// <summary>
        /// Creates a failed validation result with a single error message.
        /// </summary>
        /// <param name="message">The error message. Cannot be null or empty.</param>
        /// <returns>A ValidationResult with the error added.</returns>
        public static ValidationResult Fail(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Error message cannot be null or empty.", nameof(message));
            var result = new ValidationResult();
            result.Errors.Add(message);
            return result;
        }

        /// <summary>
        /// Adds an error message to this result.
        /// </summary>
        /// <param name="message">The error message to add. Cannot be null or empty.</param>
        public void AddError(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Error message cannot be null or empty.", nameof(message));
            Errors.Add(message);
        }

        /// <summary>
        /// Combines another ValidationResult into this one by adding its errors.
        /// </summary>
        /// <param name="other">The other ValidationResult to combine.</param>
        public void Combine(ValidationResult other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            Errors.AddRange(other.Errors);
        }

        /// <summary>
        /// Creates a combined ValidationResult from multiple results.
        /// </summary>
        /// <param name="results">The ValidationResults to combine.</param>
        /// <returns>A new ValidationResult containing all errors from the inputs.</returns>
        public static ValidationResult Combine(IEnumerable<ValidationResult> results)
        {
            var combined = new ValidationResult();
            foreach (var result in results)
            {
                combined.Combine(result);
            }
            return combined;
        }
    }
}