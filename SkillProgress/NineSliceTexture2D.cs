using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SkillProgress
{
    public class NineSliceTexture2D : IDisposable
    {
        public Texture2D Texture { get; }
        public Rectangle? SourceRectangle { get; set; }

        public int LeftPadding
        {
            get => leftPadding;
            set
            {
                leftPadding = value;
                CreateSourcePatches();
            }
        }

        public int RightPadding
        {
            get => rightPadding;
            set
            {
                rightPadding = value;
                CreateSourcePatches();
            }
        }

        public int TopPadding
        {
            get => topPadding;
            set
            {
                topPadding = value;
                CreateSourcePatches();
            }
        }

        public int BottomPadding
        {
            get => bottomPadding;
            set
            {
                bottomPadding = value;
                CreateSourcePatches();
            }
        }

        private Rectangle[] sourcePatches;
        private int leftPadding;
        private int rightPadding;
        private int topPadding;
        private int bottomPadding;

        public NineSliceTexture2D(Texture2D texture)
        {
            Texture = texture;
        }

        public NineSliceTexture2D(Texture2D texture, Rectangle sourceRectangle) : this(texture)
        {
            SourceRectangle = sourceRectangle;
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle area, Color color)
        {
            var areaRects = CreatePatches(area);

            for (int i = 0; i < areaRects.Length; ++i)
            {
                spriteBatch.Draw(Texture, areaRects[i], sourcePatches[i], color);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle area)
        {
            Draw(spriteBatch, area, Color.White);
        }
        
        private Rectangle[] CreatePatches(Rectangle rectangle)
        {
            var x = rectangle.X;
            var y = rectangle.Y;
            var w = rectangle.Width;
            var h = rectangle.Height;
            var middleWidth = w - LeftPadding - RightPadding;
            var middleHeight = h - TopPadding - BottomPadding;
            var bottomY = y + h - BottomPadding;
            var rightX = x + w - RightPadding;
            var leftX = x + LeftPadding;
            var topY = y + TopPadding;
            var patches = new[]
            {
                new Rectangle(x,      y,        LeftPadding,  TopPadding),      // top left
                new Rectangle(leftX,  y,        middleWidth,  TopPadding),      // top middle
                new Rectangle(rightX, y,        RightPadding, TopPadding),      // top right
                new Rectangle(x,      topY,     LeftPadding,  middleHeight),    // left middle
                new Rectangle(leftX,  topY,     middleWidth,  middleHeight),    // middle
                new Rectangle(rightX, topY,     RightPadding, middleHeight),    // right middle
                new Rectangle(x,      bottomY,  LeftPadding,  BottomPadding),   // bottom left
                new Rectangle(leftX,  bottomY,  middleWidth,  BottomPadding),   // bottom middle
                new Rectangle(rightX, bottomY,  RightPadding, BottomPadding)    // bottom right
            };
            return patches;
        }

        private void CreateSourcePatches()
        {
            sourcePatches = CreatePatches(SourceRectangle ?? Texture.Bounds);
        }

        public void Dispose()
        {
            if (!Texture.IsDisposed)
                Texture.Dispose();
        }
    }
}