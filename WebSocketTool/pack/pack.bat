libz.exe inject-dll --assembly ../bin/Release/WebSocketTool.exe --include ../bin/Release/*.dll --move

mkdir output
move ../bin/Release/WebSocketTool.exe output/WebSocketTool.exe