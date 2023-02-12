using System;
using System.Diagnostics;

namespace RagnarokInfo
{
    class Calculator
    {
        public class Level
        {
            public long current { get; set; }
            public int current_level { get; set; }
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

        public bool logged { get; set; }
        public int account { get; set; }
        public String name { get; set; }
        public Level base_level { get; set; }
        public Level job_level { get; set; }

        public Calculator()
        {
            account = 0;
            name = "";
            base_level = new Level();
            job_level = new Level();
        }

        public void setCharacterValues(ref Character_Info character, Calculator calc)
        {
            calc.base_level.leveled = false;
            character.Base.actual = calc.base_level.current;
            character.Base.remaining = calc.base_level.required - calc.base_level.current;
            character.Base.percent = ((double)character.Base.actual / calc.base_level.required) * 100;

            calc.job_level.leveled = false;
            character.Job.actual = calc.job_level.current;
            character.Job.remaining = calc.job_level.required - calc.job_level.current;
            character.Job.percent = ((double)character.Job.actual / calc.job_level.required) * 100;
        }

        public void time(Exp_template exp, Stopwatch stopWatch, double elapsed)
        {
            double elapsedMilliseconds = Math.Max(0, stopWatch.ElapsedMilliseconds);
            elapsed = 3600000.0 / elapsedMilliseconds;
            exp.hour = exp.gained * elapsed;
            exp.level_required = base_level.required;
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

        public void setLevels(object[] values)
        {
            account = (int)values[0];
            logged = (bool)values[1];
            name = (String)values[2];

            base_level.current = (long)values[3];
            base_level.current_level = (int)values[4];
            base_level.required = (long)values[5];

            job_level.current = (long)values[6];
            job_level.current_level = (int)values[7];
            job_level.required = (long)values[8];
        }
    }
}
