package org.volq.volunteer.ui;

import android.content.Intent;
import android.content.res.Configuration;
import android.content.res.Resources;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.support.v4.widget.DrawerLayout;
import android.support.v7.app.ActionBar;
import android.support.v7.app.ActionBarDrawerToggle;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.view.ViewTreeObserver;
import android.widget.AdapterView;
import android.widget.FrameLayout;

import com.hippo.util.UiUtils;
import com.hippo.widget.DrawerListView;

import org.volq.volunteer.R;
import org.volq.volunteer.widget.UserPlane;

public abstract class DrawerActivity extends AbsActionBarActivity
        implements DrawerLayout.DrawerListener, AdapterView.OnItemClickListener,
        ViewTreeObserver.OnGlobalLayoutListener {
    public static final int POSITION_NEWS = 0;
    public static final int POSITION_FIND_DOINGS = 1;
    public static final int POSITION_DOINGS = 2;
    public static final int POSITION_FRIENDS = 3;
    public static final int POSITION_MY_STATUS = 4;
    public static final int POSITION_QR_CODE = 5;
    public static final int POSITION_MY_QR_CODE = 6;

    private Resources mResources;

    private DrawerLayout mDrawerLayout;
    private FrameLayout mMainLayout;
    private View mDrawerLeft;
    private UserPlane mUserPlane;
    private DrawerListView mDrawerListView;

    private ActionBar mActionBar;
    private ActionBarDrawerToggle mDrawerToggle;

    @Override
    public void onDrawerClosed(View drawerView) {
        mDrawerToggle.onDrawerClosed(drawerView);
    }

    @Override
    public void onDrawerOpened(View drawerView) {
        mDrawerToggle.onDrawerOpened(drawerView);
    }

    @Override
    public void onDrawerSlide(View drawerView, float slideOffset) {
        mDrawerToggle.onDrawerSlide(drawerView, slideOffset);
    }

    @Override
    public void onDrawerStateChanged(int newState) {
        mDrawerToggle.onDrawerStateChanged(newState);
    }

    @Override
    protected void onPostCreate(Bundle savedInstanceState) {
        super.onPostCreate(savedInstanceState);

        // Sync the toggle state after onRestoreInstanceState has occurred.
        mDrawerToggle.syncState();
    }

    private void startActivityLater(final Intent intent) {
        postDelayed(new Runnable() {
            @Override
            public void run() {
                startActivity(intent);
            }
        }, 200); // TODO
    }

    @Override
    public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
        if (position == mDrawerListView.getActivatedPosition())
            return;

        Intent intent;
        switch (position) {
        case POSITION_NEWS:
            mDrawerLayout.closeDrawers();
            intent = new Intent(DrawerActivity.this, NewsActivity.class);
            startActivityLater(intent);
            break;
        case POSITION_FIND_DOINGS:
            mDrawerLayout.closeDrawers();
            intent = new Intent(DrawerActivity.this, FindDoingsActivity.class);
            startActivityLater(intent);
            break;
        case POSITION_DOINGS:
            mDrawerLayout.closeDrawers();
            intent = new Intent(DrawerActivity.this, DoingsActivity.class);
            startActivityLater(intent);
            break;
        case POSITION_FRIENDS:
            mDrawerLayout.closeDrawers();
            intent = new Intent(DrawerActivity.this, FriendsActivity.class);
            startActivityLater(intent);
            break;
        case POSITION_MY_STATUS:
            mDrawerLayout.closeDrawers();
            intent = new Intent(DrawerActivity.this, UserStatusActivity.class);
            intent.setAction(UserStatusActivity.ACTION_ACCOUNT_STATUS);
            startActivityLater(intent);
            break;
        case POSITION_QR_CODE:
            mDrawerLayout.closeDrawers();
            intent = new Intent(DrawerActivity.this, ScanActivity.class);
            intent.setAction(ScanActivity.ACTION_VOLUNTEER_ACTION);
            startActivity(intent);
            break;
        case POSITION_MY_QR_CODE:
            mDrawerLayout.closeDrawers();
            intent = new Intent(DrawerActivity.this, QRCodeActivity.class);
            startActivityLater(intent);
            break;
        }
    }

    @Override
    public void onConfigurationChanged(Configuration newConfig) {
        super.onConfigurationChanged(newConfig);
        mDrawerToggle.onConfigurationChanged(newConfig);
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.drawer_activity);

        mResources = getResources();

        mDrawerLayout = (DrawerLayout) findViewById(R.id.drawer_layout);
        mMainLayout = (FrameLayout) mDrawerLayout.findViewById(R.id.main_layout);
        mDrawerLeft = mDrawerLayout.findViewById(R.id.drawer_left);
        mUserPlane = (UserPlane) mDrawerLeft.findViewById(R.id.user_plane);
        mDrawerListView = (DrawerListView) mDrawerLeft.findViewById(R.id.drawer_list);

        mDrawerLayout.setDrawerListener(this);
        mDrawerLayout.getViewTreeObserver().addOnGlobalLayoutListener(this);

        Drawable[] drawableArray = {
                mResources.getDrawable(R.drawable.ic_drawer_home),
                mResources.getDrawable(R.drawable.ic_drawer_games),
                mResources.getDrawable(R.drawable.ic_drawer_my_game),
                mResources.getDrawable(R.drawable.ic_drawer_friends),
                mResources.getDrawable(R.drawable.ic_drawer_person),
                mResources.getDrawable(R.drawable.ic_drawer_camera),
                mResources.getDrawable(R.drawable.ic_drawer_qrcode)
        };
        mDrawerListView.setData(drawableArray, mResources.getStringArray(R.array.drawer_list));
        mDrawerListView.setOnItemClickListener(this);
        mDrawerListView.setSelector(new ColorDrawable(Color.TRANSPARENT));

        mActionBar = getSupportActionBar();
        mActionBar.setDisplayHomeAsUpEnabled(true);
        mActionBar.setDisplayShowHomeEnabled(false);

        mDrawerToggle = new ActionBarDrawerToggle(this, mDrawerLayout,
                R.string.drawer_open, R.string.drawer_close);
    }

    @Override
    public void onGlobalLayout() {
        int newWidth = Math.min(mDrawerLayout.getWidth() - UiUtils.dp2pix(56),
                UiUtils.dp2pix(320)); // TODO
        ViewGroup.LayoutParams lp = mDrawerLeft.getLayoutParams();
        if (lp.width != newWidth) {
            lp.width = newWidth;
            mDrawerLeft.setLayoutParams(lp);
        }
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        if (mDrawerToggle.onOptionsItemSelected(item)) {
            return true;
        } else {
            return super.onOptionsItemSelected(item);
        }
    }

    public void setCustomView(View view) {
        mMainLayout.removeAllViews();
        mMainLayout.addView(view);
    }

    public void setCustomView(int layoutResID) {
        mMainLayout.removeAllViews();
        getLayoutInflater().inflate(layoutResID, mMainLayout);
    }

    public boolean isDrawerOpen(int drawerGravity) {
        return mDrawerLayout.isDrawerOpen(drawerGravity);
    }

    public void closeDrawers() {
        mDrawerLayout.closeDrawers();
    }

    public void setDrawerListActivatedPosition(int position) {
        mDrawerListView.setActivatedPosition(position);
    }

}
