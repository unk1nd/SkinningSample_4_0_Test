#region File Description
//-----------------------------------------------------------------------------
// SkinnedModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using SkinnedModel;
#endregion

namespace SkinnedModelPipeline
{
    /// <summary>
    /// Custom processor extends the builtin framework ModelProcessor class,
    /// adding animation support.
    /// </summary>
    [ContentProcessor]
    public class SkinnedModelProcessor : ModelProcessor
    {
        /// <summary>
        /// The main Process method converts an intermediate format content pipeline
        /// NodeContent tree to a ModelContent object with embedded animation data.
        /// </summary>
        public override ModelContent Process(NodeContent input,
                                             ContentProcessorContext context)
        {
            ValidateMesh(input, context, null);

            // Find the skeleton.
            BoneContent skeleton = MeshHelper.FindSkeleton(input);

            if (skeleton == null)
                throw new InvalidContentException("Input skeleton not found.");

            // We don't want to have to worry about different parts of the model being
            // in different local coordinate systems, so let's just bake everything.
            FlattenTransforms(input, skeleton);

            // Read the bind pose and skeleton hierarchy data.
            IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);

            if (bones.Count > SkinnedEffect.MaxBones)
            {
                throw new InvalidContentException(string.Format(
                    "Skeleton has {0} bones, but the maximum supported is {1}.",
                    bones.Count, SkinnedEffect.MaxBones));
            }

            List<Matrix> bindPose = new List<Matrix>();
            List<Matrix> inverseBindPose = new List<Matrix>();
            List<int> skeletonHierarchy = new List<int>();

            foreach (BoneContent bone in bones)
            {
                bindPose.Add(bone.Transform);
                inverseBindPose.Add(Matrix.Invert(bone.AbsoluteTransform));
                skeletonHierarchy.Add(bones.IndexOf(bone.Parent as BoneContent));
            }

            // Convert animation data to our runtime format.
            Dictionary<string, AnimationClip> animationClips;
            animationClips = ProcessAnimations(skeleton.Animations, bones, context, input.Identity);

            // Chain to the base ModelProcessor class so it can convert the model data.
            ModelContent model = base.Process(input, context);

            // Store our custom animation data in the Tag property of the model.
            model.Tag = new SkinningData(animationClips, bindPose,
                                         inverseBindPose, skeletonHierarchy);

            return model;
        }


        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContentDictionary
        /// object to our runtime AnimationClip format.
        /// </summary>
        static Dictionary<string, AnimationClip> ProcessAnimations(
            AnimationContentDictionary animations, IList<BoneContent> bones,
            ContentProcessorContext context, ContentIdentity sourceIdentity)
        {
            // Build up a table mapping bone names to indices.
            Dictionary<string, int> boneMap = new Dictionary<string, int>();

            for (int i = 0; i < bones.Count; i++)
            {
                string boneName = bones[i].Name;

                if (!string.IsNullOrEmpty(boneName))
                    boneMap.Add(boneName, i);
            }

            // Convert each animation in turn.
            Dictionary<string, AnimationClip> animationClips;
            animationClips = new Dictionary<string, AnimationClip>();

            foreach (KeyValuePair<string, AnimationContent> animation in animations)
            {
                AnimationClip processed = ProcessAnimation(animation.Value, boneMap, animation.Key);

                animationClips.Add(animation.Key, processed);
            }

            // Check to see if there's an animation clip definition
            // Here, we're checking for a file with the _Anims suffix.
            // So, if your model is named dude.fbx, we'd add dude_Anims.xml in the same folder
            // and the pipeline will see the file and use it to override the animations in the
            // original model file.
            string SourceModelFile = sourceIdentity.SourceFilename;
            string SourcePath = Path.GetDirectoryName(SourceModelFile);
            string AnimFilename = Path.GetFileNameWithoutExtension(SourceModelFile);
            AnimFilename += "_Anims.xml";
            string AnimPath = Path.Combine(SourcePath, AnimFilename);
            if (File.Exists(AnimPath))
            {
                context.AddDependency(AnimPath);

                AnimationDefinition AnimDef = context.BuildAndLoadAsset<XmlImporter, AnimationDefinition>(new ExternalReference<XmlImporter>(AnimPath), null);

                //breaks up original animation clips into new clips
                if (animationClips.ContainsKey(AnimDef.originalClipName))
                {
                    //graps main clip
                    AnimationClip MainClip = animationClips[AnimDef.originalClipName];
                    //remove original clip from animations
                    animationClips.Remove(AnimDef.originalClipName);

                    foreach (AnimationDefinition.clipPart Part in AnimDef.ClipParts)
                    {
                        //calculate frame times
                        TimeSpan StartTime = GetTimeSpanForFrame(Part.StartFrame, AnimDef.originalFrameCount, MainClip.Duration.Ticks);
                        TimeSpan EndTime = GetTimeSpanForFrame(Part.EndFrame, AnimDef.originalFrameCount, MainClip.Duration.Ticks);

                        //get keyframes for animation clip thats in start and end time
                        List<Keyframe> Keyframes = new List<Keyframe>();
                        foreach (Keyframe AnimFrame in MainClip.Keyframes)
                        {
                            if ((AnimFrame.Time >= StartTime) && (AnimFrame.Time <= EndTime))
                            {
                                Keyframe NewFrame = new Keyframe(AnimFrame.Bone, AnimFrame.Time - StartTime, AnimFrame.Transform);
                                Keyframes.Add(NewFrame);
                            }
                        }

                        //process events
                        /*List<AnimationEvent> Events = new List<AnimationEvent>();
                        if (Part.Events != null)
                        {
                            foreach (AnimationDefinition.clipPart.Event Event in Part.Events)
                            {
                                TimeSpan EventTime = GetTimeSpanForFrame(Event.Keyframe, AnimDef.originalFrameCount, MainClip.Duration.Ticks);
                                EventTime -= StartTime;

                                AnimationEvent newEvent = new AnimationEvent();
                                newEvent.EventTime = EventTime;
                                newEvent.EventName = Event.Name;
                                Events.Add(newEvent);
                            }
                        }*/

                        AnimationClip newClip = new AnimationClip(EndTime - StartTime, Keyframes, Part.ClipName);
                        animationClips[Part.ClipName] = newClip;
                    }
                }
            }

           /* if (animationClips.Count == 0)
            {
                throw new InvalidContentException(
                            "Input file does not contain any animations.");
            }*/

            return animationClips;
        }

