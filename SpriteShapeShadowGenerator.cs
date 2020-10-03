public static class ShadowCaster2DExtensions
{
    /// <summary>
    /// Replaces the path that defines the shape of the shadow caster.
    /// </summary>
    /// <remarks>
    /// Calling this method will change the shape but not the mesh of the shadow caster. Call SetPathHash afterwards.
    /// </remarks>
    /// <param name="shadowCaster">The object to modify.</param>
    /// <param name="path">The new path to define the shape of the shadow caster.</param>
    public static void SetPath(this ShadowCaster2D shadowCaster, Vector3[] path)
    {
        FieldInfo shapeField = typeof(ShadowCaster2D).GetField("m_ShapePath",
            BindingFlags.NonPublic |
            BindingFlags.Instance);
        shapeField.SetValue(shadowCaster, path);
    }

    /// <summary>
    /// Replaces the hash key of the shadow caster, which produces an internal data rebuild.
    /// </summary>
    /// <remarks>
    /// A change in the shape of the shadow caster will not block the light, it has to be rebuilt using this function.
    /// </remarks>
    /// <param name="shadowCaster">The object to modify.</param>
    /// <param name="hash">The new hash key to store. It must be different from the previous key to produce the rebuild. You can use a random number.</param>
    public static void SetPathHash(this ShadowCaster2D shadowCaster, int hash)
    {
        FieldInfo hashField = typeof(ShadowCaster2D).GetField("m_ShapePathHash",
            BindingFlags.NonPublic |
            BindingFlags.Instance);
        hashField.SetValue(shadowCaster, hash);
    }
}

