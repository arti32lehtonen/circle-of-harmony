# Circle Of Harmony

This is a repository for the Version 1.1 of the game Circle Of Harmony.

Game page: https://arti32lehtonen.itch.io/circle-of-harmony

![Alt Text](RepositoryAssets/game_example.gif)

**Attention!**
This is an anonymized version of the project.
Some of the assets have unsuitable licences for open source.
I replaced such assets with the proper analogues.

## Build

To build the project you need to:
1. Clone the repository

    `git clone https://github.com/arti32lehtonen/circle-of-harmony.git`
2. Open the project using UnityHub 
3. Import TMP Essentials and TMP Examples & Extras packages 
4. Build the project

## How to make changes

You can use your assets without changing project structure.
All you need to do is replace the following files using the same names:
* `Assets/Audio/*.mp3` - edit sound effects and main music theme
* `Assets/Images/*.png` - edit animals icons
* `Assets/Images/UIButtons/*.png` - edit UI elements
* `Assets/Fonts/*.otf` - edit game fonts  


You can change several game parameters by changing configs. Configs are ScriptableObjects inside Unity.
* `Configs/Initialization` - configs, which state possible initializations for the game fields
* `Configs/Packs` - configs, which state what new options can be added at each game round
* `Configs/GlobalSettings` - global game settings  
* `Configs/Specimens` - each config states the characteristics of the corresponding specimen  
