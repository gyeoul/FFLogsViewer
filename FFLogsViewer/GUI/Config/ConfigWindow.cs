using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace FFLogsViewer.GUI.Config;

public class ConfigWindow : Window
{
    public LayoutTab LayoutTab = new();

    public ConfigWindow()
        : base("Configuration##FFLogsViewerConfigWindow")
    {
        this.RespectCloseHotkey = true;

        this.Flags = ImGuiWindowFlags.AlwaysAutoResize;
    }

    public override void Draw()
    {
        ImGui.BeginTabBar("ConfigTabs");

        if (ImGui.BeginTabItem("其他"))
        {
            MiscTab.Draw();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("布局"))
        {
            this.LayoutTab.Draw();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("数据"))
        {
            StatsTab.Draw();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("样式"))
        {
            StyleTab.Draw();
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }
}
