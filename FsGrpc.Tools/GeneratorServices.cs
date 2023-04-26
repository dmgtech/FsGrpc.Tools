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

using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Grpc.Tools
{
    // Abstract class for language-specific analysis behavior, such
    // as guessing the generated files the same way protoc does.
    public abstract class GeneratorServices
    {
        protected readonly TaskLoggingHelper Log;
        protected GeneratorServices(TaskLoggingHelper log) { Log = log; }

        public abstract string[] GetPossibleOutputs(ITaskItem protoItem);

        // Calculate part of proto path relative to root. Protoc is very picky
        // about them matching exactly, so can be we. Expect root be exact prefix
        // to proto, minus some slash normalization.
        protected static string GetRelativeDir(string root, string proto, TaskLoggingHelper log)
        {
            string protoDir = Path.GetDirectoryName(proto);
            string rootDir = EndWithSlash(Path.GetDirectoryName(EndWithSlash(root)));
            if (rootDir == s_dotSlash)
            {
                // Special case, otherwise we can return "./" instead of "" below!
                return protoDir;
            }
            if (Platform.IsFsCaseInsensitive)
            {
                protoDir = protoDir.ToLowerInvariant();
                rootDir = rootDir.ToLowerInvariant();
            }
            protoDir = EndWithSlash(protoDir);
            if (!protoDir.StartsWith(rootDir))
            {
                log.LogWarning("Protobuf item '{0}' has the ProtoRoot metadata '{1}' " +
                               "which is not prefix to its path. Cannot compute relative path.",
                    proto, root);
                return "";
            }
            return protoDir.Substring(rootDir.Length);
        }

        // './' or '.\', normalized per system.
        protected static string s_dotSlash = "." + Path.DirectorySeparatorChar;

        protected static string EndWithSlash(string str)
        {
            if (str == "")
            {
                return s_dotSlash;
            }

            if (str[str.Length - 1] != '\\' && str[str.Length - 1] != '/')
            {
                return str + Path.DirectorySeparatorChar;
            }

            return str;
        }
    };

    public class FSharpGeneratorServices : GeneratorServices
    {
        public FSharpGeneratorServices(TaskLoggingHelper log) : base(log) { }

        public override string[] GetPossibleOutputs(ITaskItem protoItem)
        {
            string proto = protoItem.ItemSpec;
            string root = protoItem.GetMetadata(Metadata.ProtoRoot);
            string filename = Path.GetFileNameWithoutExtension(proto);
            string outdir = protoItem.GetMetadata(Metadata.OutputDir);
            string relative = GetRelativeDir(root, proto, Log);
            string pathStem = Path.Combine(outdir, relative, filename);
            return new string[] { pathStem + ".proto.gen.fs" };
        }
    };
}
