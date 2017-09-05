package org.volq.volunteer.ui;

import android.os.Bundle;
import android.support.v4.app.FragmentTransaction;
import android.support.v4.view.PagerAdapter;
import android.support.v4.view.ViewPager;
import android.support.v7.app.ActionBar;
import android.support.v7.app.ActionBar.Tab;

import org.volq.volunteer.R;

public abstract class ViewPagerActivity extends DrawerActivity {
    private static final String TAG = ViewPagerActivity.class.getSimpleName();

    private ActionBar mActionBar;

    private ViewPager mViewPager;

    private ViewPager.OnPageChangeListener mOnPageChangeListener;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setCustomView(R.layout.viewpager);

        mViewPager = (ViewPager) findViewById(R.id.pager);

        mActionBar = getSupportActionBar();
        mActionBar.setNavigationMode(ActionBar.NAVIGATION_MODE_TABS);

        mViewPager.setOnPageChangeListener(new ViewPager.OnPageChangeListener() {
            @Override
            public void onPageScrolled(int arg0, float arg1, int arg2) {
                if (mOnPageChangeListener != null)
                    mOnPageChangeListener.onPageScrolled(arg0, arg1, arg2);
            }

            @Override
            public void onPageSelected(int position) {
                if (mOnPageChangeListener != null)
                    mOnPageChangeListener.onPageSelected(position);

                mActionBar.setSelectedNavigationItem(position);
            }

            @Override
            public void onPageScrollStateChanged(int arg0) {
                if (mOnPageChangeListener != null)
                    mOnPageChangeListener.onPageScrollStateChanged(arg0);
            }
        });
    }

    public int getCurrentItem() {
        return mViewPager.getCurrentItem();
    }

    @SuppressWarnings("deprecation")
    public void setViewPageAdapter(PagerAdapter adapter) {
        mViewPager.setAdapter(adapter);

        ActionBar.TabListener tabListener = new ActionBar.TabListener() {
            @Override
            public void onTabReselected(Tab tab, FragmentTransaction ft) {
                // Empty
            }

            @Override
            public void onTabSelected(Tab tab, FragmentTransaction ft) {
                mViewPager.setCurrentItem(tab.getPosition());
            }

            @Override
            public void onTabUnselected(Tab tab, FragmentTransaction ft) {
                // Empty
            }
        };

        for (int i = 0; i < adapter.getCount(); i++) {
            mActionBar.addTab(
                    mActionBar.newTab()
                            .setText(adapter.getPageTitle(i))
                            .setTabListener(tabListener));
        }
    }

    public void setOnPageChangeListener(ViewPager.OnPageChangeListener l) {
        mOnPageChangeListener = l;
    }
}
