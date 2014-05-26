using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using SkinnedModel;

namespace SkinnedModel
{
    public class AnimationBlender 
    {
        //current animation playing
        private AnimationPlayer currentAnimation;
        //animation to blend to
        private AnimationPlayer nextAnimation;
     //  public  TimeSpan CurrentPosition;

        TimeSpan nextFrameInterval, frameInterval;

        
        //total time to blend to next animation
        private TimeSpan totalBlendTime = TimeSpan.FromMilliseconds(300);
        //how far along current blend time
        private TimeSpan currentBlendTime;

        public AnimationBlender(AnimationPlayer startingAnimation)
        {
            currentAnimation = startingAnimation;

            //GetBoneTransforms = new List<Matrix>(blendedAvatarBones);
        }




        /*public void List<Matrix> 
        {
            get { return boneTransforms; }
        }
         private Matrix[] blendedBones = new Matrix[]
        private void List<Matrix> boneTransforms;
        */
        public void changeAnimation(AnimationPlayer next)
        {
            if ((currentAnimation.isPlaying == false) && (currentAnimation.requiresFinish == false))
            {
                nextAnimation = next;
            }
            else
            {
                currentAnimation = next;
            }
        }

        private void onAnimationEnd()
        {
            if (nextAnimation != null)
            {
                currentAnimation = nextAnimation;
                nextAnimation = null;
            }
            else
            { 
                //idle-animation
            }
        }

        public TimeSpan CurrentPosition
        {
            get
            {
                TimeSpan returnValue;

                if (nextAnimation != null)
                {
                    returnValue = nextAnimation.CurrentTime;
                }
                else
                {
                    returnValue = currentAnimation.CurrentTime;
                }
                Update(TimeSpan.Zero, false);

                return returnValue;
            }

            set { }
         /*   {
                
                if (nextAnimation != null)
                {
                     
                }
                else
                {
                    currentAnimation.CurrentTime = value;
                }
            }*/
        }

       /* public TimeSpan Length
        {
            get
            {
                if (nextAnimation != null)
                {
                    return nextAnimation.Length;
                }
                else
                {
                    return currentAnimation.Length;
                }
            }
        }*/

        

        public void Play(AnimationPlayer animation)
        {
            //animation to blend to
            nextAnimation = animation;
            //reset animation and start at beginning
            //nextAnimation.CurrentPosition = TimeSpan.Zero;

            //sets current blend position
            currentBlendTime = TimeSpan.Zero;
        }

        public void Update(TimeSpan elapsedAnimationTime, bool loop)
        {

            if (nextFrameInterval >= frameInterval)
            { 
                
            }
            /*//updates current animation
            currentAnimation.Update(elapsedAnimationTime, loop);

            //if not blend to animation, cop current transforms
            if (nextAnimation == null)
            {
                currentAnimation.BoneTransforms.CopyTo(blendedAvatarBones, 0);
                return;
            }

            nextAnimation.Update(elapsedAnimationTime, loop);

            currentBlendTime += elapsedAnimationTime;


            float blendAmount = (float)(currentBlendTime.TotalSeconds / totalBlendTime.TotalSeconds);

            if (blendAmount >= 1.0f)
            {
                currentAnimation = nextAnimation;
                nextAnimation.BoneTransforms.CopyTo(blendedAvatarBones, 0);
                nextAnimation = null;
                return;
            }

            Quaternion currentRotation, nextRotation, blendedRotation;
            Vector3 currentTranslation, nextTranslation, blendedTranslation;

            for (int i = 0; i < blendedAvatarBones.Length; ++i)
            {
                currentRotation = Quaternion.CreateFromRotationMatrix(currentAnimation.BoneTransforms[i]);
                currentTranslation = currentAnimation.BoneTransforms[i].Translation;
                nextRotation = Quaternion.CreateFromRotationMatrix(nextAnimation.BoneTransforms[i]);
                nextTranslation = nextAnimation.BoneTransforms[i].Translation;

                Quaternion.Slerp(ref currentRotation, ref nextRotation, blendAmount, out blendedRotation);

                Vector3.Lerp(ref currentTranslation, ref nextTranslation, blendAmount, out blendedTranslation);

                blendedAvatarBones[i] = Matrix.CreateFromQuaternion(blendedRotation) * Matrix.CreateTranslation(blendedTranslation);
            }*/
        }


    }
}
