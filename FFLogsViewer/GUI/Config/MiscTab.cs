using System;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Logging;
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

        var isCachingEnabled = Service.Configuration.IsCachingEnabled;
        if (ImGui.Checkbox("Enable caching", ref isCachingEnabled))
        {
            Service.Configuration.IsCachingEnabled = isCachingEnabled;
            hasChanged = true;
        }

        Util.DrawHelp("Build a cache of fetched characters to avoid using too much API points (see Layout tab for more info on points).\n" +
                      "The cache is cleared every hour, you can also manually clear it in the main window.");

        ImGui.Text("API client:");

        var configurationClientId = Service.Configuration.ClientId;
        if (ImGui.InputText($"{Service.Localization.GetString("Misc_API_ClientID")}##ClientId", ref configurationClientId, 50))
        {
            Service.Configuration.ClientId = configurationClientId;
            Service.FFLogsClient.SetToken();
            hasChanged = true;
        }

        var configurationClientSecret = Service.Configuration.ClientSecret;
        if (ImGui.InputText($"{Service.Localization.GetString("Misc_API_ClientSecret")}##ClientSecret", ref configurationClientSecret, 50))
        {
            Service.Configuration.ClientSecret = configurationClientSecret;
            Service.FFLogsClient.SetToken();
            hasChanged = true;
        }

        if (Service.FFLogsClient.IsTokenValid)
        {
            ImGui.TextColored(ImGuiColors.HealerGreen, "This client is valid.");
        }
        else
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, "This client is NOT valid.");
            if (FFLogsClient.IsConfigSet())
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
                ImGui.TextWrapped("If you are certain that the API client is valid, this may indicate that FF Logs is unreachable.\nMake sure you can open it in your browser before trying again.");
                ImGui.PopStyleColor();
                if (ImGui.Button("Open FF Logs"))
                {
                    Util.OpenLink("https://www.fflogs.com/");
                }

                ImGui.SameLine();
                if (ImGui.Button("Try again"))
                {
                    Service.FFLogsClient.SetToken();
                }
            }
        }

        if (ImGui.CollapsingHeader(Service.Localization.GetString("Misc_API_Client_Tutorial")))
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Bullet();
            ImGui.Text(Service.Localization.GetString("Misc_API_Client_Tutorial_1"));
            ImGui.SameLine();
            if (ImGui.Button($"{Service.Localization.GetString("ClickHere")}##APIClientLink"))
            {
                Util.OpenLink("https://www.fflogs.com/api/clients/");
            }

            ImGui.AlignTextToFramePadding();
            ImGui.Bullet();
            ImGui.Text(Service.Localization.GetString("Misc_API_Client_Tutorial_2"));

            ImGui.AlignTextToFramePadding();
            ImGui.Bullet();
            ImGui.Text(Service.Localization.GetString("Misc_API_Client_Tutorial_3"));
            ImGui.SameLine();
            if (ImGui.Button($"{Service.Localization.GetString("Copy")}##APIClientCopyName"))
            {
                CopyToClipboard("Plugin");
            }

            ImGui.AlignTextToFramePadding();
            ImGui.Bullet();
            ImGui.Text(Service.Localization.GetString("Misc_API_Client_Tutorial_4"));
            ImGui.SameLine();
            if (ImGui.Button($"{Service.Localization.GetString("Copy")}##APIClientCopyUrl"))
            {
                CopyToClipboard("https://www.example.com");
            }

            ImGui.AlignTextToFramePadding();
            ImGui.Bullet();
            ImGui.Text(Service.Localization.GetString("Misc_API_Client_Tutorial_5"));

            ImGui.AlignTextToFramePadding();
            ImGui.Bullet();
            ImGui.Text(Service.Localization.GetString("Misc_API_Client_Tutorial_6"));
        }

        if (hasChanged)
        {
            Service.Configuration.Save();
        }
    }

    private static void CopyToClipboard(string text)
    {
        try
        {
            ImGui.SetClipboardText(text);
            Service.Interface.UiBuilder.AddNotification(text, "Copied to clipboard", NotificationType.Success);
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Could not set clipboard text.");
            Service.Interface.UiBuilder.AddNotification(text, "Could not copy to clipboard", NotificationType.Error);
        }
    }
}
