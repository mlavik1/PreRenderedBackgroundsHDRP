using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class ResourcePathUtils
{
    public static string GetDepthTexturePath(PrerenderingCamera camera)
    {
        return System.IO.Path.Combine(GetCameraResourcesPath(camera), "depth.png");
    }

    public static string GetBackgroundTexturePath(PrerenderingCamera camera)
    {
        return System.IO.Path.Combine(GetCameraResourcesPath(camera), "background.png");
    }

    public static string GetCameraResourcesPath(PrerenderingCamera camera)
    {
        return System.IO.Path.Combine(GetSceneResourcesPath(camera.gameObject.scene), camera.name);
    }

    public static string GetSceneResourcesPath(Scene scene)
    {
        return System.IO.Path.Combine(Application.streamingAssetsPath, "PrerenderedCameras", scene.name);
    }

    public static void EnsureResourceFolderExists(PrerenderingCamera camera)
    {
        string sceneResPath = GetSceneResourcesPath(camera.gameObject.scene);
        if (!Directory.Exists(sceneResPath))
            Directory.CreateDirectory(sceneResPath);
        
        string cameraResPath = GetCameraResourcesPath(camera);
        if (!Directory.Exists(cameraResPath))
            Directory.CreateDirectory(cameraResPath);
    }

    public static void MoveOldResourceFiles(PrerenderingCamera camera, string oldPath)
    {
        EnsureResourceFolderExists(camera);
        string newPath = GetCameraResourcesPath(camera);

        DirectoryInfo dirInfo = new DirectoryInfo(newPath);
        Debug.Assert(dirInfo.Exists);

        string[] files = Directory.GetFiles(oldPath, "*.*", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            FileInfo fileInfo = new FileInfo(file);
            string newFilePath = Path.Combine(dirInfo.FullName, fileInfo.Name);
            if (File.Exists(newFilePath))
                File.Replace(oldPath, newPath, newPath + ".backup");
            else
                fileInfo.MoveTo(Path.Combine(dirInfo.FullName, fileInfo.Name));
        }

        Directory.Delete(oldPath);
    }
}
