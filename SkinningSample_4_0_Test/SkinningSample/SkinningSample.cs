#region File Description
//-----------------------------------------------------------------------------
// SkinningSample.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkinnedModel;
#endregion

namespace SkinningSample
{
    /// <summary>
    /// Sample game showing how to display skinned character animation.
    /// </summary>
    public class SkinningSampleGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        KeyboardState currentKeyboardState = new KeyboardState();
        KeyboardState lastKeyboardState;

        public float playSpeed = 0.3f;

        Model currentModel;
        AnimationPlayer animationPlayer;
        AnimationClip idle, jump, fall, fpos, idle2, run, attack, die;
      //   AnimationClip animations[];

        float cameraArc = 0;
        float cameraRotation = 0;
        float cameraDistance = 100;
        bool isPlaying = false;

        #endregion

        #region Initialization


        public SkinningSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load the model.
            currentModel = Content.Load<Model>("InfectedDruggie");

            // Look up our custom skinning information.
            SkinningData skinningData = currentModel.Tag as SkinningData;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            animationPlayer = new AnimationPlayer(skinningData);

            //AnimationClip clip = skinningData.AnimationClips["Take 001"];

            //animationPlayer.RegisteredEvents["Fall"].Add("FireFrame", new AnimationPlayer.EventCallBack(OnFire));

           // animations = new AnimationClip[3];
            //animations[0] = new AnimationClip[skinningData.AnimationClips];

            fpos = skinningData.AnimationClips["Fpos"];
            idle = skinningData.AnimationClips["Idle"];
            idle2 = skinningData.AnimationClips["Idle2"];
            run = skinningData.AnimationClips["Run"];
            attack = skinningData.AnimationClips["Attack"];
            die = skinningData.AnimationClips["Die"];

            /* animations[0] = idle;
             animations[1] = jump;
             animations[3] = fall;
             */


            // start with idle animation
           
                animationPlayer.StartClip(run, true);
 
              
        }

       

        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            UpdateCamera(gameTime);
            TimeSpan elapsedTime = TimeSpan.FromSeconds(gameTime.ElapsedGameTime.TotalSeconds * playSpeed);
            
            animationPlayer.Update(elapsedTime, true, Matrix.Identity);
            /*
            if (animationPlayer.Done)
            {
                animationPlayer.StartClip(idle, true);
            }
            */
            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;

            device.Clear(Color.Black);

            Matrix[] bones = animationPlayer.GetSkinTransforms();

            // Compute camera matrices.
            Matrix view = Matrix.CreateTranslation(0, -40, 0) * 
                          Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                          Matrix.CreateRotationX(MathHelper.ToRadians(cameraArc)) *
                          Matrix.CreateLookAt(new Vector3(0, 0, -cameraDistance), 
                                              new Vector3(0, 0, 0), Vector3.Up);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                    device.Viewport.AspectRatio,
                                                                    1,
                                                                    10000);

            // Render the skinned mesh.
            foreach (ModelMesh mesh in currentModel.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);

                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = Matrix.CreateScale(0.15f); 

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }
                mesh.Draw();
            }
            base.Draw(gameTime);
        }

        
        #endregion

        #region Handle Input

        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        private void HandleInput()
        {
            currentKeyboardState = Keyboard.GetState();
            
           
            
                if (currentKeyboardState.IsKeyDown(Keys.Space))
                {
                    animationPlayer.StartClip(die, false);
                }

                
                if (currentKeyboardState.IsKeyDown(Keys.D1))
                {
                    animationPlayer.StartClip(attack, false);
                }
            
                if (currentKeyboardState.IsKeyDown(Keys.D2)) 
                {
                    animationPlayer.StartClip(run, true);  
                }
            

                if (currentKeyboardState.IsKeyDown(Keys.Enter))
                {
                    animationPlayer.StartClip(idle, true);
                }
            

            // Exit
            if (currentKeyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            lastKeyboardState = currentKeyboardState;
           
        }
        #endregion

        #region cameraInput
        /// <summary>
        /// Handles camera input.
        /// </summary>
        private void UpdateCamera(GameTime gameTime)
        {
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Check for input to rotate the camera up and down around the model.
            if (currentKeyboardState.IsKeyDown(Keys.Up) ||
                currentKeyboardState.IsKeyDown(Keys.W))
            {
                cameraArc += time * 0.1f;
            }
            
            if (currentKeyboardState.IsKeyDown(Keys.Down) ||
                currentKeyboardState.IsKeyDown(Keys.S))
            {
                cameraArc -= time * 0.1f;
            }

            // Limit the arc movement.
            if (cameraArc > 90.0f)
                cameraArc = 90.0f;
            else if (cameraArc < -90.0f)
                cameraArc = -90.0f;

            // Check for input to rotate the camera around the model.
            if (currentKeyboardState.IsKeyDown(Keys.Right) ||
                currentKeyboardState.IsKeyDown(Keys.D))
            {
                cameraRotation += time * 0.1f;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Left) ||
                currentKeyboardState.IsKeyDown(Keys.A))
            {
                cameraRotation -= time * 0.1f;
            }

            // Check for input to zoom camera in and out.
            if (currentKeyboardState.IsKeyDown(Keys.Z))
                cameraDistance += time * 0.25f;

            if (currentKeyboardState.IsKeyDown(Keys.X))
                cameraDistance -= time * 0.25f;

            // Limit the camera distance.
            if (cameraDistance > 500.0f)
                cameraDistance = 500.0f;
            else if (cameraDistance < 10.0f)
                cameraDistance = 10.0f;

            if (currentKeyboardState.IsKeyDown(Keys.R))
            {
                cameraArc = 0;
                cameraRotation = 0;
                cameraDistance = 100;
            }
        }


        #endregion
    }


    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (SkinningSampleGame game = new SkinningSampleGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
