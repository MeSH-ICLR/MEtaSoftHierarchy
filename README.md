# First-Person Shooting (FPS) Environment with Random Terrain
This repository contains a reference implementation of our Hierarchical Meta Reinforcement Learning for Multi-Task Environments proposed in ICLR 2021.

## Introduction

The environment is modified based on the unity open source project FPS micrograme. 
The Gamebot SDK has been embedded in to make it a trainable environment. 
The terrain of this environment is generated randomly in each run, according to the generation rules mentioned in our paper.

You can try playing the game by selecting the corresponding platform package under the folder ./app_play/.

## Environment Setup

1. Tensorflow 1.15.0
2. Python 3.7.7
3. Gym 0.17.2
4. Protobuf 3.12.2



