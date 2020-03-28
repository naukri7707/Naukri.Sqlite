using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NUnitTest
{
    public static class Evaluation
    {
        public static Action<string> LogEvent;

        public static long Test(Action testFunc, bool log = true)
        {
            var sw = new Stopwatch();
            sw.Start();
            testFunc();
            sw.Stop();
            if (log)
            {
                LogEvent(sw.ElapsedMilliseconds.ToString());
            }
            return sw.ElapsedMilliseconds;
        }

        public static long Test(int loopTime, Action testFunc, bool log = true)
        {
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < loopTime; i++)
            {
                testFunc();
            }
            sw.Stop();
            if (log)
            {
                LogEvent(sw.ElapsedMilliseconds.ToString());
            }
            return sw.ElapsedMilliseconds;
        }

        public static long Test(int loopTime, int testTime,Action testFunc, bool log = true)
        {
            byte flag = 0;
            long res = 0L;
            decimal resM = 0M;
            double resD = 0D;
            for (int i = 0; i < testTime; i++)
            {
                long time = Test(loopTime, testFunc, false);
                switch (flag)
                {
                    case 0:
                        if (res < res + time)
                        {
                            res += time;
                        }
                        else
                        {
                            flag = 1;
                            resM = (decimal)res + time;
                        }
                        break;
                    case 1:
                        if (flag == 1 && resM < resM + time)
                        {
                            resM += time;
                        }
                        else
                        {
                            flag = 2;
                            resD = (double)resM + time;
                        }
                        break;
                    default:
                        resD += time;
                        break;
                }
            }
            if (flag == 0)
            {
                res /= testTime;
            }
            else if (flag == 1)
            {
                res = (long)(resM / testTime);
            }
            else
            {
                res = (long)(resD / testTime);
            }
            if (log)
            {
                LogEvent(res.ToString());
            }
            return res;
        }
    }
}
