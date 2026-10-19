#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UImage = UnityEngine.UI.Image;

/// <summary>
/// Panggil via menu Tools > Setup Percobaan2
/// </summary>
public static class SetupPercobaan2
{
    [MenuItem("Tools/Setup Percobaan2")]
    public static void Run()
    {
        var canvasT = GameObject.Find("Canvas").transform;
        var spriteUI = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // SlotPlateTetes
        var ls = canvasT.Find("SlotPlateTetes"); if (ls != null) Undo.DestroyObjectImmediate(ls.gameObject);
        var spGO = new GameObject("SlotPlateTetes", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(spGO, "SlotPlateTetes");
        spGO.transform.SetParent(canvasT, false);
        var spRT = (RectTransform)spGO.transform;
        spRT.anchorMin = spRT.anchorMax = new Vector2(.5f,.5f); spRT.pivot = new Vector2(.5f,.5f);
        spRT.sizeDelta = new Vector2(180,110); spRT.anchoredPosition = new Vector2(0,30);
        var imgSP = spGO.AddComponent<UImage>(); imgSP.sprite=spriteUI; imgSP.type=UImage.Type.Sliced; imgSP.color=new Color(1,1,1,.12f); imgSP.raycastTarget=false;
        var slotP = spGO.AddComponent<SlotTabung>(); slotP.namaSlot="Plate"; slotP.penandaArea=imgSP;
        spGO.SetActive(false);

        // PanelNotifP2
        var ln = canvasT.Find("PanelNotifP2"); if (ln!=null) Undo.DestroyObjectImmediate(ln.gameObject);
        var nGO = new GameObject("PanelNotifP2", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(nGO, "PanelNotifP2");
        nGO.transform.SetParent(canvasT, false);
        var nRT = (RectTransform)nGO.transform;
        nRT.anchorMin=nRT.anchorMax=new Vector2(.5f,1); nRT.pivot=new Vector2(.5f,1);
        nRT.sizeDelta=new Vector2(240,44); nRT.anchoredPosition=new Vector2(0,-20);
        var imgN = nGO.AddComponent<UImage>(); imgN.sprite=spriteUI; imgN.type=UImage.Type.Sliced; imgN.color=new Color(.05f,.15f,.05f,.85f);
        var tGO = new GameObject("Teks", typeof(RectTransform));
        tGO.transform.SetParent(nGO.transform, false);
        var tRT = (RectTransform)tGO.transform; tRT.anchorMin=Vector2.zero; tRT.anchorMax=Vector2.one; tRT.offsetMin=tRT.offsetMax=Vector2.zero;
        var tn = tGO.AddComponent<TextMeshProUGUI>(); tn.text="Berhasil dicampur"; tn.fontSize=14; tn.alignment=TextAlignmentOptions.Center; tn.color=Color.white;
        nGO.SetActive(false);

        // ManajerSpotPlate
        GameObject ml = GameObject.Find("ManajerLatihan");
        ManajerSpotPlate msp = ml.GetComponent<ManajerSpotPlate>() ?? Undo.AddComponent<ManajerSpotPlate>(ml);
        Undo.RecordObject(msp, "MSP");
        msp.slotPlate=slotP; msp.panelNotif=nGO; msp.teksNotif=tn;
        msp.objekPlateTetes=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PraktikumKimia/Praktikum1/percobaan2/Alat/plattetes.fbx");
        msp.objekSpatulaAsam=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PraktikumKimia/Praktikum1/percobaan2/Alat/spatula.fbx");
        msp.objekSerbukAsam=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PraktikumKimia/Praktikum1/percobaan2/Bahan/asambenzonat.fbx");
        msp.objekTusukGigi=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PraktikumKimia/Praktikum1/percobaan2/Alat/tusukgigi.fbx");
        msp.objekTespen   =AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PraktikumKimia/Praktikum1/percobaan2/Alat/tespen.fbx");
        Transform p2 = null;
        foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
            if (t.gameObject.scene.IsValid() && t.name == "Percobaan2") { p2 = t; break; }
        msp.soal1=p2.Find("Soal1").gameObject; msp.soal2=p2.Find("Soal2").gameObject; msp.soal3=p2.Find("Soal3").gameObject;
        msp.soal4=p2.Find("Soal4").gameObject; msp.soal5=p2.Find("Soal5").gameObject; msp.soal6=p2.Find("Soal6").gameObject; msp.soal7=p2.Find("Soal7").gameObject;

        string mf="Assets/PraktikumKimia/Praktikum1/percobaan2/Material";
        if (!AssetDatabase.IsValidFolder(mf)) AssetDatabase.CreateFolder("Assets/PraktikumKimia/Praktikum1/percobaan2","Material");
        string mp=mf+"/LarutanSpot.mat"; var mat=AssetDatabase.LoadAssetAtPath<Material>(mp);
        if (mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.SetFloat("_Surface",1);mat.SetFloat("_ZWrite",0);mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");mat.SetOverrideTag("RenderType","Transparent");mat.renderQueue=3000;mat.SetColor("_BaseColor",new Color(.6f,.85f,1,.65f));AssetDatabase.CreateAsset(mat,mp);AssetDatabase.SaveAssets();}
        msp.matLarutan=msp.matPadatan=mat; EditorUtility.SetDirty(msp);

        // Cards
        var akuFBX =AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PraktikumKimia/Praktikum1/percobaan2/Bahan/akuades.fbx");
        var asmFBX =AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PraktikumKimia/Praktikum1/percobaan2/Bahan/asambenzonat.fbx");
        var spFBX  =AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PraktikumKimia/Praktikum1/percobaan2/Alat/spatula.fbx");
        var tkFBX  =AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PraktikumKimia/Praktikum1/percobaan2/Alat/tusukgigi.fbx");
        var tpFBX  =AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PraktikumKimia/Praktikum1/percobaan2/Alat/tespen.fbx");
        string[] cn={"PlatTetes","Spatula","Tusuk Gigi","Tes Konduktor","Asam Benzoat","Etanol","Petroleum Eter","MGCL","Aquades"};
        GameObject[] cf={akuFBX,spFBX,tkFBX,tpFBX,asmFBX,akuFBX,akuFBX,akuFBX,akuFBX};
        string[] cs={"PlateTetes","Spatula","TusukGigi","TesKonduktor","AsamBenzoat","Etanol","PetroleumEter","MGCL2","Aquades"};
        for (int i=0;i<cn.Length;i++)
            foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (!t.gameObject.scene.IsValid()||t.name!=cn[i]) continue;
                var c=t.GetComponent<CardTahanGeser3D>()??Undo.AddComponent<CardTahanGeser3D>(t.gameObject);
                if (c.objek3D==null&&cf[i]!=null) c.objek3D=cf[i];
                if (cs[i]=="AsamBenzoat"&&spFBX!=null) c.objek3D=spFBX;
                if (cs[i]=="Aquades") { if (string.IsNullOrEmpty(c.namaBahanSP)) c.namaBahanSP=cs[i]; }
                else { c.penerimaSP=msp;c.namaBahanSP=cs[i]; }
                c.waktuTahan=.25f;c.periksaKesesuaian=true;EditorUtility.SetDirty(c);
            }

        // GerbangSpotPlate
        string[] ss={"Soal1","Soal2","Soal3","Soal4","Soal5","Soal6","Soal7"};
        GerbangSpotPlate.Syarat[] sy={GerbangSpotPlate.Syarat.PlateSiap,GerbangSpotPlate.Syarat.Kolom1,GerbangSpotPlate.Syarat.Kolom2,GerbangSpotPlate.Syarat.Kolom3,GerbangSpotPlate.Syarat.Baris1,GerbangSpotPlate.Syarat.Baris2,GerbangSpotPlate.Syarat.Baris3};
        string[] sb={"Soal2","Soal3","Soal4","Soal5","Soal6","Soal7",""};
        for (int i=0;i<ss.Length;i++){
            var st=p2.Find(ss[i]); if(st==null)continue;
            var bt=st.Find("ButtonLanjut")??st.Find("ButtonData"); if(bt==null)continue;
            var g=bt.GetComponent<GerbangSpotPlate>()??Undo.AddComponent<GerbangSpotPlate>(bt.gameObject);
            Undo.RecordObject(g,"GSP"); g.syarat=sy[i];
            if(!string.IsNullOrEmpty(sb[i])){g.aktifkan=p2.Find(sb[i]).gameObject;g.nonaktifkan=st.gameObject;}
            var btn=bt.GetComponent<Button>();btn.onClick.RemoveAllListeners();
            bool sudahTerhubung=false;
            for(int li=0;li<btn.onClick.GetPersistentEventCount();li++)
                if(btn.onClick.GetPersistentTarget(li)==g && btn.onClick.GetPersistentMethodName(li)=="Lanjut") { sudahTerhubung=true; break; }
            if(!sudahTerhubung) UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick,g.Lanjut);
            EditorUtility.SetDirty(g);
        }

        // ManajerTataLetak
        var tl=ml.GetComponent<ManajerTataLetak>(); Undo.RecordObject(tl,"TL");
        bool ada=false; foreach(var a in tl.aturan)if(a.nama=="SlotPlateTetes"){ada=true;break;}
        if(!ada){var a2=new ManajerTataLetak.AturanTampil{nama="SlotPlateTetes",objek=new List<GameObject>{spGO},panelSoal=new List<GameObject>()};for(int i=1;i<=7;i++){var s=p2.Find("Soal"+i);if(s)a2.panelSoal.Add(s.gameObject);}tl.aturan.Add(a2);}
        EditorUtility.SetDirty(tl);

        // ManajerPercobaan wadah
        var mprc=ml.GetComponent<ManajerPercobaan>(); Undo.RecordObject(mprc,"wadah");
        var w=new List<GameObject>(mprc.wadahSlotPercobaan1??new GameObject[0]);
        foreach(var x in new[]{spGO,nGO})if(!w.Contains(x))w.Add(x);
        mprc.wadahSlotPercobaan1=w.ToArray(); EditorUtility.SetDirty(mprc);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Setup Percobaan2 selesai!");
    }
}
#endif
