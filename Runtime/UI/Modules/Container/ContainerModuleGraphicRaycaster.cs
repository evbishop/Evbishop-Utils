using Evbishop.Runtime.ModulesSystem;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace Evbishop.Runtime.UI.Modules.Container
{
    [Title("Graphic Raycaster")]
    public class ContainerModuleGraphicRaycaster : ContainerModule, INeedComponent<GraphicRaycaster>
    {
        [OdinSerialize] public GraphicRaycaster Component { get; set; }
        [field: SerializeField, LabelText("Disable GR on hide")] public bool IsDisablingGraphicRaycasterOnHide { get; private set; } = true;
    }
}