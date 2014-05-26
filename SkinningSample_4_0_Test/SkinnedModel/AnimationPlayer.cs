#region File Description
//-----------------------------------------------------------------------------
// AnimationPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace SkinnedModel
{
    /// <summary>
    /// The animation player is in charge of decoding bone position
    /// matrices from an animation clip.
    /// </summary>
    public class AnimationPlayer
    {
        #region Fields


        // Information about the currently playing animation clip.
        AnimationClip currentClipValue;
        TimeSpan currentTimeValue;
        int currentKeyframe;
        //Animation currentAnimation, nextAnimation;
        public bool Done { get; private set; }
        bool loop;

        public bool isPlaying = false;
        public bool requiresFinish= false;
       
        // Current animation transform matrices.
        Matrix[] boneTransforms;
        Matrix[] worldTransforms;
        Matrix[] skinTransforms;


        // Backlink to the bind pose and skeleton hierarchy data.
        SkinningData skinningDataValue;

        /*public delegate void EventCallBack(string Event);

        Dictionary<string, Dictionary<string, EventCallBack>> registeredEvents = new Dictionary<string, Dictionary<string, EventCallBack>>();
        public Dictionary<string, Dictionary<string, EventCallBack>> RegisteredEvents
        {
            get { return registeredEvents; }
        }*/


        #endregion


        /// <summary>
        /// Constructs a new animation player.
        /// </summary>
        public AnimationPlayer(SkinningData skinningData)
        {
            // Construct the event dictionaries for each clip
          /*  foreach (string clipName in skinningData.AnimationClips.Keys)
            {
                registeredEvents[clipName] = new Dictionary<string, EventCallBack>();
            }

            if (skinningData == null)
                throw new ArgumentNullException("skinningData");*/

            skinningDataValue = skinningData;

            boneTransforms = new Matrix[skinningData.BindPose.Count];
            worldTransforms = new Matrix[skinningData.BindPose.Count];
            skinTransforms = new Matrix[skinningData.BindPose.Count];
        }


        /// <summary>
        /// Starts decoding the specified animation clip.
        /// </summary>
        public void StartClip(AnimationClip clip, bool loop)
        {
            /*if (clip == null)
                throw new ArgumentNullException("clip");*/

            currentClipValue = clip;
            currentTimeValue = TimeSpan.Zero;
            currentKeyframe = 0;
            Done = false;
            this.loop = loop;

            // Initialize bone transforms to the bind pose.
            skinningDataValue.BindPose.CopyTo(boneTransforms, 0);
        }


        /// <summary>
        /// Advances the current animation position.
        /// </summary>
        public void Update(TimeSpan time, bool relativeToCurrentTime,
                           Matrix rootTransform)
        {
             currentTimeValue += time;

            UpdateBoneTransforms(time, relativeToCurrentTime);
            UpdateWorldTransforms(rootTransform);
            UpdateSkinTransforms();

           
        }

        /// <summary>
        /// Helper used by the Update method to refresh the BoneTransforms data.
        /// </summary>
        public void UpdateBoneTransforms(TimeSpan time, bool relativeToCurrentTime)
        {
            /*  if (currentClipValue == null)
                  throw new InvalidOperationException(
                              "AnimationPlayer.Update was called before StartClip");*/

            // Store the previous time
            TimeSpan lastTime = time;

            // Update the animation position.
            if (relativeToCurrentTime)
            {
                lastTime = currentTimeValue;
                time += currentTimeValue;

                // Check for events
                // CheckEvents(ref time, ref lastTime);*/

                // If we reached the end, loop back to the start.

                while (time >= currentClipValue.Duration)
                {
                    if (loop)
                    {
                        time -= currentClipValue.Duration;
                        currentKeyframe = 0;
                        skinningDataValue.BindPose.CopyTo(boneTransforms, 0);
                    }
                    else
                    {
                        Done = true;
                        time  = currentClipValue.Duration;
                        
                        break;
                    }
                }



                // If we've looped, reprocess the events
                /* if (hasLooped)
                 {
                     CheckEvents(ref time, ref lastTime);
                 }*/
                // }

                /* if ((time < TimeSpan.Zero) || (time >= currentClipValue.Duration))
                     throw new ArgumentOutOfRangeException("time");*/

                // If the position moved backwards, reset the keyframe index.
                /*bool HasResetKeyframe = false;
                if (time < currentTimeValue)
                {
                    HasResetKeyframe = true;
                    currentKeyframe = 0;
                    skinningDataValue.BindPose.CopyTo(boneTransforms, 0);
                }*/

                currentTimeValue = time;

                // Read keyframe matrices.
                IList<Keyframe> keyframes = currentClipValue.Keyframes;

                while (currentKeyframe < keyframes.Count)
                {
                    Keyframe keyframe = keyframes[currentKeyframe];

                    // Stop when we've read up to the current time position.
                    if (keyframe.Time > currentTimeValue)
                        break;

                    // Use this keyframe.
                    boneTransforms[keyframe.Bone] = keyframe.Transform;

                    currentKeyframe++;
                }
            }
        }

        /// <summary>
        /// Helper used by the Update method to refresh the WorldTransforms data.
        /// </summary>
        public void UpdateWorldTransforms(Matrix rootTransform)
        {
            // Root bone.
            worldTransforms[0] = boneTransforms[0] * rootTransform;

            // Child bones.
            for (int bone = 1; bone < worldTransforms.Length; bone++)
            {
                int parentBone = skinningDataValue.SkeletonHierarchy[bone];

                worldTransforms[bone] = boneTransforms[bone] *
                                             worldTransforms[parentBone];
            }
        }

        /// <summary>
        /// Helper used by the Update method to refresh the SkinTransforms data.
        /// </summary>
        public void UpdateSkinTransforms()
        {
            for (int bone = 0; bone < skinTransforms.Length; bone++)
            {
                skinTransforms[bone] = skinningDataValue.InverseBindPose[bone] *
                                            worldTransforms[bone];
            }
        }

        /// <summary>
        /// Checks to see if any events have passed
        /// </summary>
      /*  private void CheckEvents(ref TimeSpan time, ref TimeSpan lastTime)
        {
            foreach (string eventName in registeredEvents[currentClipValue.Name].Keys)
            {
                // Find the event
                foreach (AnimationEvent animEvent in currentClipValue.Events)
                {
                    if (animEvent.EventName == eventName)
                    {
                        TimeSpan eventTime = animEvent.EventTime;
                        if ((lastTime < eventTime) && (time >= eventTime))
                        {
                            // Call the event
                            registeredEvents[currentClipValue.Name][eventName](eventName);
                        }
                    }
                }
            }
        }*/

        public TimeSpan getCurrentTime()
        {
            return currentTimeValue;
        }

        /// <summary>
        /// Gets the current bone transform matrices, relative to their parent bones.
        /// </summary>
        public Matrix[] GetBoneTransforms()
        {
            return boneTransforms;
        }


        /// <summary>
        /// Gets the current bone transform matrices, in absolute format.
        /// </summary>
        public Matrix[] GetWorldTransforms()
        {
            return worldTransforms;
        }


        /// <summary>
        /// Gets the current bone transform matrices,
        /// relative to the skinning bind pose.
        /// </summary>
        public Matrix[] GetSkinTransforms()
        {
            return skinTransforms;
        }


        /// <summary>
        /// Gets the clip currently being decoded.
        /// </summary>
        public AnimationClip CurrentClip
        {
            get { return currentClipValue; }
        }


        /// <summary>
        /// Gets the current play position.
        /// </summary>
        public TimeSpan CurrentTime
        {
            get { return currentTimeValue; }
        }
    }
}
