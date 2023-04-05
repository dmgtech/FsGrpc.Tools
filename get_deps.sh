curl 'https://www.nuget.org/api/v2/package/Grpc.Tools/2.51.0' -L -o /tmp/Grpc.Tools.zip -s

cd protoc_plugins

mkdir -p protoc_windows_x86
mkdir -p protoc_windows_x64
mkdir -p protoc_linux_x86
mkdir -p protoc_linux_x64
mkdir -p protoc_linux_aarch64
mkdir -p protoc_macos_x64

unzip -p /tmp/Grpc.Tools.zip tools/linux_arm64/protoc > protoc_linux_aarch64/protoc
unzip -p /tmp/Grpc.Tools.zip tools/linux_x64/protoc > protoc_linux_x64/protoc
unzip -p /tmp/Grpc.Tools.zip tools/linux_x86/protoc > protoc_linux_x86/protoc
unzip -p /tmp/Grpc.Tools.zip tools/macosx_x64/protoc > protoc_macos_x64/protoc
unzip -p /tmp/Grpc.Tools.zip tools/windows_x64/protoc.exe > protoc_windows_x64/protoc.exe
unzip -p /tmp/Grpc.Tools.zip tools/windows_x86/protoc.exe > protoc_windows_x86/protoc.exe
