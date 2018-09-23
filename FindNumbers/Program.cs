using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FindNumbers
{
    class Program
    {
        static double toSearch = 10958;
        static long iter = 0;
        static int OpsNum = 9;
        static int OpdNum = 10;
        static StreamWriter file;
        static bool stop = false;

        static void Main(string[] args)
        {
            Console.WriteLine("Number to search:");
            toSearch = double.Parse(Console.ReadLine());
            file = new StreamWriter(File.OpenWrite("output.txt"));

            //Start indicator
            Task.Run(() =>
            {
                while (!stop)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Iteration: " + iter);
                }
            });

            //Main loop
            Parallel.For(0, 12, (i) =>
            {
                Num[] results = new Num[OpdNum];
                for (int k = 0; k < OpdNum; k++)
                    results[k] = new Num(k);
                int[] order = new int[OpsNum];
                Op[] operations = new Op[OpsNum]; //between digits
                bool[] alreadyInOrder = new bool[OpsNum];

                IterateOperations(operations, 0, results, order, alreadyInOrder, (Op)(i % 2), (Op)(i % 6));
            });

            file.Close();
            Console.WriteLine("Completed!");
            Console.Out.Flush();
            Console.ReadLine();
        }

        static void IterateOperations(Op[] operations, int opIndex, Num[] results, int[] order, bool[] alreadyInOrder, Op firstOp = Op.Sum, Op secondOp = Op.Sum)
        {
            if (stop) return;
            iter++;

            //Check combination
            int i = 0;
            for (i = 0; i < OpsNum; i++)
                alreadyInOrder[i] = false;

            IterateBraces(operations, order, 0, results, alreadyInOrder);

            //Go deeper
            if (opIndex == OpsNum)
                return;

            //Go parallel
            if (opIndex == 0)
            {
                operations[0] = firstOp;
                operations[1] = secondOp;
                IterateOperations(operations, 2, results, order, alreadyInOrder);
            }
            else
            {
                for (i = 0; i < 6; i++)
                {
                    operations[opIndex] = (Op)i;
                    IterateOperations(operations, opIndex + 1, results, order, alreadyInOrder);
                }
            }
        }


        static void IterateBraces(Op[] operations, int[] order, int orderIndex, Num[] results, bool[] alreadyInOrder)
        {
            //Calculate
            if (orderIndex == OpsNum)
            {
                Calculate(operations, order, results);
                return;
            }

            //Go deeper
            int i = 0;
            for (i = 0; i < OpsNum; i++)
            {
                //if concatenation found, go deeper, because it's required computation
                if (operations[i] == Op.Concat && !alreadyInOrder[i])
                {
                    order[orderIndex] = i;
                    alreadyInOrder[i] = true;
                    IterateBraces(operations, order, orderIndex + 1, results, alreadyInOrder);
                    return;
                }
            }
            for (i = 0; i < OpsNum; i++)
            {
                //check all other combinations
                if (!alreadyInOrder[i])
                {
                    order[orderIndex] = i;
                    alreadyInOrder[i] = true;
                    IterateBraces(operations, order, orderIndex + 1, results, alreadyInOrder);
                    alreadyInOrder[i] = false;
                }
            }
        }

        static void Calculate(Op[] operations, int[] order, Num[] results)
        {
            //Fill array with 1,2,3,4,5,6,7,8,9
            int i = 0;
            for (i = 0; i < OpdNum; i++)
                results[i].Clear(i);

            //For every operation in selected order
            //Calculate it and write result to neighbor cells
            for (i = 0; i < OpsNum; i++)
            {
                switch (operations[order[i]])
                {
                    case Op.Sum:
                        results[order[i]].Value = results[order[i]].Value + results[order[i] + 1].Value;
                        break;
                    case Op.Sub:
                        results[order[i]].Value = results[order[i]].Value - results[order[i] + 1].Value;
                        break;
                    case Op.Mul:
                        results[order[i]].Value = results[order[i]].Value * results[order[i] + 1].Value;
                        break;
                    case Op.Div:
                        //check for 0 division
                        if (results[order[i] + 1].Value == 0)
                            return;
                        results[order[i]].Value = results[order[i]].Value / results[order[i] + 1].Value;
                        break;
                    case Op.Concat:
                        results[order[i]].Value = results[order[i]].Value * 10 + results[order[i] + 1].Value;
                        break;
                    case Op.Pow:
                        /*if (results[order[i]].Value % 1 != 0 || results[order[i] + 1].Value % 1 != 0)
                            return;*/
                        results[order[i]].Value = Math.Pow(results[order[i]].Value, results[order[i] + 1].Value);
                        break;
                }

                //if too big
                /*if (results[order[i]].Value > int.MaxValue || results[order[i]].Value < int.MinValue)
                    return;*/
                    
                results[order[i] + 1].Ref = results[order[i]];
            }

            if (results[order[7]].Value == toSearch)
            {
                //Print result
                string output = "Number: " + toSearch + ". Operations: " + op(operations[0]) + 1 + op(operations[1]) + 2 + op(operations[2]) + 3 + op(operations[3])
                    + 4 + op(operations[4]) + 5 + op(operations[5]) + 6 + op(operations[6]) + 7 + op(operations[7]) + 8 + op(operations[8]) +
                    ". Order: " + order.Select(a => a.ToString()).Aggregate((a1, a2) => a1 + "-" + a2);
                Console.WriteLine(output);
                file.WriteLine(output);
                file.Flush();
                //stop = true;
            }
        }

        enum Op
        {
            Sum = 0,
            Sub,
            Mul,
            Div,
            Pow,
            Concat
        }

        static string op(Op value)
        {
            switch (value)
            {
                case Op.Sum:
                    return "+";
                case Op.Sub:
                    return "-";
                case Op.Mul:
                    return "*";
                case Op.Div:
                    return "/";
                case Op.Pow:
                    return "^";
                case Op.Concat:
                    return "";
            }
            return "";
        }

        class Num
        {
            double val;
            public Num Ref;
            public double Value
            {
                get
                {
                    if (Ref != null)
                        return Ref.Value;
                    else
                        return val;
                }
                set
                {
                    if (Ref != null)
                        Ref.Value = value;
                    else
                        val = value;
                }
            }

            public Num(double value)
            {
                Clear(value);
            }

            public void Clear(double value)
            {
                Ref = null;
                val = value;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }


    }
}
