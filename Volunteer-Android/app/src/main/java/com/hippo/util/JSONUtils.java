package com.hippo.util;

import org.json.JSONObject;
import org.json.JSONArray;

public final class JSONUtils {

    /**
     * Will not return null, return []
     */
    public static String[] getStringArray(JSONObject jo, String name) {
        try {
            JSONArray ja = jo.getJSONArray(name);
            int length = ja.length();
            String[] result = new String[length];
            for (int i = 0; i < length; i++) {
                result[i] = ja.getString(i);
            }
            return result;
        } catch (Throwable e) {
            return new String[0];
        }
    }

}
