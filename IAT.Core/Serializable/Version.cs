using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Xml.Schema;    

namespace IAT.Core.Serializable
{
    /// <summary>
    /// Represents a software version number consisting of release, major, minor, and trivial components.
    /// </summary>
    /// <remarks>The Version class provides functionality to parse, compare, and represent version numbers in
    /// the format 'Release.Major.Minor.Trivial'. Instances of this class are immutable after construction. Use the
    /// Compare or CompareTo methods to determine the ordering of two version instances.</remarks>
    public sealed class Version
    {
        /// <summary>
        /// Gets the release number associated with the current instance.
        /// </summary>
        [XmlElement("Release", Form = XmlSchemaForm.Unqualified)]
        public int Release { get; private set; }

        /// <summary>
        /// Gets the major version number of the object.
        /// </summary>
        [XmlElement("Major", Form = XmlSchemaForm.Unqualified)] 
        public int Major { get; private set; }

        /// <summary>
        /// Gets the minor version number component.
        /// </summary>
        [XmlElement("Minor", Form = XmlSchemaForm.Unqualified)] 
        public int Minor { get; private set; }

        /// <summary>
        /// Gets the trivial value associated with this instance.
        /// </summary>
        [XmlElement("Trivial", Form = XmlSchemaForm.Unqualified)]
        public int Trivial { get; private set; }

        /// <summary>
        /// Parses a version string in the format 'X.Y.Z.W' into a Version object.
        /// </summary>
        /// <remarks>If the input string does not match the expected format, the method may throw an
        /// exception when attempting to convert the matched groups to integers.</remarks>
        /// <param name="str">A string representing the version to parse. The string must be in the format 'X.Y.Z.W', where X, Y, Z, and W
        /// are non-negative integers.</param>
        /// <returns>A Version object representing the parsed version components from the input string.</returns>
        static public Version ParseVersion(String str)
        {
            Regex regEx = new Regex("([0-9]+)\\.([0-9]+)\\.([0-9]+)\\.([0-9]+)");
            Match match = regEx.Match(str);
            return new Version() {
                Release = Convert.ToInt32(match.Groups[1].Value),
                Major = Convert.ToInt32(match.Groups[2].Value),
                Minor = Convert.ToInt32(match.Groups[3].Value),
                Trivial = Convert.ToInt32(match.Groups[4].Value)
            };
        }

        /// <summary>
        /// Initializes a new instance of the Version class using the specified version string.
        /// </summary>
        /// <remarks>The version string is typically in the format 'major.minor[.build[.revision]]'. If
        /// the string is not in a valid format, an exception may be thrown.</remarks>
        public Version() {}
    }
}
