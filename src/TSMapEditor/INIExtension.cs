﻿using Rampastring.Tools;
using System;

namespace TSMapEditor
{
    public static class INIExtension
    {
        /// <summary>
        /// Performs an action for every value of an INI section.
        /// </summary>
        public static void DoForEveryValueInSection(this IniFile iniFile, string sectionName, Action<string> action)
        {
            var section = iniFile.GetSection(sectionName);
            if (section == null)
                return;

            foreach (var kvp in section.Keys)
            {
                if (string.IsNullOrWhiteSpace(kvp.Value))
                    continue;

                action(kvp.Value);
            }
        }
    }
}
