using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QFighterPolice.Models;

namespace QFighterPolice.Functions
{
    public static class ConfigManager
    {
        public static JObject GetConfig()
            => (JObject)JsonConvert.DeserializeObject(File.ReadAllText("Assets/config.json"));

        public static List<Question> GetQuestions()
            => JsonConvert.DeserializeObject<List<Question>>(File.ReadAllText("Assets/questions.json"));
    }
}
