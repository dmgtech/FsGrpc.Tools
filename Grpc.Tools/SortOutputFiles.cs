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

        public class Wrapper: IComparable<Wrapper>, IComparable
        {
            public ITaskItem Item{get;set;}
            public Wrapper(ITaskItem taskItem)
            {
                Item = taskItem;
            }

            int IComparable<Wrapper>.CompareTo(Wrapper other)
            {
                return this.Item.ItemSpec.CompareTo(other.Item.ItemSpec);
            }

            public int CompareTo(object obj)
            {
                var otherObj = obj as Wrapper;
                return (this as IComparable<Wrapper>).CompareTo(otherObj);
            }
        }

        public override bool Execute()
        {
            var dependencyLookup = new Dictionary<string, List<Wrapper>>();

            foreach(var file in Dependencies)
            {
                var dep = new Wrapper(new TaskItem(file));
                var fileName = dep.Item.GetMetadata(Metadata.Source);

                List<Wrapper> deps = null;
                if(dependencyLookup.TryGetValue(fileName, out deps))
                {
                    deps.Add(dep);
                }
                else
                {
                    dependencyLookup[fileName] = new List<Wrapper> { dep };
                }
            }

            Func<Wrapper, IEnumerable<Wrapper>> depLookup = input => {
                var source = input.Item.ItemSpec;
                foreach(var item in dependencyLookup)
                {
                    if(item.Key.Contains(source))
                    {
                        return item.Value;
                    }
                }
                return new Wrapper[] {};
            };
            var sorter = DependencySort.depSort(depLookup);

            SortedFilesToCompile = FilesToCompile.Select(x=>new Wrapper(x)).OrderBy(sorter).Select(x=>x.Item).ToArray();

            return !Log.HasLoggedErrors;
        }
    };
}
