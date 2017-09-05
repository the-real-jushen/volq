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

package org.volq.volunteer.ui;

import android.app.Activity;
import android.content.Intent;
import android.graphics.drawable.ColorDrawable;
import android.os.Bundle;
import android.view.View;
import android.view.ViewTreeObserver;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.TextView;

import com.hippo.util.AnimatorUtils;
import com.nineoldandroids.animation.Animator;
import com.nineoldandroids.animation.ValueAnimator;
import com.nineoldandroids.view.ViewHelper;
import com.squareup.picasso.Picasso;

import org.volq.volunteer.R;

public class ShowImageActivity extends Activity
        implements ViewTreeObserver.OnGlobalLayoutListener,
        View.OnClickListener {
    private static final String TAG = ShowImageActivity.class.getSimpleName();

    private static final int COLOR_START = 0xff000000;
    private static final int COLOR_DARKNESS_APLHA = 0x9e;

    public static final String KEY_X = "x";
    public static final String KEY_Y = "y";
    public static final String KEY_WIDTH = "width";
    public static final String KEY_HEIGHT = "height";
    public static final String KEY_URL = "url";
    public static final String KEY_TEXT = "text";

    private View mContainer;
    private ImageView mImage;
    private TextView mText;

    private ColorDrawable mBgDrawable;

    private int mStartX;
    private int mStartY;
    private int mStartWidth;
    private int mStartHeight;
    private String mUrl;
    private String mTextStr;

    private float mStartScale;
    private int mEndX;
    private int mEndY;

    private boolean mIsInAnimation = false;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        // Disable animate
        overridePendingTransition(0, 0);
        setContentView(R.layout.activity_show_image);

        mContainer = findViewById(R.id.container);
        mImage = (ImageView) mContainer.findViewById(R.id.image);
        mText = (TextView) mContainer.findViewById(R.id.text);

        mBgDrawable = new ColorDrawable(COLOR_START);
        mBgDrawable.setAlpha(0);
        mContainer.setBackgroundDrawable(mBgDrawable);
        mContainer.setOnClickListener(this);

        Intent intent = getIntent();
        mStartX = intent.getIntExtra(KEY_X, 0);
        mStartY = intent.getIntExtra(KEY_Y, 0);
        mStartWidth = intent.getIntExtra(KEY_WIDTH, 0);
        mStartHeight = intent.getIntExtra(KEY_HEIGHT, 0);
        mUrl = intent.getStringExtra(KEY_URL);
        mTextStr = intent.getStringExtra(KEY_TEXT);

        ViewTreeObserver vto = mContainer.getViewTreeObserver();
        if (vto.isAlive()) {
            vto.addOnGlobalLayoutListener(this);
        }

        ViewHelper.setPivotX(mImage, 0);
        ViewHelper.setPivotY(mImage, 0);

        Picasso.with(this).load(mUrl).into(mImage);
    }

    @Override
    public void onGlobalLayout() {
        if (mImage.getDrawable() == null) {
            return;
        }

        int screenWidth = mContainer.getWidth();
        int screenHeight = mContainer.getHeight();
        int imageWidth = mImage.getDrawable().getIntrinsicWidth();
        int imageHeight = mImage.getDrawable().getIntrinsicHeight();
        float screenAspect = screenWidth / (float) screenHeight;
        float imageAspect = imageWidth / (float) imageHeight;
        if (mTextStr == null) {
            // No text, just make Image full screen
            int targetWidth;
            int targetHeight;
            if (imageAspect > screenAspect) {
                targetWidth = screenWidth;
                targetHeight = (int) (screenWidth / imageAspect);
            } else {
                targetWidth = (int) (screenHeight * imageAspect);
                targetHeight = screenHeight;
            }

            FrameLayout.LayoutParams lp = (FrameLayout.LayoutParams) mImage.getLayoutParams();
            lp.width = targetWidth;
            lp.height = targetHeight;

            mStartScale = mStartWidth / (float) targetWidth;
            mEndX = (screenWidth - targetWidth) / 2;
            mEndY = (screenHeight - targetHeight) / 2;
        } else {

        }

        ValueAnimator sAnimator = ValueAnimator.ofFloat(mStartScale, 1.0f);
        sAnimator.setDuration(300);
        sAnimator.addUpdateListener(new ValueAnimator.AnimatorUpdateListener() {
            @Override
            public void onAnimationUpdate(ValueAnimator animation) {
                ViewHelper.setScaleX(mImage, (Float) animation.getAnimatedValue());
                ViewHelper.setScaleY(mImage, (Float) animation.getAnimatedValue());
            }
        });
        ValueAnimator txAnimator = ValueAnimator.ofInt(mStartX, mEndX);
        txAnimator.setDuration(300);
        txAnimator.addUpdateListener(new ValueAnimator.AnimatorUpdateListener() {
            @Override
            public void onAnimationUpdate(ValueAnimator animation) {
                ViewHelper.setTranslationX(mImage, (Integer) animation.getAnimatedValue());
            }
        });
        ValueAnimator tyAnimator = ValueAnimator.ofInt(mStartY, mEndY);
        tyAnimator.setDuration(300);
        tyAnimator.addUpdateListener(new ValueAnimator.AnimatorUpdateListener() {
            @Override
            public void onAnimationUpdate(ValueAnimator animation) {
                ViewHelper.setTranslationY(mImage, (Integer) animation.getAnimatedValue());
            }
        });
        ValueAnimator aAnimator = ValueAnimator.ofInt(0, COLOR_DARKNESS_APLHA);
        aAnimator.setDuration(300);
        aAnimator.addUpdateListener(new ValueAnimator.AnimatorUpdateListener() {
            @Override
            public void onAnimationUpdate(ValueAnimator animation) {
                mBgDrawable.setAlpha((Integer) animation.getAnimatedValue());
                mBgDrawable.invalidateSelf();
            }
        });
        aAnimator.addListener(new AnimatorUtils.SimpleAnimatorListener() {
            @Override
            public void onAnimationEnd(Animator animation) {
                mIsInAnimation = false;
            }
        });
        mIsInAnimation = true;
        sAnimator.start();
        txAnimator.start();
        tyAnimator.start();
        aAnimator.start();
    }

    @Override
    public void onBackPressed() {
        if (!mIsInAnimation) {
            mIsInAnimation = true;
            doBackAnimation();
        }
    }

    private void doBackAnimation() {
        ValueAnimator sAnimator = ValueAnimator.ofFloat(1.0f, mStartScale);
        sAnimator.setDuration(300);
        sAnimator.addUpdateListener(new ValueAnimator.AnimatorUpdateListener() {
            @Override
            public void onAnimationUpdate(ValueAnimator animation) {
                ViewHelper.setScaleX(mImage, (Float) animation.getAnimatedValue());
                ViewHelper.setScaleY(mImage, (Float) animation.getAnimatedValue());
            }
        });
        ValueAnimator txAnimator = ValueAnimator.ofInt(mEndX, mStartX);
        txAnimator.setDuration(300);
        txAnimator.addUpdateListener(new ValueAnimator.AnimatorUpdateListener() {
            @Override
            public void onAnimationUpdate(ValueAnimator animation) {
                ViewHelper.setTranslationX(mImage, (Integer) animation.getAnimatedValue());
            }
        });
        ValueAnimator tyAnimator = ValueAnimator.ofInt(mEndY, mStartY);
        tyAnimator.setDuration(300);
        tyAnimator.addUpdateListener(new ValueAnimator.AnimatorUpdateListener() {
            @Override
            public void onAnimationUpdate(ValueAnimator animation) {
                ViewHelper.setTranslationY(mImage, (Integer) animation.getAnimatedValue());
            }
        });
        ValueAnimator aAnimator = ValueAnimator.ofInt(COLOR_DARKNESS_APLHA, 0);
        aAnimator.setDuration(300);
        aAnimator.addUpdateListener(new ValueAnimator.AnimatorUpdateListener() {
            @Override
            public void onAnimationUpdate(ValueAnimator animation) {
                mBgDrawable.setAlpha((Integer) animation.getAnimatedValue());
                mBgDrawable.invalidateSelf();
            }
        });
        aAnimator.addListener(new AnimatorUtils.SimpleAnimatorListener() {
            @Override
            public void onAnimationEnd(Animator animation) {
                finish();
            }
        });
        sAnimator.start();
        txAnimator.start();
        tyAnimator.start();
        aAnimator.start();
    }

    @Override
    public void onClick(View v) {
        onBackPressed();
    }

    @Override
    public void finish() {
        super.finish();
        // Disable animate
        overridePendingTransition(0, 0);
    }

}
