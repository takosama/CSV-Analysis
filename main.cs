

using System;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Collections;
using System.Collections.Generic;
namespace ConsoleApp35
{
    public unsafe class MyStr
    {
        Vector128<sbyte>[] maskArray = new Vector128<sbyte>[16];
        public MyStr()
        {
            maskArray[15] = Sse2.SetVector128(-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            maskArray[14] = Sse2.SetVector128(0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            maskArray[13] = Sse2.SetVector128(0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            maskArray[12] = Sse2.SetVector128(0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            maskArray[11] = Sse2.SetVector128(0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            maskArray[10] = Sse2.SetVector128(0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            maskArray[09] = Sse2.SetVector128(0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            maskArray[08] = Sse2.SetVector128(0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0);
            maskArray[07] = Sse2.SetVector128(0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0);
            maskArray[06] = Sse2.SetVector128(0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0);
            maskArray[05] = Sse2.SetVector128(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0);
            maskArray[04] = Sse2.SetVector128(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0);
            maskArray[03] = Sse2.SetVector128(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0);
            maskArray[02] = Sse2.SetVector128(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0);
            maskArray[01] = Sse2.SetVector128(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0);
            maskArray[00] = Sse2.SetVector128(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1);
        }

        volatile string str = "ああaaaa,123,1234,bbbbbbbbbbbbbbbbbbbasdaadsaf0010bbbbbbbbbbbbbbbbbbbbbasdaadsあｓｄｆｇｈｊｋｌ；af0010bb,0\r\naaaab,1234,12345,abbbb,1\n";

        //index が入るはずの配列
        volatile int[] tmpArray = new int[100];
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
                tmpArray[i] = index;
                i++;
                index++;
            }
        }
        //24




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

        [BenchmarkDotNet.Attributes.Benchmark]
        public void GetControlIndexSIMD()
        {
            var span = str.AsSpan();
            fixed (char* p = &span.GetPinnableReference())
            {
                int index = 0;
                int i = 0;
                sbyte* pp = (sbyte*)p;
                while (true)
                {
                    int cnt = GetControlIndexSIMD(pp + index * 2);
                    if (cnt == -1)
                        break;
                    index += cnt;
                    tmpArray[i] = index;
                    i++;
                    index++;
                }
            }
        }


        Vector128<sbyte> _koron = Sse2.SetAllVector128((sbyte)',');
        Vector128<sbyte> _lf = Sse2.SetAllVector128((sbyte)'\n');
        Vector128<sbyte> _0 = Sse2.SetAllVector128((sbyte)'\0');

        Vector128<sbyte> maskMove0 = Sse2.SetVector128(-1, -1, -1, -1, -1, -1, -1, -1, 14, 12, 10, 8, 6, 4, 2, 0);
        Vector128<sbyte> maskMove1 = Sse2.SetVector128(14, 12, 10, 8, 6, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1, -1);


        unsafe int GetControlIndexSIMD(sbyte* c)
        {
            int cnt = 0;
            start:
            var str = Sse2.LoadVector128(c);

             
            str = Ssse3.Shuffle(str, maskMove0);

            var position = Sse2.Or(Sse2.CompareEqual(str, _koron), Sse2.CompareEqual(str, _lf));

            var mask = Sse2.MoveMask(position);
            var mask0 = Sse2.MoveMask(Sse2.CompareEqual(str, _0));


            int n = Popcnt.PopCount((uint)((~mask) & (mask - 1)));
            if (n > 8) n = 8;
            int m = Popcnt.PopCount((uint)((~mask0) & (mask0 - 1)));

            if (m < n)
                return -1;

            if (mask == 0)
            {
                cnt += 8;
                c += 16;
                goto start;
            }

            return cnt + n;
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public unsafe void GetPositionSIMD2()
        {
            var span = str.AsSpan();
            fixed (char* p = &span.GetPinnableReference())
            {
                 GetPositionSIMD2((sbyte*)p, sizeof(char) * span.Length);
            }
        }


        unsafe void GetPositionSIMD2(sbyte* p, int max)
        {
            int i = 0;
            int cnt = 0;

            start:
            var tmp0 = Sse2.LoadVector128(p);
            p += 16;
            var tmp1 = Sse2.LoadVector128(p);
            p += 16;
        
            tmp0 = Ssse3.Shuffle(tmp0, maskMove0);
            tmp1 = Ssse3.Shuffle(tmp1, maskMove1);

            var tmp = Sse2.Add(tmp0, tmp1);
          
            var cmp0 = Sse2.CompareEqual(tmp, _lf);
            var cmp1 = Sse2.CompareEqual(tmp, _koron);
            var cmpControl = Sse2.Add(cmp0, cmp1);
            var cmpZ = Sse2.CompareEqual(tmp, _0);

            var maskControl = Sse2.MoveMask(cmpControl);
            var maskZero = Sse2.MoveMask(cmpZ);

            var posZero = Popcnt.PopCount((uint)((~maskZero) & (maskZero - 1)));

            loop:
            var posControl = Popcnt.PopCount((uint)((~maskControl) & (maskControl - 1)));

            if (posControl == 32)
            {
                cnt += 16;
                if (max <= cnt*2)
                    return;
                else
                    goto start;
            }

            if (posControl > posZero)
                return;

            tmpArray[i] = posControl + cnt;
            i++;

            maskControl &= ~(1 << (posControl));
            goto loop;
        }
    }


    class Program
    {



        unsafe static void Main(string[] args)
        {

          // BenchmarkDotNet.Running.BenchmarkRunner.Run<MyStr>();
           new MyStr().GetPositionSIMD2();

            // new MyStr().GetControlIndexSIMD();//.と\nの位置を解析する(simd)
            //  new MyStr().GetControlIndex();//.と\nの位置を解析する(普通)
            //普通140ns simd11ns  i7 4210
            //普通135ns simd10ns  i5 7200



            //      Console.ReadLine();
            //int n = Parse(arr,out int e);
        }
    }


}
