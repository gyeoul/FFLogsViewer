using System;
using System.Numerics;
using Dalamud.Interface;
using FFLogsViewer.Manager;
using ImGuiNET;

namespace FFLogsViewer.GUI.Main;

public class MenuBar
{
    public static void Draw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(6 * ImGuiHelpers.GlobalScale, ImGui.GetStyle().ItemSpacing.Y));

        if (ImGui.BeginMenuBar())
        {
            ImGui.PushFont(UiBuilder.IconFont);

            if (ImGui.MenuItem(FontAwesomeIcon.Eraser.ToIconString()))
            {
                Service.CharDataManager.ResetDisplayedChar();
                Service.MainWindow.SetErrorMessage(string.Empty);
                Service.MainWindow.ResetSize();
            }

            if (ImGui.MenuItem(FontAwesomeIcon.Cog.ToIconString()))
            {
                Service.ConfigWindow.Toggle();
            }

            ImGui.PopFont();

            ImGui.PushStyleColor(ImGuiCol.Text, Service.CharDataManager.DisplayedChar.Job.Color);
            if (ImGui.BeginMenu(Service.CharDataManager.DisplayedChar.Job.Name + "##JobMenu"))
            {
                foreach (var job in Service.GameDataManager.Jobs)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, job.Color);
                    if (ImGui.MenuItem(job.Name))
                    {
                        Service.CharDataManager.DisplayedChar.Job = job;
                        if (Service.CharDataManager.DisplayedChar.IsInfoSet())
                        {
                            Service.CharDataManager.DisplayedChar.FetchData();
                        }
                    }

                    ImGui.PopStyleColor();
                }

                ImGui.EndMenu();
            }

            ImGui.PopStyleColor();

            if (ImGui.BeginMenu(Service.CharDataManager.DisplayedChar.OverriddenMetric != null
                                    ? Service.CharDataManager.DisplayedChar.OverriddenMetric.Name
                                    : Service.Configuration.Metric.Name))
            {
                foreach (var metric in GameDataManager.AvailableMetrics)
                {
                    if (ImGui.MenuItem(metric.Name))
                    {
                        Service.CharDataManager.DisplayedChar.OverriddenMetric = metric;
                        if (Service.CharDataManager.DisplayedChar.IsInfoSet())
                        {
                            Service.CharDataManager.DisplayedChar.FetchData();
                        }
                    }
                }

                ImGui.EndMenu();
            }

            if (!Service.Configuration.IsUpdateDismissed)
            {
                var isButtonHidden = !ImGui.IsPopupOpen("##UpdateMessage") && DateTime.Now.Second % 2 == 0;
                if (isButtonHidden)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, Vector4.Zero);
                }

                ImGui.PushFont(UiBuilder.IconFont);

                ImGui.SameLine();
                if (ImGui.MenuItem(FontAwesomeIcon.InfoCircle.ToIconString()))
                {
                    ImGui.OpenPopup("##UpdateMessage");
                }

                ImGui.PopFont();

                if (isButtonHidden)
                {
                    ImGui.PopStyleColor();
                }

                if (!Service.Configuration.IsUpdateDismissed)
                {
                    Util.SetHoverTooltip("Update message");
                }

                if (ImGui.BeginPopup("##UpdateMessage", ImGuiWindowFlags.NoMove))
                {
                    ImGui.Text(Service.Localization.GetString("MenuBar_UpdateMessage_Text"));

                    if (ImGui.Button($"{Service.Localization.GetString("Dismiss")}##UpdateMessage"))
                    {
                        Service.Configuration.IsUpdateDismissed = true;
                        Service.Configuration.Save();
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.SameLine();
                    if (ImGui.Button(Service.Localization.GetString("OpenGithubRepo")))
                    {
                        Util.OpenLink("https://github.com/NukoOoOoOoO/FFLogsViewer");
                    }

                    ImGui.EndPopup();
                }
            }

            ImGui.EndMenuBar();
        }

        ImGui.PopStyleVar();
    }
}
