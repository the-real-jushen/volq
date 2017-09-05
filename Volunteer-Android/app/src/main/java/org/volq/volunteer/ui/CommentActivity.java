package org.volq.volunteer.ui;

import android.app.AlertDialog;
import android.app.Dialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.os.Bundle;
import android.support.v7.widget.StaggeredGridLayoutManager;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.Toast;

import com.hippo.widget.ContentLoadLayout;
import com.hippo.widget.FloatingActionButton;
import com.hippo.widget.recyclerview.EasyRecyclerView;
import com.squareup.picasso.Picasso;

import org.volq.volunteer.R;
import org.volq.volunteer.cardview.CommentHolder;
import org.volq.volunteer.client.VltClient;
import org.volq.volunteer.data.Comment;

public class CommentActivity extends AbsActionBarActivity {

    public static final String KEY_ID = "id";
    public static final String KEY_TYPE = "type";

    private VltClient mClient;

    private String mId;
    private String mType;

    private ContentLoadLayout mContentLoadLayout;
    private FloatingActionButton mFab;

    private DoingsHelper mDoingsHelper;
    private StaggeredGridLayoutManager mLayoutManager;

    private Dialog mDialog;


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_comment);

        mClient = VltClient.getInstance(this);

        Intent intent = getIntent();
        mId = intent.getStringExtra(KEY_ID);
        mType = intent.getStringExtra(KEY_TYPE);

        mContentLoadLayout = (ContentLoadLayout) findViewById(R.id.content_load_layout);
        mFab = (FloatingActionButton) findViewById(R.id.fab);

        mDoingsHelper = new DoingsHelper();
        mLayoutManager = new StaggeredGridLayoutManager(1, StaggeredGridLayoutManager.VERTICAL);
        mContentLoadLayout.setContentHelper(mDoingsHelper);
        mContentLoadLayout.setLayoutManager(mLayoutManager);

        mContentLoadLayout.setOnItemClickListener(new EasyRecyclerView.OnItemClickListener() {
            @Override
            public void onItemClick(EasyRecyclerView parent, View view, int position, long id) {
                mDialog = createCommentDialog(mDoingsHelper.getData(position));
                mDialog.show();
            }
        });

        mDoingsHelper.refresh();
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.comment, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.action_comment:
                if (mDialog == null) {
                    mDialog = createCommentDialog();
                    mDialog.show();
                }
                return true;
        }
        return super.onOptionsItemSelected(item);
    }

    private AlertDialog createCommentDialog() {
        return createCommentDialog();
    }

    private AlertDialog createCommentDialog(final Comment fatherComment) {
        View view = getLayoutInflater().inflate(R.layout.dialog_comment, null);
        return new AlertDialog.Builder(this).setTitle(R.string.comment).setView(view).setNegativeButton(android.R.string.cancel, null)
                .setPositiveButton(R.string.comment, new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        EditText et = (EditText) mDialog.findViewById(R.id.edit_comment);
                        boolean isCommentOnComment = fatherComment != null;
                        mClient.postComments(mId, isCommentOnComment, fatherComment.Id, et.getText().toString(), new VltClient.OnPostCommentListener() {
                            @Override
                            public void onSuccess() {
                                Toast.makeText(CommentActivity.this, "评论大成功", Toast.LENGTH_SHORT).show();

                                mDoingsHelper.refresh();
                            }

                            @Override
                            public void onFailure(Exception e) {
                                Toast.makeText(CommentActivity.this, "评论大失败", Toast.LENGTH_SHORT).show();
                            }
                        });
                        mDialog = null;
                    }
                }).setOnCancelListener(new DialogInterface.OnCancelListener() {
                    @Override
                    public void onCancel(DialogInterface dialog) {
                        mDialog = null;
                    }
                }).create();
    }


    private class DoingsHelper extends ContentLoadLayout.ContentHelper<Comment, CommentHolder>
            implements VltClient.OnGetCommentsListener {

        private int mIndex;

        @Override
        public void getContent(int index) {
            mIndex = index;
            mClient.getComments(mId, mType, index, this);
        }

        @Override
        public int getItemCount() {
            return getDataSize();
        }

        @Override
        public CommentHolder onCreateViewHolder(ViewGroup parent, int viewType) {
            return CommentHolder.createViewHolder(CommentActivity.this.getLayoutInflater(), parent);
        }

        @Override
        public void onBindViewHolder(CommentHolder holder, int position) {
            super.onBindViewHolder(holder, position);

            Comment c = getData(position);
            Picasso.with(CommentActivity.this).load(c.Avatar).into(holder.avatar);
            holder.name.setText(c.UserName);
            holder.date.setText(VltClient.formatDateToHuman(c.Time));
            holder.text.setText(c.Content);
        }

        @Override
        public void onSuccess(Comment[] comments) {
            onGetContentSuccess(mIndex, comments, comments.length <= 0);
        }

        @Override
        public void onFailure(Exception e) {
            onGetContentFailure(mIndex, e);
        }
    }
}
