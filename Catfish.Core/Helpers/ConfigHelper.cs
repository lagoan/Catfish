﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catfish.Core.Helpers
{
    public static class ConfigHelper
    {
        public static List<string> GetSettingArray(string key, char seperator)
        {
            var val = ConfigurationManager.AppSettings[key];
            if (val != null)
            {
                List<string> arr = val.Split(new char[] { seperator }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => s.Length > 0)
                    .ToList();
                return arr;
            }
            else
                return new List<string>();
        }

        private static List<CultureInfo> mLanguages;
        public static List<CultureInfo> Languages
        {
            get
            {
                if (mLanguages == null)
                {
                    var codes = GetSettingArray("LanguageCodes", '|');

                    if (codes.Count == 0)
                        mLanguages = new List<CultureInfo>() { new CultureInfo("en") };
                    else
                    {
                        mLanguages = new List<CultureInfo>();
                        for (int i = 0; i < codes.Count; ++i)
                            mLanguages.Add(new CultureInfo(codes[i]));
                    }
                }
                return mLanguages;
            }
        }

        public static string GetLanguageLabel(string languageCode)
        {
            string label = Languages.Where(c => c.TwoLetterISOLanguageName == languageCode).Select(c => c.NativeName).FirstOrDefault();
            return string.IsNullOrEmpty(label) ? languageCode : label;
        }
    }
}
