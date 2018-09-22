using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FindNumbers
{
    class Program
    {
        static double toSearch = 10958;
        static long iter = 0;
        static StreamWriter file;
        static int OpsNum = 9;
        static int OpdNum = 10;

        static void Main(string[] args)
        {
            Console.WriteLine("Number to search:");
            toSearch = double.Parse(Console.ReadLine());

            file = new StreamWriter(File.OpenWrite("output.txt"));
            Op[] operations = new Op[OpsNum]; //between digits
            IterateOperations(operations, 0);

            Console.WriteLine("Completed!");
            file.Close();
            Console.ReadKey();
        }


        static void IterateOperations(Op[] operations, int opIndex)
        {
            //Check combination
            int[] order = new int[OpsNum];
            for (int i = 0; i < OpsNum; i++)
                order[i] = -1;
            int orderIndex = 0;
            IterateBraces(operations, order, orderIndex);

            //Go deeper
            if (opIndex == OpsNum)
                return;

            //Go parallel
            if (opIndex == 0)
            {
                Parallel.For(0, 6, (i) =>
                {
                    Op[] operationsCopy = new Op[OpsNum];
                    Array.Copy(operations, operationsCopy, OpsNum);
                    operationsCopy[opIndex] = (Op)i;
                    IterateOperations(operationsCopy, opIndex + 1);
                });
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    operations[opIndex] = (Op)i;
                    IterateOperations(operations, opIndex + 1);
                }
            }
        }


        static void IterateBraces(Op[] operations, int[] order, int orderIndex)
        {
            //Calculate
            if (orderIndex == OpsNum)
            {
                Calculate(operations, order);
                return;
            }

            //Go deeper
            for (int i = 0; i < OpsNum; i++)
            {
                //if concatenation found, go deeper, because it's required computation
                if (operations[i] == Op.Concat 
                    && order[0] != i && order[1] != i && order[2] != i && order[3] != i && order[4] != i && order[5] != i && order[6] != i && order[7] != i && order[8] != i)
                {
                    order[orderIndex] = i;
                    IterateBraces(operations, order, orderIndex + 1);
                    return;
                }
            }
            for (int i = 0; i < OpsNum; i++)
            {
                //check all other combinations
                if (order[0] != i && order[1] != i && order[2] != i && order[3] != i && order[4] != i && order[5] != i && order[6] != i && order[7] != i  && order[8] != i)
                {
                    order[orderIndex] = i;
                    IterateBraces(operations, order, orderIndex + 1);
                    order[orderIndex] = -1;
                }
            }
        }

        static void Calculate(Op[] operations, int[] order)
        {
            iter++;
            if (iter % 10000000 == 0)
            {
                Console.WriteLine("Iteration: " + iter + ", " + operations.Select(a => a.ToString()).Aggregate((a1, a2) => a1 + a2));
            }

            //Fill array with 1,2,3,4,5,6,7,8,9
            Num[] results = new Num[OpdNum];
            for (int i = 0; i < OpdNum; i++)
                results[i] = new Num(i);

            //For every operation in selected order
            //Calculate it and write result to neighbor cells
            for (int i = 0; i < OpsNum; i++)
            {
                double result = 0;
                double operandOne = results[order[i]].Value;
                double operandTwo = results[order[i] + 1].Value;
                switch (operations[order[i]])
                {
                    case Op.Sum:
                        result = operandOne + operandTwo;
                        break;
                    case Op.Sub:
                        result = operandOne - operandTwo;
                        break;
                    case Op.Mul:
                        result = operandOne * operandTwo;
                        break;
                    case Op.Div:
                        //check for 0 division
                        if (operandTwo == 0)
                            return;
                        result = operandOne / operandTwo;
                        break;
                    case Op.Concat:
                        result = operandOne * 10 + operandTwo;
                        break;
                    case Op.Pow:
                        result = Math.Pow(operandOne, operandTwo);
                        break;
                }

                //if too big
                if (result > int.MaxValue || result < int.MinValue)
                    return;

                results[order[i]].Value = result;
                results[order[i] + 1].Ref = results[order[i]];
            }

            if (results[order[7]].Value == toSearch)
            {
                file.WriteLine("Found! Operations: " + operations.Select(a => a.ToString()).Aggregate((a1, a2) => a1 + a2) +
                    ". Order: " + order.Select(a => a.ToString()).Aggregate((a1, a2) => a1 + a2));
                Console.WriteLine("Found! Operations: " + operations.Select(a => a.ToString()).Aggregate((a1, a2) => a1 + a2) +
                    ". Order: " + order.Select(a => a.ToString()).Aggregate((a1, a2) => a1 + a2));
                file.Flush();
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

        class Num
        {
            double val;
            public Num Ref { get; set; }
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
                val = value;
                Ref = null;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }


    }
}
