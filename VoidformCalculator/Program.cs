using System.IO;

namespace VoidformCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            Priest Reldarus = new Priest(2251, 2149, 3);
            if (File.Exists(@"C:\Temp\Voidform.csv"))
            {
                File.Delete(@"C:\Temp\Voidform.csv");
            }
            using (StreamWriter sw = new StreamWriter(@"C:\Temp\Voidform.csv", true))
            {
                sw.WriteLine("Time,DamageMultiplier,TempVoidHaste,TempLingeringHaste,TempChorusCrit");
                while (Reldarus.TimePassed < 240)
                {
                    sw.WriteLine(Reldarus.ToString());
                    Reldarus.NextSecond();
                }
            }
        }
    }
}
