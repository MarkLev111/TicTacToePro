using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TicTacToePro
{
    public class GitHub
    {
        [JsonPropertyName("tag_name")]
        public string latestVersion { get; set; }

        [JsonPropertyName("html_url")]
        public string latestUrl { get; set; }

        public GitHub() { }
    }
}
