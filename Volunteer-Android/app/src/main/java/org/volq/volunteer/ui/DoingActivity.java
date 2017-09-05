package org.volq.volunteer.ui;

import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.os.Bundle;
import android.support.v7.app.ActionBar;
import android.support.v7.widget.Toolbar;
import android.text.Html;
import android.view.Gravity;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.view.ViewTreeObserver;
import android.widget.HorizontalScrollView;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.widget.RatingBar;
import android.widget.ScrollView;
import android.widget.TextView;
import android.widget.Toast;

import com.hippo.drawable.DrawerArrowDrawable;
import com.hippo.util.Log;
import com.hippo.util.MathUtils;
import com.hippo.util.UiUtils;
import com.hippo.util.ViewUtils;
import com.hippo.widget.AutoWrapLayout;
import com.hippo.widget.FloatingActionButton;
import com.hippo.widget.NotifyingScrollView;
import com.nineoldandroids.view.ViewHelper;
import com.squareup.picasso.Picasso;

import org.volq.volunteer.R;
import org.volq.volunteer.client.VltClient;
import org.volq.volunteer.data.Comment;
import org.volq.volunteer.data.Doing;
import org.volq.volunteer.data.User;
import org.volq.volunteer.data.VolunteersRecord;
import org.volq.volunteer.util.VltUtils;
import org.volq.volunteer.widget.PentagonParameterView;

import de.hdodenhof.circleimageview.CircleImageView;

