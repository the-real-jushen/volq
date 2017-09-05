package org.volq.volunteer.ui;

import android.content.Intent;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuItem;
import android.webkit.WebView;
import android.widget.Toast;

import org.volq.volunteer.R;
import org.volq.volunteer.client.VltClient;
import org.volq.volunteer.data.Summary;

public class SummaryActivity extends AbsActionBarActivity {

    public static final String KEY_ID = "id";

    private String mId;
    private VltClient mClient;

    private WebView mWebView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_summary);

        Intent intent = getIntent();
        mId = intent.getStringExtra(KEY_ID);

        mClient = VltClient.getInstance(this);

        mWebView = (WebView) findViewById(R.id.web_view);


        mClient.getSummary(mId, new VltClient.OnGetSummaryListener() {
            @Override
            public void onSuccess(Summary summary) {
                mWebView.loadData(summary.content, "text/html", "utf-8");
            }

            @Override
            public void onFailure(Exception e) {
                Toast.makeText(SummaryActivity.this, e.getMessage(), Toast.LENGTH_SHORT).show();
                finish();
            }
        });
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
                Intent intent = new Intent(SummaryActivity.this, CommentActivity.class);
                intent.putExtra(CommentActivity.KEY_ID, mId);
                intent.putExtra(CommentActivity.KEY_TYPE, VltClient.COMMENT_TYPE_SUMMARY);
                startActivity(intent);
                return true;
        }
        return super.onOptionsItemSelected(item);
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        mWebView.destroy();
    }
}
