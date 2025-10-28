# Gacha-Unity

## Directory Structure
GachaGame/                                  # Unity Project Root
├── Assets/
│   ├── Scenes/                             # Game Scenes
│   │   └── MainScene.unity                 # Main game scene
│   │
│   ├── Scripts/                            # All C# Scripts
│   │   ├── Network/                        # Network related scripts
│   │   │   └── WebSocketManager.cs         # WebSocket connection manager
│   │   │
│   │   ├── Gacha/                          # Gacha system scripts
│   │   │   ├── GachaManager.cs             # Main gacha logic controller
│   │   │   └── GachaAnimation.cs           # Gacha animation handler
│   │   │
│   │   ├── UI/                             # UI related scripts
│   │   │   └── ResultCard.cs               # Result card display
│   │   │
│   │   └── Models/                         # Data models (optional organization)
│   │       └── DataModels.cs               # All data classes in one file
│   │
│   ├── Prefabs/                            # Reusable game objects
│   │   └── ResultCard.prefab               # Character result card prefab
│   │
│   ├── Materials/                          # Materials and shaders
│   │   ├── CardBackground.mat              # Card background material
│   │   └── ParticleMaterial.mat            # Particle effect material
│   │
│   ├── Sprites/                            # 2D images and textures
│   │   ├── UI/                             # UI sprites
│   │   │   ├── button_normal.png
│   │   │   ├── button_pressed.png
│   │   │   ├── panel_background.png
│   │   │   └── card_frame.png
│   │   │
│   │   └── Characters/                     # Character images
│   │       ├── char_placeholder.png
│   │       └── new_badge.png
│   │
│   ├── Audio/                              # Sound files
│   │   ├── BGM/                            # Background music
│   │   │   └── main_theme.mp3
│   │   │
│   │   └── SFX/                            # Sound effects
│   │       ├── gacha_start.wav
│   │       ├── ssr_reveal.wav
│   │       ├── sr_reveal.wav
│   │       ├── r_reveal.wav
│   │       └── button_click.wav
│   │
│   ├── Animations/                         # Animation clips and controllers
│   │   ├── CardReveal.anim
│   │   └── ButtonPress.anim
│   │
│   ├── Fonts/                              # Custom fonts
│   │   └── GameFont.ttf
│   │
│   ├── Plugins/                            # Third-party plugins
│   │   └── NativeWebSocket/               # WebSocket library
│   │       └── (NativeWebSocket files)
│   │
│   └── Resources/                          # Runtime loaded assets
│       └── Characters/                     # Character data
│
├── Packages/                               # Unity Package Manager
│   └── manifest.json                       # Package dependencies
│
├── ProjectSettings/                        # Unity project settings
│   ├── ProjectSettings.asset
│   ├── TagManager.asset
│   └── (other settings)
│
└── Builds/                                 # Build output (not in version control)
    ├── iOS/                                # iOS build
    └── Android/                            # Android build