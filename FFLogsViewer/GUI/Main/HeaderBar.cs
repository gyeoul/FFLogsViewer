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
    public string ErrorMessage = string.Empty;
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
        ImGui.InputTextWithHint("##FirstName", "名字", ref Service.CharDataManager.DisplayedChar.FirstName, 15, ImGuiInputTextFlags.CharsNoBlank);

        ImGui.SameLine();
        ImGui.SetNextItemWidth(calcInputSize);
        ImGui.InputTextWithHint("##WorldName", "服务器", ref Service.CharDataManager.DisplayedChar.WorldName, 15, ImGuiInputTextFlags.CharsNoBlank);

        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.Search))
        {
            Service.CharDataManager.DisplayedChar.FetchData();
        }

        Util.SetHoverTooltip("搜索");

        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.Crosshairs))
        {
            Service.CharDataManager.DisplayedChar.FetchTargetChar();
        }

        Util.SetHoverTooltip("当前目标");

        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.Clipboard))
        {
            Service.CharDataManager.DisplayedChar.FetchClipboardCharacter();
        }

        Util.SetHoverTooltip("从剪贴板导入");

        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.UsersCog))
        {
            ImGui.OpenPopup("##PartyList");
        }

        Util.SetHoverTooltip("小队成员");

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

                        var icon = Service.PartyListManager.GetJobIcon(partyMember.JobId);
                        var iconSize = 25 * ImGuiHelpers.GlobalScale;
                        if (icon != null)
                        {
                            ImGui.Image(icon.ImGuiHandle, new Vector2(iconSize));
                        }
                        else
                        {
                            ImGui.Text("(?)");
                            Util.SetHoverTooltip("在获取图标时发生错误.\n" +
                                                 "Please create an issue on the GitHub with a screenshot of the red lines in /xllog.\n" +
                                                 "This is probably due to TexTools corrupting your game files.\n" +
                                                 "This shouldn't affect the party members functionality.");
                        }

                        ImGui.SameLine();
                        if (ImGui.Selectable($"##PartyListSel{i}", false, ImGuiSelectableFlags.SpanAllColumns, new Vector2(0, iconSize)))
                        {
                            Service.CharDataManager.DisplayedChar.FetchTextCharacter($"{partyMember.Name}@{partyMember.World}");
                        }

                        ImGui.TableNextColumn();

                        var middleCursorPosY = ImGui.GetCursorPosY() + (iconSize / 2) - (ImGui.CalcTextSize("R").Y / 2);
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
                ImGui.Text("找不到小队成员");
            }

            ImGui.EndPopup();
        }

        ImGui.PopStyleVar();

        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 2);

        if (!Service.FfLogsClient.IsTokenValid)
        {
            var message = FFLogsClient.IsConfigSet()
                              ? "API client 无效, 点我打开设置."
                              : "API client 为空, 点我打开设置.";
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
            Util.CenterSelectable(message, ref this.isConfigClicked);
            ImGui.PopStyleColor();

            return;
        }

        if (this.ErrorMessage == string.Empty)
        {
            if (Service.CharDataManager.DisplayedChar.IsDataLoading)
            {
                Util.CenterText("加载中...");
            }
            else
            {
                if (Service.CharDataManager.DisplayedChar.IsDataReady)
                {
                    Util.CenterSelectable(
                        $"正在查看 {Service.CharDataManager.DisplayedChar.LoadedFirstName}@{Service.CharDataManager.DisplayedChar.LoadedWorldName} 的logs",
                        ref this.isProfileLinkClicked);

                    Util.SetHoverTooltip("点我到 FFlogs 上查看");
                }
                else
                {
                    Util.CenterText("等待玩家信息...");
                }
            }
        }
        else
        {
            Util.CenterTextColored(ImGuiColors.DalamudRed, this.ErrorMessage);
        }

        if (Service.Configuration.Layout.Count == 0)
        {
            Util.CenterSelectable("你还没设置布局呢. 点我打开菜单.", ref this.isConfigClicked);
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
            ImGui.CalcTextSize("名字").X,
            ImGui.CalcTextSize("服务器").X,
            ImGui.CalcTextSize(Service.CharDataManager.DisplayedChar.FirstName).X,
            ImGui.CalcTextSize(Service.CharDataManager.DisplayedChar.WorldName).X,
        }.Max() + (ImGui.GetStyle().FramePadding.X * 2);
    }

    private static float GetMinWindowSize()
    {
        return ((GetMinInputWidth() + (ImGui.GetStyle().ItemSpacing.X * 2)) * 3) + GetButtonsWidth();
    }
}
