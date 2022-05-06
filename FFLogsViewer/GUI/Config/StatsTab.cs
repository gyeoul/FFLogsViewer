using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using FFLogsViewer.Manager;
using FFLogsViewer.Model;
using ImGuiNET;

namespace FFLogsViewer.GUI.Config;

public class StatsTab
{
    public static void Draw()
    {
        var hasChanged = false;
        if (ImGui.BeginCombo(Service.Localization.GetString("Stats_DefaultMetric"), Service.Configuration.Metric.Name))
        {
            foreach (var metric in GameDataManager.AvailableMetrics)
            {
                if (ImGui.Selectable(metric.Name))
                {
                    Service.Configuration.Metric = metric;
                    hasChanged = true;
                }
            }

            ImGui.EndCombo();
        }

        Util.SetHoverTooltip(Service.Localization.GetString("Stats_DefaultMetric_Help"));

        if (ImGui.BeginTable(
                "##ConfigStatsTable",
                4,
                ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp,
                new Vector2(-1, -1)))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn("##PositionCol", ImGuiTableColumnFlags.WidthFixed, 38 * ImGuiHelpers.GlobalScale);
            var minAliasSize = Service.Configuration.Stats.Select(stat => ImGui.CalcTextSize(stat.Alias).X).Prepend(ImGui.CalcTextSize(Service.Localization.GetString("Alias")).X).Max() + 10;
            ImGui.TableSetupColumn(Service.Localization.GetString("Alias"), ImGuiTableColumnFlags.WidthFixed, minAliasSize);
            ImGui.TableSetupColumn(Service.Localization.GetString("Stats"));
            ImGui.TableSetupColumn(Service.Localization.GetString("Enabled"));
            ImGui.TableHeadersRow();

            for (var i = 0; i < Service.Configuration.Stats.Count; i++)
            {
                ImGui.PushID($"##ConfigStatsTable{i}");

                var stat = Service.Configuration.Stats[i];
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                if (Util.DrawButtonIcon(FontAwesomeIcon.ArrowUp, new Vector2(2, ImGui.GetStyle().FramePadding.Y)))
                {
                    Util.IncList(Service.Configuration.Stats, i);
                    hasChanged = true;
                }

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(2, 0));

                ImGui.SameLine();
                if (Util.DrawButtonIcon(FontAwesomeIcon.ArrowDown, new Vector2(2, ImGui.GetStyle().FramePadding.Y)))
                {
                    Util.DecList(Service.Configuration.Stats, i);
                    hasChanged = true;
                }

                ImGui.PopStyleVar();

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                hasChanged |= ImGui.InputText("##Alias", ref stat.Alias, 20);

                if (stat.Type == StatType.BestAmount)
                {
                    Util.SetHoverTooltip(Service.Localization.GetString("Stats_BestAmountHelp"));
                }

                ImGui.TableNextColumn();
                ImGui.Text(stat.Name);

                ImGui.TableNextColumn();
                var offset = (ImGui.GetContentRegionAvail().X - (22 * ImGuiHelpers.GlobalScale)) / 2;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
                if (ImGui.Checkbox("##CheckBox", ref stat.IsEnabled))
                {
                    Service.MainWindow.ResetSize();
                    hasChanged = true;
                }

                ImGui.PopID();
            }

            ImGui.EndTable();
        }

        if (hasChanged)
        {
            Service.Configuration.Save();
        }
    }
}
