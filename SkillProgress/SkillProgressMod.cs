using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace SkillProgress
{
    public class SkillProgressMod : Mod
    {
        public static SkillProgressMod Instance { get; private set; }
        
        private ProgressBarsRenderer progressBarsRenderer;
        
        public override void Entry(IModHelper helper)
        {
            Instance = this;
            
            helper.Events.GameLoop.UpdateTicked += OnTick;
            helper.Events.Display.Rendering += OnPreRender;
            helper.Events.Display.RenderedHud += OnGui;
            helper.Events.GameLoop.SaveLoaded += OnAfterLoad;
            helper.Events.GameLoop.DayStarted += AfterDayStarted;

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
        }

        private void OnAfterLoad(object sender, SaveLoadedEventArgs e)
        {
            progressBarsRenderer = new ProgressBarsRenderer(Game1.player);
        }

        private void AfterDayStarted(object sender, DayStartedEventArgs e)
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

        private void OnTick(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.currentGameTime == null || progressBarsRenderer == null)
                return;
            
            if (!Context.IsPlayerFree)
                return;

            progressBarsRenderer.Tick();
        }

        private void OnPreRender(object sender, RenderingEventArgs e)
        {
            if (!Context.IsPlayerFree || progressBarsRenderer == null)
                return;

            progressBarsRenderer.Render();
        }

        private void OnGui(object sender, RenderedHudEventArgs e)
        {
            if (!Context.IsPlayerFree || progressBarsRenderer == null)
                return;
            
            progressBarsRenderer.Draw();
        }
    }
}