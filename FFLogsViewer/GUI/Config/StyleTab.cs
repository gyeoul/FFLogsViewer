using ImGuiNET;

namespace FFLogsViewer.GUI.Config;

public class StyleTab
{
    public static void Draw()
    {
        var style = Service.Configuration.Style;
        var hasStyleChanged = false;
        ImGui.Text(Service.Localization.GetString("Style_MainWindow"));

        ImGui.Indent();

        if (ImGui.Checkbox(Service.Localization.GetString("Style_CloseWindowWithESC"), ref style.IsCloseHotkeyRespected))
        {
            Service.MainWindow.RespectCloseHotkey = style.IsCloseHotkeyRespected;
            hasStyleChanged = true;
        }

        var tmpWindowFlags = (int)style.MainWindowFlags;
        if (ImGui.CheckboxFlags(Service.Localization.GetString("Style_NoTitleBar"), ref tmpWindowFlags, (int)ImGuiWindowFlags.NoTitleBar))
        {
            style.MainWindowFlags = (ImGuiWindowFlags)tmpWindowFlags;
            Service.MainWindow.Flags = style.MainWindowFlags;
            hasStyleChanged = true;
        }

        if (ImGui.RadioButton(Service.Localization.GetString("Style_FixedSize"), style.IsSizeFixed))
        {
            style.IsSizeFixed = true;
            style.MainWindowFlags &= ~ImGuiWindowFlags.AlwaysAutoResize;
            Service.MainWindow.Flags = style.MainWindowFlags;
            hasStyleChanged = true;
        }

        ImGui.SameLine();
        var isAutoResizeFlagSet = (style.MainWindowFlags & ImGuiWindowFlags.AlwaysAutoResize) != 0;
        if (ImGui.RadioButton(Service.Localization.GetString("Style_AutoResize"), isAutoResizeFlagSet && !style.IsSizeFixed))
        {
            style.IsSizeFixed = false;
            style.MainWindowFlags |= ImGuiWindowFlags.AlwaysAutoResize;
            Service.MainWindow.Flags = style.MainWindowFlags;
            Service.MainWindow.ResetSize();
            hasStyleChanged = true;
        }

        Util.DrawHelp(Service.Localization.GetString("Style_AutoResize_Help"));

        ImGui.SameLine();
        if (ImGui.RadioButton(Service.Localization.GetString("Style_FixedMinSize"), !isAutoResizeFlagSet && !style.IsSizeFixed))
        {
            style.IsSizeFixed = false;
            style.MainWindowFlags &= ~ImGuiWindowFlags.AlwaysAutoResize;
            Service.MainWindow.Flags = style.MainWindowFlags;
            hasStyleChanged = true;
        }

        Util.DrawHelp(Service.Localization.GetString("Style_FixedMinSize_Help"));

        if (style.IsSizeFixed)
        {
            ImGui.Indent();

            if (ImGui.CheckboxFlags(Service.Localization.GetString("Style_NoResize"), ref tmpWindowFlags, (int)ImGuiWindowFlags.NoResize))
            {
                style.MainWindowFlags = (ImGuiWindowFlags)tmpWindowFlags;
                Service.MainWindow.Flags = style.MainWindowFlags;
                hasStyleChanged = true;
            }

            ImGui.Unindent();
        }

        if (!isAutoResizeFlagSet && !style.IsSizeFixed)
        {
            ImGui.Indent();
            hasStyleChanged |=
                ImGui.SliderFloat(Service.Localization.GetString("Style_MinSize"), ref Service.Configuration.Style.MinMainWindowWidth, 1, 2000);

            ImGui.Unindent();
        }

        ImGui.Unindent();

        ImGui.Text(Service.Localization.GetString("Style_MainWindowTable"));

        ImGui.Indent();

        if (ImGui.Button(Service.Localization.GetString("Style_BordersCustomization")))
        {
            ImGui.OpenPopup("##Borders");
        }

        hasStyleChanged |= ImGui.Checkbox(Service.Localization.GetString("Style_HeaderSeparator"), ref style.IsHeaderSeparatorDrawn);

        var tmpTableFlags2 = (int)style.MainTableFlags;
        if (ImGui.CheckboxFlags($"{Service.Localization.GetString("Style_AlternateRowBackground")}##TableFlag", ref tmpTableFlags2, (int)ImGuiTableFlags.RowBg))
        {
            style.MainTableFlags = (ImGuiTableFlags)tmpTableFlags2;
            hasStyleChanged = true;
        }

        if (ImGui.BeginPopup("##Borders", ImGuiWindowFlags.NoMove))
        {
            var tmpTableFlags = (int)style.MainTableFlags;
            var hasChanged = false;
            hasChanged |= ImGui.CheckboxFlags($"{Service.Localization.GetString("Style_BordersCustomization_Borders")}##TableFlag", ref tmpTableFlags, (int)ImGuiTableFlags.Borders);
            hasChanged |= ImGui.CheckboxFlags($"{Service.Localization.GetString("Style_BordersCustomization_BordersH")}##TableFlag", ref tmpTableFlags, (int)ImGuiTableFlags.BordersH);
            hasChanged |= ImGui.CheckboxFlags($"{Service.Localization.GetString("Style_BordersCustomization_BordersV")}##TableFlag", ref tmpTableFlags, (int)ImGuiTableFlags.BordersV);
            hasChanged |= ImGui.CheckboxFlags($"{Service.Localization.GetString("Style_BordersCustomization_BordersInner")}##TableFlag", ref tmpTableFlags, (int)ImGuiTableFlags.BordersInner);
            hasChanged |= ImGui.CheckboxFlags($"{Service.Localization.GetString("Style_BordersCustomization_BordersOuter")}##TableFlag", ref tmpTableFlags, (int)ImGuiTableFlags.BordersOuter);
            hasChanged |= ImGui.CheckboxFlags($"{Service.Localization.GetString("Style_BordersCustomization_BordersInnerH")}##TableFlag", ref tmpTableFlags, (int)ImGuiTableFlags.BordersInnerH);
            hasChanged |= ImGui.CheckboxFlags($"{Service.Localization.GetString("Style_BordersCustomization_BordersInnerV")}##TableFlag", ref tmpTableFlags, (int)ImGuiTableFlags.BordersInnerV);
            hasChanged |= ImGui.CheckboxFlags($"{Service.Localization.GetString("Style_BordersCustomization_BordersOuterH")}##TableFlag", ref tmpTableFlags, (int)ImGuiTableFlags.BordersOuterH);
            hasChanged |= ImGui.CheckboxFlags($"{Service.Localization.GetString("Style_BordersCustomization_BordersOuterV")}##TableFlag", ref tmpTableFlags, (int)ImGuiTableFlags.BordersOuterV);
            if (hasChanged)
            {
                style.MainTableFlags = (ImGuiTableFlags)tmpTableFlags;
                hasStyleChanged = true;
            }
        }

        ImGui.Unindent();

        if (hasStyleChanged)
        {
            Service.Configuration.Save();
        }
    }
}
