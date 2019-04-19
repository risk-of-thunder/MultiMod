using MultiMod.Interface;
using UnityEngine;
using UnityEngine.UI;

public class ExampleMod : MonoBehaviour
{
    public GameObject prefab;
    public ContentHandler Content { get; set; }
    void Start()
    {
        Debug.Log("Hello World!!???");


        On.RoR2.UserProfile.HasSurvivorUnlocked += (o, s, i) => true;

        On.RoR2.UserProfile.HasDiscoveredPickup += (o, s, i) => true;

        On.RoR2.UserProfile.HasAchievement += (o, s, i) => true;

        On.RoR2.UserProfile.CanSeeAchievement += (o, s, i) => true;

        On.RoR2.UserProfile.HasUnlockable_UnlockableDef += (o, s, i) => true;

        var canvas = RoR2.RoR2Application.instance.mainCanvas;
        if (canvas == null)
        {
            Debug.Log("Couldn't find `MainCanvas` in scene.");
        } else
        {
            Debug.Log("Found main canvas.");
        }

        var gobj = GameObject.Instantiate(prefab);
        gobj.transform.SetParent(canvas.transform, false);
        var tobj = gobj.transform.Find("Text");
        var text = tobj.GetComponent<Text>();
        text.text = $"IsModded={RoR2.RoR2Application.isModded}";
        var rect = gobj.GetComponent<RectTransform>();
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.anchorMin = new Vector2(0.00f, 0.00f);
        rect.anchorMax = new Vector2(1.00f, 1.00f);
    }
}