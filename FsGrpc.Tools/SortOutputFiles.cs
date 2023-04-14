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
        [Required]
        public ITaskItem[] Dependencies { get; set; }

        [Required]
        public ITaskItem[] FilesToCompile { get; set; }

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
                var dep = filesToCompile.FirstOrDefault(x=>x.Item.GetMetadata(Metadata.Source) == file.ItemSpec);
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
