# XrAvenue Game Jam (Politecnico di Torino) — VR Trash Sorter

A short VR narrative game made in **72 hours** during a **3‑day game jam at Politecnico di Torino**, for the **Avenue / XrAvenue project**.

You play as a **trash sorter** working for a regime.  
You stand in front of a window, with **three bins** and an endless stream of objects to classify.

It sounds simple. It isn’t.

Every item you sort (and *how* you sort it) becomes a choice — and those choices slowly reshape **the world outside the window**… and **your own fate**.

---

## Gameplay video

- Trailer / gameplay video: *(add link)*

> Tip: upload to YouTube/Vimeo, then paste the link here.

---

## Screenshots

> Add a few images to a folder like `Docs/Screenshots/` and embed them here.

- *(add screenshot 1)*
- *(add screenshot 2)*
- *(add screenshot 3)*

Example:

```md
![Screenshot 1](Docs/Screenshots/screenshot-1.png)
```

---

## What the game is about

You’re doing a repetitive job for a system you don’t control.  
The game asks a pretty uncomfortable question:

> If your only power is “small decisions”, can you still cause (or stop) something catastrophic?

As days go by, consequences pile up. Some outcomes are subtle.  
Others are… not subtle at all.

---

## Core gameplay

- You’re always in the same “workstation” space:
  - a **window** in front of you (your only view of the world)
  - **3 trash bins**
  - trash items arriving that you can pick up and sort
- **Sorting is the main mechanic**, but it’s also the narrative engine:
  - your choices influence what happens outside
  - your choices influence what happens *to you*
  - the story branches toward different endings (often bad ones)

---

## VR interactions (hand-first)

We built the interaction systems ourselves and focused on **hand-based gameplay**:
- grabbing / placing objects into bins
- interacting with the environment in VR
- hand-driven UI choices when needed

The project integrates **XR Hands** (hand tracking) and is built in **Unity**.

---

## Dialogue + “days” system (made for the jam)

To make the world feel alive within the jam scope, we implemented:
- a **dialogue system** to deliver narrative beats and reactions
- a **day progression system** (“shifts”) so the game evolves over time
- state that carries forward, so choices matter beyond the current moment

---

## Visual style

We went for a **PS1-inspired low-poly look**:
- intentionally simple geometry
- nostalgic, slightly eerie vibes
- a **pixelize shader** to push the “old screen / distorted reality” feeling

---

## Team

Made by a **team of 3** during the game jam at Politecnico di Torino:

- **Federico Colombo** — Programmer
- **Alberto Taddei** — Assistant Programmer, Sound Design, Narrative Designer
- **Vincenzo Sacco** — Artist

---

## Tech notes

- Engine: **Unity**
- Main language: **C#**
- Shaders: **ShaderLab / HLSL**
- XR: Unity XR stack + **XR Hands**

Repo languages (GitHub stats):
- C# 73.9%
- ShaderLab 13.2%
- Wolfram Language 3.1%
- C 2.7%
- C++ 2.7%
- HLSL 2.4%
- CMake 2%

---

## How to open the project

1. Clone:
   ```bash
   git clone https://github.com/federicocolombo12/XrAvenue_GameJam.git
   ```
2. Open with Unity Hub (Unity **2022 LTS+** recommended).
3. Make sure XR is configured (XR Plugin Management / OpenXR or your target platform).
4. Open the main scene and press Play / Build.

---

## Notes / disclaimer

This was made in **72 hours**, so expect jam code: quick decisions, hacks, and duct tape.  
But also: lots of heart, and a concept we still love.

---

## Contact

If you want to ask something about the project, open an issue or contact **@federicocolombo12**.
