using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using FFLogsViewer.Model;
using ImGuiNET;

namespace FFLogsViewer.GUI.Config;

public class PopupEntry
{
    public enum Mode
    {
        Adding,
        Editing,
    }

    private LayoutEntry AddLayoutEntry { get; set; } = LayoutEntry.CreateEncounter();
    private LayoutEntry EditLayoutEntry { get; set; } = null!;

    public int EditingIndex;
    private Mode mode = Mode.Adding;
    private bool hasDeleted;

    public void Open()
    {
        if (this.mode == Mode.Adding)
        {
            this.AddLayoutEntry.Alias = string.Empty;
        }
        else if (this.mode == Mode.Editing)
        {
            this.EditLayoutEntry = (LayoutEntry)Service.Configuration.Layout[this.EditingIndex].Clone();
        }

        ImGui.OpenPopup("##PopupEntry");
    }

    public void SwitchMode(Mode popupMode)
    {
        this.mode = popupMode;
    }

    public void Draw()
    {
        if (ImGui.BeginPopup("##PopupEntry"))
        {
            var currLayoutEntry = this.mode == Mode.Adding ? this.AddLayoutEntry : this.EditLayoutEntry;

            var tmpLayoutEntryType = currLayoutEntry.Type;
            if (ImGui.RadioButton(Service.Localization.GetString("Encounter"), tmpLayoutEntryType == LayoutEntryType.Encounter))
            {
                currLayoutEntry.Type = LayoutEntryType.Encounter;
            }

            ImGui.SameLine();
            if (ImGui.RadioButton(Service.Localization.GetString("PopupEntry_Header"), tmpLayoutEntryType == LayoutEntryType.Header))
            {
                currLayoutEntry.Type = LayoutEntryType.Header;
            }

            Util.DrawHelp(Service.Localization.GetString("PopupEntry_Header_Help"));

            var alias = currLayoutEntry.Alias;
            if (ImGui.InputText(Service.Localization.GetString("Alias"), ref alias, 400))
            {
                currLayoutEntry.Alias = alias;
            }

            Util.DrawHelp(Service.Localization.GetString("PopupEntry_Header_Alias_Help"));

            if (!Service.GameDataManager.IsDataReady && !Service.GameDataManager.IsDataLoading && !Service.GameDataManager.HasFailed)
            {
                Service.GameDataManager.FetchData();
            }

            if (currLayoutEntry.Type == LayoutEntryType.Encounter)
            {
                if (!Service.GameDataManager.IsDataReady && Service.GameDataManager.IsDataLoading)
                {
                    ImGui.Text(Service.Localization.GetString("FetchingData"));
                    ImGui.EndPopup();
                    return;
                }

                if (!Service.GameDataManager.IsDataLoading && Service.GameDataManager.HasFailed)
                {
                    if (ImGui.Button(Service.Localization.GetString("PopupEntry_FetchDataFailed")))
                    {
                        Service.GameDataManager.HasFailed = false;
                    }

                    ImGui.EndPopup();
                    return;
                }

                this.DrawEntryEncounter(currLayoutEntry);
            }
            else
            {
                this.DrawEntryHeader(currLayoutEntry);
            }

            if (this.hasDeleted)
            {
                this.hasDeleted = false;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }

    private void DrawEntryEncounter(LayoutEntry currLayoutEntry)
    {
        var expansions = Service.GameDataManager.GameData!.Data!.WorldData!.Expansions!;
        if (ImGui.BeginCombo($"{Service.Localization.GetString("Expansion")}##PopupEntryExpansion", currLayoutEntry.Expansion))
        {
            for (var i = 0; i < expansions.Count; i++)
            {
                ImGui.PushID(i);
                if (ImGui.Selectable(expansions[i].Name))
                {
                    currLayoutEntry.Expansion = expansions[i].Name!;
                    currLayoutEntry.Zone = "-";
                    currLayoutEntry.ZoneId = 0;
                    currLayoutEntry.Encounter = "-";
                    currLayoutEntry.EncounterId = 0;
                }

                ImGui.PopID();
            }

            ImGui.EndCombo();
        }

        var zones = expansions!.FirstOrDefault(expansion => expansion.Name == currLayoutEntry.Expansion)?.Zones;
        if (ImGui.BeginCombo($"{Service.Localization.GetString("Zone")}##PopupEntryZone", currLayoutEntry.Zone))
        {
            if (zones is { Count: > 0 })
            {
                for (var i = 0; i < zones.Count; i++)
                {
                    ImGui.PushID(i);
                    if (ImGui.Selectable(zones[i].Name))
                    {
                        currLayoutEntry.Zone = zones[i].Name!;
                        currLayoutEntry.ZoneId = zones[i].Id!.Value;
                        currLayoutEntry.Encounter = "-";
                        currLayoutEntry.EncounterId = 0;
                    }

                    ImGui.PopID();
                }
            }
            else
            {
                ImGui.Selectable(Service.Localization.GetString("PopupEntry_NoZoneFound"), true, ImGuiSelectableFlags.Disabled);
            }

            ImGui.EndCombo();
        }

        var encounters = zones?.FirstOrDefault(zone => zone.Name == currLayoutEntry.Zone)?.Encounters;
        if (ImGui.BeginCombo($"{Service.Localization.GetString("Encounter")}##PopupEntryEncounter", currLayoutEntry.Encounter))
        {
            if (encounters is { Count: > 0 })
            {
                for (var i = 0; i < encounters.Count; i++)
                {
                    ImGui.PushID(i);
                    if (ImGui.Selectable(encounters[i].Name))
                    {
                        currLayoutEntry.Encounter = encounters[i].Name!;
                        currLayoutEntry.EncounterId = encounters[i].Id!.Value;
                    }

                    ImGui.PopID();
                }
            }
            else
            {
                ImGui.Selectable(Service.Localization.GetString("PopupEntry_NoEncounterFound"), true, ImGuiSelectableFlags.Disabled);
            }

            ImGui.EndCombo();
        }

        var difficulties = zones?.FirstOrDefault(zone => zone.Name == currLayoutEntry.Zone)?.Difficulties;
        if (difficulties != null && difficulties.Count != 0)
        {
            if (difficulties.Count == 1)
            {
                currLayoutEntry.Difficulty = difficulties[0].Name!;
                currLayoutEntry.DifficultyId = difficulties[0].Id!.Value;
            }
            else
            {
                if (ImGui.BeginCombo($"{Service.Localization.GetString("Difficulty")}##PopupEntryDifficulty", currLayoutEntry.Difficulty))
                {
                    for (var i = 0; i < difficulties.Count; i++)
                    {
                        ImGui.PushID(i);
                        if (ImGui.Selectable(difficulties[i].Name))
                        {
                            currLayoutEntry.Difficulty = difficulties[i].Name!;
                            currLayoutEntry.DifficultyId = difficulties[i].Id!.Value;
                        }

                        ImGui.PopID();
                    }

                    ImGui.EndCombo();
                }
            }
        }

        var isButtonDisabled = !currLayoutEntry.IsEncounterValid();
        if (Util.DrawDisabledButton(this.mode == Mode.Adding ? Service.Localization.GetString("Add") : Service.Localization.GetString("Edit"), isButtonDisabled)
            && !isButtonDisabled)
        {
            if (this.mode == Mode.Adding)
            {
                Service.Configuration.Layout.Add((LayoutEntry)currLayoutEntry.Clone());
                Service.Configuration.IsDefaultLayout = false;
                Service.Configuration.Save();
            }
            else
            {
                if (!Service.Configuration.Layout[this.EditingIndex].Compare(currLayoutEntry))
                {
                    Service.Configuration.Layout[this.EditingIndex] = currLayoutEntry;
                    Service.Configuration.IsDefaultLayout = false;
                    Service.Configuration.Save();
                }
            }

            ImGui.CloseCurrentPopup();
        }

        if (isButtonDisabled)
        {
            ImGui.SameLine();
            ImGui.TextColored(ImGuiColors.DalamudGrey, Service.Localization.GetString("PopupEntry_SelectAnEncounter"));
        }

        if (this.mode == Mode.Adding && Service.Configuration.Layout.Any(layoutEntry => currLayoutEntry.Type != LayoutEntryType.Header &&
                                                                             currLayoutEntry.Type == layoutEntry.Type &&
                                                                             currLayoutEntry.Expansion == layoutEntry.Expansion &&
                                                                             currLayoutEntry.Zone == layoutEntry.Zone &&
                                                                             currLayoutEntry.Encounter == layoutEntry.Encounter))
        {
            ImGui.SameLine();
            ImGui.Text(Service.Localization.GetString("PopupEntry_EncounterAlreadyInLayout"));
        }

        if (this.mode == Mode.Editing)
        {
            this.DrawDeleteButton();
        }
    }

    private void DrawEntryHeader(LayoutEntry currLayoutEntry)
    {
        if (ImGui.Button(this.mode == Mode.Adding ? Service.Localization.GetString("Add") : Service.Localization.GetString("Edit")))
        {
            var newLayoutEntry = currLayoutEntry.CloneHeader();

            if (this.mode == Mode.Adding)
            {
                Service.Configuration.Layout.Add(newLayoutEntry);
                Service.Configuration.IsDefaultLayout = false;
                Service.Configuration.Save();
            }
            else
            {
                if (!Service.Configuration.Layout[this.EditingIndex].Compare(newLayoutEntry))
                {
                    Service.Configuration.Layout[this.EditingIndex] = newLayoutEntry;
                    Service.Configuration.IsDefaultLayout = false;
                    Service.Configuration.Save();
                }
            }

            ImGui.CloseCurrentPopup();
        }

        if (this.mode == Mode.Editing)
        {
            this.DrawDeleteButton();
        }
    }

    private void DrawDeleteButton()
    {
        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.Trash, new Vector2(2, ImGui.GetStyle().FramePadding.Y)))
        {
            ImGui.OpenPopup("##Trash");
        }

        if (ImGui.BeginPopup("##Trash", ImGuiWindowFlags.NoMove))
        {
            if (Util.DrawButtonIcon(FontAwesomeIcon.Trash, new Vector2(2, ImGui.GetStyle().FramePadding.Y)))
            {
                Service.Configuration.Layout.RemoveAt(this.EditingIndex);
                Service.Configuration.IsDefaultLayout = false;
                Service.Configuration.Save();

                this.hasDeleted = true;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }
}
