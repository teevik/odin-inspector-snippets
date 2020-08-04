public class FmodParameterDropdownAttribute : Attribute
{
    public readonly string EventPathResolver;

    public FmodParameterDropdownAttribute(string eventPathResolver)
    {
        EventPathResolver = eventPathResolver;
    }
}

public class FmodParameterDropdownAttributeDrawer : OdinAttributeDrawer<FmodParameterDropdownAttribute, string>
{
    private ValueResolver<string> eventPathResolver;

    protected override void Initialize()
    {
        this.eventPathResolver = ValueResolver.Get<string>(this.Property, this.Attribute.EventPathResolver);
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
        if (this.eventPathResolver.HasError) this.eventPathResolver.DrawError();
        else
        {
            GUILayout.BeginHorizontal();
            
            var width = 15f;
            if (label != null) width += GUIHelper.BetterLabelWidth;
            
            var results = OdinSelector<string>.DrawSelectorDropdown(label ?? GUIContent.none, GUIContent.none,
                CreateSelector, GUIStyle.none, GUILayoutOptions.Width(width));
            if (results != null) ValueEntry.SmartValue = results.First();
            
            if (Event.current.type == EventType.Repaint)
            {
                var position = GUILayoutUtility.GetLastRect().AlignRight(15f);
                position.y += 4f;
                SirenixGUIStyles.PaneOptions.Draw(position, GUIContent.none, 0);
            }
            
            GUILayout.BeginVertical();
            CallNextDrawer(null);
            GUILayout.EndVertical();
            
            GUILayout.EndHorizontal();
        }
    }

    private OdinSelector<string> CreateSelector(Rect buttonRect)
    {
        var eventInstance = FMODUnity.RuntimeManager.CreateInstance(eventPathResolver.GetValue());
        eventInstance.getDescription(out var description);
        description.getParameterDescriptionCount(out var parameterDescriptionCount);

        var parameters = new List<string>();
        
        for (var i = 0; i < parameterDescriptionCount; i++)
        {
            description.getParameterDescriptionByIndex(i, out var parameterDescription);
            
            parameters.Add(parameterDescription.name);
        }
        
        var selector = new GenericSelector<string>(parameters);
        
        buttonRect.xMax = GUIHelper.GetCurrentLayoutRect().xMax;

        selector.EnableSingleClickToSelect();
        selector.ShowInPopup(buttonRect);
        return selector;
    }
}
