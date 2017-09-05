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
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package com.hippo.widget;

import android.annotation.TargetApi;
import android.content.Context;
import android.content.res.TypedArray;
import android.os.Build;
import android.text.Editable;
import android.text.TextUtils;
import android.text.TextWatcher;
import android.util.AttributeSet;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.EditText;
import android.widget.FrameLayout;
import android.widget.TextView;

import com.nineoldandroids.animation.Animator;
import com.nineoldandroids.animation.AnimatorListenerAdapter;
import com.nineoldandroids.animation.ObjectAnimator;

import org.volq.organizer.R;

// Get code from https://gist.github.com/jromero/233cfd8ef22a8b8c29ea
/**
 * Layout which an {@link android.widget.EditText} to show a floating label when the hint is hidden
 * due to the user inputting text.
 *
 * @see <a href="https://dribbble.com/shots/1254439--GIF-Mobile-Form-Interaction">Matt D. Smith on Dribble</a>
 * @see <a href="http://bradfrostweb.com/blog/post/float-label-pattern/">Brad Frost's blog post</a>
 */
public class FloatLabelEditText extends FrameLayout {

    private static final long ANIMATION_DURATION = 150;

    private static final Trigger[] sTriggerArray = {
            Trigger.TEXT,
            Trigger.FOCUS
    };

    private TextView mLabel;
    private EditText mEditText;

    private Trigger mTrigger = Trigger.TEXT;

    private String mHint;

    public FloatLabelEditText(Context context, AttributeSet attrs) {
        super(context, attrs);
        init(context, attrs);
    }

    public FloatLabelEditText(Context context, AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        init(context, attrs);
    }

    @TargetApi(Build.VERSION_CODES.LOLLIPOP)
    public FloatLabelEditText(Context context, AttributeSet attrs, int defStyleAttr, int defStyleRes) {
        super(context, attrs, defStyleAttr, defStyleRes);
        init(context, attrs);
    }

    private void init(Context context, AttributeSet attrs) {
        mLabel = new TextView(context);
        mLabel.setVisibility(View.INVISIBLE);

        final TypedArray a = context
                .obtainStyledAttributes(attrs, R.styleable.FloatLabelEditText);
        mLabel.setTextAppearance(context,
                a.getResourceId(R.styleable.FloatLabelEditText_fllLabelAppearance,
                        android.R.style.TextAppearance_Small));
        mHint = a.getString(R.styleable.FloatLabelEditText_fllHint);
        final int index = a.getInt(R.styleable.FloatLabelEditText_fllTrigger, -1);
        if (index >= 0) {
            setTrigger(sTriggerArray[index]);
        }
        a.recycle();
        addView(mLabel, LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT);

        mEditText = (EditText) LayoutInflater.from(getContext()).inflate(
                R.layout.widget_float_label_edit_text, this, false);
        final LayoutParams lp = new LayoutParams(LayoutParams.MATCH_PARENT, LayoutParams.WRAP_CONTENT);
        lp.gravity = Gravity.BOTTOM;
        lp.topMargin = (int) mLabel.getTextSize();
        addView(mEditText, lp);

        mEditText.setHint(mHint);
        mLabel.setText(mHint);

        // Add a TextWatcher so that we know when the text input has changed
        mEditText.addTextChangedListener(mTextWatcher);

        // Add focus listener to the EditText so that we can notify the label that it is activated.
        // Allows the use of a ColorStateList for the text color on the label
        mEditText.setOnFocusChangeListener(mOnFocusChangeListener);

        // Align with label and edittext
        mLabel.setPadding(mEditText.getPaddingLeft(), 0, 0, 0);
    }

    public void setTrigger(Trigger trigger) {
        mTrigger = trigger;
    }

    /**
     * Show the label using an animation
     */
    protected void showLabel() {
        if (mLabel.getVisibility() != View.VISIBLE) {
            mLabel.setVisibility(View.VISIBLE);

            final ObjectAnimator alphaAnim = ObjectAnimator.ofFloat(mLabel, "alpha",
                    0f, 1f);
            alphaAnim.setAutoCancel(true);
            alphaAnim.setDuration(ANIMATION_DURATION);

            final ObjectAnimator translationYAnim = ObjectAnimator.ofFloat(mLabel,
                    "translationY", mLabel.getHeight(), 0f);
            translationYAnim.setAutoCancel(true);
            translationYAnim.setDuration(ANIMATION_DURATION);

            alphaAnim.start();
            translationYAnim.start();
        }
    }

    /**
     * Hide the label using an animation
     */
    protected void hideLabel() {
        if (mLabel.getVisibility() != View.GONE) {
            final ObjectAnimator alphaAnim = ObjectAnimator.ofFloat(mLabel, "alpha",
                    1f, 0f);
            alphaAnim.setAutoCancel(true);
            alphaAnim.setDuration(ANIMATION_DURATION);

            final ObjectAnimator translationYAnim = ObjectAnimator.ofFloat(mLabel,
                    "translationY", 0f, mLabel.getHeight());
            translationYAnim.setAutoCancel(true);
            translationYAnim.setDuration(ANIMATION_DURATION);
            translationYAnim.addListener(new AnimatorListenerAdapter() {
                @Override
                public void onAnimationEnd(Animator animation) {
                    mLabel.setVisibility(View.GONE);
                }
            });

            alphaAnim.start();
            translationYAnim.start();
        }
    }

    public Editable getText() {
        return mEditText.getText();
    }

    private TextWatcher mTextWatcher = new TextWatcher() {

        @Override
        public void afterTextChanged(Editable s) {
            // Only takes affect if mTrigger is set to TEXT
            if (mTrigger != Trigger.TEXT) {
                return;
            }

            if (TextUtils.isEmpty(s)) {
                hideLabel();
            } else {
                showLabel();
            }
        }

        @Override
        public void beforeTextChanged(CharSequence s, int start, int count, int after) {
            // Empty
        }

        @Override
        public void onTextChanged(CharSequence s, int start, int before, int count) {
            // Empty
        }
    };

    private OnFocusChangeListener mOnFocusChangeListener = new OnFocusChangeListener() {

        @TargetApi(Build.VERSION_CODES.HONEYCOMB)
        @Override
        public void onFocusChange(View view, boolean focused) {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.HONEYCOMB) {
                mLabel.setActivated(focused);
            }

            if (mTrigger != Trigger.FOCUS) {
                return;
            }

            if (focused) {
                mEditText.setHint("");
                showLabel();
            } else {
                mEditText.setHint(mHint);
                hideLabel();
            }
        }
    };

    /**
     * Options for trigger to show a floating label
     */
    public enum Trigger {
        /**
         * Active the trigger when input text
         */
        TEXT (0),

        /**
         * Active the trigger when get focus
         */
        FOCUS (1);

        Trigger(int ni) {
            nativeInt = ni;
        }
        final int nativeInt;
    }
}
