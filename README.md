# Bachelor thesis
Martin Pavlík

## medium of contents
- Configs..................Contains config folders and files of the trained models  
- GamePrototype.....folder of the Unity project, contains the created environments  
- results.................................folder containing trained mlagent models  

## Instructions for opening project files

### Unity project
Instructions for opening the GamePrototype unity project:  
- required version of Unity is Unity version 6000.2.8f1 (should be downloaded by itself when trying to open project)  
- Use the add button in Projects, and choose add project from disk  
- Once added open project from Unity, however the mlagents package needs to be added for things to work  
- The Unity project will likely throw an error about the package not being found, select continue despite the error so the package can be added  
- this will open the Unity project in safe mode  


#### Adding the manually downloaded package of mlagents 4.0.0
Here we will download mlagents, but not the online version avaiable inside Unity as that one is outdated.  
Instead we will use the package folder that has been downloaded from the mlagents repository.   
  
If the ml-agents package is not available and needs to redownloaded:  
git clone --branch release_23 https://github.com/Unity-Technologies/ml-agents.git  
  
once downloaded inside the Unity project:  
- select the window list from it choose package management and then inside of the package manager  
- click the plus button to add a package and choose install package from disk   
- find where the package is stored, go into ml-agents, then into com.unity.ml-agents and select package.json  
- this should add the package to unity (if it didn't, it is possible that the package is already present but the path is incorrect, remove this package and repeat the previous steps)  
  
with this it should be already possible the trained models inside the training environments:  
- from assets selects scenes  
- learning contains learning scenes  
- robotTests contains tests  
- from robotTests select any of the 5 environments  

the agents are already pre prepared  
All that is needed is to set the desired agent type to active, and within the "Trainer" object under located under the TrainXSet object, input the desired agent into both "agent object"  
and "Agent" inside the trainer script (this is done by either draggin the object into it or clicking the circle icon on the right of the slot and finding the object).  
Do the same for the tester object by inputing it into Subject Robot.  


### Mlagent trainers
instructions available on: https://docs.unity3d.com/Packages/com.unity.ml-agents@4.0/manual/Installation.html  
Training instructions  
  
Python 3.10.12 recommended  
  
If using conda, create a virtual environment like this:  
conda create -n mlagents python=3.10.12 && conda activate mlagents  
  
If the ml-agents package is not available and needs to redownloaded (This is the same one as in Unity Project):  
git clone --branch release_23 https://github.com/Unity-Technologies/ml-agents.git  

#### Mlagents instalation
- the version used for this was Version: 1.2.0.dev0  
- to download this version go into the mlagents package file  
- cd /path/to/ml-agents  
- and run these install commands  
- python -m pip install ./ml-agents-envs  
- python -m pip install ./ml-agents  
  
Using this within the virtual environment should successfully install the correct version of mlagents.  
  
It is possible to run this with python -m pip install mlagents==1.1.0 but due to it being an older version it is likely to cause issues.  
However, this is only needed to run training.  

#### Running training
To run any of the training environments select the correct environment, and take the desired agent from assets/PreFabs/mlagents and input the prefab object the same way as with tester  
into the Trainer(drag it from the assets into it or by finding it though selection in the slot).
Then run one of the desired commands within the external prompt, and then press play within the unity editor.
Important! Used agent should correspond to the used behavior name located inside the configs, while the names should be self explanatory, it possible to check what behavior the agent uses
by looking at the agent, finding behavior parameters and checking the behavior name.

Commands:  
  
Memory model  
  
mlagents-learn Configs/MemoryConfig/memory1Explore.yaml --run-id=memory02   
mlagents-learn Configs/MemoryConfig/memory2Find.yaml --run-id=memory02 --resume  
mlagents-learn Configs/MemoryConfig/memory3Attack.yaml --run-id=memory02 --resume  
mlagents-learn Configs/MemoryConfig/memory4Dodge.yaml --run-id=memory02 --resume  
mlagents-learn Configs/MemoryConfig/memory5Battle.yaml --run-id=memory02 --resume  
  
CNN model  
  
mlagents-learn Configs/ConvolutionConfig/convol0Move.yaml --run-id=convolution  
mlagents-learn Configs/ConvolutionConfig/convol1Explore.yaml --run-id=convolution  
mlagents-learn Configs/ConvolutionConfig/convol2Find.yaml --run-id=convolution --resume  
mlagents-learn Configs/ConvolutionConfig/convol3Attack.yaml --run-id=convolution --resume  
mlagents-learn Configs/ConvolutionConfig/convol4Dodge.yaml --run-id=convolution --resume  
mlagents-learn Configs/ConvolutionConfig/convol5Battle.yaml --run-id=convolution --resume  
  
  
ResNet model  
  
mlagents-learn src/Configs/ResNetConfig/ResNet1Explore.yaml --run-id=resnet  
mlagents-learn src/Configs/ResNetConfig/ResNet2Find.yaml --run-id=resnet --resume  
mlagents-learn src/Configs/ResNetConfig/ResNet3Attack.yaml --run-id=resnet --resume  
mlagents-learn src/Configs/ResNetConfig/ResNet4Dodge.yaml --run-id=resnet --resume  
mlagents-learn src/Configs/ResNetConfig/ResNet5Battle.yaml --run-id=resnet --resume  
  
If you want to run a new id just change --run-id-resnet="insert new id here".  
For resuming training of any id that already had training done use "--resume".  
If you need to overwrite an existing id use --force (Be carefull with this one as it will overwrite any progress done on a model with this id, and backups can be hours old).  
If you are resuming with a different environment and changing config for the curriculum, it possible you may need to reset progress on "obstacles"  
To reset it, go into src/results/"selected model"/run_logs/training_status.json and within it change the the obstacles variable back to 0.  
It is possible to reset or just roll back any lesson like this.   
  
  
Within the results is include an earlier version of memory training under the folder memoryOLD, the one used in results is memory02.  

#### Viewing results
Commands for viewing any of the results using tensorboard:  
  
tensorboard --logdir ".\src\results\memoryOLD"  
tensorboard --logdir ".\src\results\convolution"  
tensorboard --logdir ".\src\results\resnet"  
tensorboard --logdir ".\src\results\memory02"  




