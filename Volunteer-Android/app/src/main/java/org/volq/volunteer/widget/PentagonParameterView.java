package org.volq.volunteer.widget;

import android.annotation.TargetApi;
import android.content.Context;
import android.content.res.TypedArray;
import android.graphics.Color;
import android.os.Build;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import com.hippo.util.UiUtils;

import org.volq.volunteer.R;

public class PentagonParameterView extends ViewGroup {
    private static final String TAG = PentagonParameterView.class.getSimpleName();

    private TextView mParameterTop;
    private TextView mParameterRight;
    private TextView mParameterRightBottom;
    private TextView mParameterLeftBottom;
    private TextView mParameterLeft;
    private View mPentagon;

    private int mPaddingBetween = UiUtils.dp2pix(8);
    private int mDefaultSize = 50; // TODO

    private PentagonParameterDrawable mPentagonDrawable;

    public PentagonParameterView(Context context) {
        super(context);
        init(context, null, 0, 0);
    }

    public PentagonParameterView(Context context, AttributeSet attrs) {
        super(context, attrs);
        init(context, attrs, 0, 0);
    }

    public PentagonParameterView(Context context, AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        init(context, attrs, defStyleAttr, 0);
    }

    @TargetApi(Build.VERSION_CODES.LOLLIPOP)
    public PentagonParameterView(Context context, AttributeSet attrs, int defStyleAttr, int defStyleRes) {
        super(context, attrs, defStyleAttr, defStyleRes);
        init(context, attrs, defStyleAttr, defStyleRes);
    }

    @SuppressWarnings("deprecation")
    private void init(Context context, AttributeSet attrs, int defStyleAttr, int defStyleRes) {
        LayoutInflater.from(getContext()).inflate(R.layout.pentagon_view, this);

        mParameterTop = (TextView) getChildAt(0);
        mParameterRight = (TextView) getChildAt(1);
        mParameterRightBottom = (TextView) getChildAt(2);
        mParameterLeftBottom = (TextView) getChildAt(3);
        mParameterLeft = (TextView) getChildAt(4);
        mPentagon = getChildAt(5);

        mPentagonDrawable = new PentagonParameterDrawable();
        mPentagon.setBackgroundDrawable(mPentagonDrawable);

        if (attrs != null) {
            TypedArray a = context.obtainStyledAttributes(attrs,
                    R.styleable.PentagonParameterView, defStyleAttr, defStyleRes);
            int stokreColor = a.getColor(R.styleable.PentagonParameterView_strokeColor, Color.BLACK);
            setStrokeColor(stokreColor);
            int fillColor = a.getColor(R.styleable.PentagonParameterView_fillColor, Color.BLACK);
            setFillColor(fillColor);
            int fillStrokeColor = a.getColor(R.styleable.PentagonParameterView_fillStrokeColor, Color.BLACK);
            setFillStrokeColor(fillStrokeColor);
            a.recycle();
        }
    }

    public void setParameterText(String top, String right, String rightBottom, String leftBottom, String left) {
        mParameterTop.setText(top);
        mParameterRight.setText(right);
        mParameterRightBottom.setText(rightBottom);
        mParameterLeftBottom.setText(leftBottom);
        mParameterLeft.setText(left);
    }

    public void setParameters(float top, float right, float rightBottom, float leftBottom, float left) {
        mPentagonDrawable.setParameters(top, right, rightBottom, leftBottom, left);
    }

    public void setStrokeColor(int color) {
        mPentagonDrawable.setStrokeColor(color);
    }

    public void setFillColor(int color) {
        mPentagonDrawable.setFillColor(color);
    }

    public void setFillStrokeColor(int color) {
        mPentagonDrawable.setFillStrokeColor(color);
    }

