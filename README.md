# My odin inspector snippets
A collection of attributes and snippets i use with Odin Inspector

## FmodParameterDropdownAttribute 

Adds a dropdown for the parameters of a fmod event
### Example usage:
```cs
[EventRef]
[SerializeField] private string someEvent;

[FmodParameterDropdown(nameof(someEvent))]
[SerializeField] private string someParameter;
```
Result:
[](/image/FmodParameterDropdown.png)
