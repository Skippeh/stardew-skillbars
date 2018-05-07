using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;

namespace SkillProgress
{
    public class ProgressBarsRenderer : IDisposable
    {
        private class AnimationState
        {
            public enum State
            {
                Idle,
                In,
                Out
            }

            public State CurrentState
            {
                get => currentState;
                set
                {
                    if (currentState == value)
                        return;
                    
                    lastState = currentState;
                    currentState = value;
                }
            }
            
            public float Opacity { get; private set; }
            public Vector2 PositionOffset { get; private set; }
            public int ListOrder { get; private set; }

            private float offsetAnimationTime = 0;
            private const float maxOffsetAnimationTime = 0.6f;

            private float opacityAnimationTime = 0;
            private const float maxOpacityAnimationTime = 0.5f;

            private float hideTime;
            private State currentState = State.Idle;
            private State? lastState;

            private static readonly Vector2 maxOffset = new Vector2(-5, 0);
            private const float showTime = 20f;
            private static int maxListOrder = 0;
            
            public AnimationState()
            {
                Opacity = 0;
                PositionOffset = maxOffset;
            }

            public void Touch()
            {
                if (CurrentState == State.Out || (CurrentState == State.Idle && lastState != State.In))
                {
                    if (CurrentState == State.Idle)
                        ListOrder = maxListOrder++;
                    
                    CurrentState = State.In;
                    ResetAnimation();
                }

                hideTime = (float) Game1.currentGameTime.TotalGameTime.TotalSeconds + showTime;
            }

            public void Tick()
            {
                float deltaTime = (float) Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
                
                if (Game1.currentGameTime.TotalGameTime.TotalSeconds >= hideTime && CurrentState == State.Idle && lastState == State.In)
                {
                    CurrentState = State.Out;
                    ResetAnimation();
                }

                if (CurrentState != State.Idle)
                {
                    offsetAnimationTime = MathHelper.Min(maxOffsetAnimationTime, offsetAnimationTime + deltaTime);
                    opacityAnimationTime = MathHelper.Min(maxOpacityAnimationTime, opacityAnimationTime + deltaTime);
                    ApplyAnimation();
                }

                if (offsetAnimationTime >= maxOffsetAnimationTime && opacityAnimationTime >= maxOpacityAnimationTime)
                {
                    CurrentState = State.Idle;
                }
            }

            public void Hide()
            {
                lastState = null;
                currentState = State.Idle;
                offsetAnimationTime = maxOffsetAnimationTime;
                opacityAnimationTime = maxOffsetAnimationTime;
                Opacity = 0;
                PositionOffset = maxOffset;
            }

            private void ResetAnimation()
            {
                offsetAnimationTime = 0;
                opacityAnimationTime = 0;
            }

            private void ApplyAnimation()
            {
                if (CurrentState == State.Idle)
                    return;
                
                float offsetT = offsetAnimationTime / maxOffsetAnimationTime;
                float opacityT = opacityAnimationTime / maxOpacityAnimationTime;

                bool animateIn = CurrentState == State.In;

                Opacity = MathHelper.SmoothStep(animateIn ? 0 : 1, animateIn ? 1 : 0, opacityT);
                PositionOffset = Vector2.SmoothStep(animateIn ? maxOffset : Vector2.Zero, animateIn ? Vector2.Zero : maxOffset, offsetT);
            }
        }

        private class ListAnimation
        {
            public float Y => MathHelper.SmoothStep(yStart, yEnd, animationTime / maxAnimationTime);
            
            private float yStart;
            private float yEnd;
            
            public float animationTime;
            private const float maxAnimationTime = 0.5f;

            public void Tick()
            {
                float deltaTime = (float) Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
                animationTime = Math.Min(maxAnimationTime, animationTime + deltaTime);
            }

            public void SetTargetY(float y)
            {
                if (Math.Abs(yEnd - y) < 0.00001f)
                    return;
                
                yStart = yEnd;
                yEnd = y;
                animationTime = 0;
            }

