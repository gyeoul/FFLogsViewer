using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Logging;
using FFLogsViewer.Manager;
using FFLogsViewer.Model;
using ImGuiNET;
using Newtonsoft.Json.Linq;

namespace FFLogsViewer;

public class CharData
{
    public Job Job = GameDataManager.GetDefaultJob();
    public Metric? OverriddenMetric;
    public Metric? LoadedMetric;
    public string FirstName = string.Empty;
    public string WorldName = string.Empty;
    public string RegionName = string.Empty;
    public string LoadedFirstName = string.Empty;
    public string LoadedWorldName = string.Empty;
    public bool IsDataLoading;
    public bool IsDataReady;

    public List<Encounter> Encounters = new();

    public void SetInfo(string firstName, string worldName)
    {
        this.FirstName = firstName;
        this.WorldName = worldName;
    }

    public bool SetInfo(PlayerCharacter playerCharacter)
    {
        if (playerCharacter.HomeWorld.GameData?.Name == null)
        {
            Service.MainWindow.SetErrorMessage(Service.Localization.GetString("Error_CannotFetchWorldName"));
            PluginLog.Error("SetInfo character world was null");
            return false;
        }

        this.FirstName = playerCharacter.Name.TextValue;
        this.WorldName = playerCharacter.HomeWorld.GameData.Name.ToString();
        return true;
    }

    public bool IsInfoSet()
    {
        return this.FirstName != string.Empty && this.WorldName != string.Empty;
    }

    public void FetchTargetChar()
    {
        var target = Service.TargetManager.Target;
        if (target is PlayerCharacter targetCharacter && target.ObjectKind != ObjectKind.Companion)
        {
            if (this.SetInfo(targetCharacter))
            {
                this.FetchData();
            }
        }
        else
        {
            Service.MainWindow.SetErrorMessage(Service.Localization.GetString("Error_InvalidTarget"));
        }
    }

    public void FetchData()
    {
        if (this.IsDataLoading)
        {
            return;
        }

        Service.MainWindow.SetErrorMessage(string.Empty);

        if (!this.IsInfoSet())
        {
            Service.MainWindow.SetErrorMessage(Service.Localization.GetString("Error_FillInfo"));
            return;
        }

        var regionName = CharDataManager.GetRegionName(this.WorldName);
        if (regionName == null)
        {
            Service.MainWindow.SetErrorMessage(Service.Localization.GetString("Error_InvalidWorldName"));
            return;
        }

        this.RegionName = regionName;

        this.IsDataLoading = true;
        this.ResetData();
        Task.Run(async () =>
        {
            var rawData = await Service.FfLogsClient.FetchLogs(this).ConfigureAwait(false);
            if (rawData == null)
            {
                this.IsDataLoading = false;
                Service.MainWindow.SetErrorMessage(Service.Localization.GetString("Error_CannotReachServer"));
                PluginLog.Error("rawData is null");
                return;
            }

            if (rawData.data?.characterData?.character == null)
            {
                if (rawData.error != null && rawData.error == "Unauthenticated.")
                {
                    this.IsDataLoading = false;
                    Service.MainWindow.SetErrorMessage(Service.Localization.GetString("Error_InvalidApiClient"));
                    PluginLog.Log($"Unauthenticated: {rawData}");
                    return;
                }

                if (rawData.errors != null)
                {
                    this.IsDataLoading = false;
                    Service.MainWindow.SetErrorMessage(Service.Localization.GetString("Error_MalformedQuery"));
                    PluginLog.Log($"Malformed GraphQL query: {rawData}");
                    return;
                }

                this.IsDataLoading = false;
                Service.MainWindow.SetErrorMessage(Service.Localization.GetString("Error_CharacterNotFound"));
                return;
            }

            var character = rawData.data.characterData.character;

            if (character.hidden == "true")
            {
                this.IsDataLoading = false;
                Service.MainWindow.SetErrorMessage(Service.Localization.GetString("Error_HiddenLogs").Replace("{Name}", this.FirstName).Replace("{World}", this.WorldName));
                return;
            }

            this.Encounters = new List<Encounter>();

            var properties = character.Properties();
            foreach (var prop in properties)
            {
                if (prop.Name != "hidden")
                {
                    this.ParseZone(prop.Value);
                }
            }

            this.IsDataReady = true;
            this.LoadedFirstName = this.FirstName;
            this.LoadedWorldName = this.WorldName;
        }).ContinueWith(t =>
        {
            this.IsDataLoading = false;
            if (!t.IsFaulted) return;
            if (t.Exception == null) return;
            Service.MainWindow.SetErrorMessage(Service.Localization.GetString("Error_NetworkingError"));
            foreach (var e in t.Exception.Flatten().InnerExceptions)
            {
                PluginLog.Error(e, "Networking error");
            }
        });
    }

