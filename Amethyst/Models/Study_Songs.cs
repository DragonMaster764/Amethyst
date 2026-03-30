using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

[BsonIgnoreExtraElements] // Prevents crashes if the DB has extra fields not listed here
public class Study_Songs
{

    [BsonId]
    [BsonRepresentation(BsonType.String)] // Allows you to use a string ID in C# for "song001"
    public string Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; }

    [BsonElement("artist")]
    public string Artist { get; set; }

    [BsonElement("genre")]
    public string Genre { get; set; }

    [BsonElement("mood_tags")]
    public List<string> MoodTags { get; set; } = new();

    [BsonElement("energy_level")]
    public string EnergyLevel { get; set; }

    [BsonElement("best_for")]
    public List<string> BestFor { get; set; } = new();

    [BsonElement("time_of_day")]
    public List<string> TimeOfDay { get; set; } = new();

    [BsonElement("instrumental")]
    public bool IsInstrumental { get; set; }

    [BsonElement("duration")]
    public string Duration { get; set; }

    [BsonElement("spotify_search_url")]
    public string SpotifySearchUrl { get; set; }

    [BsonElement("apple_music_search_url")]
    public string AppleMusicSearchUrl { get; set; }

    [BsonElement("youtube_search_url")]
    public string YoutubeSearchUrl { get; set; }

    [BsonElement("description")]
    public string Description { get; set; }

    [BsonElement("link_type")]
    public string LinkType { get; set; }
}