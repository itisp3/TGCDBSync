using System;
using System.Text.Json.Serialization;

public class BanListJSONResponse
{
    public class BanListResponse
    {
        [JsonPropertyName("banList")]
        public List<PlayerRecord> BanList { get; set; }

        [JsonPropertyName("notInterested")]
        public List<PlayerRecord> NotInterested { get; set; }
    }

    public class PlayerRecord
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("player_name")]
        public string PlayerName { get; set; }
    }
}