        private static TimeSpan GetTimeSpanForFrame(int FrameIndex, int TotalFrameCount, long TotalTicks)
        {
            float MaxFrameIndex = (float)TotalFrameCount - 1;
            float AmountOfAnimation = (float)FrameIndex / MaxFrameIndex;
            float NumTicks = AmountOfAnimation * (float)TotalTicks;
            return new TimeSpan((long)NumTicks);
        }


        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContent
        /// object to our runtime AnimationClip format.
        /// </summary>
        static AnimationClip ProcessAnimation(AnimationContent animation,
                                              Dictionary<string, int> boneMap, string clipName)
        {
            List<Keyframe> keyframes = new List<Keyframe>();

            // For each input animation channel.
            foreach (KeyValuePair<string, AnimationChannel> channel in
                animation.Channels)
            {
                // Look up what bone this channel is controlling.
                int boneIndex = boneMap[channel.Key];


                // Convert the keyframe data.
                foreach (AnimationKeyframe keyframe in channel.Value)
                {
                    keyframes.Add(new Keyframe(boneIndex, keyframe.Time,
                                               keyframe.Transform));
                }
            }
            // Sort the merged keyframes by time.
            keyframes.Sort(CompareKeyframeTimes);

            return new AnimationClip(animation.Duration, keyframes,  clipName);
        }


        /// <summary>
        /// Comparison function for sorting keyframes into ascending time order.
        /// </summary>
        static int CompareKeyframeTimes(Keyframe a, Keyframe b)
        {
            return a.Time.CompareTo(b.Time);
        }


        /// <summary>
        /// Makes sure this mesh contains the kind of data we know how to animate.
        /// </summary>
        static void ValidateMesh(NodeContent node, ContentProcessorContext context,
                                 string parentBoneName)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Validate the mesh.
                if (parentBoneName != null)
                {
                    context.Logger.LogWarning(null, null,
                        "Mesh {0} is a child of bone {1}. SkinnedModelProcessor " +
                        "does not correctly handle meshes that are children of bones.",
                        mesh.Name, parentBoneName);
                }

                if (!MeshHasSkinning(mesh))
                {
                    context.Logger.LogWarning(null, null,
                        "Mesh {0} has no skinning information, so it has been deleted.",
                        mesh.Name);

                    mesh.Parent.Children.Remove(mesh);
                    return;
                }
            }
            else if (node is BoneContent)
            {
                // If this is a bone, remember that we are now looking inside it.
                parentBoneName = node.Name;
            }

            // Recurse (iterating over a copy of the child collection,
            // because validating children may delete some of them).
            foreach (NodeContent child in new List<NodeContent>(node.Children))
                ValidateMesh(child, context, parentBoneName);
        }


        /// <summary>
        /// Checks whether a mesh contains skininng information.
        /// </summary>
        static bool MeshHasSkinning(MeshContent mesh)
        {
            foreach (GeometryContent geometry in mesh.Geometry)
            {
                if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Weights()))
                    return false;
            }

            return true;
        }


        /// <summary>
        /// Bakes unwanted transforms into the model geometry,
        /// so everything ends up in the same coordinate system.
        /// </summary>
        static void FlattenTransforms(NodeContent node, BoneContent skeleton)
        {
            foreach (NodeContent child in node.Children)
            {
                // Don't process the skeleton, because that is special.
                if (child == skeleton)
                    continue;

                // Bake the local transform into the actual geometry.
                MeshHelper.TransformScene(child, child.Transform);

                // Having baked it, we can now set the local
                // coordinate system back to identity.
                child.Transform = Matrix.Identity;

                // Recurse.
                FlattenTransforms(child, skeleton);
            }
        }


        /// <summary>
        /// Force all the materials to use our skinned model effect.
        /// </summary>
        [DefaultValue(MaterialProcessorDefaultEffect.SkinnedEffect)]
        public override MaterialProcessorDefaultEffect DefaultEffect
        {
            get { return MaterialProcessorDefaultEffect.SkinnedEffect; }
            set { }
        }
    }
}
