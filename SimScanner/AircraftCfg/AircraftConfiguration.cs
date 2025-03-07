﻿/*
 * Copyright (c) 2021. Bert Laverman
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using IniParser;
using IniParser.Model;
using Rakis.Logging;
using SimScanner.Sim;
using SimScanner.Model;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SimScanner.AircraftCfg
{
    public abstract class AircraftConfiguration
    {
        public const string CategoryAirplanes = "Airplanes";
        public const string CategoryRotorcraft = "Rotorcraft";

        private static readonly Logger log = Logger.GetLogger(typeof(AircraftConfiguration));

        public Simulator Simulator { get; set; }
        public List<string> Categories { get; } = new List<string> {
            CategoryAirplanes, CategoryRotorcraft,
        };

        private readonly List<Aircraft> entries = new();
        public List<Aircraft> Entries => entries;

        public AircraftConfiguration(Simulator simulator)
        {
            Simulator = simulator;
        }

        public abstract void ScanSimObjects();

        public ICollection<string> ScannedCategories => new List<string>(Directory.GetFiles(Path.Combine(Simulator.InstallationPath, "SimObjects")));

        protected void AddAircraftFrom(string filename, string category)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.AllowCreateSectionsOnFly = false;
            parser.Parser.Configuration.AllowDuplicateKeys = true;
            parser.Parser.Configuration.AllowDuplicateSections = true;
            parser.Parser.Configuration.CaseInsensitive = true;
            parser.Parser.Configuration.CommentRegex = new Regex(@"(#|;|//)(.*)");
            parser.Parser.Configuration.ThrowExceptionsOnError = false;

            log.Debug?.Log($"Reading {filename}");
            IniData data = parser.ReadFile(filename);
            if (data == null)
            {
                log.Error?.Log($"Unable to parse {filename}. Skipping.");
                return;
            }
            var general = data["general"];
            var variation = data["variation"];

            string type = (general == null) ? ("["+variation["base_container"]+"]") : general["atc_type"];
            string model = (general == null) ? ("[" + variation["base_container"] + "]") : general["atc_model"];
            foreach (SectionData collection in data.Sections)
            {
                string heading = collection.SectionName.ToLower();
                if (heading.StartsWith("fltsim."))
                {
                    var fltsim = data[heading];
                    log.Debug?.Log($"Adding '{fltsim["title"]}'");
                    entries.Add(new(fltsim["title"], fltsim["atc_type"] ?? type, fltsim["atc_model"] ?? model, general?["category"] ?? category));
                }
            }
        }

        protected void ScanDirectory(string path, string category = null)
        {
            log.Debug?.Log($"Scanning '{path}' for aircraft.");
            foreach (string objectPath in Directory.GetDirectories(path))
            {
                string filename = Path.Combine(objectPath, "aircraft.cfg");
                if (File.Exists(filename))
                {
                    AddAircraftFrom(filename, category);
                }
            }
        }

        public void SortEntries()
        {
            entries.Sort((Aircraft e1, Aircraft e2) => e1.Title.CompareTo(e2.Title));
            log.Trace?.Log($"Sorted {entries.Count} simobject entries.");
        }
    }
}
