# Hovr Bird
Follow instructions here (https://thoughtbot.com/blog/how-to-git-with-unity) to ensure managing this repo is as painless as possible.

# iOS Release
Ensure this is added to the Info.plist file: "App Uses Non-Exempt Encryption: NO".

# Cloning for the first time
1. Install git-lfs from here: https://git-lfs.github.com/ (ensure you have the same version installed as everyone else; run `git lfs version` after installation to see the version)
1. Clone this repo
1. Open Unity Hub and click Add, then add the base project directory "HovrBird"
1. Open the project from Unity Hub
1. If there's an error running the game, go into File -> Build Settings and Switch Platform to either iOS or Android. There's a bug right now with preprocessor directives to enable/disable ad functionality in the Unity Editor vs for actual devices.
