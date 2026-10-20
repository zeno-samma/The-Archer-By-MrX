using MrX.Player;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MrX
{
    public class HeroModelSwapTool : EditorWindow
    {
        protected SerializedObject heroSO;
        protected GameObject fbxAsset;

        [MenuItem("CONTEXT/HeroBehavior/Swap Hero Model")]
        private static void ProcessComponentData(MenuCommand command)
        {
            var heroBehavior = (HeroBehavior)command.context;

            var window = GetWindow<HeroModelSwapTool>("Character Model Swap Tool");
            window.heroSO = new SerializedObject(heroBehavior);
        }

        protected virtual void OnDisable()
        {
            if(heroSO != null) heroSO.Dispose();
        }

        private void OnGUI()
        {
            if (heroSO == null || heroSO.targetObject == null)
            {
                EditorGUILayout.LabelField("Couldn't find HeroBehavior Component");
                return;
            }

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var heroObject = heroSO.targetObject as HeroBehavior;

            if (prefabStage == null || heroObject.gameObject.scene != prefabStage.scene)
            {
                EditorGUILayout.LabelField("Open this tool from inside the prefab.");
                return;
            }

                var animatorProp = heroSO.FindProperty("animator");
            var animator = animatorProp.objectReferenceValue as Animator;
            if (animator == null)
            {
                EditorGUILayout.LabelField("The HeroBehavior component does not have an Animator assigned.");
                return;
            }

            if(animator.avatar == null || !animator.avatar.isHuman)
            {
                EditorGUILayout.LabelField("The Animator does not have a valid humanoid avatar assigned.");
                return;
            }

            EditorGUILayout.LabelField("Selected Hero: " + heroSO.targetObject.name);

            var selectedObject = (GameObject)EditorGUILayout.ObjectField("Select FBX File:", fbxAsset, typeof(GameObject), false);

            if (EditorGUI.EndChangeCheck())
            {
                if (selectedObject == null)
                {
                    fbxAsset = null;
                }
                else
                {
                    var assetPath = AssetDatabase.GetAssetPath(selectedObject);
                    if (Path.GetExtension(assetPath).ToLower() == ".fbx")
                    {
                        var testAnimator = selectedObject.GetComponent<Animator>();
                        if(testAnimator == null)
                        {
                            fbxAsset = null;
                            EditorUtility.DisplayDialog(
                                "Invalid FBX File",
                                "The selected .fbx file does not contain an Animator component. Please select a valid .fbx model.",
                                "OK"
                            );
                            return;
                        }

                        if(testAnimator.avatar == null || !testAnimator.avatar.isHuman)
                        {
                            fbxAsset = null;
                            EditorUtility.DisplayDialog(
                                "Invalid FBX File",
                                "The selected .fbx file does not contain a valid humanoid avatar. Please select a valid humanoid .fbx model.",
                                "OK"
                            );
                            return;
                        }
                        fbxAsset = selectedObject;
                    }
                    else
                    {
                        // Reject the file and warn the user
                        fbxAsset = null;
                        EditorUtility.DisplayDialog(
                            "Invalid File Type",
                            "Please select a valid .fbx file. Prefabs and other models are not allowed.",
                            "OK"
                        );
                    }
                }
            }
            
            GUI.enabled = fbxAsset != null;
            if (GUILayout.Button("Swap Model"))
            {
                SwapModel(animator);
            }
            GUI.enabled = true;
        }

        protected virtual void SwapModel(Animator animator)
        {
            var animatorParent = animator.transform.parent;
            GameObject newHeroObject = PrefabUtility.InstantiatePrefab(fbxAsset, animatorParent) as GameObject;

            var newAnimator = newHeroObject.GetComponent<Animator>();
            var newAvatar = newAnimator.avatar;

            var newRenderers = newAnimator.GetComponentsInChildren<Renderer>();
            var oldRenderers = animator.GetComponentsInChildren<Renderer>();

            var scaleFactor = 1f;

            if (oldRenderers.Length != 0 && newRenderers.Length != 0)
            {
                var oldBoundingBox = oldRenderers[0].bounds;
                for (int i = 1; i < oldRenderers.Length; i++)
                {
                    oldBoundingBox.Encapsulate(oldRenderers[i].bounds);
                }

                var newBoundingBox = newRenderers[0].bounds;
                for (int i = 1; i < newRenderers.Length; i++)
                {
                    newBoundingBox.Encapsulate(newRenderers[i].bounds);
                }

                var oldHeight = oldBoundingBox.size.y;
                var newHeight = newBoundingBox.size.y;
                scaleFactor = oldHeight / newHeight;
                
            }

            newHeroObject.transform.localScale *= scaleFactor;

            EditorUtility.CopySerialized(animator, newAnimator);
            newAnimator.avatar = newAvatar;
            EditorUtility.SetDirty(newAnimator);

            var addedObjects = GetAddedOverrideRoots(animator.gameObject);
            var boneTypes = System.Enum.GetValues(typeof(HumanBodyBones));

            foreach (var addedObject in addedObjects)
            { 
                var moved = false;
                foreach (var boneType in boneTypes)
                {
                    var boneTransform = animator.GetBoneTransform((HumanBodyBones)boneType);
                    if (boneTransform == null) continue;

                    if (addedObject.parent == boneTransform)
                    {
                        var newBoneTransform = newAnimator.GetBoneTransform((HumanBodyBones)boneType);
                        if (newBoneTransform != null)
                        {
                            Undo.RecordObject(addedObject, "Reparent Added Object");
                            var localPosition = addedObject.localPosition;
                            var localScale = addedObject.localScale;

                            addedObject.SetParent(newBoneTransform, false);

                            addedObject.localPosition = localPosition / scaleFactor;
                            addedObject.localScale = localScale / scaleFactor;
                        }
                        else
                        {
                            Undo.RecordObject(addedObject, "Reparent Added Object");

                            var localPosition = addedObject.localPosition;
                            var localScale = addedObject.localScale;

                            addedObject.SetParent(newAnimator.transform, false);

                            addedObject.localPosition = localPosition / scaleFactor;
                            addedObject.localScale = localScale / scaleFactor;
                        }
                        moved = true;
                        break;
                    }
                }

                if(!moved)
                {
                    Undo.RecordObject(addedObject, "Reparent Added Object");

                    var localPosition = addedObject.localPosition;
                    var localScale = addedObject.localScale;

                    addedObject.SetParent(newAnimator.transform, false);

                    addedObject.localPosition = localPosition / scaleFactor;
                    addedObject.localScale = localScale / scaleFactor;
                }
            }
            var eventsHandler = animator.GetComponent<HeroEventsHandler>();
            newAnimator.gameObject.AddComponent(eventsHandler.GetType());

            heroSO.FindProperty("animator").objectReferenceValue = newAnimator;
            heroSO.ApplyModifiedProperties();

            var hero = heroSO.targetObject as HeroBehavior;
            var renderersHandler = hero.GetComponentInChildren<RenderersHandler>();

            if(renderersHandler != null)
            {
                using (var renderersSO = new SerializedObject(renderersHandler))
                {
                    var renderersProp = renderersSO.FindProperty("renderers");
                    renderersProp.arraySize = newRenderers.Length;
                    for (int i = 0; i < newRenderers.Length; i++)
                    {
                        renderersProp.GetArrayElementAtIndex(i).objectReferenceValue = newRenderers[i];
                    }
                    renderersSO.ApplyModifiedProperties();
                }
            }

            DestroyImmediate(animator.gameObject);
            this.Close();
        }

        protected List<Transform> GetAddedOverrideRoots(GameObject model)
        {
            var result = new List<Transform>();

            foreach (Transform t in model.GetComponentsInChildren<Transform>(true))
            {
                if (t == model.transform ||
                    !PrefabUtility.IsAddedGameObjectOverride(t.gameObject))
                {
                    continue;
                }

                // If an added object already contains this object,
                // moving the parent will move the entire subtree.
                if (t.parent != model.transform &&
                    t.parent != null &&
                    PrefabUtility.IsAddedGameObjectOverride(t.parent.gameObject))
                {
                    continue;
                }

                result.Add(t);
            }

            return result;
        }
    }
}