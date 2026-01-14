## Requirements:
- Unity version 6000.2.14f1
- Tested using the Vive Focus 3 with the eye-tracking module

## Install:
- Simply clone the repository for use in a Unity project

## RippleVision Study:
- Start "MS_StarupScene" to begin study
	- in the controller of the StartupScene, various settings can be adjusted, such as the used scenes, gaze guidance techniques, and maximum duration of a search
- The user (study participant) will need to click the trigger of a VR hand controller to notify the framework of him finding an object; alternatively if Debugging is enabled (Inspector view of the Controller object in the "MS_StartupScene" press "L" to advance to the enxt scene)
- data will be collected in the Assets/Data folder or a persistent datapath, depending on setup
- main files for RippleVision: Shader (Assets/Shaders/RippleShader), Script (Assets/Scripts/VisionCatcher/Types/RippleVision)
- (the cyberpunk-like realistic scene cannot be redistributed due to licensing restrictions and is not included in this repository)


This repository is provided as-is and is not actively maintained. It is shared for the sake of research transparency and to support open science.