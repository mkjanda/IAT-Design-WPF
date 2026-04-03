using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
;

namespace IAT.Core.Models.Serializable
{
    /// <summary>
    /// Represents a software version number consisting of release, major, minor, and trivial components.
    /// </summary>
    /// <remarks>The Version class provides functionality to parse, compare, and represent version numbers in
    /// the format 'Release.Major.Minor.Trivial'. Instances of this class are immutable after construction. Use the
    /// Compare or CompareTo methods to determine the ordering of two version instances.</remarks>
    public sealed class Version
    {
        private int Release, Major, Minor, Trivial;

        private void ParseVersion(String str)
        {
            Regex regEx = new Regex("([0-9]+)\\.([0-9]+)\\.([0-9]+)\\.([0-9]+)");
            Match match = regEx.Match(str);
            Release = Convert.ToInt32(match.Groups[1].Value);
            Major = Convert.ToInt32(match.Groups[2].Value);
            Minor = Convert.ToInt32(match.Groups[3].Value);
            Trivial = Convert.ToInt32(match.Groups[4].Value);
        }

        public Version(String version)
        {
            ParseVersion(version);
        }

        static public int Compare(Version v1, Version v2)
        {
            if (v1.Release != v2.Release)
                return v2.Release - v1.Release;
            if (v1.Major != v2.Major)
                return v2.Major - v1.Major;
            if (v1.Minor != v2.Minor)
                return v2.Minor - v1.Minor;
            if (v1.Trivial != v2.Trivial)
                return v2.Trivial - v1.Trivial;
            return 0;
        }

        public int CompareTo(Version v)
        {
            if (Release != v.Release)
                return Release - v.Release;
            if (Major != v.Major)
                return Major - v.Major;
            if (Minor != v.Minor)
                return Minor - v.Minor;
            if (Trivial != v.Trivial)
                return Trivial - v.Trivial;
            return 0;
        }

        public override string ToString()
        {
            return String.Format("{0}.{1}.{2}.{3}", Release, Major, Minor, Trivial);
        }
    }
}
