# Portfolio Gameplay Unity 2D Platformer

## Overview
Prototype of a 2D Gameplay Platformer with spells casting and movement mecanics based on Celeste.
The project focuses on movement responsiveness, spell integration, state management and basic save system implementation.

This repository contains only the core gameplay scripts for demonstration purposes.

## Technical Focus
- Rigidbody2D-based movement system (velocity control & air/ground state separation)
- Modular spell system (Projectile, Laser, Dash ability)
- Finite State Machine for player states (Idle, Run, Jump, Cast, Dash)
- Basic enemy AI and phase-based boss behavior
- Checkpoint-based Save System (stats + progression persistence)
- UI integration (menu, reload, level selection)
- Animation state synchronization with gameplay logic
- Basic audio integration (SFX, musics)

## System Architecture Highlights
- Boss teleportation system using raycasts and layer validation
- Context-aware damage system to prevent unintended triggers

## Technical Challenges Solved
- Reliable save/load system with contextual statistics validation
- Boss teleport position calculation with:
  - Maximum attempt logic
  - Raycast-based collision validation
  - Layer filtering
- Fixing uncontrolled death triggers caused by improper damage layer checks

## Known Limitations
- Player animation system currently handled by a monolithic script (planned refactor into modular animation controller)

## Video Demonstration
https://youtu.be/7dGAr11Z9BA

## Author
Tom BAUDIN - Gameplay programming student - Unity / C#
portfolio-geii-2a.tom-baudin.fr
