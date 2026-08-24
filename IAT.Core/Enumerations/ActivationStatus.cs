using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// Represents the activation status of a user or entity within the system, indicating whether they are activated, not activated, have an unverified email, or have a version inconsistency.
    /// </summary>
    public enum ActivationStatus
    {
        /// <summary>
        /// The user or entity has not been activated.
        /// </summary>
        [Description("The user or entity has not been activated.")]
        NotActivated,
        /// <summary>
        /// The user's email has not been verified.
        /// </summary>
        [Description("The user's email has not been verified.")]
        EMailNotVerified,
        /// <summary>
        /// The user or entity is activated.
        /// </summary>
        [Description("The user or entity is activated.")]
        Activated,
        /// <summary>
        /// There is a version inconsistency.
        /// </summary>
        [Description("There is a version inconsistency.")]
        InconsistentVersion

    }
}
