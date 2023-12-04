using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace CharlesEngine
{
    public class SceneSorting
    {

        private static Dictionary<int, int> SortingLayersById = new Dictionary<int, int>(10);

        [MenuItem("Tools/Charles Engine/Actions/Bring Object Up #%W", false, 51)] // shift left 
        private static void MoveUp()
        {
            Move(+1);
        }
        
        [MenuItem("Tools/Charles Engine/Actions/Move Object Down #%X", false, 51)] // shift right
        private static void MoveDown()
        {
            Move(-1);
        }
        
        //[MenuItem("Tools/Charles Engine/Scene Sorting/Synchronize to highest", false, 51)]
        private static void SynchroHighest()
        {
            var selected = (GameObject) Selection.activeObject;
            if (selected == null)
            {
                return;
            }
            InitSorting();
            
            var renderers = selected.GetComponentsInChildren<SpriteRenderer>();
            var tmpros = selected.GetComponentsInChildren<TextMeshPro>();

            var topLayer = -1;
            var topOrder = -1;

            if (renderers.Length > 0)
            {
                Array.Sort(renderers,
                    (a, b) => a.sortingLayerID == b.sortingLayerID
                        ? a.sortingOrder.CompareTo(b.sortingOrder)
                        : SortingLayersById[a.sortingLayerID].CompareTo(SortingLayersById[b.sortingLayerID]));

                topLayer = renderers[0].sortingLayerID;
                topOrder = renderers[0].sortingOrder;
            }

            
            if (tmpros.Length > 0)
            {
                Array.Sort(tmpros,
                    (a, b) => a.sortingLayerID == b.sortingLayerID
                        ? a.sortingOrder.CompareTo(b.sortingOrder)
                        : SortingLayersById[a.sortingLayerID].CompareTo(SortingLayersById[b.sortingLayerID]));

                if (topLayer == -1 || SortingLayersById[topLayer] < SortingLayersById[tmpros[0].sortingLayerID])
                {
                    topLayer = tmpros[0].sortingLayerID;
                    topOrder = tmpros[0].sortingOrder;
                }
                else if (SortingLayersById[topLayer] == SortingLayersById[tmpros[0].sortingLayerID])
                {
                    topOrder = Math.Max(tmpros[0].sortingOrder, topOrder);
                }
            }
            
            foreach (var rnd in renderers)
            {
                rnd.sortingOrder = topOrder;
                rnd.sortingLayerID = topLayer;
            }
            
            foreach (var tmpro in tmpros)
            {
                tmpro.sortingOrder = topOrder;
                tmpro.sortingLayerID = topLayer;
            }
        }

        private static void InitSorting()
        {
            SortingLayersById.Clear();
            foreach (var layer in SortingLayer.layers)
            {
                SortingLayersById.Add(layer.id,layer.value);
                //Debug.Log("adding name "+layer.name+ " id:"+layer.id+" value:"+layer.value);
            }
        }

        private static void Move(int delta)
        {
            
            var selected = (GameObject) Selection.activeObject;
            if (selected == null)
            {
                return;
            }
            InitSorting();
            
            var renderers = selected.GetComponentsInChildren<SpriteRenderer>();
            var tmpros = selected.GetComponentsInChildren<TextMeshPro>();
   
            foreach (var rnd in renderers)
            {
                rnd.sortingOrder = rnd.sortingOrder + delta;
            }
            
            foreach (var tmpro in tmpros)
            {
                tmpro.sortingOrder = tmpro.sortingOrder + delta;
            }
        }
        
      //  [MenuItem("Tools/Charles Engine/Temp")]
        private static void ParseSubs()
        {
            var allText = Resources.Load<TextAsset>("module").text;
            var allLines = allText.Split(new []{'\n'});
            string subStart = "1@@00:";
            var sublines = allLines.Where(l => l.IndexOf(subStart) > 0).ToArray();
            foreach (var line in sublines)
            {
                var sidx = line.IndexOf(subStart);
                var endix = line.IndexOf("}", sidx)-sidx-1;
                var id = GetVideoName(line);
                var subs = line.Substring(sidx, endix).Replace("@@", Environment.NewLine);
                var parent = GetParent(line);
                var it = 0;
                while (it++ < 45)
                {
                    var p = FindParent(parent, allLines);
                    if (p == null) break;
                    if( p.StartsWith("#B") )
                    {
                        var sss = new SubClasss();
                        sss.Name = GetName(line).Replace(".flv","").Replace(".f4v","").Replace(" ","")+".srt";
                        sss.Subs = subs;
                        sss.Folder = GetName(p);
                        sss.id = id;
                /*        Debug.Log(sss.Name);
                        Debug.Log(subs);
                        Debug.Log(sss.Folder);
                        Debug.Log("---");*/
                        Subs.Add(sss);
                        break;
                    }
                    parent = GetParent(p);
                }
            }

            var d = new Dictionary<string,List<string>>();
            foreach (var s in Subs)
            {
                var folder = "SubtitleExport/" + s.Folder;
                Directory.CreateDirectory(folder);
                var fname = replaceDiac(s.Name);
                if (fname.Length > 32)
                {
                    fname = fname.Substring(0, 30) + ".srt";
                }

                var subText = s.Subs.Trim();
                var de = subText.IndexOf("DEU");
                var cz = subText.IndexOf("CZE");
                var en = subText.IndexOf("ENG");
                try
                {
                    var rusSub = subText.Substring(0,  de - 4);
                    File.WriteAllText(folder + "/" + fname.Replace(".srt", "_ru.srt"), rusSub, Encoding.UTF8);
                    var deuSub = subText.Substring(de + 6, cz - 6 - de - 3);
                    File.WriteAllText(folder + "/" + fname.Replace(".srt", "_de.srt"), deuSub, Encoding.UTF8);
                    var czeSub = subText.Substring(cz + 6, en - 6 - cz - 3);
                    File.WriteAllText(folder + "/" + fname.Replace(".srt", "_cz.srt"), czeSub, Encoding.UTF8);
                    var enSub = subText.Substring(en + 6);
                    File.WriteAllText(folder + "/" + fname.Replace(".srt", "_en.srt"), enSub, Encoding.UTF8);
                }
                catch
                {
                    Debug.Log("error with "+subText);
                    continue;
                }
    
                if (d.ContainsKey(folder)== false)
                {
                    d[folder] = new List<string>();
                }
                d[folder].Add("copy /y \"D:\\svoboda\\m2\\modul\\"+ s.id + "\" \"" + fname.Replace("srt","flv")+"\"");
            }
            foreach (var keyValuePair in d)
            {
                File.WriteAllText(keyValuePair.Key+"/ids.txt", string.Join( System.Environment.NewLine, keyValuePair.Value.ToArray() ), Encoding.UTF8);
            }
        }

        private static string replaceDiac(string sName)
        {
            const string DI = "ÁČĎÉĚÍŇÓŘŠŤÚŮÝŽáčďéěíňóřšťúůýž";
            const string RP = "ACDEEINORSTUUYZacdeeinorstuuyz";
            var s = "";
            for (int i = 0; i < sName.Length; i++)
            {
                var chr = sName[i];
                if (chr == '?' || chr == '!' || chr == ',') continue;
                var ind = DI.IndexOf(chr);
                if (ind >= 0)
                {
                    s += RP[ind];
                }
                else
                {
                    s += sName[i];
                }
            }

            return s;
        }

        private class SubClasss
        {
            public string Folder;
            public string Subs;
            public string Name;
            public string id;
        }
        private static Dictionary<string, string> Parents = new Dictionary<string, string>();
        private static List<SubClasss> Subs = new List<SubClasss>();
        
        private static string GetParent(string line)
        {
            var result = Regex.Match(line,"#[A-Z][A-Z]?\\[[A-Z][0-9]*,\\\"[^\"]*\",([A-Z][0-9]*),");
            if (result.Success)
            {
                return result.Groups[1].Value;
            }
            throw new Exception("No parent found "+line);
        }
        private static string GetVideoName(string line)
        {
            var result = Regex.Match(line,"[^,]*,\"[^\"]*\",[^,]*,[^,]*,I(\\d*)");
            if (result.Success)
            {
                if (result.Groups[1].Value.Length < 5)
                {
                    Debug.Log("kk " + line);
                }
                return "A"+result.Groups[1].Value+"__ENG_RUS_CZE_DEU.flv";
            }
            throw new Exception("No video found "+line);
        }
                
        private static string GetName(string line)
        {
            var result = Regex.Match(line,"#[A-Z][A-Z]?\\[[A-Z][0-9]*,\\\"([^\"]*)\"");
            if (result.Success)
            {
                return result.Groups[1].Value;
            }
            throw new Exception("No name found "+line);
        }

        private static string FindParent(string parentId, string[] lines)
        {
            if (Parents.ContainsKey(parentId))
            {
                return Parents[parentId];
            }

            foreach (var line in lines)
            {
                var idx = line.IndexOf(parentId);
                if (idx > 0 && idx < 6)
                {                    
                    Parents[parentId] = line;
                    return line;
                }
            }
            
            throw new Exception("Not found "+parentId);
        }
    }
}