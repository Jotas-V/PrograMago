# TCC-40 Visual Refinement Design

**Date:** 2026-09-28  
**Status:** Approved by the user

## Goal

Make the workspace UI feel like one scene with the arena, keep the code editor and its controls readable, and make character and arena effects move at a calmer pace.

## Approved direction

- Continue the arena's earth texture below the grass line and use the arena's palette and pixel scale as the visual reference.
- Keep wood as a thin horizontal finish around the code editor. Avoid large leafy corner ornaments inside the editor's usable area.
- Give the editor a clear, spacious dark writing surface. Keep the existing arena/tutorial split and arrange the code toolbar and battle control so they do not compete with the editor.
- Keep button artwork and text in separate visual areas. Labels must not sit on top of baked-in icons; reuse current art where it fits rather than adding unrelated sprites.
- Preserve native sprite proportions and use sliced/tiled rendering only where it prevents distortion. Keep the UI scale consistent with the 1920x1080 Canvas reference.

## Animation direction

- Align the atmospheric sprites' Pixels Per Unit with the arena art.
- Slow down the character and ambient frame loops so movement reads clearly.
- Animate wind and leaves as gentle, occasional crossings from one side of the arena to the other, with varied pauses between passes. Do not leave them stationed at the center.
- Keep the firefly as a small, low-key local effect.

## Acceptance criteria

- Code text stays inside a clean writing area without overlapping foliage or wood decorations.
- The earth below the arena looks continuous with the arena ground; the wood trim remains visually thin.
- Bottom toolbar labels and battle button labels remain legible and do not overlap decorative icons.
- Arena and UI assets preserve their pixel proportions at the project Canvas reference resolution.
- Wind and leaves visibly traverse the arena with pauses, and all sprite animation playback is slower than the current presentation.
- No unrelated replacement sprites are introduced.

## Out of scope

- New enemy types or enemy artwork for enemies not implemented in the game.
- Replacing the existing wizard, training dummy, projectile, or arena artwork.
