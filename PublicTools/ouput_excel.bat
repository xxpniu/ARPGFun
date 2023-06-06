
SET IMPORT_PATH=../proto/
SET CSHARP_PATH=../src/csharp
pushd ToolBin
ExcelOut.exe dir:../econfigs namespace:EConfig exportJson:../src/json/ exportCs:%CSHARP_PATH%/ExcelConfig.cs ex:*.xlsx debug:false mode:data
IF not %ERRORLEVEL% == 0 exit  %ERRORLEVEL%
popd

Copy src\json\  ..\client\Assets\Resources\Json\  /Y
IF not %ERRORLEVEL% == 0 exit  %ERRORLEVEL%
::Copy src\json\  ..\Server\Configs\ /Y
::IF not %ERRORLEVEL% == 0 exit  %ERRORLEVEL%

python uploadzk.py --host 129.211.9.75:2181 --root /configs --dir ./src/json
IF not %ERRORLEVEL% == 0 exit  %ERRORLEVEL%


