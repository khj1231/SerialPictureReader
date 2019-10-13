using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FormTTP
{
    public class MyMath
    {
        /************************************************************************
        * Adaptive thresholding
        */
        int width, height;
       public MyMath(int _width, int _height)
        {
            width = _width;
            height = _height;
        }

        /// <summary>
        /// 根据算法，得出阈值
        /// </summary>
        /// <param name="receData">像素点的数组</param>
        /// <returns>返回的阈值</returns>
        public int threshold(byte[] receData)
        {
            int numPixels = width * height;
            int i;
            // Calculate histogram
            //const int HISTOGRAM_SIZE = 256;
            int[] histogram = new int[256];
            long sum = 0;
            long sumB = 0;
            long q1 = 0, q2;
            int length;
            double max = 0;
            int threshold = 0;
            double m1, m2, m1m2, variance;
            int value;
            length = numPixels;
            while (true)
            {
                if (--length < 0)
                    break;
                value = receData[length];
                histogram[value] += 1;
            }

            // Calculate weighted sum of histogram values
            // 计算直方图

            for (i = 0; i < 256; ++i)
            {
                sum += i * histogram[i];
            }

            // Compute threshold
            // 计算阈值

            for (i = 0; i < 256; ++i)
            {
                // Weighted background
                q1 += histogram[i];
                if (q1 == 0)
                    continue;

                // Weighted foreground
                q2 = numPixels - q1;
                if (q2 == 0)
                    break;

                sumB += i * histogram[i];
                m1 = (double)sumB / q1;
                m2 = ((double)sum - sumB) / q2;
                m1m2 = m1 - m2;
                variance = m1m2 * m1m2 * q1 * q2;
                // 方差最大时返回当前像素值
                if (variance >= max)
                {
                    threshold = i;
                    max = variance;
                }
            }
            return threshold;
        }

    }
}
