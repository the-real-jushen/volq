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

package org.volq.volunteer.widget;

import android.graphics.Canvas;
import android.graphics.ColorFilter;
import android.graphics.Paint;
import android.graphics.Path;
import android.graphics.PixelFormat;
import android.graphics.Rect;
import android.graphics.drawable.Drawable;

import com.hippo.util.MathUtils;
import com.hippo.util.UiUtils;

public class PentagonParameterDrawable extends Drawable {
    public static final float PENTAGON_X = 0.185f;
    public static final float PENTAGON_Y = 0.380f;

    private Paint mStrokePaint;
    private Paint mFillPaint;
    private Paint mFillStrokePaint;

    private Path mStrokePath;
    private Path mFillPath;

    private Rect mBounds;

    private int mStrokeWidth = UiUtils.dp2pix(2);
    private int mFillStrokeWidth = UiUtils.dp2pix(2);

    private float mParameterValueTop;
    private float mParameterValueRight;
    private float mParameterValueRightBottom;
    private float mParameterValueLeftBottom;
    private float mParameterValueLeft;

    public PentagonParameterDrawable() {
        mBounds = new Rect();
        mStrokePath = new Path();
        mFillPath = new Path();
        mStrokePaint = new Paint(Paint.ANTI_ALIAS_FLAG);
        mFillPaint = new Paint(Paint.ANTI_ALIAS_FLAG);
        mFillStrokePaint = new Paint(Paint.ANTI_ALIAS_FLAG);
        mStrokePaint.setStyle(Paint.Style.STROKE);
        mStrokePaint.setStrokeWidth(mStrokeWidth);
        mStrokePaint.setStrokeJoin(Paint.Join.ROUND);
        mFillPaint.setStyle(Paint.Style.FILL);
        mFillStrokePaint.setStyle(Paint.Style.STROKE);
        mFillStrokePaint.setStrokeWidth(mFillStrokeWidth);
        mFillStrokePaint.setStrokeJoin(Paint.Join.ROUND);
    }

    public void setStrokeColor(int color) {
        mStrokePaint.setColor(color);
    }

    public void setFillColor(int color) {
        mFillPaint.setColor(0x8ae91e63);
    }

    public void setFillStrokeColor(int color) {
        mFillStrokePaint.setColor(color);
    }

    public void setParameters(float top, float right, float rightBottom, float leftBottom, float left) {
        mParameterValueTop = top;
        mParameterValueRight = right;
        mParameterValueRightBottom = rightBottom;
        mParameterValueLeftBottom = leftBottom;
        mParameterValueLeft = left;
        update();
    }

    @Override
    protected void onBoundsChange(Rect bounds) {
        mBounds.set(bounds);
        update();
    }

    private void update() {
        int width = mBounds.width();
        int height = mBounds.height();
        float centerX = 0.5f * width;
        float centerY = 0.55f * (height - 2 * mStrokeWidth) + mStrokeWidth;
        float topX = centerX;
        float topY = mStrokeWidth;
        float rightX = width - mStrokeWidth;
        float rightY = height * PENTAGON_Y;
        float rightBottomX = width * (1 - PENTAGON_X);
        float rightBottomY = height - mStrokeWidth;
        float leftBottomX = width * PENTAGON_X;
        float leftBottomY = height - mStrokeWidth;
        float leftX = mStrokeWidth;
        float leftY = height * PENTAGON_Y;

        mStrokePath.reset();
        mStrokePath.moveTo(centerX, centerY);
        mStrokePath.lineTo(topX, topY);
        mStrokePath.lineTo(rightX, rightY);
        mStrokePath.lineTo(centerX, centerY);
        mStrokePath.lineTo(rightBottomX, rightBottomY);
        mStrokePath.lineTo(leftBottomX, leftBottomY);
        mStrokePath.lineTo(centerX, centerY);
        mStrokePath.lineTo(leftX, leftY);
        mStrokePath.lineTo(topX, topY);
        mStrokePath.moveTo(rightX, rightY);
        mStrokePath.lineTo(rightBottomX, rightBottomY);
        mStrokePath.moveTo(leftX, leftY);
        mStrokePath.lineTo(leftBottomX, leftBottomY);

        mFillPath.reset();
        mFillPath.moveTo(MathUtils.lerp(centerX, topX, mParameterValueTop),
                MathUtils.lerp(centerY, topY, mParameterValueTop));
        mFillPath.lineTo(MathUtils.lerp(centerX, rightX, mParameterValueRight),
                MathUtils.lerp(centerY, rightY, mParameterValueRight));
        mFillPath.lineTo(MathUtils.lerp(centerX, rightBottomX, mParameterValueRightBottom),
                MathUtils.lerp(centerY, rightBottomY, mParameterValueRightBottom));
        mFillPath.lineTo(MathUtils.lerp(centerX, leftBottomX, mParameterValueLeftBottom),
                MathUtils.lerp(centerY, leftBottomY, mParameterValueLeftBottom));
        mFillPath.lineTo(MathUtils.lerp(centerX, leftX, mParameterValueLeft),
                MathUtils.lerp(centerY, leftY, mParameterValueLeft));
        mFillPath.close();
    }

    @Override
    public void draw(Canvas canvas) {
        canvas.drawPath(mStrokePath, mStrokePaint);
        canvas.drawPath(mFillPath, mFillPaint);
        canvas.drawPath(mFillPath, mFillStrokePaint);
    }

    @Override
    public void setAlpha(int alpha) {
        // Not support
    }

    @Override
    public void setColorFilter(ColorFilter cf) {
        // Not support
    }

    @Override
    public int getOpacity() {
        return PixelFormat.OPAQUE;
    }

}
