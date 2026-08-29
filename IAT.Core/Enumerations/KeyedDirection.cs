using System;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.ComponentModel;

namespace IAT.Core.Enumerations;

/// <summary>
/// Represents the direction in which a stimulus has been keyed in an Implicit Association Test (IAT). This enumeration is used to indicate whether a stimulus has been keyed to the left, right, or not keyed at all. It provides a clear and structured way to represent the response direction for stimuli during the test.
/// </summary>
public enum KeyedDirection
{
    [Description("Indicates the stimulus is keyed left")]
    Left,

    [Description("Indicates the stimulus is keyed right")]
    Right,

    [Description("Indicates the stimulus has not been keyed")]
    None

}
