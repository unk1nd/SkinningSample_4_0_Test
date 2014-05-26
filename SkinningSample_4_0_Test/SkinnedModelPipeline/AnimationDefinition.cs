using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace SkinnedModelPipeline
{
    class AnimationDefinition
    {
        public string originalClipName { get; set; }
        public int originalFrameCount { get; set; }

        public class clipPart
        {
            public string ClipName { get; set; }
            public int StartFrame { get; set; }
            public int EndFrame { get; set; }

            public class Event
            {
                public string Name { get; set; }
                public int Keyframe { get; set; }
            }


            [Microsoft.Xna.Framework.Content.ContentSerializer(Optional = true)]
            public List<Event> Events { get; set; }
        }

        public List<clipPart> ClipParts { get; set; }
    }
}
