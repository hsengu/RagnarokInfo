using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RagnarokInfo
{
    public class Character_Info
    {
        public class Exp_template
        {
            public int max { get; set; }
            public ulong actual { get; set; }
            public ulong initial { get; set; }
            public ulong gained { get; set; }
            public ulong level_initial { get; set; }
            public ulong level_required { get; set; }
            public ulong previous_value { get; set; }
            public ulong previous_gained { get; set; }
            public ulong remaining { get; set; }
            public double hour { get; set; }
            public double percent { get; set; }
            public bool is_max() { return max == (int)level_initial; }

            public Exp_template(int level)
            {
                max = level;
                actual = 0;
                initial = 0;
                gained = 0;
                level_initial = 0;
                level_required = 0;
                previous_value = 0;
                previous_gained = 0;
                remaining = 0;
                hour = 0;
                percent = 0;
            }
        }

        public class Homunculus
        {

        }

        public class Pet
        {

        }

        public int Account { get; set; }
        public String Name { get; set; }
        public bool Logged_In { get; set; }
        public Exp_template Base { get; set; }
        public Exp_template Job { get; set; }

        public Character_Info()
        {
            Account = 0;
            Name = "";
            Logged_In = false;
            Base = new Exp_template(185);
            Job = new Exp_template(65);
        }

        public void set(ulong b_current, ulong j_current, ulong b_lvl_curr, ulong j_lvl_curr, ulong b_req, ulong j_req, bool log, int account, String name)
        {
            Account = account;
            Name = name;
            Logged_In = log;

            Base.actual = Base.initial = b_current;
            Base.level_initial = b_lvl_curr;
            Base.remaining = b_req - Base.actual;
            Base.previous_value = Base.previous_gained = Base.gained = 0;
            Base.percent = Base.hour = 0;

            Job.actual = Job.initial = j_current;
            Job.level_initial = j_lvl_curr;
            Job.remaining = j_req - Job.actual;
            Job.previous_value = Job.previous_gained = Job.gained = 0;
            Job.percent = Job.hour = 0;
        }
    }
}
