# Masterwork Resonance

🌐 Language: [Русский](README.ru.md) | [English](README.md)

[Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3741570565)

**Masterwork Resonance** is a RimWorld mod that adds rare random resonance bonuses to high-quality crafted gear.

When your colonists craft a **Masterwork** or **Legendary** weapon, armor or apparel item, it can awaken a random **Resonance** effect. The bonus is stored directly on the item and is shown in the item inspection panel.

## Features

* Random resonance bonuses for crafted Masterwork and Legendary gear
* Works with melee weapons, ranged weapons, armor and apparel
* 40% awakening chance for Masterwork items
* 80% awakening chance for Legendary items
* Color-coded resonance roll quality
* Mood resonance displayed in the pawn Needs tab
* English and Russian localization
* RimWorld 1.5 / 1.6 support
* Multiplayer-friendly deterministic craft rolls

## Awakening Chances

| Quality    | Resonance Chance |
| ---------- | ---------------: |
| Masterwork |              40% |
| Legendary  |              80% |

If resonance does not awaken, the item remains a normal Masterwork or Legendary item.

Existing items are not enchanted retroactively. Only newly crafted items can awaken resonance.

## Possible Resonance Effects

Weapons can receive bonuses such as:

* Melee or ranged damage bonus
* Reduced melee cooldown
* Reduced ranged cooldown
* Reduced aiming time
* Melee hit chance bonus
* Shooting accuracy bonus
* Melee dodge bonus
* Mood bonus while equipped

Armor and apparel can receive bonuses such as:

* Movement speed bonus
* Improved armor rating
* Increased item durability
* Reduced incoming damage while worn
* Melee dodge bonus
* Mood bonus while worn
* Reduced pain while worn

## Roll Quality Colors

Each resonance rolls within its own stat range.

| Color  | Meaning     |
| ------ | ----------- |
| Bronze | Low roll    |
| Orange | Medium roll |
| Green  | High roll   |

Low rolls are still positive bonuses. The color only shows how strong the result is compared to the maximum possible roll for that resonance.

## Mood Resonance

Items with mood resonance add a visible mood entry in the pawn's Needs tab:

**Resonant equipment**

The bonus is applied only while the resonant item is equipped or worn.

## Compatibility

The mod is designed to be compatible with other mods that create weapons, armor, and clothing, if the item:

* uses RimWorld's quality system
* is crafted as Masterwork or Legendary
* is recognized as a valid weapon or apparel item

## Multiplayer

Resonance generation uses deterministic craft rolls.

Basic multiplayer crafting tests were performed without desyncs, and generated resonance values matched between players.

## Supported Versions

* RimWorld 1.5
* RimWorld 1.6

## Languages

* English
* Russian

Other languages fall back to English.

## Requirements

Requires [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077).

## Building from Source

The project is built as a RimWorld C# mod.

You need references to:

* `Assembly-CSharp.dll`
* `UnityEngine.CoreModule.dll`
* `UnityEngine.IMGUIModule.dll`
* `UnityEngine.TextRenderingModule.dll`
* `0Harmony.dll`

After building, place the compiled mod DLL into:

```text
Assemblies/
```

## Steam Workshop

You can subscribe to the mod here:

[Masterwork Resonance on Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3741570565)
