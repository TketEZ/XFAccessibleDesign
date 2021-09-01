using System;
namespace PerformanceAI.Helpers
{
    //https://docs.microsoft.com/en-us/xamarin/xamarin-forms/data-cloud/azure-cognitive-services/speech-recognition
    public class SpeechServiceConstants
    {
        // API key can be a shared, multi-resource key or an individual service key
        // and can be found and regenerated in the Azure portal
        public static string CognitiveServicesApiKey = "9d5228308f65468c9bd90905464fd4e9";

        // Endpoint is based on your configured region, for example "westus"
        public static string CognitiveServicesRegion = "uksouth";
    }
}
