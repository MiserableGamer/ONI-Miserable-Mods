namespace MorningExercise
{
    public static class MorningExerciseStrings
    {
        public static class DUPLICANTS
        {
            public static class CHORES
            {
                public static class EXERCISE
                {
                    public static LocString NAME = "Exercise";
                    public static LocString STATUS = "Exercising";
                    public static LocString TOOLTIP = "This Duplicant is doing their morning exercise routine";
                }

                public static class WAITINGFOREXERCISE
                {
                    public static LocString NAME = "Waiting for Exercise";
                    public static LocString STATUS = "Waiting for exercise equipment";
                    public static LocString TOOLTIP = "This Duplicant is waiting for a Manual Exerciser to become available for their morning exercise";
                }
            }

            public static class MODIFIERS
            {
                public static class WARMUP
                {
                    public static LocString NAME = "Warm Up";
                    public static LocString DESCRIPTION = "This Duplicant has warmed up from their morning exercise and feels more athletic.";
                    public static LocString TOOLTIP = "This Duplicant's morning workout has boosted their Athletics";
                }

                public static class BIONICWARMUP
                {
                    public static LocString NAME = "Bionic Warm Up";
                    public static LocString DESCRIPTION = "This Bionic Duplicant has completed their morning exercise routine and feels energized.";
                    public static LocString TOOLTIP = "This Bionic Duplicant's morning workout has boosted their Morale";
                }
            }

        }

        public static class BUILDINGS
        {
            public static class PREFABS
            {
                public static class MANUALEXERCISER
                {
                    public static LocString NAME = STRINGS.UI.FormatAsLink("Manual Exerciser", "MANUALEXERCISER");
                    public static LocString DESC = "A dedicated exercise machine for Duplicants to use during Morning Exercise schedule blocks.\n\nProvides no power, but allows Duplicants to exercise and gain Athletics or Morale buffs.";
                    public static LocString EFFECT = "Allows Duplicants to exercise during Morning Exercise schedule blocks.";
                }
            }
        }

        public static class UI
        {
            public static class SCHEDULEGROUPS
            {
                public static class MORNINGEXERCISE
                {
                    public const string ID = "MorningExercise";

                    public static LocString NAME = "Morning Exercise";
                    public static LocString DESCRIPTION = "During Morning Exercise time, my Duplicants will find a " +
                        STRINGS.UI.FormatAsLink("Manual Exerciser", "MANUALEXERCISER") +
                        " to use as a treadmill for a quick workout.\n\nAfter exercising, they gain the " +
                        STRINGS.UI.PRE_KEYWORD + "Warm Up" + STRINGS.UI.PST_KEYWORD +
                        " buff which grants +3 Athletics for 6 minutes.";
                    public static LocString NOTIFICATION_TOOLTIP = "During " +
                        STRINGS.UI.PRE_KEYWORD + "Morning Exercise" + STRINGS.UI.PST_KEYWORD +
                        " shifts, my Duplicants will work out on a Manual Exerciser and gain an Athletics buff.";
                }
            }
        }
    }
}

