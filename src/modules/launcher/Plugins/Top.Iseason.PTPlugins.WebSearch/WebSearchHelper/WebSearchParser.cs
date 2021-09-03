// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Top.Iseason.PTPlugins.WebSearch.WebSearchHelper
{
    public static class WebSearchParser
    {
        private static readonly string Path = AppDomain.CurrentDomain.BaseDirectory + @"\Plugins\Top.Iseason.PTPlugins.WebSearch\config.json";
        private static readonly List<SearchEngine> SearchEngineList = new List<SearchEngine>();

        public static void Init()
        {
            using (StreamReader file = File.OpenText(Path))
            {
                using (JsonDocument document = JsonDocument.Parse(file.ReadToEnd()))
                {
                    var root = document.RootElement;
                    var tokens = root.GetProperty("searchers").EnumerateArray();
                    foreach (var data in tokens)
                    {
                        SearchEngine result = new SearchEngine
                        {
                            Key = data.GetProperty("key").ToString(),
                            Name = data.GetProperty("name").ToString(),
                            Url = data.GetProperty("url").ToString(),
                            Default = data.GetProperty("default").ToString(),
                        };
                        SearchEngineList.Add(result);
                    }
                }
            }
        }

        private static bool QueryStartsWith(string input, string key, out string query)
        {
            if (string.IsNullOrEmpty(key))
            {
                query = input;
                return true;
            }

            if (string.Equals(input, key, StringComparison.CurrentCultureIgnoreCase))
            {
                query = string.Empty;
                return true;
            }

            key += " ";
            if (input.StartsWith(key, StringComparison.CurrentCultureIgnoreCase))
            {
                query = input.Substring(key.Length);
                return true;
            }

            query = null;
            return false;
        }

        private static bool ParseWebsearchQuery(string input, SearchEngine engine, out WebsearchParserResult result)
        {
            if (QueryStartsWith(input, engine.Key, out string query))
            {
                string queryUrl;
                if (string.IsNullOrEmpty(query))
                {
                    queryUrl = engine.Default;
                }
                else
                {
                    queryUrl = string.Format(CultureInfo.InvariantCulture, engine.Url, HttpUtility.UrlEncode(query, Encoding.UTF8));
                }

                result = new WebsearchParserResult
                {
                    Urls = queryUrl,
                    Name = engine.Name,
                    Query = query,
                };

                return true;
            }

            result = null;
            return false;
        }

        public static bool ParseQuery(string input, out WebsearchParserResult result)
        {
            if (input == null)
            {
                result = null;
                return false;
            }

            foreach (var searcher in SearchEngineList)
            {
                if (ParseWebsearchQuery(input, searcher, out result))
                {
                    return true;
                }
            }

            result = null;
            return false;
        }
    }
}
