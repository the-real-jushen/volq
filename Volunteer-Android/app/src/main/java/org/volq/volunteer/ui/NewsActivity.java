package org.volq.volunteer.ui;

import android.app.AlertDialog;
import android.app.Dialog;
import android.app.ProgressDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.support.v7.widget.StaggeredGridLayoutManager;
import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Toast;

import com.hippo.app.DoubleClickBackHelper;
import com.hippo.network.HttpHelper;
import com.hippo.util.Log;
import com.hippo.util.UiUtils;
import com.hippo.util.ViewUtils;
import com.hippo.widget.ContentLoadLayout;
import com.hippo.widget.recyclerview.EasyRecyclerView;
import com.hippo.widget.recyclerview.MarginItemDecoration;
import com.squareup.picasso.Picasso;

import org.volq.volunteer.R;
import org.volq.volunteer.account.VltAccount;
import org.volq.volunteer.account.VltAccountStore;
import org.volq.volunteer.cardview.FeedHolder;
import org.volq.volunteer.client.VltClient;
import org.volq.volunteer.data.CheckUpdateInfo;
import org.volq.volunteer.data.Feed;
import org.volq.volunteer.network.VltHttpHelper;
import org.volq.volunteer.widget.UserPlane;

import java.io.File;
import java.lang.ref.WeakReference;

