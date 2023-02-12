using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RagnarokInfo
{
    public class Exp_template
    {
        public int max { get; set; }
        public long actual { get; set; }
        public long initial { get; set; }
        public long gained { get; set; }
        public int level_initial { get; set; }
        public long level_required { get; set; }
        public long previous_value { get; set; }
        public long previous_gained { get; set; }
        public long remaining { get; set; }
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
        public int max { get; set; }
        public String Name { get; set; }
        public int Out { get; set; }
        public int Loyalty { get; set; }
        public long Exp { get; set; }
        public long Exp_Required { get; set; }
        public int Hunger { get; set; }
        public int Hunger_Initial { get; set; }
        public int Beep { get; set; }

        public Homunculus(int level, int beep)
        {
            max = level;
            Name = "";
            Out = 0;
            Loyalty = 0;
            Exp = 0;
            Exp_Required = 0;
            Hunger = 0;
            Hunger_Initial = 0;
            Beep = beep;
        }

    }

    public class Pet
    {
        public String Name { get; set; }
        public int Out { get; set; }
        public int Loyalty { get; set; }
        public int Hunger { get; set; }
        public int Hunger_Initial { get; set; }
        public int Beep { get; set; }

        public Pet(int beep)
        {
            Name = "";
            Out = 0;
            Loyalty = 0;
            Hunger = 0;
            Hunger_Initial = 0;
            Beep = beep;
        }
    }

    public class Character_Info
    {
        public int Account { get; set; }
        public String Name { get; set; }
        public bool Logged_In { get; set; }
        public Exp_template Base { get; set; }
        public Exp_template Job { get; set; }
        public Homunculus homunculus { get; set; }
        public Pet pet { get; set; }

        public Character_Info()
        {
            Account = 0;
            Name = "";
            Logged_In = false;
            Base = new Exp_template(185);
            Job = new Exp_template(65);
            homunculus = new Homunculus(185, 12);
            pet = new Pet(76);
        }

        public void set(object[] valuesArray)
        {
            Account = (int)valuesArray[0];
            Logged_In = (bool)valuesArray[1];
            Name = (string)valuesArray[2];

            Base.actual = Base.initial = (long)valuesArray[3];
            Base.level_initial = (int)valuesArray[4];
            Base.remaining = (long)valuesArray[5] - (long)valuesArray[3];
            Base.previous_value = Base.previous_gained = Base.gained = 0;
            Base.percent = Base.hour = 0;

            Job.actual = Job.initial = (long)valuesArray[6];
            Job.level_initial = (int)valuesArray[7];
            Job.remaining = (long)valuesArray[8] - (long)valuesArray[6];
            Job.previous_value = Job.previous_gained = Job.gained = 0;
            Job.percent = Job.hour = 0;
        }
    }
}
