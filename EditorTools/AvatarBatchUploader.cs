using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase.Editor.BuildPipeline;
using VRC.SDKBase.Editor;
using VRC.SDKBase;
using System;
using System.Linq;
using System.Threading.Tasks;
using VRC.SDK3A.Editor;
using VRC.Core;
using VRC.SDKBase.Editor.Api;
using System.IO;

namespace eepyfemboi.EditorTools
{
    public class AvatarBatchUploader : EditorWindow
    {
        private GameObject parentObject;
        private string description;
        private string nameAppend;
        private VRCAvatar avatarData;
        private bool isUploadInProgress = false;
        private string thumbPath;

        private static IVRCSdkAvatarBuilderApi builder;
        //private IVRCSdkPanelApi panel;

        [MenuItem("Tools/Avatar Batch Uploader")]
        public static void ShowWindow()
        {
            GetWindow<AvatarBatchUploader>("Avatar Batch Uploader");
        }

        [InitializeOnLoadMethod]
        public static void RegisterSDKCallback()
        {
            VRCSdkControlPanel.OnSdkPanelEnable += AddBuildHook;
        }

        private static void AddBuildHook(object sender, EventArgs e)
        {
            VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out builder);
        }

        private void OnGUI()
        {
            GUILayout.Label("Avatar Batch Uploader", EditorStyles.boldLabel);

            parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", parentObject, typeof(GameObject), true);
            description = EditorGUILayout.TextField("Description", description);
            nameAppend = EditorGUILayout.TextField("Name Append", nameAppend);

            if (GUILayout.Button("Upload Avatars"))
            {
                UploadAvatars();
            }
        }

        private async void UploadAvatars()
        {
            if (parentObject == null || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(nameAppend))
            {
                Debug.LogError("Please ensure all fields are correctly filled.");
                return;
            }

            foreach (Transform child in parentObject.transform)
            {
                VRCAvatarDescriptor avatarDescriptor = child.GetComponent<VRCAvatarDescriptor>();
                if (avatarDescriptor == null)
                {
                    Debug.LogWarning($"No VRCAvatarDescriptor found on {child.name}");
                    continue;
                }

                PipelineManager pipelineManager = child.GetComponent<PipelineManager>();
                if (pipelineManager == null)
                {
                    Debug.LogError($"No VRC_AvatarPipelineManager found on {child.name}");
                    continue;
                }

                if (string.IsNullOrEmpty(pipelineManager.blueprintId))
                {
                    SetAvatarProperties(avatarDescriptor);
                }
                else
                {
                    try
                    {
                        avatarData = await VRCApi.GetAvatar(pipelineManager.blueprintId, true);
                    } catch
                    {
                        SetAvatarProperties(avatarDescriptor);
                    }
                }

                isUploadInProgress = true;
                await BuildAndUploadAvatar(avatarDescriptor);

                while (isUploadInProgress)
                {
                    await Task.Delay(5000);
                }

                Debug.Log($"Did: {child.name}");

                await Task.Delay(30000);
            }
        }

        private void SetAvatarProperties(VRCAvatarDescriptor avatarDescriptor)
        {
            avatarData = new VRCAvatar();
            Transform childTransform = avatarDescriptor.transform;

            string[] nameParts = childTransform.name.Split('|');
            if (nameParts.Length < 2)
            {
                Debug.LogError($"Invalid name format on {childTransform.name}. Expected format: 'anything|AvatarName'");
                return;
            }

            string avatarName = nameParts[1] + nameAppend;

            if (builder != null)
            {
                avatarData.Name = avatarName;
                avatarData.Description = description;
                avatarData.ReleaseStatus = "public";
                avatarData.Tags = new List<string>();
                AvatarBuilderSessionState.AvatarName = avatarName;
                AvatarBuilderSessionState.AvatarDesc = description;
                AvatarBuilderSessionState.AvatarReleaseStatus = "public";
                AvatarBuilderSessionState.AvatarTags = "";
                //avatarData.Name = avatarName;
                //avatarData.Description = description;
                //builder.SetAvatarName(childTransform.gameObject, avatarName);
                //builder.SetAvatarDescription(childTransform.gameObject, description);
            }
            else
            {
                Debug.LogError("Builder instance not found. Ensure the SDK window is open.");
                return;
            }

            MeshRenderer meshRenderer = childTransform.GetComponentsInChildren<MeshRenderer>()
                .FirstOrDefault(renderer => renderer.sharedMaterial != null && renderer.sharedMaterial.mainTexture != null);

            if (meshRenderer != null)
            {
                Texture2D thumbnailTexture = (Texture2D)meshRenderer.sharedMaterial.mainTexture;
                string thumbnailPath = AssetDatabase.GetAssetPath(thumbnailTexture);//$"Assets/Thumbnails/{avatarName}.png";
                                                                                    //SaveTextureAsPNG(thumbnailTexture, thumbnailPath);

                if (builder != null)
                {

                    AvatarBuilderSessionState.AvatarThumbPath = thumbnailPath;
                    thumbPath = thumbnailPath;

                    //builder.SetAvatarThumbnail(childTransform.gameObject, thumbnailPath);
                }
                else
                {
                    Debug.LogError("Builder instance not found. Ensure the SDK window is open.");
                }
            }
            else
            {
                Debug.LogError($"No MeshRenderer with a valid main texture found on {childTransform.name}");
            }
        }

        private async Task BuildAndUploadAvatar(VRCAvatarDescriptor avatarDescriptor)
        {
            if (builder != null)
            {
                try
                {
                    isUploadInProgress = true;
                    await builder.BuildAndUpload(avatarDescriptor.gameObject, avatarData, thumbPath);
                    //await builder.BuildAndUpload(avatarDescriptor.gameObject);
                    Debug.Log($"Successfully uploaded avatar: {avatarDescriptor.gameObject.name}");
                    isUploadInProgress = false;
                }
                catch (Exception e)
                {
                    isUploadInProgress = false;
                    Debug.LogError($"Failed to upload avatar: {avatarDescriptor.gameObject.name} - {e.Message}");
                }
            }
            else
            {
                isUploadInProgress = false;
                Debug.LogError("Builder instance not found. Ensure the SDK window is open.");
            }
        }

        private void SaveTextureAsPNG(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
        }
    }
}
#endif
