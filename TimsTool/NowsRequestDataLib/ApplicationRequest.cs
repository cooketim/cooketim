namespace NowsRequestDataLib
{
    public class ApplicationRequest
    {
        public ApplicationRequest(bool isLinkedApplication, bool isLiveProceedings, bool isHeardWithOtherCases, bool isResentenceOrActivation, bool withDebug)
        {
            IsLinkedApplication = isLinkedApplication;
            IsLiveProceedings = isLiveProceedings;
            IsHeardWithOtherCases = isHeardWithOtherCases;
            IsResentenceOrActivation = isResentenceOrActivation;
            WithDebug = withDebug;
        }

        /// <summary>
        /// Linked or standalone application
        /// </summary>
        public bool IsLinkedApplication { get; set; }

        /// <summary>
        /// For a linked application, live or continued proceedings
        /// </summary>
        public bool IsLiveProceedings { get; set; }

        /// <summary>
        /// For a linked application, heard with additional cases or not?
        /// </summary>
        public bool IsHeardWithOtherCases { get; set; }

        /// <summary>
        /// For a linked application in continued proceedings, cloned results sentenced or application sentence results?
        /// </summary>
        public bool IsResentenceOrActivation { get; set; }

        /// <summary>
        /// Debug Requirements
        /// </summary>
        public bool WithDebug { get; set; }
    }
}
