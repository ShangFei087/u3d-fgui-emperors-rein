using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GameMaker
{
    public class SoundHelper
    {
        /*
        public SoundHelper(Func<object, string> getSoundPath)
        {
            this.getSoundPath = getSoundPath;
        }

        public SoundHelper()
        {
            this.getSoundPath = (enumObj) => null;
        }
        
         private Func<object, string> getSoundPath;        
        */

        
        public SoundHelper(Func<object, GSHandler> getGSHandler)
        {
            this.getGSHandler = getGSHandler;
        }

        public SoundHelper()
        {
            this.getGSHandler = (enumObj) => null;
        }
        
        
        private Func<object, GSHandler> getGSHandler;
        
        
        private string GetAudioClipName(string path)
        {
            string[] res = path.Split('/');
            string name = res[res.Length - 1].Split('.')[0];
            return name;
        }

       
       /*
        public void PlaySoundEff(object enumObj, bool loop = false)
        {
            string path = "";
            if (enumObj is GameMaker.SoundKey)
                path = $"{GlobalSoundModel.Instance.soundRootPath}/{GlobalSoundModel.Instance.soundRelativePath[(GameMaker.SoundKey)enumObj]}";
            else
                path = getSoundPath(enumObj);

            SoundManager.Instance.PlaySoundEffX(path, loop);

        }
*/
        public void PlaySoundEff(object enumObj)
        {
            GSHandler gsh = null;
            if (enumObj is GameMaker.SoundKey)
                gsh = GlobalSoundModel.Instance.gsHandlers[(GameMaker.SoundKey)enumObj];
            else
                gsh = getGSHandler(enumObj);

            GSManager.Instance.PlaySoundEff(gsh);
        }
        

        
        /*
        public void PlaySoundSingle(object enumObj, bool loop = false)
        {
            string path = "";

            GSHandler gsh;
            if (enumObj is GameMaker.SoundKey)
                path = GlobalSoundModel.Instance.soundRelativePath[(GameMaker.SoundKey)enumObj];
            else
                path = getSoundPath(enumObj);
            GSManager.Instance.PlaySoundEffSingle(path, loop);
        }
        */
        public void PlaySoundEffSingle(object enumObj)
        {
            GSHandler gsh = null;
            if (enumObj is GameMaker.SoundKey)
                gsh = GlobalSoundModel.Instance.gsHandlers[(GameMaker.SoundKey)enumObj];
            else
                gsh = getGSHandler(enumObj);
            GSManager.Instance.PlaySoundEffSingle(gsh);
        }

        

        /*
        public void PlayMusicSingle(object enumObj, bool loop = false)
        {
            string path = "";
            if (enumObj is GameMaker.SoundKey)
                path = $"{GlobalSoundModel.Instance.soundRootPath}/{GlobalSoundModel.Instance.soundRelativePath[(GameMaker.SoundKey)enumObj]}";
            else
                path = getSoundPath(enumObj);
            GSManager.Instance.PlayMusicSingle(path, loop);
        }*/
        
        public void PlayMusicSingle(object enumObj)
        {
            GSHandler gsh = null;
            if (enumObj is GameMaker.SoundKey)
                gsh = GlobalSoundModel.Instance.gsHandlers[(GameMaker.SoundKey)enumObj];
            else
                gsh = getGSHandler(enumObj);
            GSManager.Instance.PlayMusicSingle(gsh);
        }

        
        /*
        public bool IsPlaySound(object enumObj)
        {
            string path = "";
            if (enumObj is GameMaker.SoundKey)
                path = $"{GlobalSoundModel.Instance.soundRootPath}/{GlobalSoundModel.Instance.soundRelativePath[(GameMaker.SoundKey)enumObj]}";
            else
                path = getSoundPath(enumObj);

            // bool isPlay = SoundManager.Instance.IsPlaySound(__SoundBB.Instance.soundName[name]);
            bool isPlay = SoundManager.Instance.IsPlaySound(path);

            DebugUtils.Log($"is play sound : {GetAudioClipName(path)} = {isPlay}");

            return isPlay;
        }
        */
        
        
        public bool IsPlaySound(object enumObj)
        {
            GSHandler gsh = null;
            if (enumObj is GameMaker.SoundKey)
                gsh = GlobalSoundModel.Instance.gsHandlers[(GameMaker.SoundKey)enumObj];
            else
                gsh = getGSHandler(enumObj);
            
            bool isPlay = GSManager.Instance.IsPlaySound(gsh.assetPath);
            
            return isPlay;
        }


        /*
        public void StopSound(object enumObj)
        {
            string path = "";
            if (enumObj is GameMaker.SoundKey)
                path = $"{GlobalSoundModel.Instance.soundRootPath}/{GlobalSoundModel.Instance.soundRelativePath[(GameMaker.SoundKey)enumObj]}";
            else
                path = getSoundPath(enumObj);
            SoundManager.Instance.StopSoundEff(path);  //这个不能用哩
        }
        */
        public void StopSound(object enumObj)
        {
            GSHandler gsh = null;
            if (enumObj is GameMaker.SoundKey)
                gsh = GlobalSoundModel.Instance.gsHandlers[(GameMaker.SoundKey)enumObj];
            else
                gsh = getGSHandler(enumObj);
            GSManager.Instance.StopSound(gsh.assetPath);  //这个不能用哩
        }

        

        /*
        public void StopMusic()
        {
            SoundManager.Instance.StopMusic();
        }
        */
        public void StopMusic()
        {
            GSManager.Instance.StopMusic();
        }
    }
}