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
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package com.hippo.widget;

import android.annotation.TargetApi;
import android.content.Context;
import android.content.res.ColorStateList;
import android.content.res.TypedArray;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.RectF;
import android.graphics.drawable.Drawable;
import android.graphics.drawable.GradientDrawable;
import android.os.Build;
import android.util.AttributeSet;
import android.view.View;

import com.hippo.util.UiUtils;
import com.hippo.util.ViewUtils;

import org.volq.volunteer.R;

public class FloatingActionButton extends View {

    private static final int DRAWABLE_WIDTH = UiUtils.dp2pix(24);

    private GasketDrawer mGasketDrawer;
    private Drawable mDrawable;

    public FloatingActionButton(Context context, AttributeSet attrs) {
        super(context, attrs);
        init(context, attrs, 0, 0);
    }

    public FloatingActionButton(Context context, AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        init(context, attrs, defStyleAttr, 0);
    }

    @TargetApi(Build.VERSION_CODES.LOLLIPOP)
    public FloatingActionButton(Context context, AttributeSet attrs, int defStyleAttr, int defStyleRes) {
        super(context, attrs, defStyleAttr, defStyleRes);
        init(context, attrs, defStyleAttr, defStyleRes);
    }

    private void init(Context context, AttributeSet attrs, int defStyleAttr, int defStyleRes) {
        TypedArray a;

        a = context.obtainStyledAttributes(attrs, R.styleable.FloatingActionButton, defStyleAttr, defStyleRes);
        int bgColor = a.getColor(R.styleable.FloatingActionButton_fabColor, Color.BLACK);
        mDrawable = a.getDrawable(R.styleable.FloatingActionButton_fabDrawable);
        a.recycle();

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
            mGasketDrawer = new GasketDrawerLollipop(this, bgColor);
        } else {
            mGasketDrawer = new GasketDrawerOld(this, bgColor);
        }
    }

    @Override
    protected void drawableStateChanged() {
        super.drawableStateChanged();

        int[] stateSet = getDrawableState();
        mGasketDrawer.onStateChange(stateSet);
    }

    @Override
    protected void onMeasure(int widthMeasureSpec, int heightMeasureSpec) {
        setMeasuredDimension(ViewUtils.getSuitableSize(getSuggestedMinimumWidth(), widthMeasureSpec),
                ViewUtils.getSuitableSize(getSuggestedMinimumHeight(), heightMeasureSpec));
    }

    protected void onDraw(Canvas canvas) {
        mGasketDrawer.draw(canvas);

        if (mDrawable != null) {
            int left;
            int top;
            int right;
            int bottom;
            int width = getWidth();
            int height = getHeight();
            int halfDrawableWidth = DRAWABLE_WIDTH / 2;
            left = width / 2 - halfDrawableWidth;
            top = height / 2 - halfDrawableWidth;
            right = left + DRAWABLE_WIDTH;
            bottom = top + DRAWABLE_WIDTH;
            mDrawable.setBounds(left, top, right, bottom);
            mDrawable.draw(canvas);
        }
    }

    public void setColor(int color) {
        mGasketDrawer.setColor(color);
    }

    public void setDrawable(int resId) {
        mDrawable = getContext().getResources().getDrawable(resId);
        invalidate();
    }

    public void setDrawable(Drawable drawable) {
        mDrawable = drawable;
        invalidate();
    }

    interface GasketDrawer {
        void onStateChange(int[] stateSet);

        void draw(Canvas canvas);

        void setColor(int color);
    }


    static class GasketDrawerOld implements GasketDrawer {

        private static final int SHADOW_RADIOUS = UiUtils.dp2pix(3);
        private static final int SHADOW_OFFSET_Y = UiUtils.dp2pix(1);
        private static final int GASKET_PADDING = SHADOW_RADIOUS + SHADOW_OFFSET_Y; // 4dp
        private static final int SHADOW_COLOR = 0x43000000;

        private View mView;

        private int mColor;
        private int mDarkerColor;
        private Paint mPaint;

        private RectF mBounds;

        private boolean mIsDark;

        private GasketDrawerOld(View view, int color) {
            mView = view;
            mBounds = new RectF();
            mColor = color;
            mDarkerColor = UiUtils.getDarkerColor(color);
            mPaint = new Paint(Paint.ANTI_ALIAS_FLAG | Paint.DITHER_FLAG);
            mPaint.setColor(mColor);
            mPaint.setStyle(Paint.Style.FILL);

            mPaint.setShadowLayer(SHADOW_RADIOUS, 0, SHADOW_OFFSET_Y, SHADOW_COLOR);
            ViewUtils.removeHardwareAccelerationSupport(mView);
        }

        public void onStateChange(int[] stateSet) {
            boolean enabled = false;
            boolean pressed = false;
            boolean focused = false;

            for (int state : stateSet) {
                if (state == android.R.attr.state_enabled) {
                    enabled = true;
                }
                if (state == android.R.attr.state_focused) {
                    focused = true;
                }
                if (state == android.R.attr.state_pressed) {
                    pressed = true;
                }
            }

            setDark(!enabled || pressed || focused);
        }

        private void setDark(boolean isDark) {
            if (mIsDark != isDark) {
                mIsDark = isDark;
                updateColor();
            }
        }

        private void updateColor() {
            mPaint.setColor(mIsDark ? mDarkerColor : mColor);
            invalidateSelf();
        }

        private void invalidateSelf() {
            mView.invalidate();
        }

        public void draw(Canvas canvas) {
            mBounds.left = GASKET_PADDING;
            mBounds.top = GASKET_PADDING;
            mBounds.right = mView.getWidth() - GASKET_PADDING;
            mBounds.bottom = mView.getHeight() - GASKET_PADDING;
            canvas.drawOval(mBounds, mPaint);
        }

        public void setColor(int color) {
            mColor = color;
            mDarkerColor = UiUtils.getDarkerColor(color);
            updateColor();
        }
    }

    static class GasketDrawerLollipop implements GasketDrawer {

        private static final int ELEVATION = UiUtils.dp2pix(5);

        private static final int[][] STATES = new int[][]{
                new int[]{-android.R.attr.state_enabled},
                new int[]{android.R.attr.state_pressed},
                new int[]{android.R.attr.state_focused},
                new int[]{}
        };

        private View mView;
        private int mColor;

        private GradientDrawable mGradientDrawable;

        @TargetApi(Build.VERSION_CODES.LOLLIPOP)
        public GasketDrawerLollipop(View view, int color) {
            mView = view;
            mColor = color;

            int darkColor = UiUtils.getDarkerColor(color);
            mGradientDrawable = new GradientDrawable();
            mGradientDrawable.setShape(GradientDrawable.OVAL);
            mGradientDrawable.setColor(new ColorStateList(STATES,
                    new int[] {darkColor, darkColor, darkColor, color}));

            mView.setBackground(mGradientDrawable);
            mView.setElevation(ELEVATION);
        }

        @Override
        public void onStateChange(int[] stateSet) {
            // Empty
        }

        @Override
        public void draw(Canvas canvas) {
            // Empty
        }

        @TargetApi(Build.VERSION_CODES.LOLLIPOP)
        @Override
        public void setColor(int color) {
            if (mColor != color) {
                mColor = color;
                int darkColor = UiUtils.getDarkerColor(color);
                mGradientDrawable.setColor(new ColorStateList(STATES,
                        new int[] {darkColor, darkColor, darkColor, color}));
            }
        }
    }
}
