using System;
using System.Diagnostics;

namespace RagnarokInfo
{
    class Calculator
    {
        public class Level
        {
            public long current { get; set; }
            public long current_level { get; set; }
            public long required { get; set; }
            public bool leveled { get; set; }

            public Level()
            {
                current = 0;
                current_level = 0;
                required = 0;
                leveled = false;
            }
        }

        public static bool logged { get; set; }
        public static int account { get; set; }
        public static String name { get; set; }
        public static Level base_level;
        public static Level job_level;

        public Calculator()
        {
            account = 0;
            name = "";
            base_level = new Level();
            job_level = new Level();
        }

        public static void setCharacterValues(Exp_template character_level_type, Level l)
        {
            character_level_type.actual = l.current;
            character_level_type.remaining = l.required - l.current;
            character_level_type.percent = ((double)character_level_type.actual / l.required) * 100;
        }

        public void calcExp(Character_Info character, Stopwatch stopWatch, ref double elapsed, ref bool firstRun, ref bool refreshOnNextLog, ref bool startNew)
        {
            setCharacterValues(character.Base, base_level);
            setCharacterValues(character.Job, job_level);

            if (logged == false && account != character.Account)
            {
                clearMem();
                ReadInfo(true);
                return;
            }
            else if (logged == false)
            {
                if (name == character.Name && name != "")
                {
                    stopWatch.Stop();
                    return;
                }
                else if (name == "")
                    firstRun = true;
                refreshOnNextLog = true;
                ReadInfo(true);
                return;
            }

            if (startNew)
            {
                character.Base.previous_value = character.Base.initial;
                character.Job.previous_value = character.Job.initial;
                startNew = false;
            }
            else if (refreshOnNextLog)
            {
                ReadInfo(true);
                refreshOnNextLog = false;
                return;
            }
            else if (character.Base.gained > 0 || character.Job.gained > 0)
                stopWatch.Start();

            checkLeveled(character.Base, base_level);
            checkLeveled(character.Job, job_level);

            if (character.Base.gained != 0 || character.Job.gained != 0)
            {
                if (stopWatch.ElapsedMilliseconds == 0)
                {
                    stopWatch.Start();
                    System.Threading.Thread.Sleep(1);
                }
            }
            else
                return;

            double elapsedMilliseconds = Math.Max(0, stopWatch.ElapsedMilliseconds);
            elapsed = 3600000.0 / elapsedMilliseconds;
            character.Base.hour = character.Base.gained * elapsed;
            character.Job.hour = character.Job.gained * elapsed;
            character.Base.level_required = base_level.required;
            character.Job.level_required = job_level.required;
        }

        public void checkLeveled(Exp_template exp, Level l)
        {
            if (l.current_level == (exp.level_initial + 1))
            {
                exp.gained += (exp.remaining - exp.initial - exp.gained + exp.previous_gained + l.current);
                exp.previous_value = exp.initial = l.current;
                exp.level_initial = l.current_level;
                exp.remaining = l.required;
                l.leveled = true;
            }

            if (!l.leveled && (l.current - exp.previous_value) != 0)
            {
                exp.gained += l.current - exp.previous_value;
                exp.previous_value = l.current;
            }
            else if (l.leveled)
            {
                exp.previous_gained = exp.gained;
            }
        }
    }
}
