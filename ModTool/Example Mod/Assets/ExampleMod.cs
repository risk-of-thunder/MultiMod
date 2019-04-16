using UnityEngine;
using UnityEngine.UI;
using ModTool.Interface;

public class ExampleMod : ModBehaviour
{
    public GameObject prefab;
    public ContentHandler Content { get; set; }
    void Start()
    {
        Debug.Log("Hello World!");

        var canvas = GameObject.Find("MainCanvas");

        if (canvas == null)
        {
            Debug.Log("Couldn't find `MainCanvas` in scene.");
        }


        var gobj = Content.Instantiate(prefab);
        gobj.transform.SetParent(canvas.transform, false);
        var rect = gobj.GetComponent<RectTransform>();
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.anchorMin = new Vector2(0.25f, 0.25f);
        rect.anchorMax = new Vector2(0.75f, 0.75f);
    }
}
