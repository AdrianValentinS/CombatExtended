using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CombatExtended.Compatibility
{
    public static class BlockerRegistry
    {
        private static bool enabled = false;
        private static List<Func<Thing, IEnumerable<IEnumerable<IntVec3>>>> shieldZonesCallback;
        private static List<Func<ProjectileCE, IntVec3, Thing, bool>> checkCellForCollisionCallbacks;
        private static List<Func<ProjectileCE, Thing, bool>> impactSomethingCallbacks;
        private static List<Func<Pawn, IntVec3, bool>> pawnUnsuppresableFromCallback;

        private static void Enable()
        {
            enabled = true;
            impactSomethingCallbacks = new List<Func<ProjectileCE, Thing, bool>>();
            checkCellForCollisionCallbacks = new List<Func<ProjectileCE, IntVec3, Thing, bool>>();
            shieldZonesCallback = new List<Func<Thing, IEnumerable<IEnumerable<IntVec3>>>>();
            pawnUnsuppresableFromCallback = new List<Func<Pawn, IntVec3, bool>>();
        }

        public static void RegisterCheckForCollisionCallback(Func<ProjectileCE, IntVec3, Thing, bool> f)
        {
            if (!enabled)
            {
                Enable();
            }
            checkCellForCollisionCallbacks.Add(f);
        }

        public static void RegisterImpactSomethingCallback(Func<ProjectileCE, Thing, bool> f)
        {
            if (!enabled)
            {
                Enable();
            }
            impactSomethingCallbacks.Add(f);
        }

        public static void RegisterShieldZonesCallback(Func<Thing, IEnumerable<IEnumerable<IntVec3>>> f)
        {
            if (!enabled)
            {
                Enable();
            }
            shieldZonesCallback.Add(f);
        }
        public static void RegisterUnsuppresableFromCallback(Func<Pawn, IntVec3, bool> f)
        {
            if (!enabled)
            {
                Enable();
            }
            pawnUnsuppresableFromCallback.Add(f);
        }

        public static bool CheckCellForCollisionCallback(ProjectileCE projectile, IntVec3 cell, Thing launcher)
        {
            if (!enabled)
            {
                return false;
            }
            foreach (var cb in checkCellForCollisionCallbacks)
            {
                if (cb(projectile, cell, launcher))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool ImpactSomethingCallback(ProjectileCE projectile, Thing launcher)
        {
            if (!enabled)
            {
                return false;
            }
            foreach (var cb in impactSomethingCallbacks)
            {
                if (cb(projectile, launcher))
                {
                    return true;
                }
            }
            return false;
        }
        public static IEnumerable<IEnumerable<IntVec3>> ShieldZonesCallback(Thing thing)
        {
            if (!enabled)
            {
                return null;
            }
            return shieldZonesCallback.SelectMany(cb => cb(thing));
        }
        public static bool PawnUnsuppresableFromCallback(Pawn pawn, IntVec3 origin)
        {
            if (!enabled)
            {
                return false;
            }
            foreach (var cb in pawnUnsuppresableFromCallback)
            {
                if (cb(pawn, origin))
                {
                    return true;
                }
            }
            return false;
        }
        public static Vector3 GetExactPosition(Vector3 origin, Vector3 curPosition, Vector3 shieldPosition, float radiusSq)
        {
            Vector3 velocity = curPosition - origin;

            double a = velocity.sqrMagnitude;
            double b = 2 * (velocity.x * (origin.x - shieldPosition.x) + velocity.z * (origin.z - shieldPosition.z));
            double c = (shieldPosition - origin).sqrMagnitude - radiusSq;
            double det = b * b - 4 * a * c;
            if (det < 0)
            {
                return curPosition;
            }
            float scalar = (float)(2 * c / (-b + Math.Sqrt(det)));
            return velocity * scalar + origin;
        }
    }
}
