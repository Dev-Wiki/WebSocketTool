set version=1.0.3

if not "%~1"=="" (
    set version=%1
)

cd ../ && rmdir /s /q bin && rmdir /s /q obj 
cd pack

devenv ../../WebSocketTool.sln /ReBuild "Release|AnyCPU" /project WebSocketTool

if NOT %errorlevel%==0 @goto :FailureOnBuild

libz.exe inject-dll --assembly ../bin/Release/WebSocketTool.exe --include ../bin/Release/*.dll --move

if not exist "output" (
    mkdir output
)

cd output
if exist "WebSocketTool-%version%.exe" (
    del /q /s WebSocketTool-%version%.exe
)
cd ../

cd ../bin/Release && move WebSocketTool.exe ../../pack/output/WebSocketTool-%version%.exe 
cd ../ && del /q /s Release && cd ../pack

goto :eof

:FailureOnBuild
echo build project failure