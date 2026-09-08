using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Text;

namespace IAT.Core.Extensions;

public static class NetExtensions
{
    public static string Red(this Color c) => c.R.ToString("X2"); 

    public static string Green(this Color c) => c.G.ToString("X2");

    public static string Blue(this Color c) => c.B.ToString("X2");
}
