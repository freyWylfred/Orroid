# Orroid 🎮

A .NET 10 Android battle game app.

🇯🇵 [日本語版 README](docs/README.ja.md)

## Screenshot

```
┌─────────────────────────┐
│    ████████████████     │  ← Enemy HP Bar
│         👾              │  ← Enemy
├─────────────────────────┤
│       HP: 100           │  ← Player HP
│      ┌───────┐          │
│      │Rapid  │          │  ← Skill with
│      │Slash  │          │    circular gauge
│      └───────┘          │
│      ┌───────┐          │
│      │Quick  │          │
│      │Slash  │          │
│      └───────┘          │
│      ┌───────┐          │
│      │Defense│          │
│      └───────┘          │
└─────────────────────────┘
```

## Features

### 🎯 Skill System
| Skill | Effect | Damage |
|-------|--------|--------|
| Rapid Slash | High damage attack | 20–35 |
| Quick Slash | Medium damage attack | 15–25 |
| Defense | Nullifies the next enemy attack | – |

### ⏱️ Gauge System
- Each skill has a **circular gauge** that fills up clockwise over time (~5 seconds)
- Skills become usable once the gauge is full
- Using a skill **reduces all other gauges by 25%**

### 👾 Enemy AI
- Attacks automatically every **2 seconds**
- Damage: 8–15

### 🏆 Win / Lose
- **Victory**: Reduce the enemy's HP to 0 → enemy disappears, EXP & Gold rewards shown
- **Defeat**: Player's HP reaches 0

## Tech Stack

- **Framework**: .NET 10 Android
- **Language**: C# 13
- **UI**: Android Native (XML Layout)
- **Custom View**: CircularGaugeView (circular progress)

## Project Structure

```
Orroid/
├── MainActivity.cs          # Main game logic
├── CircularGaugeView.cs     # Custom circular gauge view
├── Orroid.csproj            # Project configuration
├── Resources/
│   └── layout/
│       └── activity_main.xml  # UI layout
└── ...
```

## Build

### Requirements
- Visual Studio 2022 (17.12+)
- .NET 10 SDK
- Android SDK (API 24+)

### Steps
```bash
# Clone
git clone https://github.com/freyWylfred/Orroid.git
cd Orroid

# Build
dotnet build

# Or open Orroid.slnx in Visual Studio and build from there
```

## License

[MIT License](LICENSE)

## Author

[@freyWylfred](https://github.com/freyWylfred)