            public void SetY(float y)
            {
                yStart = y;
                yEnd = y;
                animationTime = maxAnimationTime;
            }
        }

        private readonly Dictionary<SkillType, SkillProgressBar> progressBars;
        private readonly Dictionary<SkillType, AnimationState> animationStates;
        private readonly Dictionary<SkillType, ListAnimation> listAnimations;
        private PlayerSkill currentSkill;
        private readonly Farmer player;
        private List<KeyValuePair<SkillType, SkillProgressBar>> visibleBars;
        
        public ProgressBarsRenderer(Farmer player)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            progressBars = new Dictionary<SkillType, SkillProgressBar>();
            animationStates = new Dictionary<SkillType, AnimationState>();
            listAnimations = new Dictionary<SkillType, ListAnimation>();
            
            for (int i = 0; i < player.experiencePoints.Length; ++i)
            {
                if (((SkillType) i) == SkillType.Luck)
                    continue;

                var progressBar = new SkillProgressBar((SkillType) i, player.experiencePoints[i]);
                
                progressBars.Add((SkillType) i, progressBar);
                animationStates.Add((SkillType) i, new AnimationState());
                listAnimations.Add((SkillType)i, new ListAnimation());
            }
        }
        
        public void Tick()
        {
            var previousSkill = currentSkill;
            currentSkill = PlayerSkill.FromPlayer(player);

            SkillType[] changes = currentSkill.GetChanges(previousSkill);

            foreach (SkillType skillType in changes)
            {
                if (!progressBars.ContainsKey(skillType))
                    continue;
                
                progressBars[skillType].UpdatePoints(currentSkill.ExperiencePoints[skillType]);
                animationStates[skillType].Touch();
            }
                
            foreach (var kv in progressBars)
            {
                kv.Value.Tick();
                animationStates[kv.Key].Tick();
                listAnimations[kv.Key].Tick();
            }
        }

        public void HideBars()
        {
            foreach (var kv in animationStates)
            {
                kv.Value.Hide();
            }
        }

        public void Render()
        {
            Draw(true);
        }

        public void Draw()
        {
            Draw(false);
        }

        private void Draw(bool renderToTexture)
        {
            var lastVisibleBars = visibleBars;
            visibleBars = progressBars.Where(kv => animationStates[kv.Key].Opacity > 0).OrderBy(kv => animationStates[kv.Key].ListOrder).ToList();

            if (lastVisibleBars != null && lastVisibleBars.Count > 0)
            {
                for (int i = visibleBars.Count - 1; i >= 0; --i)
                {
                    var listAnimation = listAnimations[visibleBars[i].Key];
                    var yTarget = i * (SkillProgressBar.TotalSize.Y + 2);

                    if (lastVisibleBars.All(kv => kv.Key != visibleBars[i].Key))
                    {
                        listAnimation.SetY(yTarget);
                    }
                    else
                        listAnimation.SetTargetY(yTarget);
                }
            }

            foreach (var kv in visibleBars)
            {
                var animationState = animationStates[kv.Key];
                
                if (animationState.Opacity > 0)
                {
                    if (!renderToTexture)
                    {
                        var listAnimation = listAnimations[kv.Key];
                        Vector2 drawPosition = Vector2.Zero;
                        
                        drawPosition.X = (Game1.viewport.Width / 2) - (SkillProgressBar.TotalSize.X / 2);
                        drawPosition.Y = (Game1.viewport.Height - 135) - listAnimation.Y;

                        drawPosition += animationState.PositionOffset;
                        
                        kv.Value.Draw(drawPosition, animationState.Opacity);
                    }
                    else
                        kv.Value.Render();
                }
            }
        }

        public void Dispose()
        {
            foreach (var kv in progressBars)
            {
                kv.Value.Dispose();
            }
        }
    }
}