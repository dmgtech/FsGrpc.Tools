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

using NUnit.Framework;
using Microsoft.Build.Utilities;

namespace Grpc.Tools.Tests
{
    public class CSharpGeneratorTest
    {
        GeneratorServices _generator;
        protected TaskLoggingHelper _log;

        [SetUp]
        public void SetUp()
        {
            _generator = new FSharpGeneratorServices(_log);
        }

        [Test]
        public void OutputDirMetadataRecognized()
        {
            var item = Utils.MakeItem("foo.proto", "OutputDir", "out");
            var poss = _generator.GetPossibleOutputs(item);
            Assert.AreEqual(1, poss.Length);
            Assert.That(poss[0], Is.EqualTo("out/foo.proto.gen.fs") | Is.EqualTo("out\\foo.proto.gen.fs"));
        }

        [Test]
        public void OutputDirPatched()
        {
            var item = Utils.MakeItem("sub/foo.proto", "OutputDir", "out");
            var output = _generator.PatchOutputDirectory(item);
            var poss = _generator.GetPossibleOutputs(output);
            Assert.AreEqual(1, poss.Length);
            Assert.That(poss[0], Is.EqualTo("out/sub/foo.proto.gen.fs") | Is.EqualTo("out\\sub\\foo.proto.gen.fs"));
        }

        [Test]
        public void OutputDirOutsideBuildDirectory()
        {
            var item = Utils.MakeItem(
                @"C:\Users\mciccotti\.nuget\packages\fsgrpc.tools\1.0.0\build\native\include\google\protobuf\descriptor.proto",
                "OutputDir", @"obj\Debug\net7.0\",
                "ProtoRoot", @"C:\Users\mciccotti\.nuget\packages\fsgrpc.tools\1.0.0\build\native\include\google\protobuf\"
                );
            var output = _generator.PatchOutputDirectory(item);
            var poss = _generator.GetPossibleOutputs(output);
            Assert.AreEqual(1, poss.Length);
            Assert.That(poss[0], Is.EqualTo("obj/Debug/net7.0/descriptor.proto.gen.fs") | Is.EqualTo("obj\\Debug\\net7.0\\descriptor.proto.gen.fs"));
        }
    };
}
