
/*
 Octopussy AudioPlayer [08-08-19] 
 
 You can load embedded clips by typing the category name
 and the starting letters in the plugin name field
 (ex : Music/  or FemaleMoan/FemPixie) then pressing
 the "Load Embedded" Button
 */

#define vamdebug
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using UnityEngine;

namespace octopussy
{
    internal class AudioPlayer : MVRScript 
    {
        PlayList playlist;
        public List<NamedAudioClip> nacs;
        AudioSourceControl audioSource;
        JSONStorableBool playIfClear;
        JSONStorableFloat pitchshift; // emo engine mod has also one. should be the slider only
        JSONStorableFloat delay;
        JSONStorableString clipsLoaded;
        JSONStorableString folderpath;
        JSONStorableAction playRandom;
        JSONStorableAction playNext;

        /*
        JSONStorableAction startPlay; // autoloop to next when over
        JSONStorableAction stopPlay;
        JSONStorableBool shuffle;
        JSONStorableString lastLoadPath;
        */

        protected AudioBulk audioBulk;
        public string LABEL = "Random Audio";

        //SelectJsonStorable<AudioSource> selector;

        public override void Init()
        {
#if vamdebug
            try
            {
#endif
                audioSource = containingAtom.GetStorableByID("HeadAudioSource") as AudioSourceControl;
                if (audioSource == null)
                    audioSource = containingAtom.GetStorableByID("AudioSource") as AudioSourceControl;
                if (audioSource == null)
                    SuperController.LogError($"{this} is not applicable to {containingAtom.name} ({containingAtom.type})");

                playRandom = new JSONStorableAction("Play Random", playRandomClipAction);
                playNext = new JSONStorableAction("Play Next", playNextClipAction);
                RegisterAction(playNext);
                RegisterAction(playRandom);

                pitchshift = new JSONStorableFloat("pitch shift",
                    0f, f => playlist.pitchshift = playlist.pitchshift = f,
                    -0.3f, 0.3f, true) {storeType = JSONStorableParam.StoreType.Full};
                RegisterFloat(pitchshift);
                CreateSlider(pitchshift, true);

                folderpath = new JSONStorableString("loadpath",
                    Application.dataPath + "/../custom/assets/audio")
                    { storeType = JSONStorableParam.StoreType.Full};
                RegisterString(folderpath);

                audioBulk = new AudioBulk();
                nacs = new List<NamedAudioClip>();

                audioBulk.InitUI(this, nacs, folderpath);
                playlist = new PlayList(nacs);

                playIfClear = new JSONStorableBool("Play if clear", true);
                RegisterBool(playIfClear);
                playIfClear.storeType = JSONStorableParam.StoreType.Full;
                CreateToggle(playIfClear,true);

                delay = new JSONStorableFloat("delay", 0f, 0f, 2f, false);
                CreateSlider(delay, true);

                //CreateButton("callback").button.onClick.AddListener(() =>playNext.actionCallback());
                CreateButton("Load Audio Folder").button.onClick.AddListener(
                    () => {
                        audioBulk.OpenLoadFolder(nacs, folderpath);
                        Loaded(nacs);
                    });
                CreateButton("Load Embeded").button.onClick.AddListener(() => {
                    if (!validInputNameAsEmbedded())
                    {
                        SuperController.LogError("no valid source");
                        return;
                    }
                    LoadEmbeddedInput();
                    Loaded(nacs);
                });
                CreateButton("Play next",true).button.onClick.AddListener(playNextClipAction);
                CreateButton("Play random",true).button.onClick.AddListener(playRandomClipAction);
                CreateButton("Stop",false).button.onClick.AddListener(audioSource.Stop);
                CreateTextField(folderpath, false).height = 10;
                CreateButton("Reload",false).button.onClick.AddListener(() => {
                        audioBulk.Load(nacs, folderpath.val);
                        Loaded(nacs);
                    }
                );
                clipsLoaded = new JSONStorableString("loaded clips", "No clips loaded");
                
                CreateTextField(clipsLoaded, false).height = 10;

#if vamdebug
            }
            catch (Exception e)
            {
                SuperController.LogError("woops"+e);
            }
#endif
        }

        private void Loaded(List<NamedAudioClip> clips)
        {
            playlist.next = 0;
            // playNext.name = "PlayNext " + folderpath.val.Split('/').Last();
            // can't change the action name after it's been instanciated
#if vamdebug
            nacs.ForEach(c => SuperController.LogMessage(c.displayName));
#endif

            clipsLoaded.val = clips.Count > 0
                ? "loaded clips " + clips.Count
                : "no clips loaded";
        }

        public void Start()
        {
            Debug.Log("start");
            if (validInputNameAsEmbedded())
            {
                LoadEmbeddedInput();
                Loaded(nacs);
            }
            else
            {
                if (folderpath.val == AudioBulk.DEFAULT_PATH && folderpath.val[0] != '~') return;
                audioBulk.Load(nacs, folderpath.val);
                playNext.name = "PlayNext " + folderpath.val.Split('/').Last();
            }
        }

        public void LoadEmbeddedInput(bool clear = true)
        {
            string[] s = pluginLabelJSON.val.Split('/');
            SuperController.LogMessage(s[0] + "/" + s[1]);
            folderpath.val = '~'+pluginLabelJSON.val;
            AddEmbeddedClips(s[0], s[1], clear);
            // playNext.name = "PlayNext " + s[0];
        }

        private bool validInputNameAsEmbedded()
        {
            return (pluginLabelJSON.val != null && pluginLabelJSON.val.Contains("/"));
        }

        public void playRandomClipAction()
        {
            if (nacs == null || nacs.Count==0)
            {
                SuperController.LogError("no audioclips list");
            }
            else
            {
                playlist.playRandomDelayedIfClear(audioSource.audioSource, 1f, delay.val);
            }
        }

        public void playNextClipAction()
        {
            if (nacs == null || nacs.Count==0)
            {
                SuperController.LogError("no audioclips list");
            }
            else
            {
                playlist.playRandomDelayedIfClear(audioSource.audioSource, 1f, delay.val);
            }
        }

        public List<NamedAudioClip> AddEmbeddedClips(string categoryname, string startname="", bool clear=true)
        {
            if (clear) nacs.Clear();
            nacs.AddRange(
                EmbeddedAudioClipManager
                    .singleton.GetCategoryClips(categoryname)
                    .Where(n => n.displayName.StartsWith(startname))
                    .ToList());
            //pluginLabelJSON.val = $"{LABEL} {categoryname}/{startname} ({nacs.Count})";
            return nacs;
        }
    }
}
