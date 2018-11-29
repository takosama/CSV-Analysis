using System;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Collections;
using System.Collections.Generic;
namespace ConsoleApp35
{
    public unsafe class MyStr
    {

        public MyStr()
        {
            uint* arr = stackalloc uint[4];
        }

        volatile string str = "aaaa,123,1234,bbbbbbbbbbbbbbbbbbbasdaadsaf0010bb,0\r\naaaab,1234,12345,abbbb,1\n";
        volatile int[] tmp = new int[100];
        //99
        [BenchmarkDotNet.Attributes.Benchmark]
        public void GetControlIndex()
        {
            var span = str.AsSpan();
            int index = 0;
            int i = 0;
            while (true)
            {
                int cnt = GetControlIndex(span, index);
                if (cnt == -1)
                    break;
                index += cnt;
                tmp[i] = index;
                i++;
                index++;
            }
        }
        //24
        [BenchmarkDotNet.Attributes.Benchmark]
        public void GetControlIndexSIMD()
        {
            var span = str.AsSpan();
            fixed (char* p = &span.GetPinnableReference())
            {
                int index = 0;
                int i = 0;
                while (true)
                {
                    int cnt = GetControlIndexSIMD(p + index);
                    if (cnt == -1)
                        break;
                    index += cnt;
                    tmp[i] = index;
                    i++;
                    index++;
                }
            }
        }
        


        unsafe int GetControlIndex(ReadOnlySpan<char> c, int index)
        {
            int i = 0;
            while (true)
            {
                int pos = i + index;

                if (c.Length == pos)
                    return -1;
                if (c[pos] == ',' || c[pos] == '\n')
                    return i;
                i++;
            }
        }

        Vector128<sbyte> _koron = Sse2.SetAllVector128((sbyte)',');
        Vector128<sbyte> _lf = Sse2.SetAllVector128((sbyte)'\n');
        Vector128<sbyte> _0 = Sse2.SetAllVector128((sbyte)'\0');

        Vector128<sbyte> maskMove0 = Sse2.SetVector128(-1, -1, -1, -1, -1, -1, -1, -1, 14, 12, 10, 8, 6, 4, 2, 0);
        //24ns
        unsafe int GetControlIndexSIMD(char* c)
        {
            int cnt = 0;
        start:
            var str = Sse2.LoadVector128((sbyte*)c);

            str = Ssse3.Shuffle(str, maskMove0);

            var position = Sse2.Or(Sse2.CompareEqual(str, _koron), Sse2.CompareEqual(str, _lf));

            var mask = Sse2.MoveMask(position);
            var mask0 = Sse2.MoveMask(Sse2.CompareEqual(str, _0));

            int n = System.Runtime.Intrinsics.X86.Popcnt.PopCount((uint)((~mask) & (mask - 1)));
            if (n > 8) n = 8;
            int m = System.Runtime.Intrinsics.X86.Popcnt.PopCount((uint)((~mask0) & (mask0 - 1)));

            if (m < n)
                return -1;

            if (mask == 0)
            {
                cnt++;
                c += 16;
                goto start;
            }
            return cnt * 8 + n;
        }
    }


    class Program
    {

        

        unsafe static void Main(string[] args)
        {

            //  BenchmarkDotNet.Running.BenchmarkRunner.Run<MyStr>();


            new MyStr().GetControlIndexSIMD();//.と\nの位置を解析する(simd)
            new MyStr().GetControlIndex();//.と\nの位置を解析する(普通)
            //普通99ns simd24ns



            Console.ReadLine();
            //int n = Parse(arr,out int e);
        }
    }
   
 
}
