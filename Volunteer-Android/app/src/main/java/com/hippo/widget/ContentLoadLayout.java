package com.hippo.widget;

import android.annotation.TargetApi;
import android.content.Context;
import android.os.Build;
import android.support.v7.widget.RecyclerView;
import android.support.v7.widget.StaggeredGridLayoutManager;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.FrameLayout;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import com.hippo.util.ArrayUtils;
import com.hippo.util.ViewUtils;
import com.hippo.widget.recyclerview.EasyRecyclerView;
import com.hippo.widget.refreshlayout.RefreshLayout;

import org.volq.volunteer.R;

import java.util.ArrayList;
import java.util.List;

public class ContentLoadLayout extends FrameLayout {

    private Context mContext;

    private EasyRecyclerView mEasyRecyclerView;
    private RefreshLayout mRefreshLayout;
    private TextView mTextView;
    private ProgressBar mProgressBar;

    private ContentHelper mContentHelper;

    private String mRefreshTip;

    public ContentLoadLayout(Context context) {
        super(context);
        init(context);
    }

    public ContentLoadLayout(Context context, AttributeSet attrs) {
        super(context, attrs);
        init(context);
    }

    public ContentLoadLayout(Context context, AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        init(context);
    }

    @TargetApi(Build.VERSION_CODES.LOLLIPOP)
    public ContentLoadLayout(Context context, AttributeSet attrs, int defStyleAttr, int defStyleRes) {
        super(context, attrs, defStyleAttr, defStyleRes);
        init(context);
    }

    private void init(Context context) {
        mContext = context;

        LayoutInflater.from(context).inflate(R.layout.widget_content_load_layout, this);
        mRefreshLayout = (RefreshLayout) getChildAt(0);
        mEasyRecyclerView = (EasyRecyclerView) mRefreshLayout.getChildAt(1);
        mTextView = (TextView) getChildAt(1);
        mProgressBar = (ProgressBar) getChildAt(2);

        mRefreshLayout.setFooterColorSchemeResources(
                R.color.material_pink_500,
                R.color.material_indigo_500,
                R.color.material_purple_500,
                R.color.material_teal_500);

        mRefreshLayout.setScrollDetecter(new RefreshLayout.ScrollDetecter() {
            @Override
            public boolean canChildScrollUp() {
                StaggeredGridLayoutManager lm = (StaggeredGridLayoutManager) mEasyRecyclerView.getLayoutManager();
                int[] into = lm.findFirstVisibleItemPositions(null);
                int length = into.length;
                for (int i = 0; i < length; i++) {
                    if (into[i] == 0 && lm.findViewByPosition(0) != null) {
                        return lm.findViewByPosition(0).getTop() < 0;
                    }
                }

                return true;
            }

            @Override
            public boolean isAlmostBottom() {
                int bottom = mEasyRecyclerView.getBottom();
                StaggeredGridLayoutManager lm = (StaggeredGridLayoutManager) mEasyRecyclerView.getLayoutManager();
                int[] into = lm.findFirstVisibleItemPositions(null);
                int length = into.length;
                if (length == 0) {
                    return false;
                } else {
                    for (int i = 0; i < length; i++) {
                        if (into[i] == lm.getItemCount() - 1 && lm.findViewByPosition(into[i]) != null) {
                            return lm.findViewByPosition(into[i]).getBottom() <= bottom;
                        }
                    }
                    return false;
                }
            }
        });

        setRefreshTip(context.getString(R.string.click_to_refresh));
    }

    private OnClickListener mTextOnClickListener = new OnClickListener() {
        @Override
        public void onClick(View v) {
            if (mContentHelper != null) {
                mContentHelper.refresh();
            }
        }
    };

    public void setRefreshTip(String tip) {
        mRefreshTip = tip;
    }

    public void showContent() {
        ViewUtils.setVisibility(mRefreshLayout, View.VISIBLE);
        ViewUtils.setVisibility(mTextView, View.GONE);
        ViewUtils.setVisibility(mProgressBar, View.GONE);
    }

    public void setInProgress() {
        ViewUtils.setVisibility(mRefreshLayout, View.GONE);
        ViewUtils.setVisibility(mTextView, View.GONE);
        ViewUtils.setVisibility(mProgressBar, View.VISIBLE);
    }

