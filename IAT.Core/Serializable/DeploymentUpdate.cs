using java.rmi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IAT.Core.Serializable
{
    [XmlRoot(ElementName = "DeploymentStatusUpdate")]
    public class DeploymentUpdate
    {

            public enum Stage
            {
                unset, compilingXSLT, encryptingCode, mungingCode, processingFileManifest, creatingBackup, restoringBackup,
                initializingDeployment, finalizingDeployment, generatingIATHTML, generatingSurveyHTML, generatingIATDescriptor, generatingSurveyDescriptor,
                generatingIATScript, generatingSurveyScript, generatingIATHeaderScript, generatingSurveyHeaderScript, processingUniqueSurveyResponses, recordingProcessedJS,
                generatingAES, timerExpired, comparingDescriptors, xsltFailure, generatingIAT, generatingSurvey, backingUpIAT, mismatchedDeploymentDescriptors, success, failed
            };

            [XmlIgnore]
            private static String[] StageMessages =
                { String.Empty, Properties.Resources.sPreparingDeploymentResources, Properties.Resources.sEncryptingCode, Properties.Resources.sMungingCode,
                Properties.Resources.sProcessingFileManifest, Properties.Resources.sCreatingBackup,
                Properties.Resources.sRestoringBackup, Properties.Resources.sInitializingDeployment, Properties.Resources.sFinalizingDeployment, Properties.Resources.sGeneratingIATHTML,
                Properties.Resources.sGeneratingSurveyHTML, Properties.Resources.sGeneratingIATDescriptor, Properties.Resources.sGeneratingSurveyDescriptor,
                Properties.Resources.sGeneratingIATScript, Properties.Resources.sGeneratingSurveyScript, Properties.Resources.sGeneratingIATHeaderScript,
                Properties.Resources.sGeneratingSurveyHeaderScript, Properties.Resources.sProcessingUniqueSurveyResponses, Properties.Resources.sRecordingProcessedJS,
                Properties.Resources.sProcessingAES, Properties.Resources.sDeploymentTimerExpired, Properties.Resources.sDeploymentComparingDescriptors,
                Properties.Resources.sDeploymentXsltFailure, Properties.Resources.sDeploymentGeneratingIAT, Properties.Resources.sDeploymentGeneratingSurvey,
                Properties.Resources.sBackingUpIAT, String.Empty, String.Empty, String.Empty
            };

            private static Dictionary<Stage, String> StateMessageDictionary = new Dictionary<Stage, String>();

            static DeploymentProgressUpdate()
            {
                Array stageArray = Enum.GetValues(typeof(Stage));
                for (int ctr = 0; ctr < stageArray.Length; ctr++)
                    StateMessageDictionary[(Stage)stageArray.GetValue(ctr)] = StageMessages[ctr];
            }

            private EStage _Stage;
            private String _ActiveItem;
            private int _ProgressMin, _ProgressMax, _CurrentProgress;
            private bool _IsLastUpdate = false;
            private CServerException ex = null;

            public CServerException DeploymentException
            {
                get
                {
                    return ex;
                }
            }

            public bool IsLastUpdate
            {
                get
                {
                    return _IsLastUpdate;
                }
            }

            public String StatusMessage
            {
                get
                {
                    if (ActiveItem != String.Empty)
                        return String.Format(StateMessageDictionary[Stage], ActiveItem);
                    return StateMessageDictionary[Stage];
                }
            }

            public EStage Stage
            {
                get
                {
                    return _Stage;
                }
            }

            public String ActiveItem
            {
                get
                {
                    return _ActiveItem;
                }
            }

            public int ProgressMin
            {
                get
                {
                    return _ProgressMin;
                }
            }

            public int ProgressMax
            {
                get
                {
                    return _ProgressMax;
                }
            }

            public int CurrentProgress
            {
                get
                {
                    return _CurrentProgress;
                }
            }

            public DeploymentProgressUpdate()
            {
                _Stage = EStage.unset;
                _ActiveItem = String.Empty;
                _ProgressMin = -1;
                _ProgressMax = -1;
                _CurrentProgress = -1;
            }
        }
}