    public bool ParseTextForChar(string rawText)
    {
        var placeholder = CharDataManager.FindPlaceholder(rawText);
        if (placeholder != null)
        {
            rawText = placeholder;
        }

        if (!Regex.IsMatch(rawText, "@[^\x00-\x7F]{1,6}"))
            return false;

        if (Service.ClientState.LocalPlayer?.HomeWorld.GameData?.Name == null)
        {
            return false;
        }

        var splitedText = rawText.Split("@");
        var firstName = splitedText[0];
        var serverName = splitedText[1];

        this.FirstName = firstName[Math.Max(0, firstName.Length - 6)..]; // Maximum name length for Chinese region is 6, same goes for Korean I think
        this.WorldName = serverName;

        return true;
    }

    public void FetchClipboardCharacter()
    {
        string clipboardRawText;
        try
        {
            if (ImGui.GetClipboardText() == null)
            {
                Service.MainWindow.SetErrorMessage(Service.Localization.GetString("Error_CouldntGetClipboardText"));
                return;
            }

            clipboardRawText = ImGui.GetClipboardText();
        }
        catch
        {
            Service.MainWindow.SetErrorMessage(Service.Localization.GetString("Error_CouldntGetClipboardText"));
            return;
        }

        this.FetchTextCharacter(clipboardRawText);
    }

    public void FetchTextCharacter(string text)
    {
        if (!this.ParseTextForChar(text))
        {
            Service.MainWindow.SetErrorMessage(Service.Localization.GetString("Error_NoCharacterFound"));
            return;
        }

        this.FetchData();
    }

    public void ResetData()
    {
        this.Encounters = new List<Encounter>();
        this.IsDataReady = false;
        this.LoadedMetric = null;
    }

    private void ParseZone(dynamic zone)
    {
        if (zone.rankings == null)
        {
            return;
        }

        // metric not valid for this zone
        if (zone.rankings.Count == 0)
        {
            this.Encounters.Add(new Encounter { ZoneId = zone.zone, IsNotValid = true });
        }

        foreach (var ranking in zone.rankings)
        {
            if (ranking.encounter == null)
            {
                continue;
            }

            var encounter = new Encounter
            {
                ZoneId = zone.zone,
                Id = ranking.encounter.id,
                Difficulty = zone.difficulty,
                Metric = zone.metric,
            };

            if (ranking.spec != null)
            {
                encounter.IsLockedIn = ranking.lockedIn;
                encounter.Best = ranking.rankPercent;
                encounter.Median = ranking.medianPercent;
                encounter.Kills = ranking.totalKills;
                encounter.Fastest = ranking.fastestKill;
                encounter.BestAmount = ranking.bestAmount;
                var jobName = Regex.Replace(ranking.spec.ToString(), "([a-z])([A-Z])", "$1 $2");
                encounter.Job = Service.GameDataManager.Jobs.FirstOrDefault(job => job.Name == jobName);
                var bestJobName = Regex.Replace(ranking.bestSpec.ToString(), "([a-z])([A-Z])", "$1 $2");
                encounter.BestJob = Service.GameDataManager.Jobs.FirstOrDefault(job => job.Name == bestJobName);
                var allStars = ranking.allStars;
                if (allStars != null)
                {
                    encounter.AllStarsPoints = allStars.points;

                    // both "-" if fresh log
                    if (allStars.rank.Type != JTokenType.String && allStars.rankPercent.Type != JTokenType.String)
                    {
                        encounter.AllStarsRank = allStars.rank;
                        encounter.AllStarsRankPercent = allStars.rankPercent;
                    }
                }
            }

            this.Encounters.Add(encounter);
        }
    }
}
