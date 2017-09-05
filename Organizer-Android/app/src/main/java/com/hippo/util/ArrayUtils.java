package com.hippo.util;

import java.util.List;

public class ArrayUtils {

    public static String[] copyOf(String[] strArray) {
        if (strArray == null) {
            return null;
        }

        int length = strArray.length;
        String[] result = new String[length];
        for (int i = 0; i < length; i++) {
            result[i] = strArray[i];
        }
        return result;
    }

    public static <E> void addAll(List<E> list, E[] array) {
        if (list == null || array == null) {
            return;
        }

        int length = array.length;
        for (int i = 0; i < length; i++) {
            list.add(array[i]);
        }
    }

    public static <E> boolean contain(E[] array, E e) {
        int length = array.length;
        for (int i = 0; i < length; i++) {
            if (array[i].equals(e)) {
                return true;
            }
        }
        return false;
    }

}
