using System.Collections.Generic;
using Dalamud;
using Dalamud.Game;
using FFLogsViewer.Properties;
using Newtonsoft.Json;

namespace FFLogsViewer.Manager;

public class LocalizationManager
{
    public enum Language
    {
        English,
        ChineseSimplified
    }

    private readonly Dictionary<Language, Dictionary<string, string?>> _strings = new();

    private readonly Language currentLanguage;

    public LocalizationManager()
    {
        LoadStrings(Language.English);
        LoadStrings(Language.ChineseSimplified);

        this.currentLanguage = Service.DataManager.Language == (ClientLanguage)4
                                   ? Language.ChineseSimplified
                                   : Language.English;
    }

    public List<Language> AvailableLanguages { get; } = new();

    public string? GetString(string? key)
    {
        return this._strings[this.currentLanguage].ContainsKey(key) ? this._strings[this.currentLanguage][key] : key;
    }

    private void LoadStrings(Language lang)
    {
        var str = lang switch
        {
            Language.English => Resources.en,
            Language.ChineseSimplified => Resources.zh_CN,
            _ => Resources.en,
        };

        this._strings[lang] = JsonConvert.DeserializeObject<Dictionary<string, string>>(str)!;
    }
}