public class NewsActivity extends DrawerActivity
        implements VltAccountStore.OnChangeAccountListener {
    private static final String TAG = NewsActivity.class.getSimpleName();

    private static final int REFRESH_ACTION_REFRESH = 0;
    private static final int REFRESH_ACTION_APPEND = 1;

    private VltAccountStore mAccountStore;
    private VltClient mClient;
    private DoubleClickBackHelper mDoubleClickBackHelper;

    private ContentLoadLayout mContentLoadLayout;

    private NewsContentHelper mContentHelper;
    private StaggeredGridLayoutManager mLayoutManager;
    private MarginItemDecoration mItemDecoration;

    private int mRefreshAction;
    private int mCurLastIndex = -1;
    private boolean mIsEnd = false;

    private IncomingHandler mHandler = new IncomingHandler(this);


    static class IncomingHandler extends Handler {
        private final WeakReference<NewsActivity> mService;

        IncomingHandler(NewsActivity service) {
            mService = new WeakReference<NewsActivity>(service);
        }
        @Override
        public void handleMessage(Message msg) {
            NewsActivity service = mService.get();
            if (service != null) {
                service.handleMessage(msg);
            }
        }
    }

    private boolean isFirstActivity() {
        Intent intent = getIntent();
        return intent != null && Intent.ACTION_MAIN.equals(intent.getAction());
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setTitle(R.string.title_news);

        // Set content
        mContentLoadLayout = new ContentLoadLayout(this);
        setCustomView(mContentLoadLayout);

        mAccountStore = VltAccountStore.getInstance(this);
        mClient = VltClient.getInstance(this);
        mDoubleClickBackHelper = new DoubleClickBackHelper();

        mContentHelper = new NewsContentHelper();
        mLayoutManager = new StaggeredGridLayoutManager(1, StaggeredGridLayoutManager.VERTICAL);
        mItemDecoration = new MarginItemDecoration(UiUtils.dp2pix(8)); // TODO
        mContentLoadLayout.setContentHelper(mContentHelper);
        mContentLoadLayout.setLayoutManager(mLayoutManager);
        mContentLoadLayout.addItemDecoration(mItemDecoration);
        mContentLoadLayout.setOnItemClickListener(new EasyRecyclerView.OnItemClickListener() {
            @Override
            public void onItemClick(EasyRecyclerView parent, View view, int position, long id) {
                Feed feed = mContentHelper.getData(position);
                String destinationLink = feed.destinationLink;
                if (destinationLink.startsWith("/Views/visitor.html?id=")) {
                    Intent intent = new Intent(NewsActivity.this, UserStatusActivity.class);
                    intent.putExtra(UserStatusActivity.KEY_USER_ID,
                            destinationLink.substring("/Views/visitor.html?id=".length()));
                    startActivity(intent);
                } else if (destinationLink.startsWith("/views/activity.html?id=")) {
                    Intent intent = new Intent(NewsActivity.this, DoingActivity.class);
                    intent.putExtra(DoingActivity.KEY_ID,
                            destinationLink.substring("/views/activity.html?id=".length()));
                    startActivity(intent);
                } else {
                    // TODO
                }
            }
        });

        setDrawerListActivatedPosition(DrawerActivity.POSITION_NEWS);

        VltAccountStore.getInstance(this).addOnChangeAccountListener(this);

        if (isFirstActivity()) {
            // It is the first activity
            VltAccount account = mAccountStore.getCurAccount();
            if (account != null) {
                // Have account
                mContentLoadLayout.setInProgress();
                UserPlane.allSetInProgress();
                final ProgressDialog pd = ProgressDialog.show(this, "Please wait",
                        "Logining", false, false);
                mClient.loginWithInfo(account.email, account.password,
                        new VltClient.OnLoginWithInfoListener() {
                            @Override
                            public void onSuccess(VltAccount account) {
                                pd.dismiss();
                                mAccountStore.addVltAccount(account);
                                UserPlane.allSetUser();
                            }

                            @Override
                            public void onFailure(Exception e) {
                                pd.dismiss();
                                mAccountStore.removeAllAccount(); // TODO
                                UserPlane.allResetUser();
                                mContentLoadLayout.setMessage("自动登录失败"); // TODO
                            }
                        });
            } else {
                // No account
                mContentLoadLayout.setMessage("未登录账号"); // TODO
            }
        } else {
            // It is not the first activity
            // Just get
            mContentHelper.refresh();
        }


        if (isFirstActivity()) {
            // Check update
            try {
                PackageInfo pi= getPackageManager().getPackageInfo(getPackageName(), 0);
                mClient.checkUpdate(pi.versionCode, new VltClient.OnCheckUpdateListener() {
                    @Override
                    public void onSuccess(CheckUpdateInfo cui) {
                        if (!cui.getIsLatest()) {
                            Log.d(TAG, "Need update");
                            createUpdateDialog(cui.getChangelog(), cui.getDownloadAppLink()).show();
                        } else {
                            Log.d(TAG, "Do not need update");
                        }
                    }

                    @Override
                    public void onFailure(Exception e) {
                        Log.d(TAG, e.getMessage());
                    }
                });
            } catch (PackageManager.NameNotFoundException e) {
                // Empty
            }
        }
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();

        VltAccountStore.getInstance(this).removeOnChangeAccountListener(this);
    }

    private File getDownloadDir() {
        return new File(getExternalFilesDir(null), "download");
    }

    private void downloadAPk(String link) throws Exception {
        VltHttpHelper vhh = new VltHttpHelper(NewsActivity.this);
        vhh.download(mClient.rightUrl(link), getDownloadDir(), "app.apk", null, new HttpHelper.OnDownloadListener() {
            @Override
            public void onStartConnecting() {

            }

            @Override
            public void onStartDownloading(int totalSize) {

            }

            @Override
            public void onNameFix(String newName) {

            }

            @Override
            public void onDownload(int downloadSize, int totalSize) {

            }

            @Override
            public void onStop() {

            }

            @Override
            public void onSuccess() {
                Log.d(TAG, "Update success");

                mHandler.sendEmptyMessage(1);
            }

            @Override
            public void onFailure(Exception e) {
                Log.d(TAG, "Update failed");

                mHandler.sendEmptyMessage(0);
            }
        });
    }

    private void handleMessage(Message msg) {
        if (msg.what == 1) {
            Intent intent = new Intent(Intent.ACTION_VIEW);
            intent.setDataAndType(Uri.fromFile(new File(getDownloadDir(), "app.apk")),
                    "application/vnd.android.package-archive");
            intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
            startActivity(intent);
        } else {
            Toast.makeText(this, "Download apk failed", Toast.LENGTH_SHORT).show();
        }
    }

    private Dialog createUpdateDialog(String changelog, final String link) {
        return new AlertDialog.Builder(this).setTitle("Update").setMessage("Need to update\n" + changelog)
                .setPositiveButton(android.R.string.ok, new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        if (which != DialogInterface.BUTTON_POSITIVE) {
                            return;
                        }
                        new Thread() {
                            public void run() {
                                try {
                                    downloadAPk(link);
                                } catch (Exception e) {
                                    e.printStackTrace();
                                }
                            }
                        }.start();
                    }
                }).setNegativeButton(android.R.string.cancel, null).create();
    }

    @Override
    public void onBackPressed() {
        // First close drawer
        if (isDrawerOpen(Gravity.START) || isDrawerOpen(Gravity.END)) {
            closeDrawers();
            return;
        }

        // Last double click to back
        if (isFirstActivity()) {
            if (mDoubleClickBackHelper.shouldBack()) {
                finish();
                return;
            } else {
                Toast.makeText(this, R.string.exit_tip, Toast.LENGTH_SHORT).show();
                return;
            }
        } else {
            finish();
        }
    }

    @Override
    public void onAddAccount() {

        Log.d(TAG, "onAddAccountListener");
        
        mContentHelper.refresh();
    }

    @Override
    public void onRemoveAccount() {
        mContentHelper.refresh();
    }


    private class NewsContentHelper extends ContentLoadLayout.ContentHelper<Feed, FeedHolder> {
        @Override
        public void getContent(final int index) {
            VltAccount account = mAccountStore.getCurAccount();
            if (account == null) {
                onGetContentFailure(index, new Exception("未登录账号"));
            } else {
                mClient.getFeeds(account.userId, index, new VltClient.OnGetFeedsListener() {
                    @Override
                    public void onSuccess(Feed[] feeds) {
                        onGetContentSuccess(index, feeds, feeds.length <= 0);
                    }

                    @Override
                    public void onFailure(Exception e) {
                        onGetContentFailure(index, e);
                    }
                });
            }
        }

        @Override
        public int getItemCount() {
            return getDataSize();
        }

        @Override
        public FeedHolder onCreateViewHolder(ViewGroup parent, int viewType) {
            return FeedHolder.createViewHolder(getLayoutInflater(), parent);
        }

        @Override
        public void onBindViewHolder(FeedHolder holder, int position) {
            super.onBindViewHolder(holder, position);

            Feed feed = getData(position);

            Picasso.with(NewsActivity.this).load(feed.fromAvatar).into(holder.avatar);
            holder.owner.setText(feed.fromName);
            holder.subtitle.setText(mClient.formatDateToHuman(feed.time));
            holder.message.setText(feed.text);
            if (feed.pictures.length == 0) {
                ViewUtils.setVisibility(holder.thumb, View.GONE);
            } else {
                ViewUtils.setVisibility(holder.thumb, View.VISIBLE);
                Picasso.with(NewsActivity.this).load(feed.pictures[0]).into(holder.thumb);
            }

        }
    }

}
