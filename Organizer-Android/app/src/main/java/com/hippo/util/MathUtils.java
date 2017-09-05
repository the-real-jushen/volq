/*
 * Copyright (C) 2014-2015 Hippo Seven
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package com.hippo.util;

import java.util.Random;

// Get code from android.util.MathUtils
public final class MathUtils {
    private static final Random sRandom = new Random();
    private static final float DEG_TO_RAD = 3.1415926f / 180.0f;
    private static final float RAD_TO_DEG = 180.0f / 3.1415926f;

    public static float abs(float v) {
        return v > 0 ? v : -v;
    }

    public static float log(float a) {
        return (float) Math.log(a);
    }

    public static float exp(float a) {
        return (float) Math.exp(a);
    }

    public static float pow(float a, float b) {
        return (float) Math.pow(a, b);
    }

    public static float max(float a, float b) {
        return a > b ? a : b;
    }

    public static float max(int a, int b) {
        return a > b ? a : b;
    }

    public static float max(float a, float b, float c) {
        return a > b ? (a > c ? a : c) : (b > c ? b : c);
    }

    public static float max(int a, int b, int c) {
        return a > b ? (a > c ? a : c) : (b > c ? b : c);
    }

    public static float max(float... arg) {
        int length = arg.length;
        if (length <= 0) {
            throw new IllegalArgumentException("Empty argument");
        } else {
            float n = arg[0];
            float m;
            for (int i = 1; i < length; i++) {
                m = arg[i];
                if (m > n)
                    n = m;
            }
            return n;
        }
    }

    public static int max(int... arg) {
        int length = arg.length;
        if (length <= 0) {
            throw new IllegalArgumentException("Empty argument");
        } else {
            int n = arg[0];
            int m;
            for (int i = 1; i < length; i++) {
                m = arg[i];
                if (m > n)
                    n = m;
            }
            return n;
        }
    }

    public static float min(float a, float b) {
        return a < b ? a : b;
    }

    public static float min(int a, int b) {
        return a < b ? a : b;
    }

    public static float min(float a, float b, float c) {
        return a < b ? (a < c ? a : c) : (b < c ? b : c);
    }

    public static float min(int a, int b, int c) {
        return a < b ? (a < c ? a : c) : (b < c ? b : c);
    }

    public static float min(float... arg) {
        int length = arg.length;
        if (length <= 0) {
            throw new IllegalArgumentException("Empty argument");
        } else {
            float n = arg[0];
            float m;
            for (int i = 1; i < length; i++) {
                m = arg[i];
                if (m < n)
                    n = m;
            }
            return n;
        }
    }

    public static int min(int... arg) {
        int length = arg.length;
        if (length <= 0) {
            throw new IllegalArgumentException("Empty argument");
        } else {
            int n = arg[0];
            int m;
            for (int i = 1; i < length; i++) {
                m = arg[i];
                if (m < n)
                    n = m;
            }
            return n;
        }
    }

    public static float dist(float x1, float y1, float x2, float y2) {
        final float x = (x2 - x1);
        final float y = (y2 - y1);
        return (float) Math.hypot(x, y);
    }

    public static float dist(float x1, float y1, float z1, float x2, float y2, float z2) {
        final float x = (x2 - x1);
        final float y = (y2 - y1);
        final float z = (z2 - z1);
        return (float) Math.sqrt(x * x + y * y + z * z);
    }

    public static float mag(float a, float b) {
        return (float) Math.hypot(a, b);
    }

    public static float mag(float a, float b, float c) {
        return (float) Math.sqrt(a * a + b * b + c * c);
    }

    public static float sq(float v) {
        return v * v;
    }

    public static float radians(float degrees) {
        return degrees * DEG_TO_RAD;
    }

    public static float degrees(float radians) {
        return radians * RAD_TO_DEG;
    }

    public static float acos(float value) {
        return (float) Math.acos(value);
    }

    public static float asin(float value) {
        return (float) Math.asin(value);
    }

    public static float atan(float value) {
        return (float) Math.atan(value);
    }

    public static float atan2(float a, float b) {
        return (float) Math.atan2(a, b);
    }

    public static float tan(float angle) {
        return (float) Math.tan(angle);
    }

    public static int lerp(int start, int stop, float amount) {
        return start + (int) ((stop - start) * amount);
    }

    public static float lerp(float start, float stop, float amount) {
        return start + (stop - start) * amount;
    }

    public static float norm(float start, float stop, float value) {
        return (value - start) / (stop - start);
    }

    public static float map(float minStart, float minStop, float maxStart, float maxStop, float value) {
        return maxStart + (maxStart - maxStop) * ((value - minStart) / (minStop - minStart));
    }

    /**
     * Returns the input value x clamped to the range [min, max].
     *
     * @param x
     * @param min
     * @param max
     * @return
     */
    public static int clamp(int x, int min, int max) {
        if (x > max) return max;
        if (x < min) return min;
        return x;
    }

    /**
     * Returns the input value x clamped to the range [min, max].
     *
     * @param x
     * @param min
     * @param max
     * @return
     */
    public static float clamp(float x, float min, float max) {
        if (x > max) return max;
        if (x < min) return min;
        return x;
    }

    /**
     * Returns the input value x clamped to the range [min, max].
     * @param x
     * @param min
     * @param max
     * @return
     */
    public static long clamp(long x, long min, long max) {
        if (x > max) return max;
        if (x < min) return min;
        return x;
    }

    /**
     * Returns the next power of two.
     * Returns the input if it is already power of 2.
     * Throws IllegalArgumentException if the input is <= 0 or
     * the answer overflows.
     *
     * @param n
     * @return
     */
    public static int nextPowerOf2(int n) {
        if (n <= 0 || n > (1 << 30)) throw new IllegalArgumentException("n is invalid: " + n);
        n -= 1;
        n |= n >> 16;
        n |= n >> 8;
        n |= n >> 4;
        n |= n >> 2;
        n |= n >> 1;
        return n + 1;
    }

    /**
     * Return is a and b is in the same side to standard.
     * If one of a and b equals standard, return true.
     *
     * @param a
     * @param b
     * @param standard
     * @return
     */
    public static boolean isSameSide(float a, float b, float standard) {
        return (a >= standard && b >= standard) || (a <= standard && b <= standard);
    }

    /*
    public static boolean closer(float a, float b, float standard) {
        float offset1 = standard - a;
        float offset2 = standard - b;
        if ((offset1 >= 0 && offset2 >= 0) ||
                (offset1 >= 0 && offset2 >= 0)) {
            return Math.abs(offset1) < Math.abs(offset2);
        } else {
            float aa = a / standard;
            float bb = b / standard;
            aa = aa > 1.0f ? 1 / aa : aa;
            bb = bb > 1.0f ? 1 / bb : bb;
            return aa > bb;
        }
    }*/

    /**
     * div(3, 35) = 1
     */
    public static int div(int a, int b) {
        return (a + b - 1) /b;
    }

    public static long div(long a, long b) {
        return (a + b - 1) /b;
    }

    public static int random(int howbig) {
        return (int) (sRandom.nextFloat() * howbig);
    }

    public static int random(int howsmall, int howbig) {
        if (howsmall >= howbig)
            return howsmall;
        return (int) (sRandom.nextFloat() * (howbig - howsmall) + howsmall);
    }

    public static float random(float howbig) {
        return sRandom.nextFloat() * howbig;
    }

    public static float random(float howsmall, float howbig) {
        if (howsmall >= howbig)
            return howsmall;
        return sRandom.nextFloat() * (howbig - howsmall) + howsmall;
    }

    public static void randomSeed(long seed) {
        sRandom.setSeed(seed);
    }
}
