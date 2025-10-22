# Cod of Duty

Welcome to **Cod of Duty**, our submission to the hackerspace game jam. https://itch.io/jam/hackerspace-gamejam-2025h/entries

Cod of Duty a fish simulator game where you take control of a fish navigating a dynamic underwater ecosystem! This game features realistic boid-based fish behavior, predator-prey dynamics, and a mathematically driven shader for fish animations that brings the underwater world to life.

---

## Features

### 🐟 **Fish Behavior**
- **Boid System**: Fish move in schools using alignment, cohesion, and separation rules.
- **Weight Classes**: Fish are categorized into weight classes, affecting their interactions:
  - Smaller fish fear larger fish.
  - Larger fish hunt smaller fish.
  - Fish of the same weight class align and school together.
- **Player Interaction**: The player fish is part of the ecosystem:
  - Boids react to the player as either prey or predator based on size.
  - The player can eat smaller fish to grow.

### 🎨 **Mathematical Fish Animation Shader**
- **Custom Shader**: Fish animations are driven mathematically using sine waves for realistic tail movement.
- **Velocity Scaling**: The shader dynamically adjusts animation speed based on the fish's movement velocity.
- **Metallic and Smoothness Effects**: The shader includes Fresnel highlights and metallic reflections for a polished underwater look.

### 🌊 **Dynamic Ecosystem**
- **Boundary System**: Fish are confined to a visible underwater box, ensuring they stay within the playable area.
- **Growth Mechanics**: Fish grow logarithmically as they eat, with a cap on maximum size.
- **Predator-Prey Dynamics**: Fish interact based on size, creating a natural food chain.

---

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/tobiasfremming/Cod-of-duty.git
   ```
2. Open the project in Unity (tested with Unity version 2023.x or later).
3. Ensure all dependencies are installed (e.g., URP for shaders).

---

## How to Play

1. **Control the Player Fish**:
   - Move around the underwater world.
   - Eat smaller fish to grow.
   - Avoid larger fish to survive.

2. **Interact with the Ecosystem**:
   - Watch fish school together and react dynamically to predators and prey.
   - Explore the underwater boundary box.

3. **Enjoy the Visuals**:
   - Admire the mathematically driven fish animations and metallic shader effects.

---

## Screenshots

Here are some screenshots showcasing the game:

![Screenshot 1](img/Screenshot%202025-10-22%20152910.png)
![Screenshot 2](img/Screenshot%202025-10-22%20152926.png)
![Screenshot 3](img/Screenshot%202025-10-22%20152938.png)

---

## Development Highlights

### 🐟 **Boid System**
The boid system is implemented in the `BoidManager.cs` and `Boid.cs` scripts. It uses:
- **Neighbor Detection**: Fish detect nearby boids using radii for alignment, cohesion, and separation.
- **Predator and Prey Logic**: Fish react to others based on weight class and size.

### 🎨 **Fish Shader**
The `FishAffineShader.shader` is a custom shader that animates fish tails mathematically:
- **Sine Wave Animation**: Tail movement is calculated using sine waves for smooth, realistic motion.
- **Velocity Scaling**: The shader dynamically adjusts animation speed based on the fish's velocity, controlled by the `FishShaderController.cs`.

---

## Known Issues
- **Boid Behavior**: Occasionally, fish may not react properly to neighbors due to perception radius settings.
- **Boundary System**: Fish may sometimes approach the edges too closely before turning back.

---

## Credits
- **Developer**: [Your Name]
- **Special Thanks**: Unity community for boid system inspiration and shader tutorials.

---

Dive into the underwater world of **Cod of Duty** and experience the life of a fish in a dynamic ecosystem! 🐟