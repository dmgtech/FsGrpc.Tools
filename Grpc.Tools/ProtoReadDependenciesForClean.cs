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

using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Linq;

namespace Grpc.Tools
{
    public class ProtoReadDependenciesForClean : Task
    {
        /// <summary>
        /// Protoc dependency files
        /// </summary>
        [Required]
        public string[] ProtoDepFiles { get; set; }

        /// <summary>
        /// All the files to delete during a clean
        /// </summary>
        [Output]
        public ITaskItem[] FilesToClean { get; private set; }

        public override bool Execute()
        {
            FilesToClean =
                ProtoDepFiles
                    .SelectMany(file =>
                        DepFileUtil.ReadDependencyOutputs(file, Log)
                            .Select(output => new TaskItem(output))
                    ).ToArray();

            return !Log.HasLoggedErrors;
        }
    };
}