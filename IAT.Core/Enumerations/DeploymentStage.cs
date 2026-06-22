using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace IAT.Core.Enumerations
{
    enum DeploymentStage {


        [Description("Preparing Deployment Resources")]
        compilingXSLT,

        [Description("Encrypting Code")] 
        encryptingCode
    }

}
