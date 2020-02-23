﻿using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Networking;
using NUnit.Framework;
using System.Collections;
using System.IO;
using GLTFast;

public class SampleModelsTest
{
    const string prefix = "glTF-Sample-Models/2.0/";

    [UnityTest]
    public IEnumerator SampleModelsTestCheckFiles()
    {
        yield return GltfSampleModels.LoadGlbFileUrls();
        CheckFileExist(GltfSampleModels.glbFileUrls);

        yield return GltfSampleModels.LoadGltfFileUrls();
        CheckFileExist(GltfSampleModels.gltfFileUrls);
    }

    void CheckFileExist(string[] files)
    {
#if !(UNITY_ANDROID && !UNITY_EDITOR)
        foreach (var file in files)
        {
            var path = Path.Combine(Application.streamingAssetsPath, Path.Combine(prefix, file));
            Assert.IsTrue(
                File.Exists(path)
                , "file {0} not found"
                , path
            );
        }
#else
		// See https://docs.unity3d.com/Manual/StreamingAssets.html
		Debug.Log("File access doesn't work on Android");
#endif
    }

    [UnityTest]
    [Timeout(1000000)]
    public IEnumerator SampleModelsTestLoadAllGlb()
    {
        yield return GltfSampleModels.LoadGlbFileUrls();

        foreach (var file in GltfSampleModels.glbFileUrls)
        {
            var path = string.Format(
#if UNITY_ANDROID && !UNITY_EDITOR
				"{0}"
#else
                "file://{0}"
#endif
                , Path.Combine(Application.streamingAssetsPath, Path.Combine(prefix, file))
            );

            Debug.LogFormat("Testing {0}", path);

            var go = new GameObject();
            var gltfAsset = go.AddComponent<GltfAsset>();

            bool done = false;

            gltfAsset.onLoadComplete += (success) => { done = true; Assert.IsTrue(success); };
            gltfAsset.url = path;

            while (!done)
            {
                yield return null;
            }
            Object.Destroy(go);
        }
    }

    [UnityTest]
    [Timeout(1000000)]
    public IEnumerator SampleModelsTestLoadAllGltf()
    {
        yield return GltfSampleModels.LoadGltfFileUrls();

        foreach (var file in GltfSampleModels.gltfFileUrls)
        {
            var path = string.Format(
#if UNITY_ANDROID && !UNITY_EDITOR
                "{0}"
#else
                "file://{0}"
#endif
                , Path.Combine(Application.streamingAssetsPath, Path.Combine(prefix, file))
            );

            Debug.LogFormat("Testing {0}", path);

            var webRequest = UnityWebRequest.Get(path);
            yield return webRequest.SendWebRequest();
            Assert.Null(webRequest.error, webRequest.error);
            Assert.IsFalse(webRequest.isNetworkError);
            Assert.IsFalse(webRequest.isHttpError);
            var json = webRequest.downloadHandler.text;

            Assert.NotNull(json);
            Assert.Greater(json.Length, 0);

            var go = new GameObject(GltfSampleModels.GetNameFromPath(file));
            var gltfAsset = go.AddComponent<GltfAsset>();

            bool done = false;

            gltfAsset.onLoadComplete += (success) => { done = true; Assert.IsTrue(success); };
            gltfAsset.url = path;

            while (!done)
            {
                yield return null;
            }
            Object.Destroy(go);
        }
    }
}
