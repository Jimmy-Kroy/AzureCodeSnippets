using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace SemanticKernelApp.Plugins
{
    public class MusicConcertPlugin
    {
        [KernelFunction, Description("Get a list of upcoming concerts")]
        public static string GetTours()
        {
            string dir = Directory.GetCurrentDirectory();
            string content = File.ReadAllText($"{dir}/data/concertdates.json");
            return content;
        }
    }
}
