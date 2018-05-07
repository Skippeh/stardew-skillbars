using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;

namespace SkillProgress
{
    public class SkillProgressMod : Mod
    {
        public static SkillProgressMod Instance { get; private set; }
        
        public SpriteBatch SpriteBatch { get; private set; }
        
        private ProgressBarsRenderer progressBarsRenderer;
        
        public override void Entry(IModHelper helper)
        {
            Instance = this;
            
            StardewModdingAPI.Events.GameEvents.UpdateTick += OnTick;
            StardewModdingAPI.Events.GraphicsEvents.OnPreRenderEvent += OnPreRender;
            StardewModdingAPI.Events.GraphicsEvents.OnPostRenderHudEvent += OnGui;
            StardewModdingAPI.Events.SaveEvents.AfterLoad += OnAfterLoad;
            StardewModdingAPI.Events.TimeEvents.AfterDayStarted += AfterDayStarted;

            var cursorsTexture = helper.Content.Load<Texture2D>("LooseSprites/Cursors.xnb", ContentSource.GameContent);
            
            SkillProgressBar.BarBackground = new NineSliceTexture2D(cursorsTexture,
                new Rectangle(316, 361, 13, 22))
            {
                LeftPadding = 2,
                TopPadding = 7,
                BottomPadding = 5,
                RightPadding = 6
            };

            SkillProgressBar.BarFill = new NineSliceTexture2D(helper.Content.Load<Texture2D>("content/xp-bar-fill.png"))
            {
                LeftPadding = 0,
                TopPadding = 0,
                BottomPadding = 0,
                RightPadding = 2
            };
            
            SkillProgressBar.ProfessionBackground = new NineSliceTexture2D(cursorsTexture, new Rectangle(293, 360, 23, 24))
            {
                LeftPadding = 4,
                TopPadding = 4,
                BottomPadding = 4,
                RightPadding = 3
            };
            
            SkillProgressBar.UiTexture = cursorsTexture;

            SpriteBatch = new SpriteBatch(Game1.graphics.GraphicsDevice);
        }

        private void OnAfterLoad(object sender, EventArgs e)
        {
            progressBarsRenderer = new ProgressBarsRenderer(Game1.player);
        }

        private void AfterDayStarted(object sender, EventArgs e)
        {
            progressBarsRenderer?.HideBars();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SkillProgressBar.BarBackground.Dispose();
                SkillProgressBar.UiTexture.Dispose();
                progressBarsRenderer.Dispose();
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (Game1.currentGameTime == null || progressBarsRenderer == null)
                return;
            
            if (!Context.IsPlayerFree)
                return;

            progressBarsRenderer.Tick();
        }

        private void OnPreRender(object sender, EventArgs e)
        {
            if (!Context.IsPlayerFree || progressBarsRenderer == null)
                return;
            
            progressBarsRenderer.Render();
        }

        private void OnGui(object sender, EventArgs e)
        {
            if (!Context.IsPlayerFree || progressBarsRenderer == null)
                return;
            
            progressBarsRenderer.Draw();
        }
    }
}