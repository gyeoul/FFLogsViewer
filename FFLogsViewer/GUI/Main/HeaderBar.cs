using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace FFLogsViewer.GUI.Main;

public class HeaderBar
{
    public string? ErrorMessage = string.Empty;
    public uint ResetSizeCount;

    private readonly Stopwatch partyListStopwatch = new();
    private bool isProfileLinkClicked;
    private bool isConfigClicked;

    public void Draw()
    {
        if (this.isConfigClicked)
        {
            Service.ConfigWindow.IsOpen = true;
            this.isConfigClicked = false;
        }

        if (this.isProfileLinkClicked)
        {
            Util.OpenLink(Service.CharDataManager.DisplayedChar);
            this.isProfileLinkClicked = false;
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4 * ImGuiHelpers.GlobalScale, ImGui.GetStyle().ItemSpacing.Y));

        var buttonsWidth = GetButtonsWidth();
        var minWindowSize = GetMinWindowSize();

        if (ImGui.GetWindowSize().X < minWindowSize || this.ResetSizeCount != 0)
        {
            ImGui.SetWindowSize(new Vector2(minWindowSize, -1));
        }

        if (!Service.Configuration.Style.IsSizeFixed
            && (Service.Configuration.Style.MainWindowFlags & ImGuiWindowFlags.AlwaysAutoResize) == 0)
        {
            ImGui.SetWindowSize(new Vector2(Service.Configuration.Style.MinMainWindowWidth > minWindowSize ? Service.Configuration.Style.MinMainWindowWidth : minWindowSize, -1));
        }

        // I hate ImGui
        var contentRegionAvailWidth = ImGui.GetContentRegionAvail().X;
        if (ImGui.GetWindowSize().X < minWindowSize || this.ResetSizeCount != 0)
        {
            contentRegionAvailWidth = minWindowSize - (ImGui.GetStyle().WindowPadding.X * 2);
            this.ResetSizeCount--;
        }

        var calcInputSize = (contentRegionAvailWidth - (ImGui.GetStyle().ItemSpacing.X * 2) - buttonsWidth) / 3;

