/*
 * Copyright (C) 2015 Hippo Seven
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

package com.hippo.effect.ripple;

import android.annotation.TargetApi;
import android.content.res.ColorStateList;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.graphics.drawable.Drawable;
import android.graphics.drawable.RippleDrawable;
import android.os.Build;
import android.view.View;


public final class RippleSalon {

    private static final boolean USE_OLD_RIPPLE =
            Build.VERSION.SDK_INT < Build.VERSION_CODES.LOLLIPOP;

    private static final Drawable MASK = new ColorDrawable(Color.BLACK);

    @TargetApi(Build.VERSION_CODES.LOLLIPOP)
    public static void addRipple(View v, ColorStateList color) {
        addRipple(v, color, v.getBackground());
    }

    @SuppressWarnings("deprecation")
    @TargetApi(Build.VERSION_CODES.LOLLIPOP)
    public static void addRipple(View v, ColorStateList color, Drawable content) {
        if (USE_OLD_RIPPLE) {
            RippleOldDrawable rippleOldDrawable = new RippleOldDrawable(color, content);
            v.setOnTouchListener(new RippleOldTouchHelper(rippleOldDrawable));
            v.setBackgroundDrawable(rippleOldDrawable);
        } else {
            RippleDrawable rippleDrawable = new RippleDrawable(color, content, MASK);
            v.setOnTouchListener(new RippleTouchHelper(rippleDrawable));
            v.setBackgroundDrawable(rippleDrawable);
        }
    }

}
