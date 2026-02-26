using UnityEngine;

[ExecuteInEditMode] // 允许在编辑器模式下运行
[RequireComponent(typeof(Renderer))] // 确保对象有Renderer组件
public class SortingLayerController : MonoBehaviour
{
    [Tooltip("选择Sorting Layer")]
    public string sortingLayerName = "Default";

    [Tooltip("Order in Layer（数值越大越靠前）")]
    public int sortingOrder = 0;

    private Renderer cachedRenderer;

    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
        UpdateSortingProperties();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 在编辑器模式下修改参数时实时更新
        if (cachedRenderer == null) 
            cachedRenderer = GetComponent<Renderer>();
        UpdateSortingProperties();
    }
#endif

    private void Update()
    {
        // 确保运行时和编辑器模式下的渲染器同步
        if (cachedRenderer == null) 
            cachedRenderer = GetComponent<Renderer>();
        UpdateSortingProperties();
    }

    /// <summary>
    /// 应用排序层级设置
    /// </summary>
    private void UpdateSortingProperties()
    {
        if (cachedRenderer != null) 
        {
            cachedRenderer.sortingLayerName = sortingLayerName;
            cachedRenderer.sortingOrder = sortingOrder;
        }
    }

    /// <summary>
    /// 获取所有可用的Sorting Layer名称（用于自定义Editor下拉菜单）
    /// </summary>
    public static string[] GetAllSortingLayerNames()
    {
#if UNITY_EDITOR
        var sortingLayers = typeof(UnityEditorInternal.InternalEditorUtility)
            .GetProperty("sortingLayerNames", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            ?.GetValue(null, null) as string[]; // 修正：添加反射调用的参数

        return sortingLayers ?? new string[] { "Default" };
#else
        return new string[] { "Default" }; // 运行时无法获取，返回默认值
#endif
    }
}