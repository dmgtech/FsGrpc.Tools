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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Grpc.Tools.Tests
{
    public class DepFileUtilTest
    {

        [Test]
        public void HashString64Hex_IsSane()
        {
            string hashFoo1 = DepFileUtil.HashString64Hex("foo");
            string hashEmpty = DepFileUtil.HashString64Hex("");
            string hashFoo2 = DepFileUtil.HashString64Hex("foo");

            StringAssert.IsMatch("^[a-f0-9]{16}$", hashFoo1);
            Assert.That(hashFoo1, Is.EqualTo(hashFoo2));
            Assert.That(hashFoo1, Is.Not.EqualTo(hashEmpty));
        }

        [Test]
        public void GetDepFilenameForProto_IsSane()
        {
            StringAssert.IsMatch(@"^out[\\/][a-f0-9]{16}_foo.protodep$",
                DepFileUtil.GetDepFilenameForProto("out", "foo.proto"));
            StringAssert.IsMatch(@"^[a-f0-9]{16}_foo.protodep$",
                DepFileUtil.GetDepFilenameForProto("", "foo.proto"));
        }

        [Test]
        public void GetDepFilenameForProto_HashesDir()
        {
            string PickHash(string fname) =>
                DepFileUtil.GetDepFilenameForProto("", fname).Substring(0, 16);

            string same1 = PickHash("dir1/dir2/foo.proto");
            string same2 = PickHash("dir1/dir2/proto.foo");
            string same3 = PickHash("dir1/dir2/proto");
            string same4 = PickHash("dir1/dir2/.proto");
            string unsame1 = PickHash("dir2/foo.proto");
            string unsame2 = PickHash("/dir2/foo.proto");

            Assert.That(same1, Is.EqualTo(same2));
            Assert.That(same1, Is.EqualTo(same3));
            Assert.That(same1, Is.EqualTo(same4));
            Assert.That(same1, Is.Not.EqualTo(unsame1));
            Assert.That(unsame1, Is.Not.EqualTo(unsame2));
        }

        [Test]
        public void GetOutputDirWithHash_IsSane()
        {
            StringAssert.IsMatch(@"^out[\\/][a-f0-9]{16}$",
                DepFileUtil.GetOutputDirWithHash("out", "foo.proto"));
            StringAssert.IsMatch(@"^[a-f0-9]{16}$",
                DepFileUtil.GetOutputDirWithHash("", "foo.proto"));
        }

        [Test]
        public void GetOutputDirWithHash_HashesDir()
        {
            string PickHash(string fname) => DepFileUtil.GetOutputDirWithHash("", fname);

            string same1 = PickHash("dir1/dir2/foo.proto");
            string same2 = PickHash("dir1/dir2/proto.foo");
            string same3 = PickHash("dir1/dir2/proto");
            string same4 = PickHash("dir1/dir2/.proto");
            string unsame1 = PickHash("dir2/foo.proto");
            string unsame2 = PickHash("/dir2/foo.proto");

            Assert.That(same1, Is.EqualTo(same2));
            Assert.That(same1, Is.EqualTo(same3));
            Assert.That(same1, Is.EqualTo(same4));
            Assert.That(same1, Is.Not.EqualTo(unsame1));
            Assert.That(unsame1, Is.Not.EqualTo(unsame2));
        }
    };
}
