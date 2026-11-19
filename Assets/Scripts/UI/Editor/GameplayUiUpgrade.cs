#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Công cụ dựng giao diện; không chạy trong bản build.</summary>
public static class GameplayUiUpgrade
{
    private static readonly Color Ink = new Color(.045f, .065f, .11f, .97f);
    private static readonly Color Paper = new Color(.94f, .9f, .8f);
    private static Sprite panel;
    private static Sprite hudPanel;
    private static TMPro.TMP_FontAsset regular;
    private static TMPro.TMP_FontAsset heading;

    private static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 pos)
    {
        var t = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        t.SetParent(parent, false);
        t.sizeDelta = size;
        t.anchoredPosition = pos;
        return t;
    }

    private static void Place(RectTransform t, Vector2 anchor, Vector2 size, Vector2 pos)
    {
        t.anchorMin = t.anchorMax = anchor;
        t.pivot = new Vector2(.5f, .5f);
        t.sizeDelta = size;
        t.anchoredPosition = pos;
        t.localScale = Vector3.one;
    }

    private static UnityEngine.UI.Image Surface(RectTransform t, Sprite sprite)
    {
        var image = t.GetComponent<UnityEngine.UI.Image>() ?? t.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.sprite = sprite;
        image.type = UnityEngine.UI.Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = 2f;
        image.color = Color.white;
        image.raycastTarget = false;
        return image;
    }

    private static void Label(string name, Transform parent, string text, Vector2 pos, Vector2 size, float fontSize, Color color)
    {
        var t = Rect(name, parent, size, pos);
        var label = t.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
        label.font = heading;
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        label.raycastTarget = false;
    }

    private static RectTransform Popup(Transform root)
    {
        var children = root.Cast<Transform>().ToArray();
        var rt = (RectTransform)root;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
        var dim = Rect("Backdrop", root, Vector2.zero, Vector2.zero);
        dim.anchorMin = Vector2.zero; dim.anchorMax = Vector2.one;
        dim.offsetMin = dim.offsetMax = Vector2.zero;
        var image = dim.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(.012f, .02f, .04f, .78f);
        var group = dim.gameObject.AddComponent<CanvasGroup>();
        dim.SetAsFirstSibling();
        var content = Rect("Content", root, new Vector2(100,100), Vector2.zero);
        content.gameObject.AddComponent<CanvasGroup>();
        foreach (var child in children) child.SetParent(content, false);
        var transition = root.gameObject.AddComponent<UIPopupTransition>();
        var so = new SerializedObject(transition);
        so.FindProperty("content").objectReferenceValue = content;
        so.FindProperty("backdrop").objectReferenceValue = group;
        so.ApplyModifiedPropertiesWithoutUndo();
        return content;
    }

    public static string Apply()
    {
        if (EditorApplication.isPlaying) return "Stop Play Mode first";
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/Map/LoadMapScene.unity") return "Wrong scene";
        var roots = scene.GetRootGameObjects();
        var canvases = roots.SelectMany(r => r.GetComponentsInChildren<Canvas>(true)).ToArray();
        var main = canvases.First(c => c.transform.Find("ExitConfirm"));
        if (main.transform.Find("ExitConfirm/Content")) return "Already upgraded";
        var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Sprites/UI/GameplayTheme/CrystalPanel.png");
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spriteBorder = new Vector4(280,280,280,280);
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        panel = AssetDatabase.LoadAssetAtPath<Sprite>(importer.assetPath);
        hudPanel = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Popup/BlossomPopupFrame.png");
        regular = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/Resources/Typography/SourceSans3-Regular SDF.asset");
        heading = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/Resources/Typography/SourceSans3-Semibold SDF.asset");
        foreach (var text in roots.SelectMany(r => r.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true)))
        {
            text.font = text.fontSize >= 40 ? heading : regular;
            text.fontStyle = TMPro.FontStyles.Normal;
            text.color = Paper;
        }

        var exit = Popup(main.transform.Find("ExitConfirm"));
        var banner = (RectTransform)exit.Find("Banner");
        Place(banner, new Vector2(.5f,.5f), new Vector2(940,460), Vector2.zero);
        Surface(banner, panel);
        var title = banner.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        Place(title.rectTransform, new Vector2(.5f,.5f), new Vector2(700,90), new Vector2(0,80));
        title.fontSize = 44;
        foreach (var name in new[]{"ButtonYes","ButtonNo"})
        {
            var b = (RectTransform)banner.Find(name);
            Place(b, new Vector2(.5f,.5f), new Vector2(280,85), new Vector2(name=="ButtonYes"?-175:175,-90));
            b.GetComponentInChildren<TMPro.TextMeshProUGUI>().fontSize = 32;
        }

        var setting = Popup(main.transform.Find("SettingGame"));
        var board = (RectTransform)setting.Find("BoadSettingVolume");
        board.anchoredPosition = new Vector2(0,70);
        Surface(board,panel).pixelsPerUnitMultiplier = 1f;
        var settingsTitle = board.Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
        settingsTitle.fontSize = 92;
        var close = (RectTransform)setting.Find("ButtonCloseSetting");
        Place(close,new Vector2(.5f,.5f),new Vector2(330,92),new Vector2(0,-270));
        close.GetComponentInChildren<TMPro.TextMeshProUGUI>().fontSize = 36;

        var info = Popup(main.transform.Find("InforGame"));
        var book = (RectTransform)info.Find("Conten");
        Surface(book,panel).pixelsPerUnitMultiplier=1.7f;
        var mask = book.GetComponent<UnityEngine.UI.Mask>();
        if(mask) mask.enabled=false;
        foreach (var image in book.GetComponentsInChildren<UnityEngine.UI.Image>(true))
        {
            if(image==book.GetComponent<UnityEngine.UI.Image>())continue;
            if(image.sprite==null && !image.GetComponent<UnityEngine.UI.Button>() && !image.GetComponent<UnityEngine.UI.Mask>()) image.color=Ink;
        }
        // Khung trang trí nằm sau nội dung và tách prefab để tái sử dụng.
        var template=Rect("CrystalPanel",null,new Vector2(1000,650),Vector2.zero);
        Surface(template,panel);
        System.IO.Directory.CreateDirectory("Assets/Prefabs/UI/Gameplay");
        PrefabUtility.SaveAsPrefabAsset(template.gameObject,"Assets/Prefabs/UI/Gameplay/CrystalPanel.prefab");
        Object.DestroyImmediate(template.gameObject);

        var toolbar=(RectTransform)main.transform.Find("Icon");
        toolbar.anchoredPosition = new Vector2(-66,-66);
        var grid=toolbar.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        grid.cellSize=new Vector2(54,54);grid.spacing=new Vector2(22,0);
        foreach(var b in toolbar.GetComponentsInChildren<UnityEngine.UI.Button>(true))
        {
            if(!b.GetComponent<UITweenButton>())b.gameObject.AddComponent<UITweenButton>();
            var colors=b.colors;colors.normalColor=Paper;colors.highlightedColor=Color.white;b.colors=colors;
        }

        var hud=canvases.First(c=>c.transform.Find("playerInfor")).transform.Find("playerInfor");
        var backing=Rect("VitalsPanel",hud,new Vector2(1080,310),new Vector2(560,-175));
        backing.anchorMin=backing.anchorMax=new Vector2(0,1);
        Surface(backing,hudPanel).color=new Color(1,1,1,.85f);
        backing.SetAsFirstSibling();
        var portrait=(RectTransform)hud.Find("LevelIcon");
        portrait.anchorMin=portrait.anchorMax=new Vector2(0,1);
        portrait.anchoredPosition=new Vector2(160,-180);
        portrait.localScale=new Vector3(.12f,.12f,1);
        foreach(var name in new[]{"HealthManage","ManaManage"})
        {
            var row=(RectTransform)hud.Find(name);
            Place(row,new Vector2(0,1),new Vector2(740,72),new Vector2(320,name=="HealthManage"?-130:-224));
            row.pivot=new Vector2(0,.5f);
            var layout=row.GetComponent<UnityEngine.UI.GridLayoutGroup>();
            layout.cellSize=new Vector2(62,62);layout.spacing=new Vector2(17,0);layout.startCorner=UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft;
            foreach(RectTransform icon in row)icon.localScale=Vector3.one;
        }
        Label("VitalsTitle",backing,"NGƯỜI ĐƯA TIN ĐÊM",new Vector2(70,100),new Vector2(600,48),28,Paper);
        var skills=(RectTransform)hud.Find("SkillHud");
        Place(skills,new Vector2(.5f,0),new Vector2(480,160),new Vector2(-160,160));
        var skillStyle=new SerializedObject(skills.GetComponent<SkillHud>());
        skillStyle.FindProperty("style.slotSize").vector2Value=new Vector2(128,128);
        skillStyle.FindProperty("style.spacing").floatValue=32;
        skillStyle.FindProperty("style.keyFontSize").intValue=28;
        skillStyle.ApplyModifiedPropertiesWithoutUndo();
        Refine();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        return "Upgraded HUD, toolbar, exit, audio, guide; saved CrystalPanel prefab";
    }

    public static string Refine()
    {
        var canvases=Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        var main=canvases.First(c=>c.transform.Find("ExitConfirm"));
        var hudCanvas=canvases.First(c=>c.transform.Find("playerInfor"));
        main.sortingOrder=10;hudCanvas.sortingOrder=0;
        var hud=(RectTransform)hudCanvas.transform.Find("playerInfor");
        hud.anchorMin=Vector2.zero;hud.anchorMax=Vector2.one;hud.offsetMin=hud.offsetMax=Vector2.zero;
        var backing=(RectTransform)hud.Find("VitalsPanel");
        backing.sizeDelta=new Vector2(680,310);backing.anchoredPosition=new Vector2(360,-175);
        if(!backing.GetComponent<HudVitalsPanel>())backing.gameObject.AddComponent<HudVitalsPanel>();
        var title=(RectTransform)backing.Find("VitalsTitle");
        title.anchorMin=title.anchorMax=new Vector2(0,.5f);title.anchoredPosition=new Vector2(600,100);
        foreach(var name in new[]{"HealthManage","ManaManage"})
            hud.Find(name).GetComponent<UnityEngine.UI.GridLayoutGroup>().childAlignment=TextAnchor.UpperLeft;
        var skills=(RectTransform)hud.Find("SkillHud");
        skills.sizeDelta=new Vector2(448,160);skills.anchoredPosition=new Vector2(0,160);
        foreach(var b in main.GetComponentsInChildren<UnityEngine.UI.Button>(true))
        {
            if(b.transform.IsChildOf(main.transform.Find("Icon")))continue;
            foreach(var t in b.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))t.color=new Color(.12f,.14f,.18f);
        }
        var info=main.transform.Find("InforGame/Content/Conten");
        foreach(var scroll in info.GetComponentsInChildren<UnityEngine.UI.ScrollRect>(true))
        {
            var bg=scroll.GetComponent<UnityEngine.UI.Image>();if(bg)bg.color=Color.clear;
            if(scroll.viewport){var mask=scroll.viewport.GetComponent<UnityEngine.UI.Mask>();if(mask)mask.showMaskGraphic=false;}
        }
        foreach(var image in info.GetComponentsInChildren<UnityEngine.UI.Image>(true))
        {
            if(image.name=="Panel")image.color=new Color(.08f,.12f,.18f,.7f);
            string spritePath=AssetDatabase.GetAssetPath(image.sprite);
            if(spritePath.EndsWith("parchment.png")) image.enabled=false;
            if(spritePath=="Resources/unity_builtin_extra" && image.rectTransform.rect.width>600 && !image.GetComponent<UnityEngine.UI.Mask>())image.color=Color.clear;
            if(image.name=="Scrollbar Vertical")image.color=new Color(.08f,.12f,.18f,1);
            if(image.name=="Handle")image.color=new Color(.6f,.66f,.68f,1);
        }
        var frame=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Popup/BlossomPopupFrame.png");
        foreach(var button in info.GetComponentsInChildren<UnityEngine.UI.Button>(true))
        {
            if(button.name=="ButtonExitInfor")continue;
            var image=button.GetComponent<UnityEngine.UI.Image>();
            if(image){image.enabled=true;image.sprite=frame;image.type=UnityEngine.UI.Image.Type.Sliced;image.pixelsPerUnitMultiplier=2;image.color=Color.white;}
            foreach(var text in button.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))text.color=Paper;
        }
        var navigation=(RectTransform)info.Find("Scroll View");
        navigation.sizeDelta=new Vector2(300,740);navigation.anchoredPosition=new Vector2(-550,-10);
        foreach(var grid in navigation.GetComponentsInChildren<UnityEngine.UI.GridLayoutGroup>(true))
        {
            grid.cellSize=new Vector2(280,68);grid.spacing=new Vector2(0,8);
        }
        foreach(var scroll in info.GetComponentsInChildren<UnityEngine.UI.Scrollbar>(true))
        {
            var rect=(RectTransform)scroll.transform;
            rect.sizeDelta=new Vector2(12,rect.sizeDelta.y);
            if(scroll.targetGraphic)scroll.targetGraphic.color=new Color(.58f,.65f,.68f,1);
        }
        foreach(var image in main.transform.Find("SettingGame").GetComponentsInChildren<UnityEngine.UI.Image>(true))
        {
            if(image.name=="Fill")image.color=new Color(.71f,.6f,.39f);
            if(image.name=="Handle")image.color=new Color(.76f,.9f,.94f);
        }
        var close=(RectTransform)main.transform.Find("SettingGame/Content/ButtonCloseSetting");
        close.anchoredPosition=new Vector2(0,-220);
        return "Refined anchoring, Canvas order and contrast";
    }

    public static string Render(string path, string popup)
    {
        var canvases=Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        var main=canvases.First(c=>c.transform.Find("ExitConfirm"));
        var names=new[]{"ExitConfirm","SettingGame","InforGame"};
        var states=names.Select(n=>main.transform.Find(n).gameObject.activeSelf).ToArray();
        var modes=canvases.Select(c=>c.renderMode).ToArray();
        var cameras=canvases.Select(c=>c.worldCamera).ToArray();
        var distances=canvases.Select(c=>c.planeDistance).ToArray();
        var cameraObject=new GameObject("UiPreviewCamera");
        var cam=cameraObject.AddComponent<Camera>();
        cam.orthographic=true;cam.orthographicSize=5;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.10f,.15f,.18f);
        cam.cullingMask=1<<5;
        var rt=new RenderTexture(1920,1080,24);cam.targetTexture=rt;
        var previous=RenderTexture.active;
        try
        {
            for(int i=0;i<names.Length;i++)main.transform.Find(names[i]).gameObject.SetActive(names[i]==popup);
            foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=cam;canvas.planeDistance=10;}
            Canvas.ForceUpdateCanvases();cam.Render();RenderTexture.active=rt;
            var tex=new Texture2D(1920,1080,TextureFormat.RGBA32,false);tex.ReadPixels(new Rect(0,0,1920,1080),0,0);tex.Apply();
            System.IO.File.WriteAllBytes(path,tex.EncodeToPNG());Object.DestroyImmediate(tex);
        }
        finally
        {
            RenderTexture.active=previous;
            for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}
            for(int i=0;i<names.Length;i++)main.transform.Find(names[i]).gameObject.SetActive(states[i]);
            Object.DestroyImmediate(cameraObject);Object.DestroyImmediate(rt);
        }
        return path;
    }
}
#endif
