set projectpath="\..\..\Slider"
set buildpath="C:\path\to\build\Desktop\Content"
set projectname="Slider"
set logpath="log.txt"

"C:\Users\rando\Documents\Unity\2022.3.5f1\Editor\Unity.exe"^
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