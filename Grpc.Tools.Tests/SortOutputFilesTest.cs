using NUnit.Framework;
using Microsoft.Build.Utilities;

namespace Grpc.Tools.Tests
{
    public class SortOutputFilesTest
    {
        [Test]
        public void ResolvesExternalDependency()
        {
            var filesToCompile =
                new Microsoft.Build.Framework.ITaskItem[]
                {
                    Utils.MakeItem(
                            @"obj\Debug\net6.0\protoc-proto\google\protobuf\compiler\plugin.proto.gen.fs",
                            "OutputDir", @"obj\Debug\net6.0\",
                            "Source", @"protoc-proto\google\protobuf\compiler\plugin.proto"),
                    Utils.MakeItem(
                            @"obj\Debug\net6.0\descriptor.proto.gen.fs",
                            "OutputDir", @"obj\Debug\net6.0\",
                            "Source", @"C:\Users\mciccotti\.nuget\packages\fsgrpc.tools\1.0.0\build\native\include\google\protobuf\descriptor.proto"),
                };
            var dependencies =
                new Microsoft.Build.Framework.ITaskItem[]
                {
                    Utils.MakeItem(
                            @"C:\Users\mciccotti\.nuget\packages\fsgrpc.tools\1.0.0\build\native\include\google\protobuf\descriptor.proto",
                            "Source", @"obj\Debug\net6.0\protoc-proto\google\protobuf\compiler\plugin.proto.gen.fs")
                };
            var sorter = new SortOutputFiles()
                {
                    FilesToCompile = filesToCompile,
                    Dependencies = dependencies
                };
            sorter.Execute();

            Assert.That(2, Is.EqualTo(sorter.SortedFilesToCompile.Length));
        }
        [Test]
        public void ResolvesLocalDependency()
        {
            var filesToCompile =
                new Microsoft.Build.Framework.ITaskItem[]
                {
                    Utils.MakeItem(
                            @"obj\Debug\net6.0\protoc-proto\google\protobuf\compiler\plugin.proto.gen.fs",
                            "OutputDir", @"obj\Debug\net6.0\",
                            "Source", @"protoc-proto\google\protobuf\compiler\plugin.proto"),
                    Utils.MakeItem(
                            @"obj\Debug\net6.0\descriptor.proto.gen.fs",
                            "OutputDir", @"obj\Debug\net6.0\",
                            "Source", @"C:\Users\mciccotti\.nuget\packages\fsgrpc.tools\1.0.0\build\native\include\google\protobuf\descriptor.proto"),
                };
            var dependencies =
                new Microsoft.Build.Framework.ITaskItem[]
                {
                    Utils.MakeItem(
                            @"C:\Users\mciccotti\.nuget\packages\fsgrpc.tools\1.0.0\build\native\include\google\protobuf\descriptor.proto",
                            "Source", @"obj\Debug\net6.0\protoc-proto\google\protobuf\compiler\plugin.proto.gen.fs")
                };
            var sorter = new SortOutputFiles()
                {
                    FilesToCompile = filesToCompile,
                    Dependencies = dependencies
                };
            sorter.Execute();

            Assert.That(2, Is.EqualTo(sorter.SortedFilesToCompile.Length));
        }
    };
}
