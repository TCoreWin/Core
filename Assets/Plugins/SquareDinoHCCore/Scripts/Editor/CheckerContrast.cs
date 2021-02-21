using System.IO;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEditorInternal;
using UnityEngine;
using PackageInfo = UnityEditor.PackageInfo;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace SquareDino.ChackerContrast {
    class CheckerContrast : EditorWindow
    {
        private static GameObject go;
        private static ListRequest listRequest;
        private static AddRequest addRequest;
        private static SearchRequest postProcessingrequest;
        private bool isHaveAsset;
        
        #if UNITY_POST_PROCESSING_STACK_V2
            private PostProcessProfile profile;
            private PostProcessLayer postProcessingLayer;
        #endif

        private static bool haveAsset;
        private static bool initialized;
        
        [MenuItem("Window/SquareDino/CheckerContrast")]
        static void Init()
        {
            CheckerContrast window = (CheckerContrast)GetWindow(typeof(CheckerContrast));
            
            listRequest = Client.List();
            EditorApplication.update += Progress;
        }
        
        private void OnGUI()
        {
            if(!initialized) return;
            
            if (haveAsset)
            {
                if(haveAsset)
                    Refresh();
            }
        }

        private static void Progress()
        {
            if (listRequest.IsCompleted)
            {
                foreach (var result in listRequest.Result)
                {
                    Debug.Log(result.name);
                    
                    if (result.name == "com.unity.postprocessing")
                    {
                        haveAsset = true;
                        break;
                    }
                }
                
                EditorApplication.update -= Progress;

                if (haveAsset)
                    initialized = true;
                else
                {
                    addRequest = Client.Add("com.unity.postprocessing");
                    EditorApplication.update += DownloadPostProcessing;
                }
            }
        }

        private static void DownloadPostProcessing()
        {
            if (addRequest.IsCompleted)
            {
                if (addRequest.Result.status == PackageStatus.Available)
                {
                    haveAsset = true;
                    initialized = true;
                }

                EditorApplication.update -= DownloadPostProcessing;
            }
        }

        private void Refresh()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            if (profile != null)
                isHaveAsset = true;
            else
                isHaveAsset = false;
            
            GUI.enabled = !isHaveAsset;
            
                if (GUILayout.Button("Create profile to PostProcessing"))
                {
                    if (profile == null)
                    {                    
                        profile = ScriptableObject.CreateInstance<PostProcessProfile>();
                        AssetDatabase.CreateAsset(profile, "Assets/CheckerContrastProfile.asset");
                        //EditorUtility.FocusProjectWindow();
                        //Selection.activeObject = profile;
                        
                        var granding = new ColorGrading();
                        granding.saturation.overrideState = true;
                        granding.saturation.value = -100;
                        
                        var grandingLinq = profile.AddSettings(granding);
                        grandingLinq.enabled.value = true;
                        EditorUtility.SetDirty(profile);
                        AssetDatabase.SaveAssets();
                    }  
                }
            
            GUI.enabled = true;

            GUI.enabled = isHaveAsset;
            
            if (GUILayout.Button("Enable contrast"))
            {
                var camera = Camera.main;

                if (go != null)
                {
                    go.transform.position = camera.transform.position;
                }
            
                if (camera != null)
                {    
                    go = new GameObject("PostProcessing Volume");
                    var boxCollier = go.AddComponent<BoxCollider>();
                    boxCollier.size = new Vector3(1, 1, 1);
                    
                    go.transform.position = camera.transform.position;
                    postProcessingLayer = camera.gameObject.AddComponent<PostProcessLayer>();
                    postProcessingLayer.volumeLayer = ~0;
                    var layer = go.AddComponent<PostProcessVolume>();
                    layer.blendDistance = 1000;
                    layer.profile = profile;
                }
            }

            if (GUILayout.Button("Disable contrast"))
            {
                DestroyImmediate(go);
                DestroyImmediate(postProcessingLayer);
            }
#endif
        }        

    }
}