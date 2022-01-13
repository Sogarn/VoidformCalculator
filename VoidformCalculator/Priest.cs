using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidformCalculator
{
    class Priest
    {
        public int TimePassed { get; set; }
        public int BaseCrit { get; set; }
        public int BaseHaste { get; set; }
        public float BaseRampToVoid { get; set; }
        public float RampToVoid { get; set; } // calculated with function
        public float BaseVoidDuration { get; set; }
        public float VoidformDuration { get; set; } // calculated with function
        public int IncomingStacks { get; set; }
        public float VoidMultiplier { get; set; }
        public float VoidHastePerStack { get; set; }
        public float TempVoidHaste { get; set; }
        public float LingeringHastePerStack { get; set; }
        public float TempLingeringHaste { get; set; }
        public float SecondsToDecay { get; set; }
        public float ChorusCritPerStack { get; set; }
        public float TempChorusCrit { get; set; }
        public float StatsPerPercent { get; set; }
        public bool Voidform { get; set; }

        public Priest(int baseCrit, int baseHaste, int chorusAzeriteStacks)
        {
            BaseCrit = baseCrit;
            BaseHaste = baseHaste;
            // 72 of a secondary stat = 1%
            StatsPerPercent = 72;

            // Default ramp to enter void form
            BaseRampToVoid = 9;
            RampToVoid = CalculateRampToVoid();
            // Default duration of voidform
            BaseVoidDuration = 26;
            VoidformDuration = CalculateVoidDuration();
            IncomingStacks = (int) VoidformDuration;

            VoidMultiplier = 1.25f / 1.1f; // Relative to baseline shadowform
            // Every second, add 0.5% haste while in void form
            VoidHastePerStack = StatsPerPercent * 0.5f;
            TempVoidHaste = 0;
            // Lingering insanity reduces by 1% per 3 seconds
            LingeringHastePerStack = StatsPerPercent;
            TempLingeringHaste = 0;
            // Lingering insanity decay
            SecondsToDecay = 3;
            // Chorus crit per stack accounting for Severe
            ChorusCritPerStack = 28 * chorusAzeriteStacks * 1.36f;
            TempChorusCrit = 0;

            // Start out of void form
            Voidform = false;
        }
        
        // Adjust ramp time based on stats
        public float CalculateRampToVoid()
        {
            // Reduces time to ramp accounting for our increased stats
            // Example with +20% haste +20% crit, 1.2 * 1.2 = 1.44 so our base ramp of 9 is divided by 1.44 to give us 6.25 second ramp
            return (BaseRampToVoid / (AboveBaselineStats()));
        }

        // Adjust voidform duration based on stats
        public float CalculateVoidDuration()
        {
            float currentBuff = AboveBaselineStats();
            // Cap max void form duration at 50% of our bonuses
            // Approximates diminishing returns since lingering insanity and chorus will stop helping you at ~40 stacks anyway
            if (currentBuff > 1.5)
            {
                return (BaseVoidDuration * 1.5f);
            }
            else
            {
                // Increase void form duration with similar formula as above
                return (BaseVoidDuration * currentBuff);
            }
        }

        // Calculate how much more stats we have than baseline to figure out ramp and void form durations
        public float AboveBaselineStats()
        {
            float baseline = (1 + (BaseCrit / StatsPerPercent / 100)) * (1 + (BaseHaste / StatsPerPercent / 100));
            float actual = (1 + ((BaseCrit + TempChorusCrit) / StatsPerPercent / 100)) * (1 + ((BaseHaste + TempVoidHaste + TempLingeringHaste) / StatsPerPercent / 100));
            return (actual / baseline);
        }

        // Incorporate bonus damage from voidform if relevant
        public float AboveBaselineDamage()
        {
            float output = AboveBaselineStats();
            output = Voidform ? output * VoidMultiplier : output;
            return output;
        }

        // Output string
        public override string ToString()
        {
            return (TimePassed + "," + AboveBaselineDamage() + "," + TempVoidHaste + "," + TempLingeringHaste + "," + TempChorusCrit);
        }

        // Get next set of stat changes
        public void NextSecond()
        {
            TimePassed += 1;
            // Voidform logic
            if (Voidform)
            {
                // Reduce void form duration
                VoidformDuration -= 1;
                // Update stats accounting for us in void form
                UpdateTempStatsVoidform();
                // If void form ended
                if (VoidformDuration <= 0)
                {
                    // Remove void form
                    Voidform = false;
                    // Give us chorus of insanity
                    TempChorusCrit = ChorusCritPerStack * IncomingStacks;
                    // Transfer void form haste to lingering insanity
                    TempLingeringHaste = TempVoidHaste;
                    // Remove void form haste
                    TempVoidHaste = 0;
                    // Get new ramp time
                    RampToVoid = CalculateRampToVoid();
                }
            }
            else
            {
                // We're ramping out of void form
                RampToVoid -= 1;
                // Update values
                UpdateTempStats();
                // Void form ready
                if (RampToVoid <= 0)
                {
                    // Enter void form
                    Voidform = true;
                    // Get new voidform duration
                    VoidformDuration = CalculateVoidDuration();
                    // Max duration determines stacks at the end
                    IncomingStacks = Convert.ToInt32(VoidformDuration);
                }
            }
        }

        public void UpdateTempStats()
        {
            // Reduce chorus of insanity stacks
            if (TempChorusCrit > 0)
            {
                TempChorusCrit -= ChorusCritPerStack;
                if (TempChorusCrit < 0)
                {
                    TempChorusCrit = 0;
                }
            }
            // Reduce lingering insanity stacks
            if (TempLingeringHaste > 0)
            {
                SecondsToDecay -= 1;
                if (SecondsToDecay <= 0)
                {
                    SecondsToDecay = 3;
                    TempLingeringHaste -= LingeringHastePerStack;
                    if (TempLingeringHaste < 0)
                    {
                        TempLingeringHaste = 0;
                    }
                }
            }
        }

        // Do above but add void form passive haste stacking too
        public void UpdateTempStatsVoidform()
        {
            UpdateTempStats();
            TempVoidHaste += VoidHastePerStack;
        }
    }
}
