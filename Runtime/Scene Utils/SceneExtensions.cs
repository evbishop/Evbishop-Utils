using System.Linq;
using UnityEngine.SceneManagement;

namespace Evbishop.Runtime.SceneUtils
{
    public static class SceneExtensions
    {
        public static SceneDesignation GetDesignation(this Scene scene)
            => SceneHelper.Instance
                .Scenes
                .Values
                .FirstOrDefault(sceneInfo =>
                    sceneInfo.SceneReference.BuildIndex == scene.buildIndex)
                .SceneDesignation;

        public static bool HasDesignation(this Scene scene, SceneDesignation sceneDesignation)
            => SceneHelper.Instance.Scenes[sceneDesignation].SceneReference.BuildIndex == scene.buildIndex;
    }
}