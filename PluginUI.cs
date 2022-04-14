using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Logging;
using ImGuiNET;

namespace FFLogsViewer
{
    internal class PluginUi : IDisposable
    {
        private readonly string[] _characterInput = new string[3];
        private readonly Vector4 _defaultColor = new( 1.0f, 1.0f, 1.0f, 1.0f );
        private readonly Dictionary<string, Vector4> _jobColors = new();
        private readonly Dictionary<string, Vector4> _logColors = new();
        private readonly FFLogsViewer _plugin;

        private float _bossesColumnWidth;

        private string _errorMessage = "";

        private bool _hasLoadingFailed;
        private bool _isConfigClicked;

        private bool _isLinkClicked;
        private bool _isRaidButtonClicked;
        private bool _isSpoilerClicked;
        private bool _isUltimateButtonClicked;
        private float _jobsColumnWidth;
        private float _logsColumnWidth;
        private CharacterData _selectedCharacterData = new();
        private bool _settingsVisible;

        private bool _visible;

        internal PluginUi(FFLogsViewer plugin)
        {
            _plugin = plugin;

            _jobColors.Add("Astrologian", new Vector4(255.0f / 255.0f, 231.0f / 255.0f, 74.0f / 255.0f, 1.0f));
            _jobColors.Add("Bard", new Vector4(145.0f / 255.0f, 150.0f / 255.0f, 186.0f / 255.0f, 1.0f));
            _jobColors.Add("Black Mage", new Vector4(165.0f / 255.0f, 121.0f / 255.0f, 214.0f / 255.0f, 1.0f));
            _jobColors.Add("Dancer", new Vector4(226.0f / 255.0f, 176.0f / 255.0f, 175.0f / 255.0f, 1.0f));
            _jobColors.Add("Dark Knight", new Vector4(209.0f / 255.0f, 38.0f / 255.0f, 204.0f / 255.0f, 1.0f));
            _jobColors.Add("Dragoon", new Vector4(65.0f / 255.0f, 100.0f / 255.0f, 205.0f / 255.0f, 1.0f));
            _jobColors.Add("Gunbreaker", new Vector4(121.0f / 255.0f, 109.0f / 255.0f, 48.0f / 255.0f, 1.0f));
            _jobColors.Add("Machinist", new Vector4(110.0f / 255.0f, 225.0f / 255.0f, 214.0f / 255.0f, 1.0f));
            _jobColors.Add("Monk", new Vector4(214.0f / 255.0f, 156.0f / 255.0f, 0.0f / 255.0f, 1.0f));
            _jobColors.Add("Ninja", new Vector4(175.0f / 255.0f, 25.0f / 255.0f, 100.0f / 255.0f, 1.0f));
            _jobColors.Add("Paladin", new Vector4(168.0f / 255.0f, 210.0f / 255.0f, 230.0f / 255.0f, 1.0f));
            _jobColors.Add("Red Mage", new Vector4(232.0f / 255.0f, 123.0f / 255.0f, 123.0f / 255.0f, 1.0f));
            _jobColors.Add("Samurai", new Vector4(228.0f / 255.0f, 109.0f / 255.0f, 4.0f / 255.0f, 1.0f));
            _jobColors.Add("Scholar", new Vector4(134.0f / 255.0f, 87.0f / 255.0f, 255.0f / 255.0f, 1.0f));
            _jobColors.Add("Summoner", new Vector4(45.0f / 255.0f, 155.0f / 255.0f, 120.0f / 255.0f, 1.0f));
            _jobColors.Add("Warrior", new Vector4(207.0f / 255.0f, 38.0f / 255.0f, 33.0f / 255.0f, 1.0f));
            _jobColors.Add("White Mage", new Vector4(255.0f / 255.0f, 240.0f / 255.0f, 220.0f / 255.0f, 1.0f));
            _jobColors.Add("Default", _defaultColor);

            _logColors.Add("Grey", new Vector4(102.0f / 255.0f, 102.0f / 255.0f, 102.0f / 255.0f, 1.0f));
            _logColors.Add("Green", new Vector4(30.0f / 255.0f, 255.0f / 255.0f, 0.0f / 255.0f, 1.0f));
            _logColors.Add("Blue", new Vector4(0.0f / 255.0f, 112.0f / 255.0f, 255.0f / 255.0f, 1.0f));
            _logColors.Add("Magenta", new Vector4(163.0f / 255.0f, 53.0f / 255.0f, 238.0f / 255.0f, 1.0f));
            _logColors.Add("Orange", new Vector4(255.0f / 255.0f, 128.0f / 255.0f, 0.0f / 255.0f, 1.0f));
            _logColors.Add("Pink", new Vector4(226.0f / 255.0f, 104.0f / 255.0f, 168.0f / 255.0f, 1.0f));
            _logColors.Add("Yellow", new Vector4(229.0f / 255.0f, 204.0f / 255.0f, 128.0f / 255.0f, 1.0f));
            _logColors.Add("Default", _defaultColor);
        }