public class DoingActivity extends AbsActionBarActivity
        implements NotifyingScrollView.OnScrollChangedListener,
        ViewTreeObserver.OnGlobalLayoutListener, VltClient.OnDoingListener {
    private static final String TAG = DoingActivity.class.getSimpleName();

    public static final String KEY_ID = "id";

    private VltClient mClient;

    private ActionBar mActionBar;

    private NotifyingScrollView mScrollView;

    private ProgressBar mProgressBar;

    private View mStretchBox;
    private CircleImageView mAvatar;
    private TextView mDoingStatus;
    private TextView mVolunteerStatus;
    private TextView mName;
    private TextView mIntroduction;

    private HorizontalScrollView mImageContainerScroll;
    private LinearLayout mImageContainer;

    private View mDoingContainer;
    private View mDoingInfoTable;
    private TextView mDoingInfo1;
    private TextView mDoingInfo2;
    private TextView mDoingInfo3;
    private TextView mDoingInfo4;
    private TextView mDoingInfo5;
    private TextView mLocationText;
    private ImageView mMapAction;
    private PentagonParameterView mPentagon;
    private AutoWrapLayout mTagContainer;
    private LinearLayout mCommentContainer;
    private ProgressBar mProgressBarComment;
    private RatingBar mRatingBar;

    private View mStatisticsContainer;
    private TextView mBrowse;
    private TextView mFavorite;
    private TextView mJoin;

    private View mPhotoContainer;
    private View mPhotoForeground;
    private ImageView mPhoto;

    private View mDetailsContainer;

    private View mHeaderBox;
    private Toolbar mToolbar;
    private TextView mTitle;
    private TextView mSubtitle;

    private FloatingActionButton mFab;

    private int mPhotoHeightPixels;
    private int mHeaderHeightPixels;
    private int mFabHeightPixels;

    private ColorDrawable mPhotoBgDrawable;

    private Doing mDoing;
    private String mId;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        // Check parameter
        Intent intent = getIntent();
        mId = intent.getStringExtra(KEY_ID);
        if (mId == null) {
            errorToFinish(getString(R.string.mesg_invaild_parameters));
            return;
        }

        setContentView(R.layout.activity_doing);
        mClient = VltClient.getInstance(this);

        mScrollView = (NotifyingScrollView) findViewById(R.id.scroll_view);

        mProgressBar = (ProgressBar) findViewById(R.id.progressBar);

        mStretchBox = findViewById(R.id.stretch_box1);
        mAvatar = (CircleImageView) findViewById(R.id.avatar);
        mDoingStatus = (TextView) findViewById(R.id.doing_status);
        mVolunteerStatus = (TextView) findViewById(R.id.volunteer_status);
        mName = (TextView) findViewById(R.id.name);
        mIntroduction = (TextView) findViewById(R.id.introduction);

        mDoingContainer = findViewById(R.id.doing_container);
        mDoingInfoTable = mDoingContainer.findViewById(R.id.doing_info_table);
        mDoingInfo1 = (TextView) mDoingInfoTable.findViewById(R.id.doing_info1);
        mDoingInfo2 = (TextView) mDoingInfoTable.findViewById(R.id.doing_info2);
        mDoingInfo3 = (TextView) mDoingInfoTable.findViewById(R.id.doing_info3);
        mDoingInfo4 = (TextView) mDoingInfoTable.findViewById(R.id.doing_info4);
        mDoingInfo5 = (TextView) mDoingInfoTable.findViewById(R.id.doing_info5);
        mLocationText = (TextView) mDoingContainer.findViewById(R.id.location_str);
        mMapAction = (ImageView) mDoingContainer.findViewById(R.id.action_map);
        mPentagon = (PentagonParameterView) mDoingContainer.findViewById(R.id.pentagon);
        mTagContainer = (AutoWrapLayout) mDoingContainer.findViewById(R.id.tag_container);
        mCommentContainer = (LinearLayout) mDoingContainer.findViewById(R.id.comment_container);
        mProgressBarComment = (ProgressBar) mDoingContainer.findViewById(R.id.progress_bar_comment);
        mRatingBar = (RatingBar) mDoingContainer.findViewById(R.id.rating_bar);

        mStatisticsContainer = findViewById(R.id.statistics_container);
        mBrowse = (TextView) findViewById(R.id.browse);
        mFavorite = (TextView) findViewById(R.id.favorite);
        mJoin = (TextView) findViewById(R.id.join);

        mImageContainer = (LinearLayout) findViewById(R.id.image_container);

        mPhotoContainer = findViewById(R.id.photo_container);
        mPhotoForeground = mPhotoContainer.findViewById(R.id.photo_foreground);
        mPhoto = (ImageView) mPhotoContainer.findViewById(R.id.photo);

        mDetailsContainer = findViewById(R.id.details_container);

        mHeaderBox = findViewById(R.id.header);
        mToolbar = (Toolbar) mHeaderBox.findViewById(R.id.toolbar);
        mTitle = (TextView) mHeaderBox.findViewById(R.id.title);
        mSubtitle = (TextView) mHeaderBox.findViewById(R.id.subtitle);

        mFab = (FloatingActionButton) findViewById(R.id.fab);

        setSupportActionBar(mToolbar);
        DrawerArrowDrawable mDrawerArrowDrawable = new DrawerArrowDrawable(this);
        mDrawerArrowDrawable.setArrow();
        mDrawerArrowDrawable.setColor(Color.WHITE);
        mActionBar = getSupportActionBar();
        mActionBar.setDisplayHomeAsUpEnabled(true);
        mActionBar.setDisplayShowHomeEnabled(false);
        mActionBar.setHomeAsUpIndicator(mDrawerArrowDrawable);

        mScrollView.setOnScrollChangedListener(this);
        ViewTreeObserver vto = mScrollView.getViewTreeObserver();
        if (vto.isAlive()) {
            vto.addOnGlobalLayoutListener(this);
        }

        mPhotoBgDrawable = new ColorDrawable(getResources().getColor(R.color.material_blue_500));
        mPhotoForeground.setBackgroundDrawable(mPhotoBgDrawable);

        mMapAction.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (mDoing != null && mDoing.Coordinate != null) {
                    Intent intent = new Intent(DoingActivity.this, MapActivity.class);
                    intent.putExtra(MapActivity.KEY_LNG, mDoing.Coordinate.lng);
                    intent.putExtra(MapActivity.KEY_LAT, mDoing.Coordinate.lat);
                    intent.putExtra(MapActivity.KEY_LOCATION_STR, mDoing.Location);
                    DoingActivity.this.startActivity(intent);
                }
            }
        });

        setTitle(null);

        ViewUtils.setVisibility(mProgressBar, View.VISIBLE);
        ViewUtils.setVisibility(mDetailsContainer, View.GONE);
        ViewUtils.setVisibility(mPhotoContainer, View.GONE);
        ViewUtils.setVisibility(mHeaderBox, View.GONE);
        ViewUtils.setVisibility(mFab, View.GONE);
        ViewUtils.setVisibility(mCommentContainer, View.GONE);

        onRefresh();
    }

    @Override
    public void onSuccess(Doing doing) {
        mDoing = doing;
        ViewUtils.setVisibility(mProgressBar, View.GONE);
        ViewUtils.setVisibility(mDetailsContainer, View.VISIBLE);
        ViewUtils.setVisibility(mPhotoContainer, View.VISIBLE);
        ViewUtils.setVisibility(mHeaderBox, View.VISIBLE);
        ViewUtils.setVisibility(mFab, View.VISIBLE);
        onGetDoing();
    }

    @Override
    public void onFailure(Exception e) {
        Toast.makeText(this, e.getMessage(), Toast.LENGTH_SHORT).show();
    }

    public void onRefresh() {
        mClient.getDoing(mId, this);
    }

    public void onGetDoing() {
        if (mDoing.Photos.length > 0) {
            String photoUrl = mDoing.Photos[0];
            Picasso.with(DoingActivity.this).load(photoUrl).into(mPhoto);
        } else {
            Picasso.with(DoingActivity.this).load(R.drawable.ic_default_doing).into(mPhoto);
        }

        mDoingStatus.setText("活动" + VltUtils.getDoingStatusString(this, mDoing.Status)); // TODO
        if (mDoing.VolunteersRecord != null) {
            mVolunteerStatus.setText("我" + VolunteersRecord.getVolunteerStatusString(this, mDoing.VolunteersRecord.volunteerStatus));
        } else {
            mVolunteerStatus.setText("我" + VolunteersRecord.getVolunteerStatusString(this, VolunteersRecord.VOLUNTEER_STATUS_UNSIGNED_IN));
        }

        mTitle.setText(mDoing.Name);
        mSubtitle.setText(mDoing.Abstract);

        // Avatar
        Picasso.with(this).load(R.drawable.ic_default_avatar).into(mAvatar);
        mClient.getUser(mDoing.OrganizationId, new VltClient.OnGetUserListener() {
            @Override
            public void onSuccess(User user) {
                Picasso.with(DoingActivity.this).load(user.avatar).into(mAvatar);
            }

            @Override
            public void onFailure(Exception e) {
                // Empty
            }
        });

        mName.setText(mDoing.OrganizationName);
        mIntroduction.setText(Html.fromHtml(mDoing.Procedure));
        int length = mDoing.Photos.length;
        if (length == 0) {
            ViewUtils.setVisibility(mImageContainer, View.GONE);
        } else {
            for (int i = 0; i < length; i++) {
                getLayoutInflater().inflate(R.layout.doing_gallery_image, mImageContainer);
                ImageView iv = (ImageView) mImageContainer.getChildAt(i);
                final String url = mDoing.Photos[i];
                Picasso.with(DoingActivity.this).load(url).into(iv);
                iv.setOnClickListener(new View.OnClickListener() {
                    @Override
                    public void onClick(View v) {
                        int[] location = new int[2];
                        ViewUtils.getLocationInWindow(v, location);

                        Intent intent = new Intent(DoingActivity.this, ShowImageActivity.class);
                        intent.putExtra(ShowImageActivity.KEY_X, location[0]);
                        intent.putExtra(ShowImageActivity.KEY_Y, location[1]);
                        intent.putExtra(ShowImageActivity.KEY_WIDTH, v.getWidth());
                        intent.putExtra(ShowImageActivity.KEY_HEIGHT, v.getHeight());
                        intent.putExtra(ShowImageActivity.KEY_URL, url);
                        startActivity(intent);
                    }
                });
            }
        }

        mDoingInfo1.setText("从 " + VltClient.formatDateToHuman(mDoing.OpenSignInTime));
        mDoingInfo2.setText("至 " + VltClient.formatDateToHuman(mDoing.CloseSignInTime));
        mDoingInfo3.setText("从 " + VltClient.formatDateToHuman(mDoing.StartTime)); // TODO
        mDoingInfo4.setText("至 " + VltClient.formatDateToHuman(mDoing.FinishTime));
        mDoingInfo5.setText(Integer.toString(mDoing.Point));
        mLocationText.setText(mDoing.Location);
        mPentagon.setParameterText(getString(R.string.pentagon_strength), getString(R.string.pentagon_intelligence), getString(R.string.pentagon_endurance), getString(R.string.pentagon_compassion), getString(R.string.pentagon_sacrifice));
        int maxProperty = MathUtils.max(mDoing.Strength, mDoing.Intelligence, mDoing.Endurance, mDoing.Compassion, mDoing.Sacrifice);
        if (maxProperty == 0) {
            mPentagon.setParameters(0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
        } else {
            mPentagon.setParameters((float) mDoing.Strength / (float) maxProperty, (float) mDoing.Intelligence / (float) maxProperty, (float) mDoing.Endurance / (float) maxProperty, (float) mDoing.Compassion / (float) maxProperty, (float) mDoing.Sacrifice / (float) maxProperty);
        }
        mTagContainer = (AutoWrapLayout) mDoingContainer.findViewById(R.id.tag_container);
        length = mDoing.Tags.length;
        for (int i = 0; i < length; i++) {
            getLayoutInflater().inflate(R.layout.tag_item, mTagContainer);
            ((TextView) mTagContainer.getChildAt(i)).setText(mDoing.Tags[i]);
        }
        mBrowse.setText(Integer.toString(mDoing.VolunteerViewedTime));
        mFavorite.setText(Integer.toString(mDoing.VolunteerFavoritedTime));
        mJoin.setText(Integer.toString(mDoing.HasSignedInVolunteerNumber));

        if (mDoing.Status > 6 || (mDoing.VolunteersRecord != null && !VolunteersRecord.canOperate(mDoing.VolunteersRecord.volunteerStatus))) {
            ViewUtils.setVisibility(mFab, View.GONE);
        } else if (mDoing.hasSignined) {
            setFabExit();
        } else {
            setFabAdd();
        }

        // Update action bar
        invalidateOptionsMenu();

        // Get comment
        mClient.getComments(mId, VltClient.COMMENT_TYPE_ACTIVITY, 0, new VltClient.OnGetCommentsListener() {
            @Override
            public void onSuccess(Comment[] comments) {
                ViewUtils.setVisibility(mProgressBarComment, View.GONE);
                ViewUtils.setVisibility(mCommentContainer, View.VISIBLE);

                TextView tv = new TextView(DoingActivity.this);
                tv.setTextColor(DoingActivity.this.getResources().getColor(R.color.material_pink_500));
                LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT);
                tv.setGravity(Gravity.CENTER);
                int length = comments.length;
                if (length == 0) {
                    tv.setText("暂无评论");
                    mCommentContainer.addView(tv, lp);
                } else if (length <= 3) {
                    for (int i = 0; i < length; i++) {
                        addCommentView(mCommentContainer, comments[i]);
                    }
                    tv.setText("没有更多评论");
                    mCommentContainer.addView(tv, lp);
                } else {
                    for (int i = 0; i < 3; i++) {
                        addCommentView(mCommentContainer, comments[i]);
                    }
                    tv.setText("查看更多评论");
                    mCommentContainer.addView(tv, lp);
                }

                mCommentContainer.setOnClickListener(new View.OnClickListener() {
                    @Override
                    public void onClick(View v) {
                        Intent intent = new Intent(DoingActivity.this, CommentActivity.class);
                        intent.putExtra(CommentActivity.KEY_ID, mId);
                        intent.putExtra(CommentActivity.KEY_TYPE, VltClient.COMMENT_TYPE_ACTIVITY);
                        startActivity(intent);
                    }
                });
            }

            private void addCommentView(LinearLayout ll, Comment c) {
                DoingActivity.this.getLayoutInflater().inflate(R.layout.comment_item, ll);
                View v = ll.getChildAt(ll.getChildCount() - 1);
                CircleImageView avatar = (CircleImageView) v.findViewById(R.id.avatar);
                TextView name = (TextView) v.findViewById(R.id.name);
                TextView date = (TextView) v.findViewById(R.id.date);
                TextView text = (TextView) v.findViewById(R.id.text);

                Picasso.with(DoingActivity.this).load(c.Avatar).into(avatar);
                name.setText(c.UserName);
                date.setText(VltClient.formatDateToHuman(c.Time));
                text.setText(c.Content);
            }

            @Override
            public void onFailure(Exception e) {
                Toast.makeText(DoingActivity.this, "获取评论失败", Toast.LENGTH_SHORT).show();
                ViewUtils.setVisibility(mProgressBarComment, View.GONE);
            }
        });

        getRating();

        mRatingBar.setOnTouchListener(new View.OnTouchListener() {
            @Override
            public boolean onTouch(View v, MotionEvent event) {
                if (event.getAction() == MotionEvent.ACTION_UP) {
                    ViewGroup vg = (ViewGroup) getLayoutInflater().inflate(R.layout.dialog_rate, null);
                    final RatingBar rb = (RatingBar) vg.getChildAt(0);
                    new AlertDialog.Builder(DoingActivity.this).setTitle(R.string.rate).setView(vg).setNegativeButton(android.R.string.cancel, null).setPositiveButton(R.string.rate, new DialogInterface.OnClickListener() {
                        @Override
                        public void onClick(DialogInterface dialog, int which) {
                            int rating = (int) rb.getRating();
                            mClient.rate(mId, rating, new VltClient.OnRateListener() {
                                @Override
                                public void onSuccess() {
                                    Toast.makeText(DoingActivity.this, "评分大成功", Toast.LENGTH_SHORT).show();
                                    getRating();
                                }

                                @Override
                                public void onFailure(Exception e) {
                                    Log.d("TAG", "评分大失败", e);
                                    Toast.makeText(DoingActivity.this, "评分大失败", Toast.LENGTH_SHORT).show();
                                }
                            });
                        }
                    }).show();
                }
                return true;
            }
        });
    }


    private void getRating() {
        mClient.getRate(mId, new VltClient.OnGetRateListener() {
            @Override
            public void onSuccess(int rate) {
                mRatingBar.setRating(rate);
            }

            @Override
            public void onFailure(Exception e) {
                Toast.makeText(DoingActivity.this, e.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void setFabAdd() {
        mFab.setDrawable(R.drawable.ic_fab_add);
        mFab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                mClient.signInDoing(mDoing.Id, new VltClient.OnSignInDoingListener() {
                    @Override
                    public void onSuccess() {
                        Toast.makeText(DoingActivity.this, "参加大成功", Toast.LENGTH_SHORT).show(); // TODO
                        setFabExit();
                    }

                    @Override
                    public void onFailure(Exception e) {
                        Log.d(TAG, e.getMessage());
                        Toast.makeText(DoingActivity.this, "参加大失败", Toast.LENGTH_SHORT).show();
                    }
                });
            }
        });
    }

    private void setFabExit() {
        mFab.setDrawable(R.drawable.ic_fab_exit_doing);
        mFab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                mClient.signOutDoing(mDoing.Id, new VltClient.OnSignOutDoingListener() {
                    @Override
                    public void onSuccess() {
                        Toast.makeText(DoingActivity.this, "退出大成功", Toast.LENGTH_SHORT).show(); // TODO
                        setFabAdd();
                    }

                    @Override
                    public void onFailure(Exception e) {
                        Log.d(TAG, e.getMessage());
                        Toast.makeText(DoingActivity.this, "退出大失败", Toast.LENGTH_SHORT).show();
                    }
                });
            }
        });
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        MenuInflater inflater = getMenuInflater();
        if (mDoing == null || !mDoing.hasFavorited) {
            inflater.inflate(R.menu.doing_favorite, menu);
        } else {
            inflater.inflate(R.menu.doing_unfavorite, menu);
        }

        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
        case android.R.id.home:
            finish();
            return true;
        case R.id.action_favorite:
            mClient.addFavorite(mDoing.Id, true, new VltClient.OnAddFavoriteListener() {
                @Override
                public void onSuccess() {
                    Toast.makeText(DoingActivity.this, "收藏大成功", Toast.LENGTH_SHORT).show(); // TODO
                    mDoing.hasFavorited = true;
                    // Update action bar
                    invalidateOptionsMenu();
                }

                @Override
                public void onFailure(Exception e) {
                    Toast.makeText(DoingActivity.this, "收藏大失败", Toast.LENGTH_SHORT).show(); // TODO
                }
            });
            return true;
        case R.id.action_unfavorite:
            mClient.addFavorite(mDoing.Id, false, new VltClient.OnAddFavoriteListener() {
                @Override
                public void onSuccess() {
                    Toast.makeText(DoingActivity.this, "取消收藏大成功", Toast.LENGTH_SHORT).show(); // TODO
                    mDoing.hasFavorited = false;
                    // Update action bar
                    invalidateOptionsMenu();
                }

                @Override
                public void onFailure(Exception e) {
                    Toast.makeText(DoingActivity.this, "取消收藏大失败", Toast.LENGTH_SHORT).show(); // TODO
                }
            });
            return true;
        case R.id.action_share:
            Intent sendIntent = new Intent();
            sendIntent.setAction(Intent.ACTION_SEND);
            sendIntent.putExtra(Intent.EXTRA_TEXT, VltUtils.buildDoingUrl(mId));
            sendIntent.setType("text/plain");
            startActivity(sendIntent);
            return true;
        case R.id.action_comment:
            Intent intent = new Intent(DoingActivity.this, SummaryActivity.class);
            intent.putExtra(SummaryActivity.KEY_ID, mId);
            startActivity(intent);
        }
        return super.onOptionsItemSelected(item);
    }

    @Override
    public void onGlobalLayout() {
        if (mDoing == null)
            return;

        mFabHeightPixels = mFab.getHeight();
        recomputePhotoAndScrollingMetrics();
    }

    private void recomputePhotoAndScrollingMetrics() {
        mHeaderHeightPixels = mHeaderBox.getHeight();
        mPhotoHeightPixels = UiUtils.dp2pix(250); // TODO
        mPhotoHeightPixels = Math.max(mPhotoHeightPixels, mHeaderHeightPixels + UiUtils.dp2pix(32)); // TODO
        // TODO make sure mHeaderHeightPixels and mPhotoHeightPixels is resonable

        ViewGroup.LayoutParams lp = mPhotoContainer.getLayoutParams();
        if (lp.height != mPhotoHeightPixels) {
            lp.height = mPhotoHeightPixels;
            mPhotoContainer.setLayoutParams(lp);
        }

        ViewGroup.MarginLayoutParams mlp = (ViewGroup.MarginLayoutParams) mDetailsContainer.getLayoutParams();
        if (mlp.topMargin != mPhotoHeightPixels) {
            mlp.topMargin = mPhotoHeightPixels;
            mDetailsContainer.setLayoutParams(mlp);
        }

        // trigger scroll handling
        onScrollChanged(mScrollView, 0, 0, 0, 0);
    }

    @Override
    public void onScrollChanged(ScrollView v, int l, int t, int oldl, int oldt) {
        if (mDoing == null)
            return;

        int scrollY = mScrollView.getScrollY();

        float newTopHeaderBox = Math.max(mPhotoHeightPixels - mHeaderHeightPixels, scrollY);
        float newTopPhotoContainer = Math.max(0, scrollY - mPhotoHeightPixels + mHeaderHeightPixels);
        float newTopFab = Math.max(mPhotoHeightPixels - mFabHeightPixels / 2,
                scrollY + mHeaderHeightPixels - mFabHeightPixels / 2);
        ViewHelper.setTranslationY(mHeaderBox, newTopHeaderBox);
        ViewHelper.setTranslationY(mPhotoContainer, newTopPhotoContainer);
        ViewHelper.setTranslationY(mFab, newTopFab);

        // Update photo foreground
        int newAlpha = MathUtils.clamp(scrollY * 0xff / (mPhotoHeightPixels - mHeaderHeightPixels), 0, 0xff);
        if (newAlpha != mPhotoBgDrawable.getAlpha()) {
            mPhotoBgDrawable.setAlpha(newAlpha);
            mPhotoBgDrawable.invalidateSelf();
        }
    }

}
