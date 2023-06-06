set projectpath="C:\path\to\unity\project\Slider\Slider"
set buildpath="C:\path\to\build\Desktop\Content"
set projectname="Slider"
set logpath="log.txt"

"C:\Program Files\Unity\Hub\Editor\2021.3.9f1\Editor\Unity.exe"^
 -quit^
 -batchmode^
 -nographics^
 -executeMethod GameBuilder.BuildAllPlatforms^
 -filename %projectname%^
 -buildRootPath %buildpath%^
 -projectPath %projectpath%^
 -logFile %logpath%

echo "Done! Closing terminal..."
timeout /t 15