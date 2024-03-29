﻿using System.Globalization;

namespace RegulSaveCleaner.Core.Constants;

public static class LocalizedNames
{
    /// <summary>
    /// Localized title of the game The Sims 3
    /// </summary>
    public static string TheSims3
    {
        get => CultureInfo.CurrentCulture.TwoLetterISOLanguageName switch
        {
            "de" => "Die Sims 3",
            "fr" => "Les Sims 3",
            "es" => "Los Sims 3",
            "nl" => "De Sims 3",
            _ => "The Sims 3"
        };
    }
}