using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;

namespace SkillProgress
{
    public class SkillProgressBar : IDisposable
    {
        public static NineSliceTexture2D BarBackground;
        public static NineSliceTexture2D BarFill;
        public static NineSliceTexture2D ProfessionBackground;
        public static Texture2D UiTexture;

        public static readonly Vector2 TotalSize = new Vector2(526, 26);

        private static readonly Dictionary<SkillType, Rectangle> skillSourceRects = new Dictionary<SkillType, Rectangle>()
        {
            [SkillType.Farming] = new Rectangle(10, 428, 10, 10),
            [SkillType.Fishing] = new Rectangle(20, 428, 10, 10),
            [SkillType.Foraging] = new Rectangle(60, 428, 10, 10),
            [SkillType.Combat] = new Rectangle(120, 428, 10, 10),
            [SkillType.Mining] = new Rectangle(30, 428, 10, 10),
            [SkillType.Luck] = new Rectangle(50, 428, 10, 10)
        };
        
        public SkillType SkillType { get; private set; }
        public int CurrentPoints { get; private set; }

        private int previousPoints;
        private int animatedPoints => CurrentPoints > 0 ? (int) Math.Round(MathHelper.SmoothStep(previousPoints, CurrentPoints, progressAnimateTime / maxProgressAnimationTime)) : 0;

        private float progressAnimateTime = 0;
        private const float maxProgressAnimationTime = 0.75f; // 0.75 seconds to fully animate to target points

        private readonly RenderTarget2D renderTarget;
        private readonly SpriteBatch spriteBatch;

        private static FlooredCurve<int, int> levelCurve = new FlooredCurve<int, int>(new List<Tuple<int, int>>
        {
            new Tuple<int, int>(0, 0),
            new Tuple<int, int>(100, 1),
            new Tuple<int, int>(380, 2),
            new Tuple<int, int>(770, 3),
            new Tuple<int, int>(1300, 4),
            new Tuple<int, int>(2150, 5),
            new Tuple<int, int>(3300, 6),
            new Tuple<int, int>(4800, 7),
            new Tuple<int, int>(6900, 8),
            new Tuple<int, int>(10000, 9),
            new Tuple<int, int>(15000, 10)
        });
        
        public SkillProgressBar(SkillType skillType, int initialPoints)
        {
            SkillType = skillType;
            CurrentPoints = initialPoints;
            previousPoints = initialPoints;

            renderTarget = new RenderTarget2D(Game1.graphics.GraphicsDevice, (int) TotalSize.X, (int) TotalSize.Y);
            spriteBatch = new SpriteBatch(Game1.graphics.GraphicsDevice);
        }
        
        public void UpdatePoints(int experiencePoints)
        {
            if (experiencePoints > levelCurve.MaxPosition)
                experiencePoints = levelCurve.MaxPosition;
            
            int difference = Math.Abs(experiencePoints - previousPoints);
            
            previousPoints = CurrentPoints;
            CurrentPoints = experiencePoints;
            progressAnimateTime = 0;
        }

        public void Tick()
        {
            float deltaTime = (float) Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            progressAnimateTime = MathHelper.Min(maxProgressAnimationTime, progressAnimateTime + deltaTime);
        }

        public void Draw(Vector2 position, float opacity)
        {
            Game1.spriteBatch.Draw(renderTarget, position, null, new Color(opacity, opacity, opacity, opacity));
            DrawText(position);
        }

        public void Render()
        {
            Game1.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, null, null);
            DrawBackground();
            DrawLevelProgress();
            DrawSkillType();
            spriteBatch.End();

            Game1.graphics.GraphicsDevice.SetRenderTarget(null);
            Game1.graphics.GraphicsDevice.Clear(new Color(5, 3, 4));
        }

        private void DrawBackground()
        {
            Rectangle area = new Rectangle(26, 5, 500, 16);
            BarBackground.Draw(spriteBatch, area, Color.White);
        }

        private void DrawLevelProgress()
        {
            Rectangle fullRect = new Rectangle(27, 9, 503 - BarBackground.RightPadding - BarBackground.LeftPadding, 8);
            Rectangle progressRect = fullRect;
            progressRect.Width = (int) (progressRect.Width * levelCurve.GetPercentageToNextPoint(animatedPoints));
            
            BarFill.Draw(spriteBatch, progressRect, Color.White);
        }

        private void DrawSkillType()
        {
            Rectangle area = new Rectangle(0, 0, 26, 26);
            ProfessionBackground.Draw(spriteBatch, area, Color.White);

            var sourceRect = skillSourceRects[SkillType];
            var center = new Vector2(area.Center.X, area.Center.Y);
            spriteBatch.Draw(UiTexture, center, sourceRect, Color.White, 0f, new Vector2(sourceRect.Width / 2, sourceRect.Height / 2), 1.8f, SpriteEffects.None, 0);
        }

        private void DrawText(Vector2 position)
        {
            string levelText = $"{SkillType} - Level {levelCurve.GetValue(animatedPoints)} ({animatedPoints}/{levelCurve.GetNextPoint(animatedPoints)} XP)";
            var textSize = Game1.smallFont.MeasureString(levelText);
            Vector2 levelPosition = position + new Vector2((500 / 2) - (textSize.X / 2) + 26, -textSize.Y);

            var mouseState = Mouse.GetState();

            if (new Rectangle((int) position.X, (int) position.Y, (int) TotalSize.X, (int) TotalSize.Y).Contains(mouseState.X, mouseState.Y))
            {
                DrawShadowedText(Game1.smallFont, levelText, levelPosition, Color.White);
            }
        }

        private void DrawShadowedText(SpriteFont font, string text, Vector2 position, Color textColor)
        {
            Game1.spriteBatch.DrawString(font, text, position + Vector2.One, new Color(0, 0, 0, textColor.A));
            Game1.spriteBatch.DrawString(font, text, position, textColor);
        }

        public void Dispose()
        {
            if (!renderTarget.IsDisposed)
                renderTarget.Dispose();

            if (!spriteBatch.IsDisposed)
                spriteBatch.Dispose();
        }
    }
}