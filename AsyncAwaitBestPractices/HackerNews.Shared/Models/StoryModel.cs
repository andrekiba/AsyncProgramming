using System;
using Newtonsoft.Json;

namespace HackerNews.Models
{
    public class StoryModel
    {
        public StoryModel(long id, string by, long score, long time, string title, string url) =>
            (Id, Author, Score, CreatedAtUnixTime, Title, Url) = (id, by, score, time, title, url);

        public DateTimeOffset CreatedAtDateTimeOffset => UnixTimeStampToDateTimeOffset(CreatedAtUnixTime);

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("by")]
        public string Author { get; set; }

        [JsonProperty("score")]
        public long Score { get; set; }

        [JsonProperty("time")]
        public long CreatedAtUnixTime { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        static DateTimeOffset UnixTimeStampToDateTimeOffset(long unixTimeStamp)
        {
            var dateTimeOffset = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, default);
            return dateTimeOffset.AddSeconds(unixTimeStamp);
        }
    }
}