        internal bool Visible
        {
            get => _visible;
            set => _visible = value;
        }

        internal bool SettingsVisible
        {
            get => _settingsVisible;
            set => _settingsVisible = value;
        }

        public void Dispose()
        {
        }

        internal void Draw()
        {
            DrawSettingsWindow();
            DrawMainWindow();
        }

        private void DrawSettingsWindow()
        {
            if (!SettingsVisible) return;

            ImGui.SetNextWindowSize(new Vector2(0, 0), ImGuiCond.Always);

            if (ImGui.Begin("FF Logs Viewer Config", ref _settingsVisible,
                    ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                    ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize))
            {
                var showSpoilers = _plugin.Configuration.ShowSpoilers;

                if (ImGui.Checkbox("Show spoilers##ShowSpoilers", ref showSpoilers))
                {
                    _plugin.ToggleContextMenuButton(showSpoilers);
                    _plugin.Configuration.ShowSpoilers = showSpoilers;
                    _plugin.Configuration.Save();
                }

                var contextMenu = _plugin.Configuration.ContextMenu;

                if (ImGui.Checkbox("Search button in context menus##ContextMenu", ref contextMenu))
                {
                    _plugin.ToggleContextMenuButton(contextMenu);
                    _plugin.Configuration.ContextMenu = contextMenu;
                    _plugin.Configuration.Save();
                }

                if (ImGui.IsItemHovered()) ImGui.SetTooltip("Add a button to search characters in most context menus.");

                if (_plugin.Configuration.ContextMenu)
                {
                    var openInBrowser = _plugin.Configuration.OpenInBrowser;

                    if (ImGui.Checkbox(@"Open in browser##OpenInBrowser", ref openInBrowser))
                    {
                        _plugin.Configuration.OpenInBrowser = openInBrowser;
                        _plugin.Configuration.Save();
                    }

                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("The button in context menus opens" +
                                         "\nFFLogs in your default browser instead" +
                                         "\nof opening the plugin window.");

                    if (!_plugin.Configuration.ContextMenuStreamer)
                    {
                        var contextMenuButtonName = _plugin.Configuration.ContextMenuButtonName ?? string.Empty;

                        if (ImGui.InputText("Button name##ContextMenuButtonName", ref contextMenuButtonName, 50))
                        {
                            _plugin.Configuration.ContextMenuButtonName = contextMenuButtonName;
                            _plugin.Configuration.Save();
                        }
                    }

                    var contextMenuStreamer = _plugin.Configuration.ContextMenuStreamer;

                    if (ImGui.Checkbox(@"Streamer mode##ContextMenuStreamer", ref contextMenuStreamer))
                    {
                        _plugin.Configuration.ContextMenuStreamer = contextMenuStreamer;
                        _plugin.Configuration.Save();
                    }

                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("When the FF Logs Viewer window is open, opening a context menu" +
                                         "\nwill automatically search for the selected player." +
                                         "\nThis mode does not add a button to the context menu.");

                    var hideInCombat = _plugin.Configuration.HideInCombat;

                    if (ImGui.Checkbox(@"Hide in combat##HideInCombat", ref hideInCombat))
                    {
                        _plugin.Configuration.HideInCombat = hideInCombat;
                        _plugin.Configuration.Save();
                    }
                }

                ImGui.Text("API client:");

                var configurationClientId = _plugin.Configuration.ClientId ?? string.Empty;

                if (ImGui.InputText("Client ID##ClientId", ref configurationClientId, 50))
                {
                    _plugin.Configuration.ClientId = configurationClientId;
                    _plugin.FfLogsClient.SetToken();
                    _plugin.Configuration.Save();
                }

                var configurationClientSecret = _plugin.Configuration.ClientSecret ?? string.Empty;

                if (ImGui.InputText("Client secret##ClientSecret", ref configurationClientSecret, 50))
                {
                    _plugin.Configuration.ClientSecret = configurationClientSecret;
                    _plugin.FfLogsClient.SetToken();
                    _plugin.Configuration.Save();
                }

                if (_plugin.FfLogsClient.IsTokenValid)
                    ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), "This client is valid.");
                else
                    ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), "This client is NOT valid.");

                ImGui.Text("How to get a client ID and a client secret:");

                ImGui.Bullet();
                ImGui.Text($"Open https://{_plugin.FflogsHost}/api/clients/ or");
                ImGui.SameLine();

                if (ImGui.Button("Click here##APIClientLink"))
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $"https://{_plugin.FflogsHost}/api/clients/",
                        UseShellExecute = true
                    });

                ImGui.Bullet();
                ImGui.Text("Create a new client");
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 7);

                ImGui.Bullet();
                ImGui.Text("Choose any name, for example: \"Plugin\"");
                ImGui.SameLine();

                if (ImGui.Button("Copy##APIClientCopyName"))
                    ImGui.SetClipboardText("Plugin");

                ImGui.Bullet();
                ImGui.Text("Enter any URL, for example: \"https://www.example.com\"");
                ImGui.SameLine();

                if (ImGui.Button("Copy##APIClientCopyURL"))
                    ImGui.SetClipboardText("https://www.example.com");

                ImGui.Bullet();
                ImGui.Text("Do NOT check the Public Client option");
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 7);

                ImGui.Bullet();
                ImGui.Text("Paste both client ID and secret above");
            }

            ImGui.End();
        }

        private void DrawMainWindow()
        {
            if (!Visible
                || _plugin.Configuration.HideInCombat && DalamudApi.Condition[ ConditionFlag.InCombat ])
                return;

            var windowHeight = 293 * ImGui.GetIO().FontGlobalScale + 100;
            var reducedWindowHeight = 58 * ImGui.GetIO().FontGlobalScale + 30;
            var windowWidth = 407 * ImGui.GetIO().FontGlobalScale;

            ImGui.SetNextWindowSize(new Vector2(windowWidth, reducedWindowHeight), ImGuiCond.FirstUseEver);

            if (ImGui.Begin("FF Logs Viewer", ref _visible,
                    ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.Columns(3, "InputColumns", true);

                var buttonsWidth = (ImGui.CalcTextSize("Target") + ImGui.CalcTextSize("Clipboard")).X + 40.0f * ImGui.GetIO().FontGlobalScale;

                var sizeMin = Math.Max(ImGui.CalcTextSize(_selectedCharacterData.FirstName).X,
                    Math.Max(ImGui.CalcTextSize(_selectedCharacterData.LastName).X,
                        ImGui.CalcTextSize(_selectedCharacterData.WorldName).X));
                var idealWindowWidth = sizeMin * 2 + buttonsWidth + 73.0f;
                if (idealWindowWidth < windowWidth) idealWindowWidth = windowWidth;
                float idealWindowHeight;

                if (_selectedCharacterData.IsEveryLogsReady && !_hasLoadingFailed)
                    idealWindowHeight = windowHeight;
                else
                    idealWindowHeight = reducedWindowHeight;
                var colWidth = (idealWindowWidth - buttonsWidth) / 2.0f;
                ImGui.SetWindowSize(new Vector2(idealWindowWidth, idealWindowHeight));

                ImGui.SetColumnWidth(0, colWidth);
                ImGui.SetColumnWidth(1, colWidth);
                ImGui.SetColumnWidth(2, buttonsWidth);


                ImGui.PushItemWidth(colWidth - 15);
                _characterInput[ 0 ] = _selectedCharacterData.FirstName;

                ImGui.InputTextWithHint("##FirstName", "First Name", ref _characterInput[ 0 ], 256,
                    ImGuiInputTextFlags.CharsNoBlank);
                _selectedCharacterData.FirstName = _characterInput[ 0 ];
                ImGui.PopItemWidth();


                ImGui.NextColumn();
                ImGui.PushItemWidth(colWidth - 14);
                _characterInput[ 2 ] = _selectedCharacterData.WorldName;

                ImGui.InputTextWithHint("##WorldName", "World Name", ref _characterInput[ 2 ], 256,
                    ImGuiInputTextFlags.CharsNoBlank);
                _selectedCharacterData.WorldName = _characterInput[ 2 ];

                ImGui.PopItemWidth();

                ImGui.NextColumn();

                if (ImGui.Button("Clipboard"))
                    try
                    {
                        _selectedCharacterData = _plugin.GetClipboardCharacter();
                        _errorMessage = "";
                        _hasLoadingFailed = false;

                        try
                        {
                            _plugin.FetchLogs(_selectedCharacterData);
                        }
                        catch
                        {
                            _errorMessage = "World not supported or invalid.";
                        }
                    }
                    catch
                    {
                        _errorMessage = "No character found in the clipboard.";
                    }

                ImGui.SameLine();

                if (ImGui.Button("Target"))
                    try
                    {
                        _selectedCharacterData = _plugin.GetTargetCharacter();
                        _errorMessage = "";
                        _hasLoadingFailed = false;

                        try
                        {
                            _plugin.FetchLogs(_selectedCharacterData);
                        }
                        catch
                        {
                            _errorMessage = "World not supported or invalid.";
                        }
                    }
                    catch
                    {
                        _errorMessage = "Invalid target.";
                    }

                ImGui.Columns();

                ImGui.Separator();

                if (ImGui.Button("Clear"))
                {
                    _selectedCharacterData = new CharacterData();
                    _errorMessage = "";
                    _hasLoadingFailed = false;
                }

                ImGui.SameLine();

                if (!_plugin.FfLogsClient.IsTokenValid)
                {
                    var message = !_plugin.IsConfigSetup() ? "Config not setup, click to open settings." : "API client not valid, click to open settings.";
                    var messageSize = ImGui.CalcTextSize(message);
                    ImGui.SetCursorPosX(ImGui.GetWindowWidth() / 2 - messageSize.X / 2);
                    messageSize.X -= 7; // A bit too large on right side
                    messageSize.Y += 1;
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.0f, 0.0f, 1.0f));

                    ImGui.Selectable(
                        message,
                        ref _isConfigClicked, ImGuiSelectableFlags.None, messageSize);
                    ImGui.PopStyleColor();

                    if (_isConfigClicked)
                    {
                        SettingsVisible = true;
                        _isConfigClicked = false;
                    }
                }
                else
                {
                    if (_errorMessage == "")
                    {
                        if (_selectedCharacterData.IsEveryLogsReady)
                        {
                            var message = $"Viewing {_selectedCharacterData.LoadedFirstName} {_selectedCharacterData.LoadedLastName}@{_selectedCharacterData.LoadedWorldName}'s logs.";
                            var messageSize = ImGui.CalcTextSize(message);
                            ImGui.SetCursorPosX(ImGui.GetWindowWidth() / 2 - messageSize.X / 2);
                            messageSize.X -= 7 * ImGui.GetIO().FontGlobalScale; // A bit too large on right side
                            messageSize.Y += 1 * ImGui.GetIO().FontGlobalScale;

                            ImGui.Selectable(
                                message,
                                ref _isLinkClicked, ImGuiSelectableFlags.None, messageSize);

                            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Click to open on FF Logs.");

                            if (_isLinkClicked)
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = $"https://{_plugin.FflogsHost}/character/{_selectedCharacterData.RegionName}/{_selectedCharacterData.WorldName}/{_selectedCharacterData.FirstName} {_selectedCharacterData.LastName}",
                                    UseShellExecute = true
                                });
                                _isLinkClicked = false;
                            }
                        }
                        else if (_selectedCharacterData.IsDataLoading)
                        {
                            ImGui.SetCursorPosX(ImGui.GetWindowWidth() / 2 - ImGui.CalcTextSize("Loading...").X / 2);
                            ImGui.TextUnformatted("Loading...");
                        }
                        else
                        {
                            ImGui.SetCursorPosX(ImGui.GetWindowWidth() / 2 - ImGui.CalcTextSize("Waiting...").X / 2);
                            ImGui.TextUnformatted("Waiting...");
                        }
                    }
                    else
                    {
                        ImGui.SetCursorPosX(ImGui.GetWindowWidth() / 2 - ImGui.CalcTextSize(_errorMessage).X / 2);
                        ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), _errorMessage);
                    }
                }


                ImGui.SameLine();

                ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.CalcTextSize("Search").X * 1.5f + 4.0f);

                if (ImGui.Button("Search"))
                {
                    if (_selectedCharacterData.IsCharacterReady())
                    {
                        _errorMessage = "";

                        try
                        {
                            _plugin.FetchLogs(_selectedCharacterData);
                        }
                        catch
                        {
                            _errorMessage = "World not supported or invalid.";
                        }

                        _hasLoadingFailed = false;
                    }
                    else
                    {
                        _errorMessage = "One of the inputs is empty.";
                    }
                }

                if (_selectedCharacterData.IsEveryLogsReady && !_hasLoadingFailed)
                {
                    ImGui.Separator();

                    ImGui.Columns(5, "LogsDisplay", true);

                    _bossesColumnWidth =
                        ImGui.CalcTextSize("Cloud of Darkness").X + 17.5f; // Biggest text in first column
                    _jobsColumnWidth = ImGui.CalcTextSize("Dark Knight").X + 17.5f; // Biggest job name

                    _logsColumnWidth = (ImGui.GetWindowWidth() - _bossesColumnWidth - _jobsColumnWidth) /
                                       3.0f;
                    ImGui.SetColumnWidth(0, _bossesColumnWidth);
                    ImGui.SetColumnWidth(1, _logsColumnWidth);
                    ImGui.SetColumnWidth(2, _logsColumnWidth);
                    ImGui.SetColumnWidth(3, _logsColumnWidth);
                    ImGui.SetColumnWidth(4, _jobsColumnWidth);

                    var raidName = _plugin.Configuration.DisplayOldRaid ? "Eden's Promise (?)" : "Asphodelos (?)";
                    ImGui.SetCursorPosX(8.0f);

                    ImGui.Selectable(
                        raidName,
                        ref _isRaidButtonClicked, ImGuiSelectableFlags.None);
                    if (ImGui.IsItemHovered()) ImGui.SetTooltip("Click to switch to " + (_plugin.Configuration.DisplayOldRaid ? "Asphodelos." : "Eden's Promise."));

                    if (_isRaidButtonClicked)
                    {
                        _plugin.Configuration.DisplayOldRaid = !_plugin.Configuration.DisplayOldRaid;
                        _plugin.Configuration.Save();
                        _isRaidButtonClicked = false;
                    }

                    ImGui.Spacing();

                    if (_plugin.Configuration.DisplayOldRaid)
                    {
                        PrintTextColumn(1, "Cloud of Darkness");
                        PrintTextColumn(1, "Shadowkeeper");
                        PrintTextColumn(1, "Fatebreaker");
                        PrintTextColumn(1, "Eden's Promise");
                        PrintTextColumn(1, "Oracle of Darkness");
                    }
                    else
                    {
                        PrintTextColumn(1, "Erichthonios");
                        PrintTextColumn(1, "Hippokampos");
                        PrintTextColumn(1, "Phoinix");
                        PrintTextColumn(1, "Hesperos");
                        PrintTextColumn(1, "Hesperos II");
                    }

                    ImGui.Spacing();
                    var ultimateName = _plugin.Configuration.DisplayOldUltimate ? "Ultimates (ShB) (?)" : "Ultimates (EW) (?)";
                    ImGui.SetCursorPosX(8.0f);

                    ImGui.Selectable(
                        ultimateName,
                        ref _isUltimateButtonClicked, ImGuiSelectableFlags.None);
                    if (ImGui.IsItemHovered()) ImGui.SetTooltip("Click to switch to " + (_plugin.Configuration.DisplayOldUltimate ? "Endwalker ultimates." : "Shadowbringers ultimates."));

                    if (_isUltimateButtonClicked)
                    {
                        _plugin.Configuration.DisplayOldUltimate = !_plugin.Configuration.DisplayOldUltimate;
                        _plugin.Configuration.Save();
                        _isUltimateButtonClicked = false;
                    }

                    ImGui.Spacing();
                    PrintTextColumn(1, "UCoB");
                    PrintTextColumn(1, "UwU");
                    PrintTextColumn(1, "TEA");

                    ImGui.Spacing();
                    PrintTextColumn(1, "Trials (Extreme)");
                    ImGui.Spacing();

                    if (_plugin.Configuration.ShowSpoilers)
                    {
                        PrintTextColumn(1, "Zodiark");
                        PrintTextColumn(1, "Hydaelyn");
                    }
                    else
                    {
                        ImGui.SetCursorPosX(8.0f);

                        ImGui.Selectable(
                            "Trial 1 (?)",
                            ref _isSpoilerClicked, ImGuiSelectableFlags.None);
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Big MSQ spoiler, click to display.");

                        ImGui.SetCursorPosX(8.0f);

                        ImGui.Selectable(
                            "Trial 2 (?)",
                            ref _isSpoilerClicked, ImGuiSelectableFlags.None);
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Big MSQ spoiler, click to display.");

                        if (_isSpoilerClicked)
                        {
                            _plugin.Configuration.ShowSpoilers = !_plugin.Configuration.ShowSpoilers;
                            _plugin.Configuration.Save();
                            _isSpoilerClicked = false;
                        }
                    }
                    // PrintTextColumn(1, "Endsinger");

                    try
                    {
                        ImGui.NextColumn();
                        PrintTextColumn(2, "Best");
                        ImGui.Spacing();
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.CloudOfDarkness : CharacterData.BossesId.Erichthonios, CharacterData.DataType.Best, 2);
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.Shadowkeeper : CharacterData.BossesId.Hippokampos, CharacterData.DataType.Best, 2);
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.Fatebreaker : CharacterData.BossesId.Phoinix, CharacterData.DataType.Best, 2);
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.EdensPromise : CharacterData.BossesId.Hesperos, CharacterData.DataType.Best, 2);
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.OracleOfDarkness : CharacterData.BossesId.HesperosII, CharacterData.DataType.Best, 2);
                        ImGui.Spacing();
                        PrintTextColumn(2, "Best");
                        ImGui.Spacing();
                        PrintDataColumn(_plugin.Configuration.DisplayOldUltimate ? CharacterData.BossesId.UCoBShB : CharacterData.BossesId.UCoB, CharacterData.DataType.Best, 2);
                        PrintDataColumn(_plugin.Configuration.DisplayOldUltimate ? CharacterData.BossesId.UwUShB : CharacterData.BossesId.UwU, CharacterData.DataType.Best, 2);
                        PrintDataColumn(_plugin.Configuration.DisplayOldUltimate ? CharacterData.BossesId.TeaShB : CharacterData.BossesId.Tea, CharacterData.DataType.Best, 2);
                        ImGui.Spacing();
                        PrintTextColumn(2, "Best");
                        ImGui.Spacing();
                        PrintDataColumn(CharacterData.BossesId.Zodiark, CharacterData.DataType.Best, 2);
                        PrintDataColumn(CharacterData.BossesId.Hydaelyn, CharacterData.DataType.Best, 2);
                        // PrintDataColumn(CharacterData.BossesId.Endsinger, CharacterData.DataType.Best, 2);

                        ImGui.NextColumn();
                        PrintTextColumn(3, "Med.");
                        ImGui.Spacing();
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.CloudOfDarkness : CharacterData.BossesId.Erichthonios, CharacterData.DataType.Median, 3);
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.Shadowkeeper : CharacterData.BossesId.Hippokampos, CharacterData.DataType.Median, 3);
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.Fatebreaker : CharacterData.BossesId.Phoinix, CharacterData.DataType.Median, 3);
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.EdensPromise : CharacterData.BossesId.Hesperos, CharacterData.DataType.Median, 3);
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.OracleOfDarkness : CharacterData.BossesId.HesperosII, CharacterData.DataType.Median, 3);
                        ImGui.Spacing();
                        PrintTextColumn(3, "Med.");
                        ImGui.Spacing();
                        PrintDataColumn(_plugin.Configuration.DisplayOldUltimate ? CharacterData.BossesId.UCoBShB : CharacterData.BossesId.UCoB, CharacterData.DataType.Median, 3);
                        PrintDataColumn(_plugin.Configuration.DisplayOldUltimate ? CharacterData.BossesId.UwUShB : CharacterData.BossesId.UwU, CharacterData.DataType.Median, 3);
                        PrintDataColumn(_plugin.Configuration.DisplayOldUltimate ? CharacterData.BossesId.TeaShB : CharacterData.BossesId.Tea, CharacterData.DataType.Median, 3);
                        ImGui.Spacing();
                        PrintTextColumn(3, "Med.");
                        ImGui.Spacing();
                        PrintDataColumn(CharacterData.BossesId.Zodiark, CharacterData.DataType.Median, 3);
                        PrintDataColumn(CharacterData.BossesId.Hydaelyn, CharacterData.DataType.Median, 3);
                        // PrintDataColumn(CharacterData.BossesId.Endsinger, CharacterData.DataType.Median, 3);

                        ImGui.NextColumn();
                        PrintTextColumn(4, "Kills");
                        ImGui.Spacing();
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.CloudOfDarkness : CharacterData.BossesId.Erichthonios, CharacterData.DataType.Kills, 4);
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.Shadowkeeper : CharacterData.BossesId.Hippokampos, CharacterData.DataType.Kills, 4);
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.Fatebreaker : CharacterData.BossesId.Phoinix, CharacterData.DataType.Kills, 4);
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.EdensPromise : CharacterData.BossesId.Hesperos, CharacterData.DataType.Kills, 4);
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.OracleOfDarkness : CharacterData.BossesId.HesperosII, CharacterData.DataType.Kills, 4);
                        ImGui.Spacing();
                        PrintTextColumn(4, "Kills");
                        ImGui.Spacing();
                        PrintDataColumn(_plugin.Configuration.DisplayOldUltimate ? CharacterData.BossesId.UCoBShB : CharacterData.BossesId.UCoB, CharacterData.DataType.Kills, 4);
                        PrintDataColumn(_plugin.Configuration.DisplayOldUltimate ? CharacterData.BossesId.UwUShB : CharacterData.BossesId.UwU, CharacterData.DataType.Kills, 4);
                        PrintDataColumn(_plugin.Configuration.DisplayOldUltimate ? CharacterData.BossesId.TeaShB : CharacterData.BossesId.Tea, CharacterData.DataType.Kills, 4);
                        ImGui.Spacing();
                        PrintTextColumn(4, "Kills");
                        ImGui.Spacing();
                        PrintDataColumn(CharacterData.BossesId.Zodiark, CharacterData.DataType.Kills, 4);
                        PrintDataColumn(CharacterData.BossesId.Hydaelyn, CharacterData.DataType.Kills, 4);
                        // PrintDataColumn(CharacterData.BossesId.Endsinger, CharacterData.DataType.Kills, 4);

                        ImGui.NextColumn();
                        PrintTextColumn(5, "Job");
                        ImGui.Separator();
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.CloudOfDarkness : CharacterData.BossesId.Erichthonios, CharacterData.DataType.Job, 5);
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.Shadowkeeper : CharacterData.BossesId.Hippokampos, CharacterData.DataType.Job, 5);
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.Fatebreaker : CharacterData.BossesId.Phoinix, CharacterData.DataType.Job, 5);
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.EdensPromise : CharacterData.BossesId.Hesperos, CharacterData.DataType.Job, 5);
                        PrintDataColumn(_plugin.Configuration.DisplayOldRaid ? CharacterData.BossesId.OracleOfDarkness : CharacterData.BossesId.HesperosII, CharacterData.DataType.Job, 5);
                        ImGui.Separator();
                        PrintTextColumn(5, "Job");
                        ImGui.Separator();
                        PrintDataColumn(_plugin.Configuration.DisplayOldUltimate ? CharacterData.BossesId.UCoBShB : CharacterData.BossesId.UCoB, CharacterData.DataType.Job, 5);
                        PrintDataColumn(_plugin.Configuration.DisplayOldUltimate ? CharacterData.BossesId.UwUShB : CharacterData.BossesId.UwU, CharacterData.DataType.Job, 5);
                        PrintDataColumn(_plugin.Configuration.DisplayOldUltimate ? CharacterData.BossesId.TeaShB : CharacterData.BossesId.Tea, CharacterData.DataType.Job, 5);
                        ImGui.Separator();
                        PrintTextColumn(5, "Job");
                        ImGui.Separator();
                        PrintDataColumn(CharacterData.BossesId.Zodiark, CharacterData.DataType.Job, 5);
                        PrintDataColumn(CharacterData.BossesId.Hydaelyn, CharacterData.DataType.Job, 5);
                        // PrintDataColumn(CharacterData.BossesId.Endsinger, CharacterData.DataType.Job, 5);

                        ImGui.Columns();
                    }
                    catch (Exception e)
                    {
                        _errorMessage = "Logs could not be loaded.";
                        PluginLog.LogError(e, "Logs could not be loaded.");
                        _hasLoadingFailed = true;
                    }
                }
            }

            ImGui.End();
        }

        private void PrintDataColumn(CharacterData.BossesId bossId, CharacterData.DataType dataType, int column)
        {
            string text = null;
            var color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            int log;

            switch (dataType)
            {
                case CharacterData.DataType.Best:
                    if (_selectedCharacterData.Bests.TryGetValue((int)bossId, out log))
                    {
                        text = log == -1 ? "-" : log.ToString();
                        color = GetLogColor(log);
                    }

                    break;

                case CharacterData.DataType.Median:
                    if (_selectedCharacterData.Medians.TryGetValue((int)bossId, out log))
                    {
                        text = log == -1 ? "-" : log.ToString();
                        color = GetLogColor(log);
                    }

                    break;

                case CharacterData.DataType.Kills:
                    if (_selectedCharacterData.Kills.TryGetValue((int)bossId, out log))
                        text = log == -1 ? "-" : log.ToString();

                    break;

                case CharacterData.DataType.Job:
                    if (_selectedCharacterData.Jobs.TryGetValue((int)bossId, out var job))
                    {
                        text = job;

                        if (!_jobColors.TryGetValue(job, out color))
                            color = _jobColors[ "Default" ];
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof( dataType ), dataType, null);
            }

            if (text != null)
            {
                PrintTextColumn(column, text, color);
            }
            else
            {
                PrintTextColumn(column, "N/A", color);
                if (ImGui.IsItemHovered()) ImGui.SetTooltip("Data not available. This is expected if the boss is not yet on FF Logs.");
            }
        }

        private void PrintTextColumn(int column, string text, Vector4 color)
        {
            var position = column switch
            {
                1 => 8.0f,
                2 or 3 or 4 => _bossesColumnWidth + _logsColumnWidth / 2.0f +
                               _logsColumnWidth * (column - 2) -
                               ImGui.CalcTextSize(text).X / 2.0f,
                _ => _bossesColumnWidth + _jobsColumnWidth / 2.0f + _logsColumnWidth * (column - 2) -
                     ImGui.CalcTextSize(text).X / 2.0f
            };

            ImGui.SetCursorPosX(position);
            ImGui.TextColored(color, text);
        }

        private void PrintTextColumn(int column, string text)
        {
            PrintTextColumn(column, text, _defaultColor);
        }

        private Vector4 GetLogColor(int log)
        {
            return log switch
            {
                < 0 => _logColors[ "Default" ],
                < 25 => _logColors[ "Grey" ],
                < 50 => _logColors[ "Green" ],
                < 75 => _logColors[ "Blue" ],
                < 95 => _logColors[ "Magenta" ],
                < 99 => _logColors[ "Orange" ],
                99 => _logColors[ "Pink" ],
                100 => _logColors[ "Yellow" ],
                _ => _logColors[ "Default" ]
            };
        }

        internal void SetCharacterAndFetchLogs(CharacterData character)
        {
            _selectedCharacterData = character;
            _errorMessage = "";
            _hasLoadingFailed = false;
            _plugin.FetchLogs(_selectedCharacterData);
        }

        internal void SetErrorMessage(string errorMessage)
        {
            _errorMessage = errorMessage;
        }
    }
}