﻿using Maxstupo.CommandBuilder;
using Maxstupo.YdlUi.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Maxstupo.YdlUi.YoutubeDL {
    public class YoutubeDLApi : CommandLineBuilder<YoutubeDLArguments> {

        public override string Version => "2018.09.08";

        public YoutubeDLApi(string executable, YoutubeDLArguments arguments = null) : base(executable, arguments) {

            ValueTranslators.Add(typeof(DateTime), (a, t, v) => {
                return ((DateTime)v).ToString("yyyyMMdd");
            });

            ValueTranslators.Add(typeof(ByteSize), (a, t, v) => {
                return ((ByteSize)v).Bytes.ToString("#.#");
            });

            AppendCheckers.Add(typeof(Dictionary<string, string>), (a, t, v) => {
                return ((Dictionary<string, string>)v).Count > 0;
            });

        }

        protected override bool DoExecute(string arguments, string workingDirectory, ProcessCallback callback) {
            if (string.IsNullOrWhiteSpace(Executable))
                return false;

            StringBuilder sb = new StringBuilder();

            bool disabledData = false;

            Process proc = new Process();
            proc.StartInfo.FileName = Executable;

            if (!string.IsNullOrWhiteSpace(arguments))
                proc.StartInfo.Arguments = arguments;

            proc.StartInfo.CreateNoWindow = callback != null;
            proc.StartInfo.Verb = "runas";
            
            proc.StartInfo.RedirectStandardOutput = callback != null;
            proc.StartInfo.RedirectStandardInput = callback != null;
            proc.StartInfo.RedirectStandardError = callback != null;

            proc.StartInfo.UseShellExecute = callback == null;
            if (!string.IsNullOrWhiteSpace(workingDirectory))
                proc.StartInfo.WorkingDirectory = workingDirectory;

            proc.EnableRaisingEvents = callback != null;

            if (callback != null) {
                proc.OutputDataReceived += (sender, e) => {
                    if (string.IsNullOrEmpty(e.Data))
                        return;

                    sb.Append(e.Data).Append('\n');

                    if (!disabledData && callback(e.Data, ProcType.DATA_RECEIVED))
                        disabledData = true;
                };

                proc.Exited += (sender, e) => {

                    proc.Dispose();

                    if (sb.Length > 0)
                        sb.Remove(sb.Length - 1, 1);

                    callback(sb.ToString(), ProcType.EXITED);
                };

                proc.ErrorDataReceived += (sender, e) => {
                    callback(e.Data, ProcType.ERROR);
                };

            }
            proc.Start();
            if (callback != null) {
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
            }
            return true;
        }

        public override bool IsCompatibleVersion(string version) {
            return this.Version == version;
        }

        public static string KeywordRegex { get { return @"%\(\w+\)(?:s{1}|(\d+)d)"; } }
        public static string KeywordTemplate { get { return "%({keyword})s"; } }

        public static Keyword[] GetKeywords() {
            return new Keyword[] {
                    new Keyword("id", false, "Video identifier"),
                    new Keyword("title", false, "Video title"),
                    new Keyword("url", false, "Video URL"),
                    new Keyword("ext", false, "Video filename extension"),
                    new Keyword("alt_title", false, "A secondary title of the video"),
                    new Keyword("display_id", false, "An alternative identifier for the video"),
                    new Keyword("uploader", false, "Full name of the video uploader"),
                    new Keyword("license", false, "License name the video is licensed under"),
                    new Keyword("creator", false, "The creator of the video"),
                    new Keyword("release_date", false, "The date (YYYYMMDD) when the video was released"),
                    new Keyword("timestamp", true, "UNIX timestamp of the moment the video became available"),
                    new Keyword("upload_date", false, "Video upload date (YYYYMMDD)"),
                    new Keyword("uploader_id", false, "Nickname or id of the video uploader"),
                    new Keyword("location", false, "Physical location where the video was filmed"),
                    new Keyword("duration", true, "Length of the video in seconds"),
                    new Keyword("view_count", true, "How many users have watched the video on the platform"),
                    new Keyword("like_count", true, "Number of positive ratings of the video"),
                    new Keyword("dislike_count", true, "Number of negative ratings of the video"),
                    new Keyword("repost_count", true, "Number of reposts of the video"),
                    new Keyword("average_rating", true, "Average rating give by users, the scale used depends on the webpage"),
                    new Keyword("comment_count", true, "Number of comments on the video"),
                    new Keyword("age_limit", true, "Age restriction for the video (years)"),
                    new Keyword("is_live", false, "Whether this video is a live stream or a fixed-length video"),
                    new Keyword("start_time", true, "Time in seconds where the reproduction should start, as specified in the URL"),
                    new Keyword("end_time", true, "Time in seconds where the reproduction should end, as specified in the URL"),
                    new Keyword("format", false, "A human-readable description of the format "),
                    new Keyword("format_id", false, "Format code specified by `--format`"),
                    new Keyword("format_note", false, "Additional info about the format"),
                    new Keyword("width", true, "Width of the video"),
                    new Keyword("height", true, "Height of the video"),
                    new Keyword("resolution", false, "Textual description of width and height"),
                    new Keyword("tbr", true, "Average bitrate of audio and video in KBit/s"),
                    new Keyword("abr", true, "Average audio bitrate in KBit/s"),
                    new Keyword("acodec", false, "Name of the audio codec in use"),
                    new Keyword("asr", true, "Audio sampling rate in Hertz"),
                    new Keyword("vbr", true, "Average video bitrate in KBit/s"),
                    new Keyword("fps", true, "Frame rate"),
                    new Keyword("vcodec", false, "Name of the video codec in use"),
                    new Keyword("container", false, "Name of the container format"),
                    new Keyword("filesize", true, "The number of bytes, if known in advance"),
                    new Keyword("filesize_approx", true, "An estimate for the number of bytes"),
                    new Keyword("protocol", false, "The protocol that will be used for the actual download"),
                    new Keyword("extractor", false, "Name of the extractor"),
                    new Keyword("extractor_key", false, "Key name of the extractor"),
                    new Keyword("epoch", true, "Unix epoch when creating the file"),
                    new Keyword("autonumber", true, "Five-digit number that will be increased with each download, starting at zero"),
                    new Keyword("playlist", false, "Name or id of the playlist that contains the video"),
                    new Keyword("playlist_index", true, "Index of the video in the playlist padded with leading zeros according to the total length of the playlist"),
                    new Keyword("playlist_id", false, "Playlist identifier"),
                    new Keyword("playlist_title", false, "Playlist title"),
                    new Keyword("playlist_uploader", false, "Full name of the playlist uploader"),
                    new Keyword("playlist_uploader_id", false, "Nickname or id of the playlist uploader"),
                    new Keyword("chapter", false, "Name or title of the chapter the video belongs to"),
                    new Keyword("chapter_number", true, "Number of the chapter the video belongs to"),
                    new Keyword("chapter_id", false, "Id of the chapter the video belongs to"),
                    new Keyword("series", false, "Title of the series or programme the video episode belongs to"),
                    new Keyword("season", false, "Title of the season the video episode belongs to"),
                    new Keyword("season_number", true, "Number of the season the video episode belongs to"),
                    new Keyword("season_id", false, "Id of the season the video episode belongs to"),
                    new Keyword("episode", false, "Title of the video episode"),
                    new Keyword("episode_number", true, "Number of the video episode within a season"),
                    new Keyword("episode_id", false, "Id of the video episode"),
                    new Keyword("track", false, "Title of the track"),
                    new Keyword("track_number", true, "Number of the track within an album or a disc"),
                    new Keyword("track_id", false, "Id of the track"),
                    new Keyword("artist", false, "Artist(s) of the track"),
                    new Keyword("genre", false, "Genre(s) of the track"),
                    new Keyword("album", false, "Title of the album the track belongs to"),
                    new Keyword("album_type", false, "Type of the album"),
                    new Keyword("album_artist", false, "List of all artists appeared on the album"),
                    new Keyword("disc_number", true, "Number of the disc or other physical medium the track belongs to"),
                    new Keyword("release_year", true, "Year (YYYY) when the album was released"),

            };
        }

    }
}
