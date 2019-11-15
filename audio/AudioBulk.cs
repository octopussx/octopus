using UnityEngine;
using System.Collections.Generic;

// uncomment the two lines to use as a  plugin

namespace octopussy {

public class AudioBulk {//: MVRScript {

        public static string DEFAULT_PATH = Application.dataPath + "/../custom/assets/audio";
        public static string lastLoadPath;
        public static JSONStorableString audiofilefilter;
        public static string[] supportedFileTypes = {"*", ".wav", ".ogg", ".mp3"}; // regex next time

        public JSONStorableStringChooser filetype;
        
        JSONStorableString _path;

        public void InitUI(MVRScript script, List<NamedAudioClip> nacs, JSONStorableString folderpath)
        {
            filetype = new JSONStorableStringChooser("File type", new List<string>(supportedFileTypes), ".wav", "File type", 
                s=>audiofilefilter.val=s);
            script.RegisterStringChooser(filetype);

            audiofilefilter = new JSONStorableString("audiofilefilter",".wav");
            lastLoadPath = DEFAULT_PATH;
            _path = folderpath;
            UIDynamicPopup popup = script.CreateScrollablePopup(filetype, false);
            popup.labelTextColor = Color.yellow;
            popup.popup.selectColor = Color.yellow;
            /*script.CreateButton("Load Audio (Folder)").button.onClick.AddListener(
                () => OpenLoadFolder(nacs, lastLoadPath));*/
            script.CreateButton("Remove all clips from scene audio").button.onClick.AddListener(
                () => URLAudioClipManager.singleton.RemoveAllClips());
            // do it
            URLAudioClipManager.singleton.RemoveAllClips();
        }

        public NamedAudioClip
        LoadAudio(string path)
        {
            string localPath = SuperController.singleton.NormalizeLoadPath(path);
            var nac = URLAudioClipManager.singleton.GetClip(localPath);
            if (nac != null)
            {
                return nac;
            }

            var clip = URLAudioClipManager.singleton.QueueClip(
                SuperController.singleton.NormalizeMediaPath(path));
            if (clip == null)
            {
                return null;
            }

            nac = URLAudioClipManager.singleton.GetClip(clip.uid);
            return nac;
        }

        public void OpenLoadFolder(List<NamedAudioClip> nacs, JSONStorableString lastPath)
        {
            SuperController.singleton.GetDirectoryPathDialog(
                (path) => {
                    if (!string.IsNullOrEmpty(path))
                    {
                        nacs.Clear();
                        nacs.AddRange(this.LoadFolder(path, filetype.val));
                        lastPath.val = _path.val = path;
                        
                    }
                }, lastPath.val);
        }

        public void OpenLoadFolder(List<NamedAudioClip> nacs,  string lastPath)
        {
            SuperController.singleton.GetDirectoryPathDialog(
                (path) => {
                    if (!string.IsNullOrEmpty(path))
                    {
                        nacs.Clear();
                        nacs.AddRange(this.LoadFolder(path, filetype.val));
                        _path.val = path;
                    }
                }, lastPath);
        }

        public void Load(List<NamedAudioClip> nacs, string path)
        {
            nacs.Clear();
            nacs.AddRange(this.LoadFolder(path, filetype.val));
        }


        public List<NamedAudioClip>
        LoadFolder(string path, string extension="", string fileprefix = "") { // regexp next time
            lastLoadPath = path;
            var clips = new List<NamedAudioClip>();

            if (extension == "*") {
                SuperController.singleton.GetFilesAtPath(path)
                    .ToList().ForEach(filePath => clips.Add(this.LoadAudio(filePath)));
            }
            else SuperController.singleton.GetFilesAtPath(path).ToList()
                    .ForEach((filePath) =>
            {
                    string filename = filePath.Substring(path.Length + 1);
                    //SuperController.LogMessage(filename);
                    if (filePath.ToLower().EndsWith(extension)
                    && filename.StartsWith(fileprefix))
                    {
                        clips.Add(this.LoadAudio(filePath));
                    }
            });
            return clips;
        }
    }
}
