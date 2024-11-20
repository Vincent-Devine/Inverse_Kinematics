# Inverse Kinematic
This project focuses on creating a custom inverse kinematics (IK) system within 
a chosen game engine. The implementation will utilize one of two
algorithms: Cyclic Coordinate Descent (CCD) or Jacobian Inverse Kinematics. <br>
Developed as part of a 4th-year Master's in Game Programming at ISART DIGITAL.

## Table of content
- [Getting Started](#getting-started)
- [Usage Guide](#usage-guide)
- [Known Issues](#known-issues)
- [Technology](#technology)
- [Credit](#credit)

## Getting Started
Clone from [Github](https://github.com/Vincent-Devine/Inverse_Kinematics) *by ssh*
```bash
git clone git@github.com:Vincent-Devine/Inverse_Kinematics.git
```

## Usage Guide
IK algorithms: **CCD** <br>

To configure the CCD settings, select the Player object from the Hierarchy. In the Inspector panel, locate the CCD script,
where you can **enable or disable bone constraints** *(enabled by default)*.

Script:<br>
- **CCD**: responsible for **calculating IK** on the bones
- **PlayerTouchWall**: switches the hand closest to a wall from animation to IK, using the CCD script to control its movement.
It also handles smooth transitions between the animation and IK systems.

### Demonstration <br>

![GIF](./Screenshot/ik_demonstration.gif)<br>
Scene: **Scenes/Demonstration** <br>
Start the scene to demonstrate inverse kinematics in action. When the player gets near a wall, their hand automatically reaches out to touch it.<br>
This interaction works only when the player is within touching range but not too close.<br>

Player controls: **W, A, S, D** for movement and the **mouse** for camera rotation.<br>

### Free trial <br>

![GIF](./Screenshot/ik_free_trial.gif)<br>
Scene: **Scenes/FreeTrial** <br>
Start the scene and navigate to the Scene view to **manually move the target** *(red dot)*.<br>
The player's hand will follow to align with the target's position.<br>

## Known Issues
1. Bone constraints <br>

The bone constraints are tailored for the demonstration scenario (touching the wall at shoulder height).
During free trial, limitations become apparent, such as the inability to raise the hand above shoulder height.

## Technology
- Engine: **Unity** *v2022.3.34f1*
- IDE: Visual Studio 2022
- Versionning: [Github](https://github.com/Vincent-Devine/Inverse_Kinematics)

## Credit
Author: **Vincent DEVINE**<br>
Project start: 16/09/2024 <br>
Project end: 09/12/2024 <br>

### Specials Thanks
- Benjamin TRAN