    public void setErrorMessage(String errorMessage) {
        ViewUtils.setVisibility(mRefreshLayout, View.GONE);
        ViewUtils.setVisibility(mTextView, View.VISIBLE);
        ViewUtils.setVisibility(mProgressBar, View.GONE);

        mTextView.setText(errorMessage + "\n\n" + mRefreshTip);
        mTextView.setOnClickListener(mTextOnClickListener);
    }

    public void setMessage(String message) {
        ViewUtils.setVisibility(mRefreshLayout, View.GONE);
        ViewUtils.setVisibility(mTextView, View.VISIBLE);
        ViewUtils.setVisibility(mProgressBar, View.GONE);

        mTextView.setText(message);
        mTextView.setOnClickListener(null);
        mTextView.setClickable(false);
    }

    public void setHeaderEnable(boolean enable) {
        mRefreshLayout.setHeaderEnable(enable);
    }

    public void setFooterEnable(boolean enable) {
        mRefreshLayout.setFooterEnable(enable);
    }

    public void setHeaderRefresh() {
        ViewUtils.setVisibility(mRefreshLayout, View.VISIBLE);
        ViewUtils.setVisibility(mTextView, View.GONE);
        ViewUtils.setVisibility(mProgressBar, View.GONE);

        mRefreshLayout.setHeaderRefreshing(true);
    }

    public void stopHeaderFooterRefresh() {
        mRefreshLayout.setHeaderRefreshing(false);
        mRefreshLayout.setFooterRefreshing(false);
    }

    public boolean isRefresh() {
        return mRefreshLayout.isHeaderRefreshing() || mRefreshLayout.isFooterRefreshing();
    }

    public void setContentHelper(ContentHelper contentHelper) {
        mContentHelper = contentHelper;
        contentHelper.mContentLoadLayout = this;

        mRefreshLayout.setOnHeaderRefreshListener(contentHelper);
        mRefreshLayout.setOnFooterRefreshListener(contentHelper);
        mEasyRecyclerView.setAdapter(contentHelper);
    }

    public void setLayoutManager(RecyclerView.LayoutManager lm){
        mEasyRecyclerView.setLayoutManager(lm);
    }

    public void addItemDecoration(RecyclerView.ItemDecoration decor) {
        mEasyRecyclerView.addItemDecoration(decor);
    }

    public void setOnItemClickListener(EasyRecyclerView.OnItemClickListener l) {
        mEasyRecyclerView.setOnItemClickListener(l);
    }

    public abstract static class ContentHelper<E, VH extends RecyclerView.ViewHolder>
            extends EasyRecyclerView.Adapter<VH> implements RefreshLayout.OnHeaderRefreshListener,
            RefreshLayout.OnFooterRefreshListener {

        private int mCurrentIndex = -1;
        private boolean mIsEnd = false;

        private ContentLoadLayout mContentLoadLayout;

        private List<E> mContent = new ArrayList<E>();

        @Override
        public void onHeaderRefresh() {
            refresh();
        }

        @Override
        public boolean onFooterRefresh() {
            if (mIsEnd) {
                return false;
            } else {
                getContent(mCurrentIndex + 1);
                return true;
            }
        }

        public int getDataSize() {
            return mContent.size();
        }

        public E getData(int index) {
            return mContent.get(index);
        }

        public void refresh() {
            getContent(0);

            // Check where the command from to change UI
            if (mContentLoadLayout.mTextView.getVisibility() == View.VISIBLE) {
                mContentLoadLayout.setInProgress();
            } else {
                mContentLoadLayout.setHeaderRefresh();
            }
        }

        public abstract void getContent(int index);

        public void onGetContentSuccess(int index, E[] data, boolean isEnd) {
            mIsEnd = isEnd;
            mCurrentIndex = index;
            mContentLoadLayout.showContent();
            mContentLoadLayout.stopHeaderFooterRefresh();
            if (index == 0) {
                mContent.clear();
                ArrayUtils.addAll(mContent, data);
                notifyDataSetChanged();
                if (data.length == 0) {
                    mContentLoadLayout.setErrorMessage("什么都没有");
                }
            } else {
                int startIndex = mContent.size();
                int length = data.length;
                ArrayUtils.addAll(mContent, data);
                notifyItemRangeInserted(startIndex, length);
            }
        }

        public void onGetContentFailure(int index, Exception e) {
            mContentLoadLayout.stopHeaderFooterRefresh();
            if (index == 0) {
                mContentLoadLayout.setErrorMessage(e.getMessage());
            } else {
                Toast.makeText(mContentLoadLayout.getContext(), e.getMessage(),
                        Toast.LENGTH_LONG).show();
            }
        }


    }

}