/// <summary>
/// https://forum.unity.com/threads/script-for-generating-shadowcaster2ds-for-tilemaps.906767/
/// </summary>
public class SpriteShapeShadowGenerator : OdinEditorWindow
{
    private static class SpriteShapeControllerTypes
    {
        // int
        public static readonly FieldInfo ColliderOffsetFieldInfo = typeof(SpriteShapeController).GetField("m_ColliderOffset", BindingFlags.NonPublic | BindingFlags.Instance);
        // int
        public static readonly FieldInfo ColliderDetailFieldInfo = typeof(SpriteShapeController).GetField("m_ColliderDetail", BindingFlags.NonPublic | BindingFlags.Instance);
        // bool
        public static readonly FieldInfo UpdateColliderFieldInfo = typeof(SpriteShapeController).GetField("m_UpdateCollider", BindingFlags.NonPublic | BindingFlags.Instance);
        // bool
        public static readonly FieldInfo OptimizeColliderFieldInfo = typeof(SpriteShapeController).GetField("m_OptimizeCollider", BindingFlags.NonPublic | BindingFlags.Instance);
        // () => JobHandle
        public static readonly MethodInfo ScheduleBakeMethodInfo = typeof(SpriteShapeController).GetMethod("ScheduleBake", BindingFlags.NonPublic | BindingFlags.Instance);
        // NativeArray<float2>
        public static readonly FieldInfo ColliderDataFieldInfo = typeof(SpriteShapeController).GetField("m_ColliderData", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    private static class ShadowCaster2DTypes
    {
        // int[]
        public static readonly FieldInfo ApplyToSortingLayersFieldInfo = typeof(ShadowCaster2D).GetField("m_ApplyToSortingLayers", BindingFlags.NonPublic | BindingFlags.Instance);
    }
    
    [MenuItem("Frostbit/Sprite Shape Shadow Generator")]
    private static void OpenWindow()
    {
        GetWindow<SpriteShapeShadowGenerator>().Show();
    }

    [HorizontalGroup(nameof(selectedGameObject))]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private GameObject selectedGameObject;

    [HorizontalGroup(nameof(selectedGameObject)), Button("Fetch settings")]
    private void FetchSettingsFromSelected()
    {
        if (selectedGameObject != null && selectedGameObject.TryGetComponent<SpriteShapeController>(out var spriteShapeController))
        {
            offset = (float) SpriteShapeControllerTypes.ColliderOffsetFieldInfo.GetValue(spriteShapeController);
            quality = (int) SpriteShapeControllerTypes.ColliderDetailFieldInfo.GetValue(spriteShapeController);
        }
    }

    [Range(-0.5f, 0.5f)]
    [SerializeField] private float offset;
    
    [Tooltip("Quality of the generated mesh")]
    [Range(4f, 16f)]
    [SerializeField] private int quality;

    [Tooltip("Sets the sorting layer of the ShadowCaster2D")]
    [SerializeField] private bool useSortingLayer;

    [Tooltip("Sets the sorting layer of the ShadowCaster2D")]
    [SortingLayerDropdown, Indent, ShowIf(nameof(useSortingLayer))] 
    [SerializeField] private int sortingLayer;

    private bool SelectedGameObjectIsASpriteShape() => 
        selectedGameObject != null && 
        selectedGameObject.HasComponent<SpriteShapeController>();
    
    [Button(ButtonSizes.Gigantic), EnableIf(nameof(SelectedGameObjectIsASpriteShape))]
    private void GenerateShadowCaster()
    {
        if (!SelectedGameObjectIsASpriteShape()) return;
        
        var shadowCaster = selectedGameObject.GetComponent<ShadowCaster2D>();
        if (shadowCaster == null)
        {
            shadowCaster = Undo.AddComponent<ShadowCaster2D>(selectedGameObject);
            shadowCaster.selfShadows = true;
        }

        var spriteShapeController = selectedGameObject.GetComponent<SpriteShapeController>();
        var shadowPath = GetShadowPath(spriteShapeController);
        
        shadowCaster.SetPath(shadowPath);
        shadowCaster.SetPathHash(Random.Range(int.MinValue, int.MaxValue)); // The hashing function GetShapePathHash could be copied from the LightUtility class
        
        ShadowCaster2DTypes.ApplyToSortingLayersFieldInfo.SetValue(shadowCaster, new[] { sortingLayer });
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        Selection.selectionChanged += SelectionChanged;
        SelectionChanged();
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= SelectionChanged;
    }
    
    private void SelectionChanged()
    {
        selectedGameObject = Selection.activeGameObject;
    }

    /// <summary>
    /// Uses ugly reflection to generate and get the m_ColliderData from a SpriteShapeController
    /// </summary>
    private Vector3[] GetShadowPath(SpriteShapeController spriteShapeController)
    {
        // Get the existing values
        var oldColliderOffsetValue = SpriteShapeControllerTypes.ColliderOffsetFieldInfo.GetValue(spriteShapeController);
        var oldColliderDetailValue = SpriteShapeControllerTypes.ColliderDetailFieldInfo.GetValue(spriteShapeController);
        var oldUpdateColliderValue = SpriteShapeControllerTypes.UpdateColliderFieldInfo.GetValue(spriteShapeController);
        var oldOptimizeColliderValue = SpriteShapeControllerTypes.OptimizeColliderFieldInfo.GetValue(spriteShapeController);

        // Set the values we want
        SpriteShapeControllerTypes.ColliderOffsetFieldInfo.SetValue(spriteShapeController, offset);
        SpriteShapeControllerTypes.ColliderDetailFieldInfo.SetValue(spriteShapeController, quality);
        SpriteShapeControllerTypes.UpdateColliderFieldInfo.SetValue(spriteShapeController, true);
        SpriteShapeControllerTypes.OptimizeColliderFieldInfo.SetValue(spriteShapeController, true);
        
        var scheduleBakeJobHandle = (JobHandle) SpriteShapeControllerTypes.ScheduleBakeMethodInfo.Invoke(spriteShapeController, new object[0]);
        
        scheduleBakeJobHandle.Complete();
        
        var colliderData = (NativeArray<float2>) SpriteShapeControllerTypes.ColliderDataFieldInfo.GetValue(spriteShapeController);
        
        // Reset to the values the SpriteShapeController had before
        SpriteShapeControllerTypes.ColliderOffsetFieldInfo.SetValue(spriteShapeController, oldColliderOffsetValue);
        SpriteShapeControllerTypes.ColliderDetailFieldInfo.SetValue(spriteShapeController, oldColliderDetailValue);
        SpriteShapeControllerTypes.UpdateColliderFieldInfo.SetValue(spriteShapeController, oldUpdateColliderValue);
        SpriteShapeControllerTypes.OptimizeColliderFieldInfo.SetValue(spriteShapeController, oldOptimizeColliderValue);
        
        // Turn the colliderData into a List<Vector3>
        var shadowPath = new List<Vector3>();
        foreach (var now in colliderData)
        {
            if (!math.any(now)) break;
            
            shadowPath.Add(new Vector3(now.x, now.y));
        }

        // Gives the SpriteShapeController it's correct m_ColliderData
        var cleanUpScheduleBakeJobHandle = (JobHandle) SpriteShapeControllerTypes.ScheduleBakeMethodInfo.Invoke(spriteShapeController, new object[0]);
        cleanUpScheduleBakeJobHandle.Complete();
        
        return shadowPath.ToArray();
    }
}
