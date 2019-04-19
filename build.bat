msbuild build.proj
Unity.exe -batchmode -nographics -projectPath ExampleMod -quit -exportPackage "Assets" "%~dp0\build\examplemod.unitypackage"