        ImGui.SetNextItemWidth(calcInputSize);
        if (ImGui.InputTextWithHint("##FirstName", Service.Localization.GetString("Main_Name"), ref Service.CharDataManager.DisplayedChar.FirstName, 18, ImGuiInputTextFlags.CharsNoBlank))
        {
            Service.CharDataManager.DisplayedChar.FirstName = Service.CharDataManager.DisplayedChar.FirstName[..Math.Min(Service.CharDataManager.DisplayedChar.FirstName.Length, 6)];
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(calcInputSize);
        ImGui.InputTextWithHint("##WorldName", Service.Localization.GetString("Main_World"), ref Service.CharDataManager.DisplayedChar.WorldName, 18, ImGuiInputTextFlags.CharsNoBlank);

        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.Search))
        {
            Service.CharDataManager.DisplayedChar.FetchData();
        }

        Util.SetHoverTooltip(Service.Localization.GetString("Main_Search"));

        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.Crosshairs))
        {
            Service.CharDataManager.DisplayedChar.FetchTargetChar();
        }

        Util.SetHoverTooltip(Service.Localization.GetString("Main_Target"));

        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.Clipboard))
        {
            Service.CharDataManager.DisplayedChar.FetchClipboardCharacter();
        }

        Util.SetHoverTooltip(Service.Localization.GetString("Main_SearchClipboard"));

        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.UsersCog))
        {
            ImGui.OpenPopup("##PartyList");
        }

        Util.SetHoverTooltip(Service.Localization.GetString("Main_PartyMembers"));

        if (ImGui.BeginPopup("##PartyList", ImGuiWindowFlags.NoMove))
        {
            Util.UpdateDelayed(this.partyListStopwatch, TimeSpan.FromSeconds(1), Service.PartyListManager.UpdatePartyList);

            var partyList = Service.PartyListManager.PartyList;
            if (partyList.Count != 0)
            {
                if (ImGui.BeginTable("##PartyListTable", 3, ImGuiTableFlags.RowBg))
                {
                    for (var i = 0; i < partyList.Count; i++)
                    {
                        if (i != 0)
                        {
                            ImGui.TableNextRow();
                        }

                        ImGui.TableNextColumn();

                        var partyMember = partyList[i];
                        var iconSize = 25 * ImGuiHelpers.GlobalScale;
                        var middleCursorPosY = ImGui.GetCursorPosY() + (iconSize / 2) - (ImGui.CalcTextSize("R").Y / 2);

                        if (ImGui.Selectable($"##PartyListSel{i}", false, ImGuiSelectableFlags.SpanAllColumns, new Vector2(0, iconSize)))
                        {
                            Service.CharDataManager.DisplayedChar.FetchTextCharacter($"{partyMember.Name}@{partyMember.World}");
                        }

                        var icon = Service.GameDataManager.JobIconsManager.GetJobIcon(partyMember.JobId);
                        if (icon != null)
                        {
                            ImGui.SameLine();
                            ImGui.Image(icon.ImGuiHandle, new Vector2(iconSize));
                        }
                        else
                        {
                            ImGui.SetCursorPosY(middleCursorPosY);
                            ImGui.Text("(?)");
                            Util.SetHoverTooltip(Service.Localization.GetString("Main_IconError"));
                        }

                        ImGui.TableNextColumn();

                        ImGui.SetCursorPosY(middleCursorPosY);
                        ImGui.Text(partyMember.Name);

                        ImGui.TableNextColumn();

                        ImGui.SetCursorPosY(middleCursorPosY);
                        ImGui.Text(partyMember.World);
                    }

                    ImGui.EndTable();
                }
            }
            else
            {
                ImGui.Text(Service.Localization.GetString("Main_NoPartyMember"));
            }

            ImGui.EndPopup();
        }

        ImGui.PopStyleVar();

        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 2);

        if (!Service.FfLogsClient.IsTokenValid)
        {
            var message = FFLogsClient.IsConfigSet()
                              ? Service.Localization.GetString("Main_InvalidAPIClient")
                              : Service.Localization.GetString("Main_APIClientNotSetup");
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
            Util.CenterSelectable(message, ref this.isConfigClicked);
            ImGui.PopStyleColor();

            return;
        }

        if (this.ErrorMessage == string.Empty)
        {
            if (Service.CharDataManager.DisplayedChar.IsDataLoading)
            {
                Util.CenterText(Service.Localization.GetString("Main_Loading"));
            }
            else
            {
                if (Service.CharDataManager.DisplayedChar.IsDataReady)
                {
                    Util.CenterSelectable(
                        Service.Localization.GetString("Main_ViewingLogs").Replace("{Name}", Service.CharDataManager.DisplayedChar.LoadedFirstName).Replace("{World}", Service.CharDataManager.DisplayedChar.LoadedWorldName),
                        ref this.isProfileLinkClicked);

                    Util.SetHoverTooltip(Service.Localization.GetString("Main_OpenOnFFLogs"));
                }
                else
                {
                    Util.CenterText(Service.Localization.GetString("Main_Waiting"));
                }
            }
        }
        else
        {
            Util.CenterTextColored(ImGuiColors.DalamudRed, this.ErrorMessage);
        }

        if (Service.Configuration.Layout.Count == 0)
        {
            Util.CenterSelectable(Service.Localization.GetString("Main_NoLayoutSetup"), ref this.isConfigClicked);
        }
    }

    private static float GetButtonsWidth()
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var buttonsWidth =
            ImGui.CalcTextSize(FontAwesomeIcon.Search.ToIconString()).X +
            ImGui.CalcTextSize(FontAwesomeIcon.Crosshairs.ToIconString()).X +
            ImGui.CalcTextSize(FontAwesomeIcon.Clipboard.ToIconString()).X +
            ImGui.CalcTextSize(FontAwesomeIcon.UsersCog.ToIconString()).X +
            (ImGui.GetStyle().ItemSpacing.X * 4) + // between items
            (ImGui.GetStyle().FramePadding.X * 8); // around buttons, 2 per
        ImGui.PopFont();
        return buttonsWidth;
    }

    private static float GetMinInputWidth()
    {
        return new[]
        {
            ImGui.CalcTextSize(Service.Localization.GetString("Main_Name")).X,
            ImGui.CalcTextSize(Service.Localization.GetString("Main_World")).X,
            ImGui.CalcTextSize(Service.CharDataManager.DisplayedChar.FirstName).X,
            ImGui.CalcTextSize(Service.CharDataManager.DisplayedChar.WorldName).X,
        }.Max() + (ImGui.GetStyle().FramePadding.X * 2);
    }

    private static float GetMinWindowSize()
    {
        return ((GetMinInputWidth() + (ImGui.GetStyle().ItemSpacing.X * 2)) * 3) + GetButtonsWidth();
    }
}
