using Newtonsoft.Json;


namespace DrumBeatDesigner.Models
{
    public static class ProjectSerializer
    {
        public static string Serialize(Project project)
        {
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All
            };

            return JsonConvert.SerializeObject(project, Formatting.Indented, settings);
        }

        public static Project Deserialize(string projectJson)
        {
            return JsonConvert.DeserializeObject<Project>(projectJson);
        }
    }
}