using java.util;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// Represents a field definition used to identify and describe data elements within the data source.
    /// </summary>
    /// <param name="name">The name of the field. This value is used to match and retrieve the corresponding field definition. The
    /// comparison is case-insensitive.</param>
    /// <param name="value">A value indicating whether the field content is encrypted. Set to <see langword="true"/> if the field requires
    /// encryption; otherwise, <see langword="false"/>.</param>
    public abstract record Field(string name, bool value)
    {
        /// <summary>
        /// Represents the field that identifies the product key in the data source.
        /// </summary>
        public static readonly Field ProductKey = new _ProductKey("IATProductCode", false);

        /// <summary>
        /// Represents the field that identifies the version of the entity or record.
        /// </summary>
        public static Field Version = new _Version("Version", false);

        /// <summary>
        /// Represents the field definition for the user's email address.
        /// </summary>
        public static Field UserEmail = new _UserEmail("UserEMail", false);

        /// <summary>
        /// Represents the field used to store the activation key for the application.
        /// </summary>
        /// <remarks>This field is typically used to identify or validate the activation status of the
        /// application. The value is intended to be unique and should be handled securely to prevent unauthorized
        /// access.</remarks>
        public static Field ActivationKey = new _ActivationKey("IATActivationKey", true);

        /// <summary>
        /// Represents the field that stores the user name associated with the client.
        /// </summary>
        public static Field UserName = new _UserName("ClientName", false);

        /// <summary>
        /// Gets a value indicating whether the content is encrypted.
        /// </summary>
        public bool Encrypted { get; private set; }

        /// <summary>
        /// Returns the corresponding field for the specified string name.
        /// </summary>
        /// <remarks>Valid field names include "iatproductcode", "version", "useremail",
        /// "iatactivationkey", "clientname", and "version_1_1_confirmed". The comparison ignores case.</remarks>
        /// <param name="name">The name of the field to retrieve. The comparison is case-insensitive.</param>
        /// <returns>The field that matches the specified name.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified name does not correspond to a known field.</exception>
        public Field FromString(String name)
        {
            return name.ToLowerInvariant() switch
            {
                "iatproductcode" => ProductKey,
                "version" => Version,
                "useremail" => UserEmail,
                "iatactivationkey" => ActivationKey,
                "clientname" => UserName,
                _ => throw new ArgumentException($"Unknown field name: {name}")
            };
        }

        private record _ProductKey(string name, bool encrypted) : Field(name, encrypted);
        private record _Version(string name, bool encrypted) : Field(name, encrypted);
        private record _UserEmail(string name, bool encrypted) : Field(name, encrypted);
        private record _ActivationKey(string name, bool encrypted) : Field(name, encrypted);
        private record _UserName(string name, bool encrypted) : Field(name, encrypted);

    }
}