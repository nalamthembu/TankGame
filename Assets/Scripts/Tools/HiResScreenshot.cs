using System.IO;
using UnityEngine;

public class HiResScreenshot : MonoBehaviour
{
    public int resWidth, resHeight;

    private bool takeHiResShot = false;

    [SerializeField] string screenshotPath;

    public string ScreenShotName(int width, int height)
    {
        string PATH = screenshotPath != string.Empty ?
            screenshotPath : Application.dataPath;

        return string.Format("{0}/screenshots/screen_{1}x{2}_{3}.png",
            PATH, width, height,
            System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    [ContextMenu("Take Screenshot")]
    public void TakeHiResShot()
    {
        takeHiResShot = true;

        if (takeHiResShot)
        {
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            Camera.main.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            Camera.main.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            Camera.main.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = ScreenShotName(resWidth, resHeight);
            File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to : {0}", filename));
            takeHiResShot = false;
        }
    }

    public void LateUpdate()
    {
        takeHiResShot |= Input.GetKeyDown(KeyCode.F1);

        if (takeHiResShot)
        {
            TakeHiResShot();
        }
    }
}
