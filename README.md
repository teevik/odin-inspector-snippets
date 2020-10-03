# My odin inspector snippets
A collection of attributes and snippets i use with Odin Inspector

## [SpriteShapeShadowGenerator](https://github.com/teevik/odin-inspector-snippets/blob/master/SpriteShapeShadowGenerator.cs)

An OdinEditorWindow for generating a ShadowCaster2D for a SpriteShapeController. Super hacky with a lot of reflection :P

Video of it: https://cdn.discordapp.com/attachments/714116075221942362/761893804533874688/Desktop_2020.10.03_-_12.04.38.13_Trim.mp4

![](https://i.imgur.com/baN9TAV.png)

## [FmodParameterDropdownAttribute](https://github.com/teevik/odin-inspector-snippets/blob/master/FmodParameterDropdownAttribute.cs)

Adds a dropdown for the parameters of a fmod event
### Example usage:
```cs
[EventRef]
[SerializeField] private string someEvent;

[FmodParameterDropdown(nameof(someEvent))]
[SerializeField] private string someParameter;
```
Result:
![](https://i.imgur.com/5U2AnQ6.png)
