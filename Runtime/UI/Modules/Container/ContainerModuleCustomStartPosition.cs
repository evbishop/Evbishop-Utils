using UnityEngine;

namespace Evbishop.Runtime.UI.Modules.Container
{
    public class ContainerModuleCustomStartPosition : ContainerModule
    {
        [SerializeField] private Vector3 _position;

        public void SetPosition(Transform transform)
        {
            transform.localPosition = _position;
        }
    }
}