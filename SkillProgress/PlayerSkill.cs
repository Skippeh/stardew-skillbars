using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace SkillProgress
{
    public struct PlayerSkill
    {
        private readonly int[] points;
        public int FarmingPoints => points[0];
        public int FishingPoints => points[1];
        public int ForagingPoints => points[2];
        public int MiningPoints => points[3];
        public int CombatPoints => points[4];
        public int LuckPoints => points[5];
        public Dictionary<SkillType, int> ExperiencePoints { get; }

        public PlayerSkill(int[] experiencePoints)
        {
            points = new int[experiencePoints.Length];
            Array.Copy(experiencePoints, points, points.Length);
            ExperiencePoints = new Dictionary<SkillType, int>();

            for (int i = 0; i < experiencePoints.Length; ++i)
            {
                ExperiencePoints.Add((SkillType) i, points[i]);
            }
        }

        public static PlayerSkill FromPlayer(Farmer farmer)
        {
            return new PlayerSkill(farmer.experiencePoints);
        }

        public SkillType[] GetChanges(PlayerSkill other)
        {
            if (other == default(PlayerSkill)) return new SkillType[0];
            
            var result = new List<SkillType>();

            for (int i = 0; i < points.Length; ++i)
            {
                if (points[i] != other.points[i])
                    result.Add((SkillType) i);
            }

            return result.ToArray();
        }

        public SkillType GetFirstChange(PlayerSkill other)
        {
            if (other == default(PlayerSkill)) throw new ArgumentNullException(nameof(other));
            return GetChanges(other).FirstOrDefault();
        }

        #region Equality
        
        public override int GetHashCode()
        {
            return points?.GetHashCode() ?? 0;
        }

        public static bool operator ==(PlayerSkill a, PlayerSkill b)
        {
            if (ReferenceEquals(a.points, b.points))
                return true;

            if (a.points == null || b.points == null)
                return false;

            if (a.points.Length != b.points.Length)
                return false;

            for (int i = 0; i < a.points.Length; ++i)
            {
                if (a.points[i] != b.points[i])
                    return false;
            }

            return true;
        }

        public static bool operator !=(PlayerSkill a, PlayerSkill b)
        {
            return !(a == b);
        }
        
        public bool Equals(PlayerSkill other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerSkill skill && Equals(skill);
        }

        #endregion
    }
}