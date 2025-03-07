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

using SimScanner.Sim;
using System.Collections.Generic;

namespace SimScanner.Scenery
{
    public abstract class SceneryConfiguration
    {

        public Simulator Simulator { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool CleanOnExit { get; set; }

        protected readonly List<SceneryEntry> entries = new();
        public List<SceneryEntry> Entries => entries;


        public SceneryConfiguration(Simulator simulator)
        {
            Simulator = simulator;
        }

        public abstract void LoadSceneryConfig();
        public abstract void LoadAddOnScenery();
        public abstract void SortEntries();
    }
}