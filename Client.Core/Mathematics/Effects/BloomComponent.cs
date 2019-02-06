using System;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.Mathematics.Effects
{
    public class BloomComponent
    {
        #region Fields

        public enum IntermediateBuffer
        {
            PreBloom,
            BlurredHorizontally,
            BlurredBothWays,
            FinalResult,
        }

        private readonly Effect bloomCombineEffect;
        private readonly Effect bloomExtractEffect;
        private readonly Effect gaussianBlurEffect;
        private SurfaceFormat format;
        private int height;
        private EffectParameter offsetsParameter;
        private EffectParameterCollection parameters;

        private RenderTarget2D renderTarget1;
        private RenderTarget2D renderTarget2;
        private Vector2[] sampleOffsets;
        private float[] sampleWeights;
        private RenderTarget2D sceneRenderTarget;
        public RenderTarget2D screenTarget;

        private BloomSettings settings = BloomSettings.PresetSettings[0];


        // Optionally displays one of the intermediate buffers used
        // by the bloom postprocess, so you can see exactly what is
        // being drawn into each rendertarget.

        private IntermediateBuffer showBuffer = IntermediateBuffer.FinalResult;

        // GC Friendly
        private Viewport viewport;
        private EffectParameter weightsParameter;
        private int width;

        public BloomSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        public IntermediateBuffer ShowBuffer
        {
            get { return showBuffer; }
            set { showBuffer = value; }
        }

        #endregion

        #region Initialization

        public BloomComponent(ContentManager Content, SpriteBatch spriteBatch)
        {
            bloomExtractEffect = Content.Load<Effect>(@"Client.Monogame.Content/Effects/BloomExtract");
            bloomCombineEffect = Content.Load<Effect>(@"Client.Monogame.Content/Effects/BloomCombine");
            gaussianBlurEffect = Content.Load<Effect>(@"Client.Monogame.Content/Effects/GaussianBlur");

            InitializeBloom(spriteBatch);
        }

        public void InitializeBloom(SpriteBatch spriteBatch)
        {
            // Look up the resolution and format of our main backbuffer.
            PresentationParameters pp = spriteBatch.GraphicsDevice.PresentationParameters;

            width = pp.BackBufferWidth;
            height = pp.BackBufferHeight;

            SurfaceFormat format = pp.BackBufferFormat;

            // Create a texture for rendering the main scene, prior to applying bloom.
            sceneRenderTarget = new RenderTarget2D(spriteBatch.GraphicsDevice, width, height, false,
                                                   format, pp.DepthStencilFormat, pp.MultiSampleCount,
                                                   RenderTargetUsage.DiscardContents);
            screenTarget = new RenderTarget2D(spriteBatch.GraphicsDevice, width, height, false,
                                              format, pp.DepthStencilFormat, pp.MultiSampleCount,
                                              RenderTargetUsage.DiscardContents);
            // Create two rendertargets for the bloom processing. These are half the
            // size of the backbuffer, in order to minimize fillrate costs. Reducing
            // the resolution in this way doesn't hurt quality, because we are going
            // to be blurring the bloom images in any case.
            int widthSize = width/2;
            int heightSize = height/2;

            renderTarget1 = new RenderTarget2D(spriteBatch.GraphicsDevice, width, height, false, format,
                                               DepthFormat.None);
            renderTarget2 = new RenderTarget2D(spriteBatch.GraphicsDevice, width, height, false, format,
                                               DepthFormat.None);
        }

        #endregion

        #region Draw

        public void CheckBackbufferSize(SpriteBatch spriteBatch)
        {
            if (width != spriteBatch.GraphicsDevice.Viewport.Width ||
                height != spriteBatch.GraphicsDevice.Viewport.Height)
            {
                InitializeBloom(spriteBatch);
#if DEBUG
                Console.WriteLine("Backbuffer Scale Changed");
#endif
            }
        }

        public void BeginDraw(SpriteBatch spriteBatch)
        {
            spriteBatch.GraphicsDevice.SetRenderTarget(sceneRenderTarget);
            spriteBatch.GraphicsDevice.Clear(Color.Transparent);
        }

        public void BeginDrawNonBloom(SpriteBatch spriteBatch)
        {
            spriteBatch.GraphicsDevice.SetRenderTarget(sceneRenderTarget);
            spriteBatch.GraphicsDevice.Clear(Color.Black);
        }

        public void DrawNonBloom(SpriteBatch spriteBatch, IGameTimeService gameTime)
        {
            viewport = spriteBatch.GraphicsDevice.Viewport;
            DrawFullscreenQuad(spriteBatch, sceneRenderTarget,
                               viewport.Width, viewport.Height,
                               null,
                               IntermediateBuffer.FinalResult);
        }

        /// <summary>
        /// This is where it all happens. Grabs a scene that has already been rendered,
        /// and uses postprocess magic to add a glowing bloom effect over the top of it.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            // Pass 1: draw the scene into rendertarget 1, using a
            // shader that extracts only the brightest parts of the image.
            bloomExtractEffect.Parameters["BloomThreshold"].SetValue(
                Settings.BloomThreshold);

            DrawFullscreenQuad(spriteBatch, sceneRenderTarget, renderTarget1,
                               bloomExtractEffect,
                               IntermediateBuffer.PreBloom);

            // Pass 2: draw from rendertarget 1 into rendertarget 2,
            // using a shader to apply a horizontal gaussian blur filter.
            SetBlurEffectParameters(1.0f / (float)renderTarget1.Width, 0);

            DrawFullscreenQuad(spriteBatch, renderTarget1, renderTarget2,
                               gaussianBlurEffect,
                               IntermediateBuffer.BlurredHorizontally);

            // Pass 3: draw from rendertarget 2 back into rendertarget 1,
            // using a shader to apply a vertical gaussian blur filter.
            SetBlurEffectParameters(0, 1.0f / (float)renderTarget1.Height);

            DrawFullscreenQuad(spriteBatch, renderTarget2, renderTarget1,
                               gaussianBlurEffect,
                               IntermediateBuffer.BlurredBothWays);

            // Pass 4: draw both rendertarget 1 and the original scene
            // image back into the main backbuffer, using a shader that
            // combines them to produce the final bloomed result.
            spriteBatch.GraphicsDevice.SetRenderTarget(screenTarget);

            parameters = bloomCombineEffect.Parameters;

            parameters["BloomIntensity"].SetValue(Settings.BloomIntensity);
            parameters["BaseIntensity"].SetValue(Settings.BaseIntensity);
            parameters["BloomSaturation"].SetValue(Settings.BloomSaturation);
            parameters["BaseSaturation"].SetValue(Settings.BaseSaturation);

            spriteBatch.GraphicsDevice.Textures[1] = sceneRenderTarget;

            viewport = spriteBatch.GraphicsDevice.Viewport;

            DrawFullscreenQuad(spriteBatch, renderTarget1,
                               viewport.Width, viewport.Height,
                               bloomCombineEffect,
                               IntermediateBuffer.FinalResult);

            spriteBatch.GraphicsDevice.SetRenderTarget(null);
        }


        /// <summary>
        /// Helper for drawing a texture into a rendertarget, using
        /// a custom shader to apply postprocessing effects.
        /// </summary>
        private void DrawFullscreenQuad(SpriteBatch spriteBatch, Texture2D texture, RenderTarget2D renderTarget,
                                        Effect effect, IntermediateBuffer currentBuffer)
        {
            spriteBatch.GraphicsDevice.SetRenderTarget(renderTarget);

            DrawFullscreenQuad(spriteBatch, texture,
                               renderTarget.Width, renderTarget.Height,
                               effect, currentBuffer);
        }

        /// <summary>
        /// Helper for drawing a texture into the current rendertarget,
        /// using a custom shader to apply postprocessing effects.
        /// </summary>
        private void DrawFullscreenQuad(SpriteBatch spriteBatch, Texture2D texture, int width, int height,
                                        Effect effect, IntermediateBuffer currentBuffer)
        {
            // If the user has selected one of the show intermediate buffer options,
            // we still draw the quad to make sure the image will end up on the screen,
            // but might need to skip applying the custom pixel shader.
            if (showBuffer < currentBuffer)
            {
                effect = null;
            }

            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, effect);
            spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            spriteBatch.End();
        }

        /// <summary>
        /// Helper for drawing a texture into the current rendertarget,
        /// using a custom shader to apply postprocessing effects.
        /// </summary>
        private void DrawFullscreenQuadFinal(SpriteBatch spriteBatch, Texture2D texture, int width, int height,
                                             Effect effect, IntermediateBuffer currentBuffer)
        {
            // If the user has selected one of the show intermediate buffer options,
            // we still draw the quad to make sure the image will end up on the screen,
            // but might need to skip applying the custom pixel shader.
            if (showBuffer < currentBuffer)
            {
                effect = null;
            }

            spriteBatch.Begin(0, BlendState.AlphaBlend, null, null, null, effect);
            spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            spriteBatch.End();
        }


        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        private void SetBlurEffectParameters(float dx, float dy)
        {
            // Look up the sample weight and offset effect parameters.

            weightsParameter = gaussianBlurEffect.Parameters["SampleWeights"];
            offsetsParameter = gaussianBlurEffect.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;

            // Create temporary arrays for computing our filter settings.
            sampleWeights = new float[sampleCount];
            sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount/2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i*2 + 1] = weight;
                sampleWeights[i*2 + 2] = weight;

                totalWeights += weight*2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings.
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
        }


        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        private float ComputeGaussian(float n)
        {
            float theta = Settings.BlurAmount;

            return (float) ((1.0/Math.Sqrt(2*Math.PI*theta))*
                            Math.Exp(-(n*n)/(2*theta*theta)));
        }

        #endregion
    }
}