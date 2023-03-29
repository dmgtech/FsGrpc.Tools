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
using Medallion.Collections;

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
            var filesToCompile = FilesToCompile.Select(x=>new Wrapper(x)).ToArray();
            foreach(var file in Dependencies)
            {
                var fileName = file.GetMetadata(Metadata.Source);
                var dep = filesToCompile.FirstOrDefault(x=>x.Item.ItemSpec.Contains(file.ItemSpec));
                if(dep != null){
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
            }

            Func<Wrapper, IEnumerable<Wrapper>> depLookup = input => {
                foreach(var item in dependencyLookup)
                {
                    if(item.Key.Contains(input.Item.ItemSpec))
                    {
                        return item.Value;
                    }
                }
                return new Wrapper[] {};
            };
            
            SortedFilesToCompile = filesToCompile.OrderTopologicallyBy(depLookup).Select(x=>x.Item).ToArray();

            return !Log.HasLoggedErrors;
        }
    };
}
