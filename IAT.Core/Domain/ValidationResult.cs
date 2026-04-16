using com.sun.corba.se.impl.protocol.giopmsgheaders;

namespace IAT.Core.Domain
{
    /// <summary>
    /// The result of validating a domain entity, such as a stimulus or trial. This class encapsulates whether the validation was successful 
    /// and, if not, provides an error message describing the reason for failure. It is used to communicate validation results in a consistent 
    /// way across the application, allowing calling code to easily check if an entity is valid and to display appropriate error messages when 
    /// it is not.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Indicates whether the validated item is valid
        /// </summary>
        public bool IsValid { get;  set; } = true;

        /// <summary>
        /// The error message supplied in the event the  item is not valid.
        /// </summary>
        public List<string> ErrorMessages { get; private set; } = new();

        /// <summary>
        /// Initializes a new instance of the ValidationResult structure with the specified validity state and error
        /// message.
        /// </summary>
        /// <param name="isValid">A value indicating whether the validation was successful. Set to <see langword="true"/> if the validation
        /// passed; otherwise, <see langword="false"/>.</param>
        /// <param name="error">The error message associated with the validation result, or <see langword="null"/> if there is no error.</param>
        public ValidationResult()
        {
            IsValid = true;
            ErrorMessages = new List<string>();
        }


        /// <summary>
        /// Gets a ValidationResult that indicates a successful validation with no error message.
        /// </summary>
        public void Success()
        {
        }


        /// <summary>
        /// Logs a validation failure by setting the IsValid property to false and adding the provided error message to the ErrorMessages list. This method can be called when a 
        /// validation check fails to record the reason for the failure and to indicate that the validated item is not valid.      
        /// </summary>
        /// <param name="message">The error message describing the reason for the validation failure.</param>
        public void Fail(string message)
        {
            ErrorMessages.Add(message);
            IsValid = false;
        }
    }
}