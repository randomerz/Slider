A more user friendly doc is available at https://feel-docs.moremountains.com/
Find out more about the asset at https://feel.moremountains.com/

Feel v3.15

## WHAT'S IN THE ASSET ?
-------------------------

The Feel asset contains the following folders :
- FeelDemos : a collection of demo scenes, all neatly arranged in their dedicated folders, showcasing how you can use Feel to improve game feel in various contexts
- FeelDemosHDRP : demos tailored for HDRP
- FeelDemosURP : demos tailored for URP
- MMFeedbacks : all you need to add custom feedbacks with only a few clicks, including MMFeedbacksForThirdParty : more feedbacks, but these have dependencies (PostProcessing, Cinemachine, Nice Vibrations...).
- MMTools : a set of tools and helpers
- NiceVibrations : the NiceVibrations package, exactly the same as https://assetstore.unity.com/packages/tools/integration/nice-vibrations-by-lofelt-hd-haptic-feedback-for-mobile-and-game-197444?aid=1011lKhG&utm_source=aff, offered as a gift


## HOW DO I ADD THIS TO MY GAME ?
------------------------------

You should probably go check out http://feel-docs.moremountains.com/how-to-install.html, there'll be more details, but basically, you can follow these simple steps :

To add Feel to your project, simply follow the simple steps below :
1. using 2019.4.35f (or a recent - and stable - version of Unity of your choice), create a new project, pick the “3D” template
2. via the asset store panel, go to the Feel page, click the download button, then the import button
3. wait until a “import Unity package” popup appears, make sure everything is checked (it should be by default), click “import”
4. open Unity’s Package Manager, install the latest version of the Post Processing package
5. in the package manager, install the latest version of the Cinemachine package
6. in the package manager, install the latest version of the TextMesh Pro package
7. in the package manager, install the latest version of the Animation 2D package (this is only useful for the Letters demo)
8. open the MMFeedbacksDemo scene (or any other demo), press play, enjoy

Note that steps 4, 5, 6 and 7 are optional, but if you want to get access to post processing, TextMesh Pro and Cinemachine feedbacks, you’ll need these.
Also note that most of the Feel demos make use of as many feedbacks as needed, and will feature most of these dependencies.
You’ll get errors in these if you haven’t installed the corresponding dependencies. You can check the MMFeedbacksMinimalDemo scene if you’re not interested in any of the Unity packages feedbacks.

## IS THERE DOCUMENTATION SOMEWHERE ?
-------------------------------------

There is!
There's a functional documentation at https://feel-docs.moremountains.com/
And a complete API documentation at https://feel-docs.moremountains.com/API/

## I STILL HAVE A QUESTION!
---------------------------

If something's still not clear, you can always drop me a line using the form at http://feel.moremountains.com/.
It's entirely possible that I forgot to document something, but please make sure you've read the documentation before filling this form.
You can also please check the FAQ before sending me an email. Chances are, your question's answered right there. If it's not, then go ahead!
Also, if you're asking for support, please send me your invoice number, along with your Unity version and the version of Feel you're using, so I can help you best.
