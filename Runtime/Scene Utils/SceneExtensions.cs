using System.Linq;
using UnityEngine.SceneManagement;

namespace Evbishop.Runtime.SceneUtils
{
    public static class SceneExtensions
    {
        public static SceneDesignation GetDesignation(this Scene scene)
        {
            var info = SceneHelper.Instance
                .Scenes
                .Values
                .FirstOrDefault(sceneInfo =>
                    sceneInfo.SceneReference == scene.path);
            if (info == null)
                return SceneDesignation.None;
            else return info.SceneDesignation;
        }

        public static bool HasDesignation(this Scene scene, SceneDesignation sceneDesignation)
            => SceneHelper.Instance.Scenes[sceneDesignation].SceneReference == scene.path;
    }
}