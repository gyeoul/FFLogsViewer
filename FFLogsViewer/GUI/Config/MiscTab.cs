using Dalamud.Interface.Colors;
using ImGuiNET;

namespace FFLogsViewer.GUI.Config;

public class MiscTab
{
    public static void Draw()
    {
        if (ImGui.Button(Service.Localization.GetString("OpenGithubRepo")))
        {
            Util.OpenLink("https://github.com/NukoOoOoOoO/FFLogsViewer");
        }

        var hasChanged = false;

        var contextMenu = Service.Configuration.ContextMenu;
        if (ImGui.Checkbox($@"{Service.Localization.GetString("Misc_EnableContextMenu")}##ContextMenu", ref contextMenu))
        {
            if (contextMenu)
            {
                ContextMenu.Enable();
            }
            else
            {
                ContextMenu.Disable();
            }

            Service.Configuration.ContextMenu = contextMenu;
            hasChanged = true;
        }

        Util.SetHoverTooltip(Service.Localization.GetString("Misc_EnableContextMenu_Help"));

        if (Service.Configuration.ContextMenu)
        {
            ImGui.Indent();
            if (!Service.Configuration.ContextMenuStreamer)
            {
                var contextMenuButtonName = Service.Configuration.ContextMenuButtonName;
                if (ImGui.InputText($@"{Service.Localization.GetString("Misc_ButtonName")}##ContextMenuButtonName", ref contextMenuButtonName, 50))
                {
                    Service.Configuration.ContextMenuButtonName = contextMenuButtonName;
                    hasChanged = true;
                }

                var openInBrowser = Service.Configuration.OpenInBrowser;
                if (ImGui.Checkbox($@"{Service.Localization.GetString("Misc_OpenInBrowser")}##OpenInBrowser", ref openInBrowser))
                {
                    Service.Configuration.OpenInBrowser = openInBrowser;
                    hasChanged = true;
                }

                Util.SetHoverTooltip(Service.Localization.GetString("Misc_OpenInBrowser_Help"));
            }

            if (!Service.Configuration.OpenInBrowser)
            {
                var contextMenuStreamer = Service.Configuration.ContextMenuStreamer;
                if (ImGui.Checkbox($@"{Service.Localization.GetString("Misc_StreamerMode")}##ContextMenuStreamer", ref contextMenuStreamer))
                {
                    Service.Configuration.ContextMenuStreamer = contextMenuStreamer;
                    hasChanged = true;
                }

                Util.SetHoverTooltip(Service.Localization.GetString("Misc_StreamerMode_Help"));
            }

            ImGui.Unindent();
        }

        ImGui.Text(Service.Localization.GetString("Misc_API_Client"));

        var configurationClientId = Service.Configuration.ClientId;
        if (ImGui.InputText($"{Service.Localization.GetString("Misc_API_ClientID")}##ClientId", ref configurationClientId, 50))
        {
            Service.Configuration.ClientId = configurationClientId;
            Service.FfLogsClient.SetToken();
            hasChanged = true;
        }

        var configurationClientSecret = Service.Configuration.ClientSecret;
        if (ImGui.InputText($"{Service.Localization.GetString("Misc_API_ClientSecret")}##ClientSecret", ref configurationClientSecret, 50))
        {
            Service.Configuration.ClientSecret = configurationClientSecret;
            Service.FfLogsClient.SetToken();
            hasChanged = true;
        }

        if (Service.FfLogsClient.IsTokenValid)
            ImGui.TextColored(ImGuiColors.HealerGreen, Service.Localization.GetString("Misc_API_ClientValid"));
        else
            ImGui.TextColored(ImGuiColors.DalamudRed, Service.Localization.GetString("Misc_API_ClientNotValid"));

        if (ImGui.CollapsingHeader(Service.Localization.GetString("Misc_API_Client_Tutorial")))
        {
            ImGui.Bullet();
            ImGui.Text(Service.Localization.GetString("Misc_API_Client_Tutorial_1"));
            ImGui.SameLine();
            if (ImGui.Button($"{Service.Localization.GetString("ClickHere")}##APIClientLink"))
            {
                Util.OpenLink("https://www.fflogs.com/api/clients/");
            }

            ImGui.Bullet();
            ImGui.Text(Service.Localization.GetString("Misc_API_Client_Tutorial_2"));
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 7);

            ImGui.Bullet();
            ImGui.Text(Service.Localization.GetString("Misc_API_Client_Tutorial_3"));
            ImGui.SameLine();
            if (ImGui.Button($"{Service.Localization.GetString("Copy")}##APIClientCopyName"))
            {
                ImGui.SetClipboardText("Plugin");
            }

            ImGui.Bullet();
            ImGui.Text(Service.Localization.GetString("Misc_API_Client_Tutorial_4"));
            ImGui.SameLine();
            if (ImGui.Button($"{Service.Localization.GetString("Copy")}##APIClientCopyUrl"))
            {
                ImGui.SetClipboardText("https://www.example.com");
            }

            ImGui.Bullet();
            ImGui.Text(Service.Localization.GetString("Misc_API_Client_Tutorial_5"));
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 7);

            ImGui.Bullet();
            ImGui.Text(Service.Localization.GetString("Misc_API_Client_Tutorial_6"));
        }

        if (hasChanged)
        {
            Service.Configuration.Save();
        }
    }
}
