﻿using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.SemanticKernel;

namespace SemanticKernelApp.Plugins
{
    public class MusicLibraryPlugin
    {
        [KernelFunction, Description("Get a list of music recently played by the user")]
        public static string GetRecentPlays()
        {
            string dir = Directory.GetCurrentDirectory();
            string content = File.ReadAllText($"{dir}/data/recentlyplayed.json");
            Debug.WriteLine("GetRecentPlays: " + content);
            return content;
        }

        [KernelFunction, Description("Get a list of all music available to the user")]
        public static string GetMusicLibrary()
        {
            string dir = Directory.GetCurrentDirectory();
            string content = File.ReadAllText($"{dir}/data/musiclibrary.json");
            Debug.WriteLine("GetMusicLibrary: " + content );
            return content;
        }

        [KernelFunction, Description("Get a list of recently played songs")]
        public static string GetRecentlyPlayedSongs()
        {
            string dir = Directory.GetCurrentDirectory();
            string content = File.ReadAllText($"{dir}/data/recentlyplayed.json");
            Debug.WriteLine("GetRecentlyPlayedSongs: " + content);
            return content;
        }

        [KernelFunction, Description("Add a song to the recently played list")]
        public static string AddToRecentlyPlayed(
            [Description("The name of the artist")] string artist,
            [Description("The title of the song")] string song,
            [Description("The song genre")] string genre)
        {
            Debug.WriteLine("AddToRecentlyPlayed: " + artist + ", " + song + ", " + genre);
            // Read the existing content from the file
            string filePath = "data/recentlyplayed.json";
            string jsonContent = File.ReadAllText(filePath);

#pragma warning disable CS8600
            var recentlyPlayed = (JsonArray)JsonNode.Parse(jsonContent);
            var newSong = new JsonObject
            {
                ["title"] = song,
                ["artist"] = artist,
                ["genre"] = genre
            };

#pragma warning disable CS8602
            recentlyPlayed.Insert(0, newSong);
            File.WriteAllText(filePath,
                JsonSerializer.Serialize(recentlyPlayed,
                    new JsonSerializerOptions { WriteIndented = true }));

            return $"Added '{song}' to recently played";
        }
    }
}