    @Override
    protected void onMeasure(int widthMeasureSpec, int heightMeasureSpec) {
        int widthMode = MeasureSpec.getMode(widthMeasureSpec);
        int heightMode = MeasureSpec.getMode(heightMeasureSpec);
        int maxWidth = MeasureSpec.getSize(widthMeasureSpec);
        int maxHeight = MeasureSpec.getSize(heightMeasureSpec);
        if (widthMode == MeasureSpec.UNSPECIFIED)
            maxWidth = Integer.MAX_VALUE;
        if (heightMode == MeasureSpec.UNSPECIFIED)
            maxHeight = Integer.MAX_VALUE;

        // measure text size
        int unspecifed = MeasureSpec.makeMeasureSpec(0, MeasureSpec.UNSPECIFIED);
        for (int i = 0; i < 5; i++) {
            View child = getChildAt(i);
            child.measure(unspecifed, unspecifed);
        }

        // measure pentagon
        int pWidth;
        int pHeight;
        int pPaddingLeft = mParameterLeft.getMeasuredWidth() + mPaddingBetween;
        int pPaddingRight = mParameterRight.getMeasuredWidth() + mPaddingBetween;
        int pPaddingTop = mParameterTop.getMeasuredHeight() + mPaddingBetween;
        int pPaddingBottom = Math.max(mParameterLeftBottom.getMeasuredHeight(),
                mParameterRightBottom.getMeasuredHeight()) + mPaddingBetween;

        pWidth = Math.max(maxWidth - pPaddingLeft - pPaddingRight, 0);
        pHeight = Math.max(maxHeight - pPaddingTop - pPaddingBottom, 0);
        pHeight = pWidth = Math.min(pWidth, pHeight);
        mPentagon.measure(MeasureSpec.makeMeasureSpec(pWidth, MeasureSpec.EXACTLY),
                MeasureSpec.makeMeasureSpec(pHeight, MeasureSpec.EXACTLY));

        int measuredWidth;
        int measuredHeight;
        if (widthMode == MeasureSpec.EXACTLY) {
            measuredWidth = maxWidth;
        } else {
            measuredWidth = Math.min(maxWidth, pWidth + pPaddingLeft + pPaddingRight);
        }
        if (heightMode == MeasureSpec.EXACTLY) {
            measuredHeight = maxHeight;
        } else {
            measuredHeight = Math.min(maxHeight, pHeight + pPaddingTop + pPaddingBottom);
        }
        setMeasuredDimension(measuredWidth, measuredHeight);
    }

    @Override
    protected void onLayout(boolean changed, int l, int t, int r, int b) {
        int width = r - l;
        int height = b - t;

        int topWidth = mParameterTop.getMeasuredWidth();
        int topHeight = mParameterTop.getMeasuredHeight();
        int rightWidth = mParameterLeft.getMeasuredWidth();
        int rightHeight = mParameterLeft.getMeasuredHeight();
        int rightBottomWidth = mParameterLeftBottom.getMeasuredWidth();
        int rightBottomHeight = mParameterLeftBottom.getMeasuredHeight();
        int leftBottomWidth = mParameterRightBottom.getMeasuredWidth();
        int leftBottomHeight = mParameterRightBottom.getMeasuredHeight();
        int leftWidth = mParameterRight.getMeasuredWidth();
        int leftHeight = mParameterRight.getMeasuredHeight();
        int maxOfBottomHeight = Math.max(rightBottomHeight, leftBottomHeight);

        // Top
        int topLeft = (width - topWidth) / 2;
        mParameterTop.layout(topLeft, 0, topLeft + topWidth, topHeight);

        // Right
        int rightTop = (int) (PentagonParameterDrawable.PENTAGON_Y * (height - maxOfBottomHeight)
                + (1 - 2 * PentagonParameterDrawable.PENTAGON_Y) * mPaddingBetween
                + (1 - PentagonParameterDrawable.PENTAGON_Y) * topHeight
                - rightHeight / 2);
        mParameterLeft.layout(width - rightWidth, rightTop, width, rightTop + rightHeight);

        // Right bottom
        int rightBottomLeft = (int) ((1 - PentagonParameterDrawable.PENTAGON_X) * (width - rightWidth)
                + (2 * PentagonParameterDrawable.PENTAGON_X - 1) * mPaddingBetween
                + (PentagonParameterDrawable.PENTAGON_X) * leftWidth
                - rightBottomWidth / 2);
        mParameterLeftBottom.layout(rightBottomLeft, height - rightBottomHeight,
                rightBottomLeft + rightBottomWidth, height);

        // Left bottom
        int leftBottomLeft = (int) (PentagonParameterDrawable.PENTAGON_X * (width - rightWidth)
                + (1 - 2 * PentagonParameterDrawable.PENTAGON_X) * mPaddingBetween
                + (1 - PentagonParameterDrawable.PENTAGON_X) * leftWidth
                - leftBottomWidth / 2);
        mParameterRightBottom.layout(leftBottomLeft, height - leftBottomHeight,
                leftBottomLeft + leftBottomWidth, height);

        // Left
        int leftTop = (int) (PentagonParameterDrawable.PENTAGON_Y * (height - maxOfBottomHeight)
                + (1 - 2 * PentagonParameterDrawable.PENTAGON_Y) * mPaddingBetween
                + (1 - PentagonParameterDrawable.PENTAGON_Y) * topHeight
                - leftHeight / 2);
        mParameterRight.layout(0, leftTop, leftWidth, leftTop + leftHeight);

        // Pentagon
        int pWidth = mPentagon.getMeasuredWidth();
        int pHeight = mPentagon.getMeasuredHeight();
        if (pWidth == 0 || pHeight == 0) {
            mPentagon.layout(0, 0, pWidth, pHeight);
        } else {
            int pLeft = (leftWidth - rightWidth + width) / 2 - pWidth / 2;
            int pTop = (topHeight - Math.max(leftBottomHeight, rightBottomHeight) + height) / 2 - pHeight / 2;
            mPentagon.layout(pLeft, pTop, pLeft + pWidth, pTop + pHeight);
        }
    }

}
