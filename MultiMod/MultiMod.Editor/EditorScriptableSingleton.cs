using UnityEngine;

namespace MultiMod.Editor
{
    public abstract class EditorScriptableSingleton<T> : ScriptableObject where T : EditorScriptableSingleton<T>
    {
        //Note: Unity versions 5.6 and earlier fail to load ScriptableObject assets for Types that are defined in an editor assembly 
        //and derive from a Type defined in a non-editor assembly.

        public static T instance
        {
            get
            {
                if (_instance == null)
                    GetInstance();

                return _instance;
            }
        }

        private static T _instance;

        protected EditorScriptableSingleton()
        {
            if (_instance == null)
                _instance = this as T;
        }

        void OnEnable()
        {
            if (_instance == null)
                _instance = this as T;
        }

        private static void GetInstance()
        {
            Debug.Log("Getting export settings.");

            if (_instance == null)
                Debug.Log("Instance was null, loading resource.");
                _instance = Resources.Load<T>(typeof(T).Name);

            if (_instance == null)
            {
                Debug.Log("Resource not found, creating...");
                _instance = CreateInstance<T>();

                if (Application.isEditor)
                {
                    Debug.Log("Creating asset resource.");
                    CreateAsset();
                }
            }
        }

        private static void CreateAsset()
        {
            AssetUtility.CreateAsset(_instance);
        }
    }
}
