#region Copyright notice and license

// Copyright 2018 gRPC authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Linq;

namespace Grpc.Tools
{
    public class SortOutputFiles : Task
    {
        /// <summary>
        /// </summary>
        [Required]
        public ITaskItem[] Dependencies { get; set; }

        /// <summary>
        /// </summary>
        [Required]
        public ITaskItem[] FilesToCompile { get; set; }

        /// <summary>
        /// </summary>
        [Output]
        public ITaskItem[] SortedFilesToCompile { get; private set; }

        public override bool Execute()
        {
            //System.Diagnostics.Debugger.Launch();
            var dependencyLookup = new Dictionary<string, List<TaskItem>>();

            foreach(var file in Dependencies)
            {
                var dep = new TaskItem(file);
                //Log.LogWarning($"proto: {dep.GetMetadata(Metadata.Source)} dependency: {dep.ItemSpec}");
                var fileName = dep.GetMetadata(Metadata.Source); //add .gen.fs ending
                Log.LogWarning($"Source: {fileName}");
                List<TaskItem> deps = null;
                if(dependencyLookup.TryGetValue(fileName, out deps))
                {
                    deps.Add(dep);
                }
                else
                {
                    dependencyLookup[fileName] = new List<TaskItem> { dep };
                }
            }

            Func<ITaskItem, IEnumerable<ITaskItem>> depLookup = input => {
                //if(dependencyLookup.ContainsKey())
                //{
//
                //}
                //else
                //{
                     return new TaskItem[] {};
                //}
            };
            var sorter = DependencySort.depSort(depLookup);

            SortedFilesToCompile = FilesToCompile.OrderBy(sorter).ToArray();

            return !Log.HasLoggedErrors;
        }
    };
}
