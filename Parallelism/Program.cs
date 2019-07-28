using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Parallelism
{
    class Program
    {
        class StructWrapper
        {
            public LargeStruct innerStruct;

        }
        unsafe struct LargeStruct
        {
            public fixed long data[1000];
        }

        public static int[] MergeSort(Span<int> array)
        {
            int[] left;
            int[] right;
            int[] output = new int[array.Length];
            if (array.Length <= 1)
            {
                return array.ToArray();
            }
            int middle = array.Length / 2;
            left = new int[middle];

            if (array.Length % 2 == 0)
            {
                right = new int[middle];
            }
            else
            {
                right = new int[middle + 1];
            }

            for (int i = 0; i < middle; i++)
            {
                left[i] = array[i];
            }
            int x = 0;

            for (int i = middle; i < array.Length; i++)
            {
                right[x] = array[i];
                x++;
            }
            left = MergeSort(left);
            right = MergeSort(right);
            output = Merge(left, right);
            return output;
        }

        public static int[] Merge(Span<int> left, Span<int> right)
        {
            int outputLength = right.Length + left.Length;
            int[] output = new int[outputLength];
            int indexL = 0;
            int indexR = 0;
            int outputIndex = 0;
            while (indexL < left.Length || indexR < right.Length)
            {
                if (indexL < left.Length && indexR < right.Length)
                {
                    if (left[indexL] <= right[indexR])
                    {
                        output[outputIndex] = left[indexL];
                        indexL++;
                        outputIndex++;
                    }
                    else
                    {
                        output[outputIndex] = right[indexR];
                        indexR++;
                        outputIndex++;
                    }
                }
                else if (indexL < left.Length)
                {
                    output[outputIndex] = left[indexL];
                    indexL++;
                    outputIndex++;
                }
                else if (indexR < right.Length)
                {
                    output[outputIndex] = right[indexR];
                    indexR++;
                    outputIndex++;
                }
            }
            return output;
        }

        static unsafe void Main(string[] args)
        {
            Random random = new Random();
            int[] items = new int[10_000_000];

            for(int i = 0; i < items.Length; i++)
            {
                items[i] = random.Next(0, 500);
            }

            int[] items2 = new int[items.Length];
            int[] items3 = new int[items.Length];

            items.CopyTo(items2.AsSpan());
            items.CopyTo(items3.AsSpan());


            Stopwatch sw = Stopwatch.StartNew();

            Array.Sort(items3);

            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks / (double)Stopwatch.Frequency);

            //Sync
            sw = Stopwatch.StartNew();

            items2 = MergeSort(items2);

            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks / (double)Stopwatch.Frequency);


            //Parallel

            Memory<int>[] splits = new Memory<int>[Environment.ProcessorCount];

            int splitLength = items.Length / splits.Length;
            int rem = items.Length % splits.Length;

            for(int i = 0; i < splits.Length - 1; i++)
            {
                splits[i] = items.AsMemory().Slice(i * splitLength, splitLength);
            }
            splits[splits.Length - 1] = items.AsMemory().Slice((splits.Length - 1) * splitLength, splitLength + rem);


            sw = Stopwatch.StartNew();

            Parallel.For(0, splits.Length, (i) =>
            {
                splits[i] = MergeSort(splits[i].Span);
            });

            for(int i = 0; i < 4; i += 2)
            {
                splits[i] = Merge(splits[i].Span, splits[i + 1].Span);
            }

            for (int i = 0; i < 2; i += 2)
            {
                splits[i] = Merge(splits[i].Span, splits[i + 2].Span);
            }

            splits[0] = Merge(splits[0].Span, splits[4].Span);

            sw.Stop();
            Console.WriteLine(sw.ElapsedTicks / (double)Stopwatch.Frequency);
        }

        static unsafe void MainLoops(string[] args)
        {

            for (int c = 1; c < 10_000_000; c *= 10)
            {
                Console.WriteLine($"C is {c}");
                StructWrapper[] items = new StructWrapper[c];

                Stopwatch sw = Stopwatch.StartNew();

                Parallel.For(0, items.Length, (v) =>
                {
                    items[v] = new StructWrapper();
                    ref LargeStruct ls = ref items[v].innerStruct;
                    for (int i = 0; i < 1000; i++)
                    {
                        ls.data[i] = v;
                    }
                });

                sw.Stop();

                Console.WriteLine(sw.ElapsedTicks / (double)Stopwatch.Frequency);

                items = new StructWrapper[c];

                sw = Stopwatch.StartNew();

                Parallel.ForEach(items, (v) =>
                {
                    //items[v] = new StructWrapper();
                    ref LargeStruct ls = ref v.innerStruct;
                    for (int i = 0; i < 1000; i++)
                    {
                        ls.data[i] = 42;
                    }
                });

                sw.Stop();
                Console.WriteLine(sw.ElapsedTicks / (double)Stopwatch.Frequency);

                items = new StructWrapper[c];
                sw = Stopwatch.StartNew();
                for (int v = 0; v < items.Length; v++)
                {
                    items[v] = new StructWrapper();
                    ref LargeStruct ls = ref items[v].innerStruct;
                    for (int i = 0; i < 1000; i++)
                    {
                        ls.data[i] = v;
                    }
                }

                sw.Stop();

                Console.WriteLine(sw.ElapsedTicks / (double)Stopwatch.Frequency);
            }

            Console.WriteLine("Hello World!");
        }
    }
}
